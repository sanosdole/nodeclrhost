namespace NodeHostEnvironment.InProcess
{
    using System.Threading.Tasks;
    using System;

    internal sealed class NodeBridge : IBridgeToNode
    {
        private readonly IHostInProcess _host;

        public NodeBridge(IHostInProcess host)
        {
            _host = host;
            Global = new JsDynamicObject(JsValue.Global, _host);
        }

        public dynamic Global { get; }
        public dynamic New()
        {
            var handle = _host.CreateObject(JsValue.Global, null);
            handle.ThrowError(_host);
            if (handle.Value == IntPtr.Zero)
                throw new InvalidOperationException("Could not create new JS object");
            return new JsDynamicObject(handle, _host);
        }

        public Task<T> Run<T>(Func<T> func)
        {
            return _host.Factory.StartNew(func);
        }

        public bool CheckAccess()
        {
            return _host.CheckAccess();
        }
    }
}