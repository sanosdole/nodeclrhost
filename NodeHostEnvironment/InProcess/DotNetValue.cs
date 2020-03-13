namespace NodeHostEnvironment.InProcess
{
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using System;

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
            if (obj is bool boolean)
                return FromBool(boolean);
            if (obj is int @int)
                return FromInt(@int);
            if (obj is long @long)
                return FromLong(@long);
            if (obj is double @double)
                return FromDouble(@double);
            if (obj is Delegate @delegate)
                return FromDelegate(@delegate, host);
            if (obj is JsValue value)
                return FromJsValue(value);
            if (obj is JsDynamicObject @object)
                return FromJsValue(@object.Handle);
            if (obj is string @string)
                return FromString(@string);
            if (obj is byte[] v)
                return FromByteArray(v);
            if (obj is Exception exception)
                return FromException(exception);
            if (obj is Task task)
                return FromTask(task, host);

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
                    foreach (var toRelease in argv)
                        host.Release(toRelease);
                    // This exception will be passed properly to JS
                    throw new InvalidOperationException($"We need at least {requiredParameters.Length} arguments!");
                }

                var mappedArgs = new object[requiredParameters.Length];
                for (int c = 0; c < requiredParameters.Length; c++)
                {
                    var paramType = requiredParameters[c].ParameterType;
                    if (!argv[c].TryGetObject(host, paramType, out object parameter))
                    {
                        // Release remaining arguments
                        foreach (var toRelease in argv.Skip(c + 1))
                            host.Release(toRelease);
                        throw new InvalidOperationException($"Cannot get {paramType.FullName} from JS handle of type {argv[c].Type}");
                    }

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
                    ReleaseFunc = ReleaseString
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

        public static DotNetValue FromLong(long value)
        {
            return new DotNetValue
            {
                Type = DotNetType.Int32,
                    Value = new IntPtr(value),
                    ReleaseFunc = null
            };
        }

        public static DotNetValue FromDouble(double value)
        {
            return new DotNetValue
            {
                Type = DotNetType.Double,
                    // TODO: Breaks on 32bit node :(
                    Value = new IntPtr(BitConverter.DoubleToInt64Bits(value)),
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
                    ReleaseFunc = ReleaseString
            };
        }

        public static DotNetValue FromTask(Task value, IHostInProcess host)
        {
            ReleaseDotNetValue releaseDotNetCallback;
            return new DotNetValue
            {
                Type = DotNetType.Task,
                    Value = host.MarshallTask(value, out releaseDotNetCallback),
                    ReleaseFunc = releaseDotNetCallback
            };
        }

        private static readonly ReleaseDotNetValue ReleaseHGlobal = ReleaseHGlobalIntern;

        private static readonly ReleaseDotNetValue ReleaseString = ReleaseStringIntern;
        private static readonly ReleaseDotNetValue ReleaseArrayPointer = ReleaseArrayPointerIntern;

        private static void ReleaseHGlobalIntern(DotNetType type, IntPtr value)
        {
            Marshal.FreeHGlobal(value);
        }

        private static IntPtr NativeUtf8FromString(string managedString)
        {
            var gcHandle = GCHandle.Alloc(managedString, GCHandleType.Pinned);
            var dataPtr = gcHandle.AddrOfPinnedObject();
            var structPtr = Marshal.AllocHGlobal(sizeof(int) + 2 * IntPtr.Size);
            Marshal.WriteInt32(structPtr, managedString.Length);
            Marshal.WriteIntPtr(structPtr, sizeof(int), dataPtr);
            Marshal.WriteIntPtr(structPtr, sizeof(int) + IntPtr.Size, GCHandle.ToIntPtr(gcHandle));
            return structPtr;
        }

        private static void ReleaseStringIntern(DotNetType type, IntPtr value)
        {
            var gcHandlePtr = Marshal.ReadIntPtr(value, sizeof(int) + IntPtr.Size);
            var gcHandle = GCHandle.FromIntPtr(gcHandlePtr);
            gcHandle.Free();
            Marshal.FreeHGlobal(value);
        }

        private static void ReleaseArrayPointerIntern(DotNetType type, IntPtr value)
        {
            var gcHandlePtr = Marshal.ReadIntPtr(value, sizeof(int) + IntPtr.Size);
            var gcHandle = GCHandle.FromIntPtr(gcHandlePtr);
            gcHandle.Free();
            Marshal.FreeHGlobal(value);
        }

        private static IntPtr ArrayPointer(byte[] array)
        {
            var gcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
            var dataPtr = gcHandle.AddrOfPinnedObject();
            var structPtr = Marshal.AllocHGlobal(sizeof(int) + 2 * IntPtr.Size);
            Marshal.WriteInt32(structPtr, array.Length);
            Marshal.WriteIntPtr(structPtr, sizeof(int), dataPtr);
            Marshal.WriteIntPtr(structPtr, sizeof(int) + IntPtr.Size, GCHandle.ToIntPtr(gcHandle));
            return structPtr;
        }
    }
}