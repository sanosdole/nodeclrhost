namespace NodeHostEnvironment.NativeHost
{
   using System;
   using System.Threading;
   using InProcess;

   internal sealed class NativeContext : IDisposable
   {
      private readonly NativeApi _api;
      private readonly IntPtr _context;
      private volatile int _disposed;

      private void CheckDisposed()
      {
         if (_disposed != 0) 
            throw new ObjectDisposedException(nameof(NativeContext));
      }
      public NativeContext(IntPtr context, NativeApi api)
      {
         _context = context;
         _api = api;
      }

      public void CompletePromise(in IntPtr deferred, in DotNetValue value)
      {
         CheckDisposed();
         _api.CompletePromise(_context, deferred, value);
      }

      public JsValue GetMember(in JsValue ownerHandle, string name)
      {
         CheckDisposed();
         return _api.GetMember(_context, ownerHandle, name);
      }

      public JsValue GetMemberByIndex(in JsValue ownerHandle, in int index)
      {
         CheckDisposed();
         return _api.GetMemberByIndex(_context, ownerHandle, index);
      }

      public JsValue SetMember(in JsValue ownerHandle, string name, in DotNetValue value)
      {
         CheckDisposed();
         return _api.SetMember(_context, ownerHandle, name, value);
      }

      public JsValue Invoke(in JsValue handle, in JsValue receiverHandle, DotNetValue[] argv)
      {
         CheckDisposed();
         return _api.Invoke(_context, handle, receiverHandle, argv?.Length ?? 0, argv);
      }

      public JsValue InvokeByName(string name, in JsValue receiverHandle, DotNetValue[] argv)
      {
         CheckDisposed();
         return _api.InvokeByName(_context, name, receiverHandle, argv?.Length ?? 0, argv);
      }

      public JsValue CreateObject(in JsValue constructor, DotNetValue[] arguments)
      {
         CheckDisposed();
         return _api.CreateObject(_context, constructor, arguments?.Length ?? 0, arguments);
      }

      public bool TryAccessArrayBuffer(in JsValue handle, out IntPtr address, out int byteLength)
      {
         CheckDisposed();
         return _api.TryAccessArrayBuffer(_context, handle, out address, out byteLength);
      }

      public void Release(JsValue handle)
      {
         CheckDisposed();
         _api.Release(handle);
      }

      public void RegisterSchedulerCallbacks(ProcessJsEventLoopEntry processJsEventLoopEntry, ProcessMicroTask processMicroTask)
      {
         if (_api.RegisterSchedulerCallbacks == null)
            throw new InvalidOperationException();
         _api.RegisterSchedulerCallbacks(_context, processJsEventLoopEntry, processMicroTask);
      }

      public void SignalMicroTask(in IntPtr data)
      {
         _api.SignalMicroTask(_context, data);
      }

      public void SignalEventLoopEntry(in IntPtr data)
      {
         _api.SignalEventLoopEntry(_context, data);
      }

      public void Dispose()
      {
         ReleaseUnmanagedResources();
         GC.SuppressFinalize(this);
      }

      ~NativeContext()
      {
         ReleaseUnmanagedResources();
      }

      private void ReleaseUnmanagedResources()
      {
         if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

         _api.CloseContext(_context);
      }
   }
}
