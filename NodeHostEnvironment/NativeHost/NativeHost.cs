using System;
using NodeHostEnvironment.InProcess;

namespace NodeHostEnvironment.NativeHost
{
    public static class NativeHost
    {
        /// <summary>
        /// Creates a native in process host.
        /// This will only work when it is called synchronously from the main entry point
        /// invoked by the `coreclr-hosting` node module.
        /// </summary>
        /// <returns></returns>
        public static IBridgeToNode Initialize(string pathToCoreClrHostingModule = null)
        {
            pathToCoreClrHostingModule = pathToCoreClrHostingModule ??
                Environment.GetEnvironmentVariable("CORECLR_HOSTING_MODULE_PATH") ??
                "./node_modules/coreclr-hosting/build/Release/coreclr-hosting.node";
            var nativeMethods = DynamicLibraryLoader.LoadApi<DelegateBasedNativeApi>(pathToCoreClrHostingModule);
            return new NodeBridge(new NativeNodeHost(nativeMethods));
        }
    }
}