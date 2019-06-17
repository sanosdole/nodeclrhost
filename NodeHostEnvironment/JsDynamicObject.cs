namespace NodeHostEnvironment
{
    using System;
    using System.Dynamic;
    using System.Linq;

    public sealed class JsDynamicObject : DynamicObject, IDisposable
    {
        private readonly JsValue _handle;
        private readonly INativeHost _host;

        public JsValue Handle => _handle;

        internal JsDynamicObject(JsValue handle, INativeHost host)
        {
            _handle = handle;
            _host = host;            
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _host.Release(_handle);
        }

        ~JsDynamicObject()
        {
            _host.Release(_handle);
        }

        public dynamic CreateNewInstance(params object[] arguments)
        {
            var result = _host.CreateObject(_handle, arguments.Select(a => DotNetValue.FromObject(a, _host)).ToArray());
            object newInstance;
            if (!result.TryGetObject(_host, out newInstance))
                throw new InvalidOperationException("Could not create new instance");
            return newInstance;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) 
        {
            var jsHandle = _host.GetMember(_handle, binder.Name);
            //binder.GetType
            //Console.WriteLine($"{binder.Name} [{binder.ReturnType.Name}] is a {jsHandle.Type} with value {jsHandle.Value}");
            return jsHandle.TryGetObject(_host, out result);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder,
                                             object[] args,
                                             out object result)
        {
            var member = _host.GetMember(_handle, binder.Name);
            try
            {
                if (member.Type != JsType.Function)
                {
                    result = null;
                    return false;
                }

                var resultHandle = _host.Invoke(member, _handle, args.Length, args.Select(a => DotNetValue.FromObject(a, _host)).ToArray());
                //Console.WriteLine($"Invoking {binder.Name} [{binder.ReturnType.Name}] gave a {resultHandle.Type} with value {resultHandle.Value}");
                resultHandle.TryGetObject(_host, out result);
                return true;
            }
            finally
            {
                _host.Release(member);
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _host.SetMember(_handle, binder.Name, DotNetValue.FromObject(value, _host));
            return true;
        }        
    }
}