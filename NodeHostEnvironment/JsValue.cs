namespace NodeHostEnvironment
{
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System;

    [StructLayout(LayoutKind.Sequential)]
    public struct JsValue
    {
        public JsType Type;
        public IntPtr Value;

        public static readonly JsValue Global = new JsValue
        {
            Type = JsType.Object,
            Value = IntPtr.Zero
        };

        public bool TryGetObject(INativeHost host, out object result)
        {
            var releaseHandle = true;
            try
            {
                switch (Type)
                {
                    case JsType.Null:
                        result = null;
                        return true;
                    case JsType.Object:
                    case JsType.Function:                        
                        releaseHandle = false;
                        result = new JsDynamicObject(this, host);
                        return true;
                    case JsType.String:
                        result = host.StringFromNativeUtf8(Value);
                        return true;
                    case JsType.Number:
                        // TODO: This will break on 32 bit systems!
                        result = BitConverter.Int64BitsToDouble((long)Value);
                        return true;
                    case JsType.Boolean:
                        result = Value != IntPtr.Zero;
                        return true;
                    case JsType.Error:
                        throw new InvalidOperationException(host.StringFromNativeUtf8(Value));

                    
                    case JsType.Undefined:
                    default:
                        result = null;
                        return false;

                }
            }
            finally
            {
                if (releaseHandle)
                    host.Release(this);
            }

        }

        public void ThrowError(INativeHost host)
        {
            if (Type != JsType.Error)
                return;
            var message = host.StringFromNativeUtf8(Value);
            host.Release(this);
            throw new InvalidOperationException(message);
        }
    }
}