namespace NodeHostEnvironment
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BridgeApi;

    public sealed class InProcessNodeBridge : IBridgeToNode
    {
        private readonly INativeHost _host;        
                
        public InProcessNodeBridge(INativeHost host)
        {
            
            _host = host;

            Global = new JsDynamicObject(JsValue.Global, _host);
        }
        public dynamic Global {get;}
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
        public void Dispose()
        {
            _host.ReleaseHost();            
        }


    }
}