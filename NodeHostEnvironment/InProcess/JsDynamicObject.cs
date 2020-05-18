namespace NodeHostEnvironment.InProcess
{
   using System;
   using System.Collections.Generic;
   using System.Dynamic;
   using System.Linq;
   using System.Threading;
   using System.Threading.Tasks;

   // Must be public for CreateNewInstance to work!
   public sealed class JsDynamicObject : DynamicObject, IDisposable
   {
      private readonly IHostInProcess _host;
      private volatile int _disposed;

      internal JsValue Handle { get; }

      internal JsDynamicObject(JsValue handle, IHostInProcess host)
      {
         Handle = handle;
         _host = host;
      }

      public bool WasDisposed => _disposed == 1;

      public void Dispose()
      {
         var wasDisposed = Interlocked.Exchange(ref _disposed, 1) == 1;
         if (wasDisposed)
            return;
         GC.SuppressFinalize(this);
         _host.Release(Handle);
      }

      ~JsDynamicObject()
      {
         var wasDisposed = Interlocked.Exchange(ref _disposed, 1) == 1;
         if (wasDisposed)
            return;
         _host.Release(Handle);
      }

      private void CheckDisposed()
      {
         if (_disposed == 1)
            throw new ObjectDisposedException("JsDynamicObject is already disposed");
      }

      public dynamic CreateNewInstance(params object[] arguments)
      {
         var result = _host.CreateObject(Handle, arguments.Select(a => DotNetValue.FromObject(a, _host)).ToArray());
         if (!result.TryGetObject(_host, typeof(object), out object newInstance))
            throw new InvalidOperationException("Could not create new instance");
         return newInstance;
      }

      public override bool TryGetMember(GetMemberBinder binder, out object result)
      {
         CheckDisposed();
         var jsHandle = _host.GetMember(Handle, binder.Name);
         return jsHandle.TryGetObject(_host, binder.ReturnType, out result);
      }

      public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
      {
         if (indexes.Length != 1)
            throw new InvalidOperationException("We only support single parameter indexer");
         CheckDisposed();

         var index = indexes[0];
         if (index == null)
            throw new ArgumentNullException(nameof(index));
         if (index is string stringIndex)
         {
            var jsHandle = _host.GetMember(Handle, stringIndex);
            return jsHandle.TryGetObject(_host, binder.ReturnType, out result);
         }

         if (index is int intIndex)
         {
            var jsHandle = _host.GetMemberByIndex(Handle, intIndex);
            return jsHandle.TryGetObject(_host, binder.ReturnType, out result);
         }

         return base.TryGetIndex(binder, indexes, out result);
      }

      public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
      {
         if (indexes.Length != 1)
            throw new InvalidOperationException("We only support single parameter indexer");
         CheckDisposed();

         var index = indexes[0];
         if (index == null)
            throw new ArgumentNullException(nameof(index));
         if (index is string stringIndex)
         {
            _host.SetMember(Handle, stringIndex, DotNetValue.FromObject(value, _host));
            return true;
         }
         /*if (index is int intIndex)
         {
             var jsHandle = _host.SetMemberByIndex(Handle, intIndex);
             return true;
         }*/

         return base.TrySetIndex(binder, indexes, value);
      }

      public override bool TryInvokeMember(InvokeMemberBinder binder,
                                           object[] args,
                                           out object result)
      {
         CheckDisposed();

         var resultHandle = _host.InvokeByName(binder.Name, Handle, args.Length, args.Select(a => DotNetValue.FromObject(a, _host)).ToArray());
         resultHandle.TryGetObject(_host, binder.ReturnType, out result);
         return true;
      }

      public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
      {
         CheckDisposed();

         var resultHandle = _host.Invoke(Handle,
                                         Handle,
                                         args.Length,
                                         args.Select(a => DotNetValue.FromObject(a, _host)).ToArray());
         resultHandle.TryGetObject(_host, binder.ReturnType, out result);
         return true;
      }

      // Converting an object to a specified type.
      public override bool TryConvert(ConvertBinder binder, out object result)
      {
         CheckDisposed();

         if (TryConvertIntern(binder.Type, out result))
            return true;

         return base.TryConvert(binder, out result);
      }

      // TODO DM 24.11.2019: This would require another bunch of reflection code to get working...
      /*
              public TaskAwaiter GetAwaiter()
              {
                  if (TryConvertIntern(typeof(Task), out object task))
                  {
                      return ((Task)task).GetAwaiter();
                  }
                  throw new InvalidOperationException("JS object is not awaitable");
              }*/

      internal bool TryConvertIntern(Type type, out object result)
      {
         // Converting to string.
         if (type == typeof(object) || type.IsAssignableFrom(GetType()))
         {
            result = this;
            return true;
         }

         //Console.WriteLine($"Converting to {type.Name}");
         if (type == typeof(string))
         {
            var jsResult = _host.InvokeByName("String",
                                              JsValue.Global,
                                              1,
                                              new DotNetValue[]
                                              {
                                                 DotNetValue.FromJsValue(Handle)
                                              });
            var gotString = jsResult.TryGetObject(_host, typeof(string), out result);
            if (!gotString)
               return false;
            return result is string;
         }

         if (typeof(Task).IsAssignableFrom(type))
         {
            var thenHandle = _host.GetMember(Handle, "then");
            try
            {
               // Ensure that Handle is a thenable/promise like object
               if (thenHandle.Type == JsType.Function)
               {
                  if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
                  {
                     // TODO DM 24.11.2019: This is inefficient. We need to do this in the native code somehow...
                     //                     How about _host.AttachTaskToPromise(Action<JsValue>,Action<JsValue>)?
                     var resultType = type.GetGenericArguments()[0];
                     var completionSourceType = typeof(TaskCompletionSource<>).MakeGenericType(resultType);
                     var tcs = Activator.CreateInstance(completionSourceType);

                     var resolve = Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(resultType),
                                                           tcs,
                                                           completionSourceType.GetMethod(nameof(TaskCompletionSource<object>.SetResult)));
                     var reject = new Action<object>(error =>
                                                     {
                                                        completionSourceType.GetMethod(nameof(TaskCompletionSource<object>.SetException))
                                                                            .Invoke(tcs, new object[] { GetExceptionFromPromiseRejection(error) });
                                                     });
                     var thenResult = _host.Invoke(thenHandle,
                                                   Handle,
                                                   2,
                                                   new[]
                                                   {
                                                      DotNetValue.FromDelegate(resolve, _host),
                                                      DotNetValue.FromDelegate(reject, _host)
                                                   });
                     // DM 29.11.2019: thenResult is always another promise
                     _host.Release(thenResult);
                     result = completionSourceType.GetProperty(nameof(TaskCompletionSource<object>.Task))
                                                  .GetValue(tcs);
                     return true;
                  }
                  else
                  {
                     var tcs = new TaskCompletionSource<object>();
                     var thenResult = _host.Invoke(thenHandle,
                                                   Handle,
                                                   2,
                                                   new DotNetValue[]
                                                   {
                                                      DotNetValue.FromDelegate(new Action(() => tcs.SetResult(null)), _host),
                                                      DotNetValue.FromDelegate(new Action<object>((error) => { tcs.SetException(GetExceptionFromPromiseRejection(error)); }), _host)
                                                   });
                     // DM 29.11.2019: thenResult is always another promise
                     _host.Release(thenResult);
                     result = tcs.Task;
                     return true;
                  }
               }
            }
            finally
            {
               _host.Release(thenHandle);
            }

            result = null;
            return false;
         }

         if (typeof(Exception).IsAssignableFrom(type))
         {
            dynamic dynamic = this;
            string stack = dynamic.stack;
            if (stack != null)
            {
               result = new InvalidOperationException($"JS Error:\n{stack}");
               return true;
            }

            result = null;
            return false;
         }

         if (type.IsArray)
         {
            var innerType = type.GetElementType();
            if (TryConvertToArray(innerType, out result))
               return true;

            result = null;
            return false;
         }

         if (type.IsGenericType && type.IsInterface)
         {
            var unclosed = type.GetGenericTypeDefinition();
            if (unclosed == typeof(IEnumerable<>) || unclosed == typeof(IReadOnlyCollection<>))
            {
               var innerType = type.GetGenericArguments()[0];
               if (TryConvertToArray(innerType, out result))
                  return true;
            }

            result = null;
            return false;
         }

         // TODO DM 17.05.2020: This dependency is unwanted (circular), we should use opend/closed for type conversion
         if (type == typeof(ArrayBuffer))
         {
            if (_host.TryAccessArrayBuffer(Handle, out var address, out var byteLength))
            {
               result = new ArrayBuffer(address, byteLength, this);
               return true;
            }
            
            result = null;
            return false;
         }

         result = null;
         return false;
      }

      private bool TryConvertToArray(Type innerType, out object result)
      {
         JsValue[] values = _host.GetArrayValues(Handle);
         if (null != values)
         {
            var array = Array.CreateInstance(innerType, values.Length);
            var i = 0;
            foreach (var value in values)
            {
               // TODO DM 05.03.2020: Should we throw or return false?
               if (!value.TryGetObject(_host, innerType, out object clrValue))
                  throw new InvalidOperationException($"Could not convert element {i} in js array to a {innerType.FullName}");
               array.SetValue(clrValue, i);
               i++;
            }

            result = array;
            return true;
         }

         result = null;
         return false;
      }

      private static Exception GetExceptionFromPromiseRejection(object error)
      {
         Exception toSet = null;
         if (error is JsDynamicObject dyna)
            if (dyna.TryConvertIntern(typeof(Exception), out object exception))
               toSet = (Exception)exception;
         if (error is string str)
            toSet = new InvalidOperationException(str);
         toSet = toSet ?? new InvalidOperationException("Unkonwn promise rejection value");
         return toSet;
      }

      public override bool TrySetMember(SetMemberBinder binder, object value)
      {
         CheckDisposed();

         _host.SetMember(Handle, binder.Name, DotNetValue.FromObject(value, _host));
         return true;
      }

      public static bool operator ==(JsDynamicObject a, JsDynamicObject b)
      {
         if (ReferenceEquals(a, b))
            return true;
         if (a is null)
            return false;
         return a.Equals(b);
      }

      // this is second one '!='
      public static bool operator !=(JsDynamicObject a, JsDynamicObject b)
      {
         if (ReferenceEquals(a, b))
            return false;
         if (a is null)
            return true;
         return !a.Equals(b);
      }

      public bool Equals(JsDynamicObject other)
      {
         if (other is null)
            return false;
         if (Handle.Type != other.Handle.Type)
            return false;

         System.Diagnostics.Debug.Assert(Handle.Type == JsType.Object || Handle.Type == JsType.Function,
                                         "Only objects are supported by JsDynamicObject atm.");

         CheckDisposed();
         // TODO: Do this using a native method, which would be more efficient.
         var gObj = _host.GetMember(JsValue.Global, "Object");
         try
         {
            var result = _host.InvokeByName("is",
                                            gObj,
                                            2,
                                            new DotNetValue[]
                                            {
                                               DotNetValue.FromJsValue(Handle),
                                               DotNetValue.FromJsValue(other.Handle)
                                            });
            return result.TryGetObject(_host, typeof(bool), out object resultObj) && (bool)resultObj;
         }
         finally
         {
            _host.Release(gObj);
         }
      }

      public override bool Equals(object obj)
      {
         if (ReferenceEquals(this, obj))
            return true;

         if (obj is JsDynamicObject other)
            return Equals(other);

         return false;
      }

      public override int GetHashCode()
      {
         // TODO: HashCode needs to be equal for equal objects. 
         //       The Handle.Value is a pointer to a reference and can not be used.
         //       So we need to call native code and ask the JS runtime for it.
         //       But JS has no hashCode function. Maybe there is a stable pointer we can utilize?
         return (int)Handle.Type;
      }
   }
}
