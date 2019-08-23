using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using NodeHostEnvironment;

namespace BlazorApp.Services
{
    internal sealed class NodeJSRuntime : IJSRuntime
    {
        public NodeHost Host {get;}
        public static readonly NodeJSRuntime Instance = new NodeJSRuntime();

        public NodeJSRuntime()
        {
            Host = NodeHost.InProcess();
            Host.Global.window.addEventListener("unload", new Action<dynamic>(e =>
                {
                    Host.Dispose();
                }));
        }

        public Task<TValue> InvokeAsync<TValue>(string identifier, params object[] args)
        {
            Host.Global.console.info($"Invoking {identifier}");
            if (!Host.Global.window.TryInvokeMember(identifier, args, out object result))
                throw new InvalidOperationException($"Could not invoke {identifier} from global!");
            return Task.FromResult((TValue)result);
        }

        public Task<TValue> InvokeAsync<TValue>(string identifier, IEnumerable<object> args, CancellationToken cancellationToken = default)
        {
            Host.Global.console.info($"Invoking {identifier}");
            if (!Host.Global.window.TryInvokeMember(identifier, args.ToArray(), out object result))
                throw new InvalidOperationException($"Could not invoke {identifier} from global!");
            return Task.FromResult((TValue)result);
        }
    }
}