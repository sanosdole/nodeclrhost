namespace NodeHostEnvironment.InProcess
{
    using System.Runtime.InteropServices;
    using System.Text;
    using System;
    using System.Threading.Tasks;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ReleaseDotNetValue(DotNetType type, IntPtr value);

    [StructLayout(LayoutKind.Sequential)]
    internal struct DotNetValue
    {
        public DotNetType Type;
        public IntPtr Value;
        public ReleaseDotNetValue ReleaseFunc;

        public static DotNetValue FromObject(object obj, IHostInProcess host)
        {
            if (null == obj)
                return new DotNetValue
                {
                    Type = DotNetType.Null,
                    Value = IntPtr.Zero,
                    ReleaseFunc = null
                };
            if (obj is bool)
                return FromBool((bool)obj);
            if (obj is int)
                return FromInt((int)obj);
            if (obj is Delegate)
                return FromDelegate((Delegate)obj, host);
            if (obj is JsValue)
                return FromJsValue((JsValue)obj);
            if (obj is JsDynamicObject)
                return FromJsValue(((JsDynamicObject)obj).Handle);
            if (obj is string)
                return FromString((string)obj);
            if (obj is byte[])
                return FromByteArray((byte[])obj);
            if (obj is Exception)
                return FromException((Exception)obj);
            if (obj is Task)
                return FromTask((Task)obj, host);

            throw new InvalidOperationException($"Unsupported object type for passing into JS: {obj.GetType().FullName}");
        }

        public static DotNetValue FromDelegate(Delegate @delegate, IHostInProcess host)
        {
            ReleaseDotNetValue releaseCallback;
            var value = host.MarshallCallback((int argc, JsValue[] argv, out DotNetValue result) =>
            {
                var requiredParameters = @delegate.Method.GetParameters();
                if (requiredParameters.Length > argc)
                {
                    throw new InvalidOperationException($"We need at least {requiredParameters.Length} arguments!");
                    // TODO: Error handling? Maybe retrun js error object?
                }

                var mappedArgs = new object[requiredParameters.Length];
                for (int c = 0; c < requiredParameters.Length; c++)
                {
                    object parameter;
                    if (!argv[c].TryGetObject(host, out parameter))
                        throw new InvalidOperationException("Cannot convert JsHandle to target type");

                    mappedArgs[c] = parameter;
                }

                var resultObj = @delegate.DynamicInvoke(mappedArgs);
                result = DotNetValue.FromObject(resultObj, host);

            }, out releaseCallback);

            return new DotNetValue
            {
                Type = DotNetType.Function,
                Value = value,
                ReleaseFunc = releaseCallback
            };
        }

        public static DotNetValue FromJsValue(JsValue value)
        {
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(value));
            Marshal.StructureToPtr(value, ptr, false);
            return new DotNetValue
            {
                Type = DotNetType.JsHandle,
                Value = ptr,
                ReleaseFunc = ReleaseHGlobal
            };
        }

        public static DotNetValue FromString(string value)
        {
            return new DotNetValue
            {
                Type = DotNetType.String,
                Value = NativeUtf8FromString(value),
                ReleaseFunc = ReleaseHGlobal
            };
        }

        public static DotNetValue FromBool(bool value)
        {
            return new DotNetValue
            {
                Type = DotNetType.Boolean,
                Value = value ? new IntPtr(1) : IntPtr.Zero,
                ReleaseFunc = null
            };
        }

        public static DotNetValue FromInt(int value)
        {
            return new DotNetValue
            {
                Type = DotNetType.Int32,
                Value = new IntPtr(value),
                ReleaseFunc = null
            };
        }

        public static DotNetValue FromByteArray(byte[] value)
        {
            return new DotNetValue
            {
                Type = DotNetType.ByteArray,
                Value = ArrayPointer(value),
                ReleaseFunc = ReleaseArrayPointer
            };
        }

        public static DotNetValue FromException(Exception value)
        {
            return new DotNetValue
            {
                Type = DotNetType.Exception,
                Value = NativeUtf8FromString($"{value.GetType().Name}: {value.Message}\n{value.StackTrace}"),
                ReleaseFunc = ReleaseHGlobal
            };
        }

        public static DotNetValue FromTask(Task value, IHostInProcess host)
        {
            // TODO DM 23.11.2019: Why does this work, but the native method does not?
            /*var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            var promiseCtor = host.GetMember(JsValue.Global, "Promise");
            var callback = DotNetValue.FromDelegate(new Action<dynamic, dynamic>(
                (resolve, reject) => {
                    if (value.IsCompleted)
                    {
                        if (value.Exception != null)
                        {
                            reject(value.Exception);
                            return;
                        }
                        resolve(GetTaskResult(value));
                        return;
                    }
                    value.ContinueWith(t => {
                        if (t.Exception != null)
                        {
                            reject(t.Exception);
                            return;
                        }
                        resolve(GetTaskResult(t));

                    }, scheduler);
                }
            ), host);
            var result = host.CreateObject(promiseCtor, new DotNetValue[] { callback });
            return DotNetValue.FromJsValue(result);
            */

            ReleaseDotNetValue releaseDotNetCallback;
            return new DotNetValue
            {
                Type = DotNetType.Task,
                Value = host.MarshallTask(value, out releaseDotNetCallback),
                ReleaseFunc = releaseDotNetCallback
            };
        }

        private static object GetTaskResult(Task t)
            {
                var type = t.GetType();
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    // DM 23.11.2019: This could be optimized if necessary
                    return type.GetProperty(nameof(Task<object>.Result)).GetValue(t);
                }
                return null;
            }

        private static readonly ReleaseDotNetValue ReleaseHGlobal = ReleaseHGlobalIntern;
        private static readonly ReleaseDotNetValue ReleaseArrayPointer = ReleaseArrayPointerIntern;

        private static void ReleaseHGlobalIntern(DotNetType type, IntPtr value)
        {
            //Console.WriteLine($"Releasing pointer {value.ToInt64():X8}");
            Marshal.FreeHGlobal(value);
        }

        private static IntPtr NativeUtf8FromString(string managedString)
        {
            int len = Encoding.UTF8.GetByteCount(managedString);
            byte[] buffer = new byte[len + 1];
            Encoding.UTF8.GetBytes(managedString, 0, managedString.Length, buffer, 0);
            IntPtr nativeUtf8 = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, nativeUtf8, buffer.Length);
            //Console.WriteLine($"Passing pointer {nativeUtf8.ToInt64():X8}");
            return nativeUtf8;
        }

        private static void ReleaseArrayPointerIntern(DotNetType type, IntPtr value)
        {
            // TODO DM 22.08.2019: Do not copy the array!
            Marshal.FreeHGlobal(value);
        }

        private static IntPtr ArrayPointer(byte[] array)
        {
            // TODO DM 22.08.2019: Do not copy the array!
            int length = array.Length;
            IntPtr native = Marshal.AllocHGlobal(array.Length + Marshal.SizeOf(length));
            Marshal.WriteInt32(native, length);
            Marshal.Copy(array, 0, new IntPtr(native.ToInt64() + Marshal.SizeOf(length)), length);
            return native;
        }

    }
}