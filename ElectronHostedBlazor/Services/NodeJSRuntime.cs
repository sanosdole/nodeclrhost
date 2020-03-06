// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ElectronHostedBlazor.Rendering;
using Microsoft.JSInterop;
using NodeHostEnvironment.BridgeApi;

namespace ElectronHostedBlazor.Services
{
    internal sealed class NodeJSRuntime : IJSRuntime, IJSInProcessRuntime
    {
        private readonly dynamic _window;

        public NodeJSRuntime(IBridgeToNode node)
        {
            _window = node.Global.window;
        }

        public T Invoke<T>(string identifier, params object[] args)
        {
            if (!_window.TryInvokeMember(identifier, args, out object result))
                throw new InvalidOperationException($"Could not invoke {identifier} from global!");
            return (T)result;
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, params object[] args)
        {
            //_logger.LogDebug("Invoking '{0}'", identifier);
            if (!_window.TryInvokeMember(identifier, args, out object result))
                throw new InvalidOperationException($"Could not invoke {identifier} from global!");
            return new ValueTask<TValue>((TValue)result);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken = default, params object[] args)
        {
            //_logger.LogDebug("Invoking '{0}'", identifier);            
            if (!_window.TryInvokeMember(identifier, args.ToArray(), out object result))
                throw new InvalidOperationException($"Could not invoke {identifier} from global!");
            return new ValueTask<TValue>((TValue)result);
        }
    }
}