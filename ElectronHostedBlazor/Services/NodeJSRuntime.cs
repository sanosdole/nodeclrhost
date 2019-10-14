// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlazorApp.Rendering;
using Microsoft.JSInterop;
using NodeHostEnvironment;

namespace BlazorApp.Services
{
    internal sealed class NodeJSRuntime : IJSRuntime
    {
        public NodeHost Host { get; }
        public static readonly NodeJSRuntime Instance = new NodeJSRuntime();

        public NodeJSRuntime()
        {
            Host = NodeHost.InProcess();
            Host.Global.window.addEventListener("unload", new Action<dynamic>(e =>
                {
                    Host.Dispose();
                }));
            RendererRegistryEventDispatcher.Register(this);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, params object[] args)
        {
            Host.Global.console.info($"Invoking {identifier}");
            if (!Host.Global.window.TryInvokeMember(identifier, args, out object result))
                throw new InvalidOperationException($"Could not invoke {identifier} from global!");
            return new ValueTask<TValue>((TValue)result);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken = default, params object[] args)
        {
            Host.Global.console.info($"Invoking {identifier}");
            if (!Host.Global.window.TryInvokeMember(identifier, args.ToArray(), out object result))
                throw new InvalidOperationException($"Could not invoke {identifier} from global!");
            return new ValueTask<TValue>((TValue)result);
        }
    }
}