namespace NodeHostEnvironment.InProcess
{
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using System;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DotNetCallback(int argc, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 0)] JsValue[] argv, out DotNetValue result);

    /// <summary>
    /// Interface for enabling unit testing without node environment
    /// </summary>
    internal interface IHostInProcess
    {
        bool CheckAccess();
        TaskFactory Factory { get; }

        // Get a handle, ownerHandler == Zero + Object => Global
        JsValue GetMember(JsValue ownerHandle, string name);
        JsValue GetMemberByIndex(JsValue ownerHandle, int index);

        // Convert handles to primitives can be done in managed code based on JsType
        // ATTENTION: 32bit node exists :(

        // Set a member
        void SetMember(JsValue ownerHandle, string name, DotNetValue value);

        // Invoke handles that represent functions
        JsValue Invoke(JsValue handle, JsValue receiverHandle, int argc, DotNetValue[] argv);

        JsValue InvokeByName(string name, JsValue receiverHandle, int argc, DotNetValue[] argv);

        JsValue CreateObject(JsValue constructor, DotNetValue[] arguments); // We use SetMember to define members

        void Release(JsValue handle);

        string StringFromNativeUtf8(IntPtr nativeUtf8);

        IntPtr MarshallCallback(DotNetCallback callback, out ReleaseDotNetValue releaseCallback);

        IntPtr MarshallTask(Task task, out ReleaseDotNetValue releaseCallback);
        JsValue[] GetArrayValues(JsValue handle);
    }
}