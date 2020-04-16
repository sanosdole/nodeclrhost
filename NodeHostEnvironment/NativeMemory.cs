namespace NodeHostEnvironment
{
    using System;
    using System.Threading;

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

        ~NativeMemory()
        {
            Dispose();
        }

        public IntPtr Pointer { get; }
        public int Length { get; }

        public void Dispose()
        {
            var toCall = Interlocked.Exchange(ref _releaseCallback, null);
            if (toCall != null)
            {
                GC.SuppressFinalize(this);
                toCall(this);
            }
        }
    }
}