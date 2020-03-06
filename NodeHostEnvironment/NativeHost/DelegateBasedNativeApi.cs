using System;
using System.Runtime.InteropServices;
using NodeHostEnvironment.InProcess;

namespace NodeHostEnvironment.NativeHost
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate DotNetValue SetupDeferred(IntPtr deferred);

    internal sealed class DelegateBasedNativeApi
    {
        public DelegateBasedNativeApi(GetContext getContext,
            ReleaseContext releaseContext,
            PostCallback postCallback,
            GetMember getMember,
            GetMemberByIndex getMemberByIndex,
            SetMember setMember,
            Invoke invoke,
            InvokeByName invokeByName,
            CreateObject createObject,
            CompletePromise completePromise,
            Release release)
        {
            GetContext = getContext;
            ReleaseContext = releaseContext;
            PostCallback = postCallback;
            GetMember = getMember;
            GetMemberByIndex = getMemberByIndex;
            SetMember = setMember;
            Invoke = invoke;
            InvokeByName = invokeByName;
            CreateObject = createObject;
            CompletePromise = completePromise;
            Release = release;
        }

        public GetContext GetContext { get; }
        public ReleaseContext ReleaseContext { get; }
        public PostCallback PostCallback { get; }
        public GetMember GetMember { get; }
        public GetMemberByIndex GetMemberByIndex { get; }
        public SetMember SetMember { get; }
        public Invoke Invoke { get; }
        public InvokeByName InvokeByName { get; }
        public CreateObject CreateObject { get; }
        public CompletePromise CompletePromise { get; }
        public Release Release { get; }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr GetContext();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ReleaseContext(IntPtr context);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int PostCallback(IntPtr context, NodeCallback callback, IntPtr data);

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
}