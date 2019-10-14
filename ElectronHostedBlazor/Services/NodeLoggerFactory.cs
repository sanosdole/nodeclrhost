// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

using Microsoft.Extensions.Logging;

namespace BlazorApp.Services
{
    internal class NodeLoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider)
        {
            // No-op
        }

        public ILogger CreateLogger(string categoryName)
            => new NodeConsoleLogger<object>();

        public void Dispose()
        {
            // No-op
        }
    }
}