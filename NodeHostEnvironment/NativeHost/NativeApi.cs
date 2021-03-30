namespace NodeHostEnvironment.NativeHost
{
    using System;
    using System.Runtime.InteropServices;
    using InProcess;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ProcessJsEventLoopEntry(IntPtr data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ProcessMicroTask(IntPtr data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate DotNetValue SetupDeferred(IntPtr deferred);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ClosingRuntime();

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeApi
    {
        public RegisterSchedulerCallbacks RegisterSchedulerCallbacks;
        public SignalEventLoopEntry SignalEventLoopEntry;
        public SignalMicroTask SignalMicroTask;
        public GetMember GetMember;
        public GetMemberByIndex GetMemberByIndex;
        public SetMember SetMember;
        public Invoke Invoke;
        public InvokeByName InvokeByName;
        public CreateObject CreateObject;
        public CompletePromise CompletePromise;
        public TryAccessArrayBuffer TryAccessArrayBuffer;
        public Release Release;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int RegisterSchedulerCallbacks(IntPtr context,
                                                     ProcessJsEventLoopEntry processJsEventLoopEntry,
                                                     ProcessMicroTask processMicroTask,
                                                     ClosingRuntime closingRuntime);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void SignalEventLoopEntry(IntPtr context, IntPtr data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void SignalMicroTask(IntPtr context, IntPtr data);

    // Get a handle, ownerHandler == Zero => Global
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate JsValue GetMember(IntPtr context, JsValue ownerHandle, [MarshalAs(UnmanagedType.LPStr)] string name); // A zero handle uses the global object.

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate JsValue GetMemberByIndex(IntPtr context, JsValue ownerHandle, int index); // A zero handle uses the global object.
    // Convert handles to primitives can be done in managed code based on JsType
    // ATTENTION: 32bit node exists :(

    // Set a member
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate JsValue SetMember(IntPtr context, JsValue ownerHandle, [MarshalAs(UnmanagedType.LPStr)] string name, DotNetValue value);

    // Invoke handles that represent functions
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate JsValue Invoke(IntPtr context, JsValue handle, JsValue receiverHandle, int argc, DotNetValue[] argv);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate JsValue InvokeByName(IntPtr context, [MarshalAs(UnmanagedType.LPStr)] string name, JsValue receiverHandle, int argc, DotNetValue[] argv);
    /*
            // Create a JSON object
            [DllImport("coreclr-hosting.node", CallingConvention = CallingConvention.Cdecl)]
            public extern static JsHandle CreateJsonObject(int argc, [MarshalAs(UnmanagedType.LPStr)] string[] argn, DotNetValue[] argv);
            
            [DllImport("coreclr-hosting.node", CallingConvention = CallingConvention.Cdecl)]
            public extern static JsHandle CreateJsonObject(string json);
             */

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate JsValue CreateObject(IntPtr context, JsValue constructor, int argc, DotNetValue[] argv); // We use SetMember to define members

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void CompletePromise(IntPtr context, IntPtr deferred, DotNetValue result);

    // Release a handle
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void Release(JsValue handle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate bool TryAccessArrayBuffer(IntPtr context, JsValue handle, out IntPtr address, out int byteLength);
}
