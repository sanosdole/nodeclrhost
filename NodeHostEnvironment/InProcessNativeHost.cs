namespace NodeHostEnvironment
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Threading.Tasks;
    using System.Threading;
    using System;

    public sealed class InProcessNativeHost : INativeHost
    {
        private readonly IntPtr _context;
        private readonly NodeTaskScheduler _scheduler;

        public InProcessNativeHost()
        {
            _context = NativeMethods.GetContext();
            if (_context == IntPtr.Zero)
                throw new InvalidOperationException("Host can only be created on the node main thread");
            _scheduler = new NodeTaskScheduler(PostCallbackIntern);
            ReleaseCallback = ReleaseCallbackIntern;
        }

        private void PostCallbackIntern(NodeCallback callback, IntPtr data)
        {
            NativeMethods.PostCallback(_context, callback, data);
        }

        public TaskFactory Factory => _scheduler.Factory;

        private void CheckInContext()
        {
            if (!_scheduler.ContextIsActive)
                throw new InvalidOperationException("We are not on the node context!");
        }

        private readonly HashSet<CallbackHolder> _registry = new HashSet<CallbackHolder>();
        public IntPtr MarshallCallback(DotNetCallback callback, out ReleaseDotNetValue releaseCallback)
        {
            Console.WriteLine("Adding callback to registry");
            var holder = new CallbackHolder(callback, this);
            _registry.Add(holder);
            releaseCallback = ReleaseCallback;
            return holder.CallbackPtr;
        }

        private ReleaseDotNetValue ReleaseCallback;

        private void ReleaseCallbackIntern(DotNetValue toRelease)
        {
            Console.WriteLine("Removing callback from registry");
            _registry.Remove(_registry.First(ch => ch.CallbackPtr == toRelease.Value));
        }

        /// <summary>
        /// This class keeps a delegate alive until the node runtime garbage collects its usages.
        /// As node does no gc on its own, this keeps callbacks alive until js code calls `global.gc()`.
        /// This requires the `--expose-gc` switch when starting node.
        /// </summary>
        private sealed class CallbackHolder
        {
            public IntPtr CallbackPtr { get; }
            public DotNetCallback Wrapped { get; }
            private readonly DotNetCallback _wrapper;
            private readonly InProcessNativeHost _parent;

            public CallbackHolder(DotNetCallback toWrap, InProcessNativeHost parent)
            {
                Wrapped = toWrap;
                _wrapper = OnCalled;
                CallbackPtr = Marshal.GetFunctionPointerForDelegate(_wrapper);
                _parent = parent;
            }

            private void OnCalled(int argc, JsValue[] argv, out DotNetValue result)
            {               
                System.Diagnostics.Debug.Assert(argc == (argv?.Length ?? 0), "Marshalling is broken");

                try
                {
                    using(_parent._scheduler.SetNodeContext())
                        Wrapped(argc, argv ?? new JsValue[0], out result);                    
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("Exception while invoking callback: {0}", e);
                    throw; // TODO: put into result
                }
                
            }
        }

        public void ReleaseHost()
        {
            // TODO: Ensure that further requests are denied with exception!
            NativeMethods.ReleaseContext(_context);
        }

        public int PostCallback(NodeCallback callback, IntPtr data)
        {
            return NativeMethods.PostCallback(_context, callback, data);
        }

        // Get a handle, ownerHandler == Zero + Object => Global
        public JsValue GetMember(JsValue ownerHandle, string name)
        {
            CheckInContext();
            return NativeMethods.GetMember(_context, ownerHandle, name);            
        }

        // Convert handles to primitives can be done in managed code based on JsType
        // ATTENTION: 32bit node exists :(

        // Set a member
        public void SetMember(JsValue ownerHandle, string name, DotNetValue value)
        {
            CheckInContext();
            var result = NativeMethods.SetMember(_context, ownerHandle, name, value);
            result.ThrowError(this);
        }

        // Invoke handles that represent functions
        public JsValue Invoke(JsValue handle, JsValue receiverHandle, int argc, DotNetValue[] argv)
        {
            CheckInContext();
            return NativeMethods.Invoke(_context, handle, receiverHandle, argc, argv);
        }

        public JsValue CreateObject(JsValue constructor, DotNetValue[] arguments)
        {
            CheckInContext();
            return NativeMethods.CreateObject(_context, constructor, arguments?.Length ?? 0, arguments);
        }

        public void Release(JsValue handle)
        {
            // This should be callable from any thread
            NativeMethods.Release(handle);
        }

        public string StringFromNativeUtf8(IntPtr nativeUtf8)
        {
            int len = 0;
            while (Marshal.ReadByte(nativeUtf8, len) != 0) ++len;
            byte[] buffer = new byte[len];
            Marshal.Copy(nativeUtf8, buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer);
        }

        [SuppressUnmanagedCodeSecurity]
        private static class NativeMethods
        {
            // TODO: LPStr => LPUTF8Str

            [DllImport("coreclr-hosting.node", CallingConvention = CallingConvention.Cdecl)]
            public extern static IntPtr GetContext();

            [DllImport("coreclr-hosting.node", CallingConvention = CallingConvention.Cdecl)]
            public extern static void ReleaseContext(IntPtr context);

            [DllImport("coreclr-hosting.node", CallingConvention = CallingConvention.Cdecl)]
            public extern static int PostCallback(IntPtr context, NodeCallback callback, IntPtr data);

            // Get a handle, ownerHandler == Zero => Global
            [DllImport("coreclr-hosting.node", CallingConvention = CallingConvention.Cdecl)]
            public extern static JsValue GetMember(IntPtr context, JsValue ownerHandle, [MarshalAs(UnmanagedType.LPStr)] string name); // A zero handle uses the global object.

            // Convert handles to primitives can be done in managed code based on JsType
            // ATTENTION: 32bit node exists :(

            // Set a member
            [DllImport("coreclr-hosting.node", CallingConvention = CallingConvention.Cdecl)]
            public extern static JsValue SetMember(IntPtr context, JsValue ownerHandle, [MarshalAs(UnmanagedType.LPStr)] string name, DotNetValue value);

            // Invoke handles that represent functions
            [DllImport("coreclr-hosting.node", CallingConvention = CallingConvention.Cdecl)]
            public extern static JsValue Invoke(IntPtr context, JsValue handle, JsValue receiverHandle, int argc, DotNetValue[] argv);
            /*
                    // Create a JSON object
                    [DllImport("coreclr-hosting.node", CallingConvention = CallingConvention.Cdecl)]
                    public extern static JsHandle CreateJsonObject(int argc, [MarshalAs(UnmanagedType.LPStr)] string[] argn, DotNetValue[] argv);
                    
                    [DllImport("coreclr-hosting.node", CallingConvention = CallingConvention.Cdecl)]
                    public extern static JsHandle CreateJsonObject(string json);
                     */

            [DllImport("coreclr-hosting.node", CallingConvention = CallingConvention.Cdecl)]
            public extern static JsValue CreateObject(IntPtr context, JsValue constructor, int argc, DotNetValue[] argv); // We use SetMember to define members

            // Release a handle
            [DllImport("coreclr-hosting.node", CallingConvention = CallingConvention.Cdecl)]
            public extern static void Release(JsValue handle);

        }

    }
}