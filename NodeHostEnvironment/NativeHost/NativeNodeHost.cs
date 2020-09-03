namespace NodeHostEnvironment.NativeHost
{
   using System;
   using System.Collections.Generic;
   using System.Diagnostics;
   using System.Reflection;
   using System.Runtime.InteropServices;
   using System.Text;
   using System.Threading.Tasks;
   using InProcess;

   internal sealed class NativeNodeHost : IHostInProcess,
                                          IDisposable
   {
      // ReSharper disable once CollectionNeverQueried.Local as we need it to prevent GC
      private readonly Dictionary<IntPtr, CallbackHolder> _registry = new Dictionary<IntPtr, CallbackHolder>();

      // ReSharper disable once CollectionNeverQueried.Local as we need it to prevent GC
      private readonly Dictionary<IntPtr, TaskHolder> _taskRegistry = new Dictionary<IntPtr, TaskHolder>();
      private readonly ReleaseDotNetValue _releaseCallback;
      private readonly ReleaseDotNetValue _releaseTaskCallback;

      private NativeContext NativeContext { get; }

      public NativeNodeHost(IntPtr context, NativeApi nativeMethods)
      {
         NativeContext = new NativeContext(context, nativeMethods);
         _releaseCallback = ReleaseCallbackIntern;
         _releaseTaskCallback = ReleaseTaskCallbackIntern;
         Scheduler = new NodeTaskScheduler(NativeContext);
      }

      internal NodeTaskScheduler Scheduler { get; }

      public TaskFactory Factory => Scheduler.Factory;

      private void CheckInContext()
      {
         if (!Scheduler.ContextIsActive)
            throw new InvalidOperationException("We are not on the node context!");
      }

      public IntPtr MarshallCallback(DotNetCallback callback, out ReleaseDotNetValue releaseCallback)
      {
         var holder = new CallbackHolder(callback, this);
         _registry.Add(holder.CallbackPtr, holder);
         releaseCallback = _releaseCallback;
         return holder.CallbackPtr;
      }

      public IntPtr MarshallTask(Task task, out ReleaseDotNetValue releaseCallback)
      {
         var holder = new TaskHolder(task, this);
         _taskRegistry.Add(holder.CallbackPtr, holder);
         releaseCallback = _releaseTaskCallback;
         return holder.CallbackPtr;
      }

      private void ReleaseCallbackIntern(DotNetType type, IntPtr value)
      {
         _registry.Remove(value);
      }

      private void ReleaseTaskCallbackIntern(DotNetType type, IntPtr value)
      {
         _taskRegistry.Remove(value);
      }

      /// <summary>
      /// This class keeps a delegate alive until the node runtime garbage collects its usages.
      /// </summary>
      private sealed class CallbackHolder

      {
         [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
         private delegate DotNetValue CallbackSignature(int argc,
                                                        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 0)]
                                                        JsValue[] argv);

         public IntPtr CallbackPtr { get; }
         private DotNetCallback Wrapped { get; }

         // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable as we want the GC Handle it brings
         private readonly CallbackSignature _wrapper;
         private readonly NativeNodeHost _parent;
         private static readonly JsValue[] EmptyJsValues = new JsValue[0];

         public CallbackHolder(DotNetCallback toWrap, NativeNodeHost parent)
         {
            Wrapped = toWrap;
            _wrapper = OnCalled;
            CallbackPtr = Marshal.GetFunctionPointerForDelegate(_wrapper);
            _parent = parent;
         }

         private DotNetValue OnCalled(int argc, JsValue[] argv)
         {
            Debug.Assert(argc == (argv?.Length ?? 0), "Marshalling is broken");

            try
            {
               return (DotNetValue)_parent.Scheduler.RunCallbackSynchronously(
                  state => Wrapped((JsValue[])state),
                  argv ?? EmptyJsValues);
            }
            catch (Exception exception)
            {
               if (exception is AggregateException aggregateException)
                  exception = UnwrapAggregateException(aggregateException);
               if (exception is TargetInvocationException targetInvocationException)
                  exception = targetInvocationException.InnerException;

               return DotNetValue.FromException(exception);
            }
         }
      }

      /// <summary>
      /// This class keeps a Task alive until the node runtime garbage collects its usages.
      /// </summary>
      private sealed class TaskHolder
      {
         public IntPtr CallbackPtr { get; }
         private readonly Task _task;

         // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable as we need it to keep this alive as long as we live!
         private readonly SetupDeferred _wrapper;
         private readonly NativeNodeHost _parent;

         public TaskHolder(Task task, NativeNodeHost parent)
         {
            _task = task;
            _wrapper = OnCalled;
            CallbackPtr = Marshal.GetFunctionPointerForDelegate(_wrapper);
            _parent = parent;
         }

         private DotNetValue OnCalled(IntPtr deferred)
         {
            try
            {
               if (_task.IsCompleted)
               {
                  var exception = UnwrapAggregateException(_task.Exception);
                  var value = exception == null
                                 ? DotNetValue.FromObject(GetResult(_task), _parent)
                                 : DotNetValue.FromException(exception);
                  _parent.NativeContext.CompletePromise(deferred, value);
               }
               else
               {
                  _task.ContinueWith(t =>
                                     {
                                        var exception = UnwrapAggregateException(t.Exception);
                                        var value = exception == null
                                                       ? DotNetValue.FromObject(GetResult(t), _parent)
                                                       : DotNetValue.FromException(exception);
                                        _parent.NativeContext.CompletePromise(deferred, value);
                                     },
                                     _parent.Scheduler);
               }

               return new DotNetValue
                      {
                         Type = DotNetType.Null,
                         Value = IntPtr.Zero,
                         ReleaseFunc = null
                      };
            }
            catch (Exception e)
            {
               return DotNetValue.FromException(e);
            }
         }

         private static object GetResult(Task t)
         {
            var type = t.GetType();
            var resultType = GetTaskResultType(type);
            if (resultType == null)
               return null;
            // TODO DM 29.11.2019: This is required to prevent failure for some .NET generated Task instances
            if (resultType.Name == "VoidTaskResult")
               return null;

            // DM 23.11.2019: This could be optimized if necessary
            var resultPropertyInfo = type.GetProperty(nameof(Task<object>.Result));
            Debug.Assert(resultPropertyInfo != null, "Task<> always has this property!");
            return resultPropertyInfo.GetValue(t);
         }

         private static Type GetTaskResultType(Type taskType)
         {
            while (taskType != typeof(Task) &&
                   (!taskType.IsGenericType || taskType.GetGenericTypeDefinition() != typeof(Task<>)))
            {
               taskType = taskType.BaseType
                          ?? throw new ArgumentException($"The type '{taskType.FullName}' is not inherited from '{typeof(Task).FullName}'.");
            }

            return taskType.IsGenericType
                      ? taskType.GetGenericArguments()[0]
                      : null;
         }
      }

      private static Exception UnwrapAggregateException(AggregateException exception)
      {
         if (null == exception)
            return null;
         exception = exception.Flatten();
         if (exception.InnerExceptions.Count == 1)
            return exception.InnerExceptions[0];
         return exception;
      }

      // Get a handle, ownerHandler == Zero + Object => Global
      public JsValue GetMember(JsValue ownerHandle, string name)
      {
         CheckInContext();
         return NativeContext.GetMember(ownerHandle, name);
      }

      public JsValue GetMemberByIndex(JsValue ownerHandle, int index)
      {
         CheckInContext();
         return NativeContext.GetMemberByIndex(ownerHandle, index);
      }

      // Convert handles to primitives can be done in managed code based on JsType
      // ATTENTION: 32bit node exists :(

      // Set a member
      public void SetMember(JsValue ownerHandle, string name, DotNetValue value)
      {
         CheckInContext();
         var result = NativeContext.SetMember(ownerHandle, name, value);
         result.ThrowError(this);
      }

      // Invoke handles that represent functions
      public JsValue Invoke(JsValue handle, JsValue receiverHandle, DotNetValue[] argv)
      {
         CheckInContext();
         return NativeContext.Invoke(handle, receiverHandle, argv);
      }

      public JsValue InvokeByName(string name, JsValue receiverHandle, DotNetValue[] argv)
      {
         CheckInContext();
         return NativeContext.InvokeByName(name, receiverHandle, argv);
      }

      public JsValue CreateObject(JsValue constructor, DotNetValue[] arguments)
      {
         CheckInContext();
         return NativeContext.CreateObject(constructor, arguments);
      }

      public JsValue[] GetArrayValues(JsValue handle)
      {
         CheckInContext();

         var lengthHandle = NativeContext.GetMember(handle, "length");
         lengthHandle.ThrowError(this); // Maybe inner exception?
         if (lengthHandle.Type != JsType.Number)
            throw new InvalidOperationException("JsValue is not an array, as it has no 'length' member");

         // TODO: This will break on 32 bit systems!
         var length = (int)BitConverter.Int64BitsToDouble((long)lengthHandle.Value);

         // Allocating the array in managed code spares us releasing native array allocation
         var result = new JsValue[length];
         for (var i = 0; i < length; i++)
         {
            // DM 05.03.2020: Move loop to native code filling result could spare n p-invokes.
            result[i] = NativeContext.GetMemberByIndex(handle, i);
         }

         return result;
      }

      public bool TryAccessArrayBuffer(JsValue handle, out IntPtr address, out int byteLength)
      {
         return NativeContext.TryAccessArrayBuffer(handle, out address, out byteLength);
      }

      public void Release(JsValue handle)
      {
         // This should be callable from any thread
         if (!handle.RequiresContextForRelease || Scheduler.ContextIsActive)
         {
            NativeContext.Release(handle);
         }
         else
         {
            Scheduler.Factory.StartNew(() => NativeContext.Release(handle));
         }
      }

      public string StringFromNativeUtf8(IntPtr nativeUtf8)
      {
         var len = 0;
         while (Marshal.ReadByte(nativeUtf8, len) != 0) ++len;
         var buffer = new byte[len];
         Marshal.Copy(nativeUtf8, buffer, 0, buffer.Length);
         return Encoding.UTF8.GetString(buffer);
      }

      public bool CheckAccess()
      {
         return Scheduler.ContextIsActive;
      }

      public void Dispose()
      {
         Scheduler.StopAndWaitTillEmpty(() =>
                                        {
                                           // We want to do this on the proper JS thread!
                                           NativeContext.Dispose();

                                           // DM 02.09.2020: After disposal of the context those will never get removed!
                                           _registry.Clear();
                                           _taskRegistry.Clear();
                                        });
      }
   }
}
