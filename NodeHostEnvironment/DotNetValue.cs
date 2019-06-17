namespace NodeHostEnvironment
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ReleaseDotNetValue(DotNetValue value);

    [StructLayout(LayoutKind.Sequential)]
    public struct DotNetValue
    {
        public DotNetType Type;
        public IntPtr Value;
        public ReleaseDotNetValue ReleaseFunc;

        public static DotNetValue FromObject(object obj, INativeHost host)
        {
            if (null == obj)
                return new DotNetValue
                {
                    Type = DotNetType.Null,
                    Value = IntPtr.Zero,
                    ReleaseFunc = null
                };
            if (obj is bool)
                return FromBool((bool) obj);
            if (obj is int)
                return FromInt((int)obj);
            if (obj is Delegate)
                return FromDelegate((Delegate) obj, host);
            if (obj is JsValue)
                return FromJsValue((JsValue) obj);
            if (obj is JsDynamicObject)
                return FromJsValue(((JsDynamicObject) obj).Handle);
            if (obj is string)
                return FromString((string) obj);

            throw new InvalidOperationException($"Unsupported object type for passing into JS: {obj.GetType().FullName}");
        }

        public static DotNetValue FromDelegate(Delegate @delegate, INativeHost host)
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

                // TODO: Propagate exception as JS error
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

        private static ReleaseDotNetValue ReleaseHGlobal = ReleaseHGlobalIntern;

        private static void ReleaseHGlobalIntern(DotNetValue value)
        {
            Marshal.FreeHGlobal(value.Value);
        }

        private static IntPtr NativeUtf8FromString(string managedString)
        {
            int len = Encoding.UTF8.GetByteCount(managedString);
            byte[] buffer = new byte[len + 1];
            Encoding.UTF8.GetBytes(managedString, 0, managedString.Length, buffer, 0);
            IntPtr nativeUtf8 = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, nativeUtf8, buffer.Length);
            return nativeUtf8;
        }

    }
}