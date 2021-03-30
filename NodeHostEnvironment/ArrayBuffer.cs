namespace NodeHostEnvironment
{
    using System;
    using System.Diagnostics;
    using InProcess;

    /// <summary>
    /// This class wraps a <see cref="JsDynamicObject"/> of a JS `ArrayBuffer` 
    /// to provide direct access to the underlying backing store memory.
    /// </summary>
    public sealed class ArrayBuffer : IDisposable
    {
        private JsDynamicObject _jsObject;

        /// <summary>
        /// The JavaScript `ArrayBuffer` object.
        /// </summary>
        public dynamic JsObject => _jsObject;

        /// <summary>
        /// Address of the underlying backing store memory.
        /// </summary>
        /// <value></value>
        public IntPtr Address { get; }

        /// <summary>
        /// The length in bytes of the backing store memory
        /// </summary>
        /// <value></value>
        public int ByteLength { get; }

        /// <summary>
        /// Whether this instance has been disposed.
        /// Do not use <see cref="Address"/> or <see cref="ByteLength"/> after this.
        /// </summary>
        public bool WasDisposed => _jsObject == null || _jsObject.WasDisposed;

        internal ArrayBuffer(IntPtr address, int byteLength, JsDynamicObject jsObject)
        {
            Debug.Assert(address != IntPtr.Zero);
            Debug.Assert(jsObject != null);
            Debug.Assert(byteLength > 0); // Maybe >= ?

            Address = address;
            ByteLength = byteLength;
            _jsObject = jsObject;
        }

        public void Dispose()
        {
            var previous = _jsObject;
            if (previous == null)
                return;
            _jsObject = null;
        }
    }
}
