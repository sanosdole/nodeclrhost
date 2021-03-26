namespace NodeHostEnvironment
{
   using System;
   using System.Collections.Generic;
   using System.Diagnostics;
   using System.Threading;

   /// <summary>
   /// Wrapper for a continuous block of native memory that can be passed to JS.
   /// </summary>
   public sealed class NativeMemory
   {
      private Action<IntPtr, int> _releaseCallback;
      private static readonly Dictionary<IntPtr, NativeMemory> TrackedMemory = new Dictionary<IntPtr, NativeMemory>();
      private int _refCount;

      private NativeMemory(IntPtr pointer, int length, Action<IntPtr, int> releaseCallback)
      {
         Pointer = pointer;
         Length = length;
         _releaseCallback = releaseCallback;
      }

      public sealed class Handle : IDisposable
      {
         private readonly NativeMemory _memory;
         private volatile int _disposed;

         public Handle(NativeMemory memory)
         {
            _memory = memory;
            _memory.AddRef();
         }

         public IntPtr Pointer => _disposed == 0 ? _memory.Pointer : throw new ObjectDisposedException(nameof(NativeMemory));
         public int Length => _disposed == 0 ? _memory.Length : throw new ObjectDisposedException(nameof(NativeMemory));

         public Handle AddReference()
         {
            if (_disposed != 0) throw new ObjectDisposedException(nameof(NativeMemory));
            return new Handle(_memory);
         }

         ~Handle()
         {
            Dispose();
         }

         public void Dispose()
         {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
               return;
            _memory.ReleaseRef();
         }
      }

      private void ReleaseRef()
      {
         if (Interlocked.Decrement(ref _refCount) == 0)
            DisposeIntern();
      }

      private void AddRef()
      {
         Interlocked.Increment(ref _refCount);
      }

      public static Handle Create(IntPtr pointer, int length, Action<IntPtr, int> releaseCallback)
      {
         if (pointer == IntPtr.Zero) throw new ArgumentException($"Cannot create {nameof(NativeMemory)} with null pointer", nameof(pointer));
         if (length <= 0) throw new ArgumentException($"Cannot create {nameof(NativeMemory)} with zero or less length", nameof(length));
         if (releaseCallback == null) throw new ArgumentNullException(nameof(releaseCallback));

         lock (TrackedMemory)
         {
            if (TrackedMemory.TryGetValue(pointer, out var cached))
            {
               Debug.Assert(cached.Pointer == pointer);
               if (cached.Length != length) throw new ArgumentException($"Cannot change length of published {nameof(NativeMemory)}", nameof(length));
               if (cached._releaseCallback != releaseCallback) throw new ArgumentException($"Cannot change releaseCallback {nameof(NativeMemory)}", nameof(releaseCallback));

               return new Handle(cached);
            }

            var nativeMemory = new NativeMemory(pointer, length, releaseCallback);
            TrackedMemory[pointer] = nativeMemory;
            return new Handle(nativeMemory);
         }
      }

      ~NativeMemory()
      {
         DisposeIntern();
      }

      private IntPtr Pointer { get; }
      private int Length { get; }

      private void DisposeIntern()
      {
         var toCall = Interlocked.Exchange(ref _releaseCallback, null);
         if (toCall == null)
            return;

         GC.SuppressFinalize(this);
         toCall(Pointer, Length);
      }
   }
}
