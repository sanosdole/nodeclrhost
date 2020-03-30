using System;

namespace NodeHostEnvironment
{
    /// <summary>
    /// Wrapper for a continuous block of native memory that can be passed to JS.
    /// </summary>
    public sealed class NativeMemory : IDisposable
    {
        private Action<NativeMemory> _releaseCallback;

        public NativeMemory(IntPtr pointer, int length, Action<NativeMemory> releaseCallback)
        {
            Pointer = pointer;
            Length = length;
            _releaseCallback = releaseCallback;
        }

        public IntPtr Pointer { get; }
        public int Length { get; }

        public void Dispose()
        {
            try
            {
                _releaseCallback(this);
            }
            finally
            {
                _releaseCallback = null;
            }
        }
    }
}