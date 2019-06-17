namespace NodeHostEnvironment
{
    using System;
    using System.Threading.Tasks;
    using BridgeApi;
    
    public sealed class NodeHost : IBridgeToNode
    {
        private readonly IBridgeToNode _bridge;

        private NodeHost(IBridgeToNode bridge)
        {
            _bridge = bridge;
        }

        public static NodeHost InProcess()
        {
            return new NodeHost(new InProcessNodeBridge(new InProcessNativeHost()));
        }

        public dynamic Global => _bridge.Global;
        public dynamic New() => _bridge.New();

        public Task<T> Run<T>(Func<T> action)
        {
            return _bridge.Run(action);
        }

        public async Task<T> Run<T>(Func<Task<T>> action)
        {
            return await Run(action);
        }

        public Task Run(Func<Task> action)
        {
            return Run<Task>(action).Unwrap();
        }

        public Task Run(Action action)
        {
            return Run(() => { action(); return (object)null; });
        }

        public void Dispose()
        {
            _bridge.Dispose();
        }        
    }
}