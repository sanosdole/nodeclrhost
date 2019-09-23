namespace NodeHostEnvironment.InProcess
{
    using System.Dynamic;
    using System.Linq;
    using System;

    // Must be public for CreateNewInstance to work!
    public sealed class JsDynamicObject : DynamicObject, IDisposable
    {
        private readonly IHostInProcess _host;

        internal JsValue Handle { get; }

        internal JsDynamicObject(JsValue handle, IHostInProcess host)
        {
            Handle = handle;
            _host = host;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _host.Release(Handle);
        }

        ~JsDynamicObject()
        {
            _host.Release(Handle);
        }

        public dynamic CreateNewInstance(params object[] arguments)
        {
            var result = _host.CreateObject(Handle, arguments.Select(a => DotNetValue.FromObject(a, _host)).ToArray());
            if (!result.TryGetObject(_host, out object newInstance))
                throw new InvalidOperationException("Could not create new instance");
            return newInstance;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var jsHandle = _host.GetMember(Handle, binder.Name);
            //Console.WriteLine($"{binder.Name} [{binder.ReturnType.Name}] is a {jsHandle.Type} with value {jsHandle.Value}");
            return jsHandle.TryGetObject(_host, out result);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder,
            object[] args,
            out object result)
        {
            var member = _host.GetMember(Handle, binder.Name);
            try
            {
                if (member.Type != JsType.Function)
                {
                    result = null;
                    return false;
                }

                var resultHandle = _host.Invoke(member, Handle, args.Length, args.Select(a => DotNetValue.FromObject(a, _host)).ToArray());
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
            _host.SetMember(Handle, binder.Name, DotNetValue.FromObject(value, _host));
            return true;
        }
    }
}