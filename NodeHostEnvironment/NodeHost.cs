namespace NodeHostEnvironment
{
    using System.Threading.Tasks;
    using System;
    using BridgeApi;
    using NodeHostEnvironment.InProcess;
    using NodeHostEnvironment.NativeHost;

    /// <summary>
    /// Proxy for <see cref="IBridgeToNode"/> implementations.
    /// Only native in-process hosts using <see cref="InProcess"/> are supported a.t.m.
    /// </summary>
    public sealed class NodeHost : IBridgeToNode
    {
        private readonly IBridgeToNode _bridge;

        private NodeHost(IBridgeToNode bridge)
        {
            _bridge = bridge;
        }

        /// <summary>
        /// Creates a native in process host.
        /// This will only work when it is called synchronously from the main entry point
        /// invoked by the `coreclr-hosting` node module.
        /// </summary>
        /// <returns></returns>
        public static NodeHost InProcess(string pathToCoreClrHostingModule = null)
        {
            pathToCoreClrHostingModule = pathToCoreClrHostingModule ??
                Environment.GetEnvironmentVariable("CORECLR_HOSTING_MODULE_PATH") ??
                "./node_modules/coreclr-hosting/build/Release/coreclr-hosting.node";
            var nativeMethods = DynamicLibraryLoader.LoadApi<DelegateBasedNativeApi>(pathToCoreClrHostingModule);
            return new NodeHost(new NodeBridge(new NativeNodeHost(nativeMethods)));
        }

        /// <inheritdoc/>
        public dynamic Global => _bridge.Global;

        /// <inheritdoc/>
        public dynamic New() => _bridge.New();

        /// <inheritdoc/>
        public Task<T> Run<T>(Func<T> func) => _bridge.Run(func);

        /// <inheritdoc/>
        public void Dispose() => _bridge.Dispose();
    }
}