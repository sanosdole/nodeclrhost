namespace NodeHostEnvironment.InProcess
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct JsValue
    {
        public JsType Type;
        public IntPtr Value;

        public static readonly JsValue Global = new JsValue { Type = JsType.Object, Value = IntPtr.Zero };

        public bool RequiresContextForRelease => Type == JsType.Object || Type == JsType.Function;

        public bool TryGetObject(IHostInProcess host, Type targetType, out object result)
        {
            var releaseHandle = true;
            try
            {
                switch (Type)
                {
                    case JsType.Null:
                        releaseHandle = false;
                        result = null;
                        return true;
                    case JsType.Object:
                    case JsType.Function:
                        releaseHandle = false;
                        var asDynamic = new JsDynamicObject(this, host);
                        var wasConverted = asDynamic.TryConvertIntern(targetType, out result);
                        if (wasConverted)
                        {
                            // Prevent GC release of `asDynamic` by disposing it now
                            // TODO DM 17.05.2020: The check for ArrayBuffer shows deeper design problems (conversion should be externalized?)
                            if (!ReferenceEquals(asDynamic, result) && targetType != typeof(ArrayBuffer))
                                asDynamic.Dispose();
                            return true;
                        }

                        result = asDynamic;
                        return true;
                    case JsType.String:
                        result = Marshal.PtrToStringUni(Value);
                        return true;
                    case JsType.Number:
                        // TODO: This will break on 32 bit systems!
                        releaseHandle = false;
                        var numberValue = BitConverter.Int64BitsToDouble((long)Value);
                        if (targetType == typeof(int) || targetType == typeof(int?))
                            result = (int)numberValue;
                        else if (targetType == typeof(long) || targetType == typeof(long?))
                            result = (long)numberValue;
                        else if (targetType == typeof(byte) || targetType == typeof(byte?))
                            result = (byte)numberValue;
                        else if (targetType == typeof(float) || targetType == typeof(float?))
                            result = (float)numberValue;
                        else
                            result = numberValue;
                        return true;
                    case JsType.Boolean:
                        releaseHandle = false;
                        result = Value != IntPtr.Zero;
                        return true;
                    case JsType.Error:
                        // This is not for error objects. This is whenever js code threw an exception!
                        throw new InvalidOperationException(host.StringFromNativeUtf8(Value));

                    case JsType.Undefined:
                        releaseHandle = false;
                        result = null;
                        return false;
                    default:
                        throw new InvalidOperationException($"Unsupported JsType '{Type}'");
                }
            }
            finally
            {
                if (releaseHandle)
                    host.Release(this);
            }
        }

        public void ThrowError(IHostInProcess host)
        {
            if (Type != JsType.Error)
                return;
            var message = host.StringFromNativeUtf8(Value);
            host.Release(this);
            throw new InvalidOperationException(message);
        }
    }
}
