// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

using Microsoft.Extensions.Logging;
using NodeHostEnvironment;

namespace ElectronHostedBlazor.Services
{
    internal class ElectronLoggerFactory : ILoggerFactory
    {
        private readonly IBridgeToNode _node;

        public ElectronLoggerFactory(IBridgeToNode node)
        {
            _node = node;
        }
        public void AddProvider(ILoggerProvider provider)
        {
            // No-op
        }

        public ILogger CreateLogger(string categoryName)
            => new ElectronConsoleLogger<object>(_node);

        public void Dispose()
        {
            // No-op
        }
    }
}