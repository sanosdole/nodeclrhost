using System;
using NodeHostEnvironment.InProcess;

namespace NodeHostEnvironment.NativeHost
{
    public static class NativeHost
    {
        internal static NativeNodeHost Host { private get; set; }
        
        /// <summary>
        /// Creates a native in process host.
        /// This will only work when it is called synchronously from the main entry point
        /// invoked by the `coreclr-hosting` node module.
        /// </summary>
        /// <returns></returns>
        public static IBridgeToNode Initialize()
        {
            return new NodeBridge(Host);
        }
    }
}