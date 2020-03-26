namespace NodeHostEnvironment.NativeHost
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using System;
    using InProcess;

    internal sealed class NativeNodeHost : IHostInProcess
    {
        private readonly IntPtr _context;
        private readonly NodeTaskScheduler _scheduler;
        private readonly Dictionary<IntPtr, CallbackHolder> _registry = new Dictionary<IntPtr, CallbackHolder>();
        private readonly Dictionary<IntPtr, TaskHolder> _taskRegistry = new Dictionary<IntPtr, TaskHolder>();
        private readonly ReleaseDotNetValue ReleaseCallback;
        private readonly ReleaseDotNetValue ReleaseTaskCallback;

        private NativeApi NativeMethods { get; }

        public NativeNodeHost(IntPtr context, NativeApi nativeMethods)
        {
            NativeMethods = nativeMethods;
            _context = context;
            _scheduler = new NodeTaskScheduler(PostCallbackIntern);
            ReleaseCallback = ReleaseCallbackIntern;
            ReleaseTaskCallback = ReleaseTaskCallbackIntern;
        }

        private void PostCallbackIntern(NodeCallback callback, IntPtr data)
        {
            NativeMethods.PostCallback(_context, callback, data);
        }

        public TaskFactory Factory => _scheduler.Factory;

        private void CheckInContext()
        {
            if (!_scheduler.ContextIsActive)
                throw new InvalidOperationException("We are not on the node context!");
        }

        public IntPtr MarshallCallback(DotNetCallback callback, out ReleaseDotNetValue releaseCallback)
        {
            var holder = new CallbackHolder(callback, this);
            _registry.Add(holder.CallbackPtr, holder);
            releaseCallback = ReleaseCallback;
            return holder.CallbackPtr;
        }

        public IntPtr MarshallTask(Task task, out ReleaseDotNetValue releaseCallback)
        {
            var holder = new TaskHolder(task, this);
            _taskRegistry.Add(holder.CallbackPtr, holder);
            releaseCallback = ReleaseTaskCallback;
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
            private delegate void CallbackSignature(int argc, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 0)] JsValue[] argv, out DotNetValue result);

            public IntPtr CallbackPtr { get; }
            public DotNetCallback Wrapped { get; }
            private readonly CallbackSignature _wrapper;
            private readonly NativeNodeHost _parent;

            public CallbackHolder(DotNetCallback toWrap, NativeNodeHost parent)
            {
                Wrapped = toWrap;
                _wrapper = OnCalled;
                CallbackPtr = Marshal.GetFunctionPointerForDelegate(_wrapper);
                _parent = parent;
            }

            private void OnCalled(int argc, JsValue[] argv, out DotNetValue result)
            {
                System.Diagnostics.Debug.Assert(argc == (argv?.Length ?? 0), "Marshalling is broken");

                try
                {
                    result = (DotNetValue) _parent._scheduler.RunCallbackSynchronously(
                        state => Wrapped((JsValue[]) state),
                        argv ?? new JsValue[0]);
                }
                catch (Exception exception)
                {
                    if (exception is AggregateException aggregateException)
                        exception = UnwrapAggregateException(aggregateException);
                    if (exception is TargetInvocationException targetInvocationexception)
                        exception = targetInvocationexception.InnerException;

                    result = DotNetValue.FromException(exception);
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
                        var value = exception == null ?
                            DotNetValue.FromObject(GetResult(_task), _parent) :
                            DotNetValue.FromException(exception);
                        _parent.NativeMethods.CompletePromise(_parent._context, deferred, value);

                    }
                    else
                    {
                        _task.ContinueWith(t =>
                        {
                            var exception = UnwrapAggregateException(t.Exception);
                            var value = exception == null ?
                                DotNetValue.FromObject(GetResult(t), _parent) :
                                DotNetValue.FromException(exception);
                            _parent.NativeMethods.CompletePromise(_parent._context, deferred, value);
                        }, TaskContinuationOptions.ExecuteSynchronously);
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

            private object GetResult(Task t)
            {
                // DM 23.11.2019: This could be optimized if necessary
                var type = t.GetType();
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    // TODO DM 29.11.2019: This is required to prevent failure for some .NET generated Task instances
                    if (type.GetGenericArguments() [0].Name == "VoidTaskResult")
                        return null;

                    return type.GetProperty(nameof(Task<object>.Result)).GetValue(t);
                }
                return null;
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

        public int PostCallback(NodeCallback callback, IntPtr data)
        {
            return NativeMethods.PostCallback(_context, callback, data);
        }

        // Get a handle, ownerHandler == Zero + Object => Global
        public JsValue GetMember(JsValue ownerHandle, string name)
        {
            CheckInContext();
            return NativeMethods.GetMember(_context, ownerHandle, name);
        }

        public JsValue GetMemberByIndex(JsValue ownerHandle, int index)
        {
            CheckInContext();
            return NativeMethods.GetMemberByIndex(_context, ownerHandle, index);
        }

        // Convert handles to primitives can be done in managed code based on JsType
        // ATTENTION: 32bit node exists :(

        // Set a member
        public void SetMember(JsValue ownerHandle, string name, DotNetValue value)
        {
            CheckInContext();
            var result = NativeMethods.SetMember(_context, ownerHandle, name, value);
            result.ThrowError(this);
        }

        // Invoke handles that represent functions
        public JsValue Invoke(JsValue handle, JsValue receiverHandle, int argc, DotNetValue[] argv)
        {
            CheckInContext();
            return NativeMethods.Invoke(_context, handle, receiverHandle, argc, argv);
        }

        public JsValue InvokeByName(string name, JsValue receiverHandle, int argc, DotNetValue[] argv)
        {
            CheckInContext();
            return NativeMethods.InvokeByName(_context, name, receiverHandle, argc, argv);
        }

        public JsValue CreateObject(JsValue constructor, DotNetValue[] arguments)
        {
            CheckInContext();
            return NativeMethods.CreateObject(_context, constructor, arguments?.Length ?? 0, arguments);
        }

        public JsValue[] GetArrayValues(JsValue handle)
        {
            CheckInContext();

            var lengthHandle = NativeMethods.GetMember(_context, handle, "length");
            lengthHandle.ThrowError(this); // Maybe inner exception?
            if (lengthHandle.Type != JsType.Number)
                throw new InvalidOperationException("JsValue is not an array, as it has no 'length' member");

            // TODO: This will break on 32 bit systems!
            var length = (int) BitConverter.Int64BitsToDouble((long) lengthHandle.Value);

            // Allocating the array in managed code spares us releasing native array allocation
            var result = new JsValue[length];
            for (int i = 0; i < length; i++)
            {
                // DM 05.03.2020: Move loop to native code filling result could spare n pinvokes.
                result[i] = NativeMethods.GetMemberByIndex(_context, handle, i);
            }
            return result;
        }

        public void Release(JsValue handle)
        {
            // This should be callable from any thread
            NativeMethods.Release(handle);
        }

        public string StringFromNativeUtf8(IntPtr nativeUtf8)
        {
            int len = 0;
            while (Marshal.ReadByte(nativeUtf8, len) != 0) ++len;
            byte[] buffer = new byte[len];
            Marshal.Copy(nativeUtf8, buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer);
        }

        public bool CheckAccess()
        {
            return _scheduler.ContextIsActive;
        }
    }
}