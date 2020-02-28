namespace NodeHostEnvironment.InProcess
{
    using System.Dynamic;
    using System.Linq;
    using System.Threading.Tasks;
    using System;

    // Must be public for CreateNewInstance to work!
    public sealed class JsDynamicObject : DynamicObject, IDisposable
    {
        private readonly IHostInProcess _host;

        internal JsValue Handle { get; }

        internal JsDynamicObject(JsValue handle, IHostInProcess host)
        {
            Handle = handle;
            _host = host;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _host.Release(Handle);
        }

        ~JsDynamicObject()
        {
            _host.Release(Handle);
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
            var jsHandle = _host.GetMember(Handle, binder.Name);
            return jsHandle.TryGetObject(_host, binder.ReturnType, out result);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder,
            object[] args,
            out object result)
        {
            var member = _host.GetMember(Handle, binder.Name);
            try
            {
                if (member.Type != JsType.Function)
                {
                    result = null;
                    return false;
                }

                var resultHandle = _host.Invoke(member, Handle, args.Length, args.Select(a => DotNetValue.FromObject(a, _host)).ToArray());
                resultHandle.TryGetObject(_host, binder.ReturnType, out result);
                ConvertDynamic(binder.ReturnType, ref result);
                return true;
            }
            finally
            {
                _host.Release(member);
            }
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            var resultHandle = _host.Invoke(Handle,
                Handle,
                args.Length,
                args.Select(a => DotNetValue.FromObject(a, _host)).ToArray());
            resultHandle.TryGetObject(_host, binder.ReturnType, out result);
            ConvertDynamic(binder.ReturnType, ref result);
            return true;
        }

        private void ConvertDynamic(Type type, ref object result)
        {
            if (result is JsDynamicObject dynamic)
            {
                if (!dynamic.TryConvertIntern(type, out object temp))
                    throw new InvalidOperationException($"Could not convert js object to {type.FullName}");
                result = temp;
            }
        }

        // Converting an object to a specified type.
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
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
                var jsResult = _host.Invoke(_host.GetMember(JsValue.Global, "String"),
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
                    if (thenHandle.Type == JsType.Function)
                    {
                        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
                        {
                            // TODO DM 24.11.2019: This is inefficient. We need to do this in the native code somehow...
                            //                     How about _host.AttachTaskToPromise(Action<JsValue>,Action<JsValue>)?
                            var resultType = type.GetGenericArguments() [0];
                            var completionSourceType = typeof(TaskCompletionSource<>).MakeGenericType(resultType);
                            var tcs = Activator.CreateInstance(completionSourceType);
                            var thenResult = _host.Invoke(thenHandle, Handle, 2, new DotNetValue[]
                            {
                                DotNetValue.FromDelegate(new Action<object>((value) =>
                                {
                                    if (value is JsDynamicObject dyna)
                                    {
                                        if (dyna.TryConvertIntern(resultType, out object temp))
                                            value = temp;
                                    }
                                    completionSourceType.GetMethod(nameof(TaskCompletionSource<object>.SetResult))
                                        .Invoke(tcs, new object[] { value });
                                }), _host),
                                DotNetValue.FromDelegate(new Action<object>((error) =>
                                {
                                    completionSourceType.GetMethod(nameof(TaskCompletionSource<object>.SetException))
                                        .Invoke(tcs, new object[] { GetExceptionFromPromiseRejection(error) });
                                }), _host)
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
                            var thenResult = _host.Invoke(thenHandle, Handle, 2, new DotNetValue[]
                            {
                                DotNetValue.FromDelegate(new Action(() => tcs.SetResult(null)), _host),
                                DotNetValue.FromDelegate(new Action<object>((error) =>
                                {
                                    tcs.SetException(GetExceptionFromPromiseRejection(error));
                                }), _host)
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
            }

            result = null;
            return false;
        }

        private static Exception GetExceptionFromPromiseRejection(object error)
        {
            Exception toSet = null;
            if (error is JsDynamicObject dyna)
                if (dyna.TryConvertIntern(typeof(Exception), out object exception))
                    toSet = (Exception) exception;
            if (error is string str)
                toSet = new InvalidOperationException(str);
            toSet = toSet ?? new InvalidOperationException("Unkonwn promise rejection value");
            return toSet;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _host.SetMember(Handle, binder.Name, DotNetValue.FromObject(value, _host));
            return true;
        }
    }
}