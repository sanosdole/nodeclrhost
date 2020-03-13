namespace NodeHostEnvironment.InProcess
{
    using System.Runtime.InteropServices;
    using System;

    [StructLayout(LayoutKind.Sequential)]
    internal struct JsValue
    {
        public JsType Type;
        public IntPtr Value;

        public static readonly JsValue Global = new JsValue
        {
            Type = JsType.Object,
            Value = IntPtr.Zero
        };

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
                            if (!ReferenceEquals(asDynamic, result))
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
                        var numberValue = BitConverter.Int64BitsToDouble((long) Value);
                        if (targetType == typeof(int))                        
                            result = (int)numberValue;
                        else if (targetType == typeof(long))                        
                            result = (long)numberValue;
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