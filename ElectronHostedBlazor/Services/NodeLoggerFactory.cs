// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

using Microsoft.Extensions.Logging;
using NodeHostEnvironment.BridgeApi;

namespace ElectronHostedBlazor.Services
{
    internal class NodeLoggerFactory : ILoggerFactory
    {
        private readonly IBridgeToNode _node;

        public NodeLoggerFactory(IBridgeToNode node)
        {
            _node = node;
        }
        public void AddProvider(ILoggerProvider provider)
        {
            // No-op
        }

        public ILogger CreateLogger(string categoryName)
            => new NodeConsoleLogger<object>(_node);

        public void Dispose()
        {
            // No-op
        }
    }
}