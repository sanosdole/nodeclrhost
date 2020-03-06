// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Services
{
    using System;
    using Microsoft.Extensions.Logging;
    using NodeHostEnvironment.BridgeApi;

    internal class NodeConsoleLogger<T> : ILogger<T>, ILogger
    {
        private readonly dynamic _console;

        public NodeConsoleLogger(IBridgeToNode node)
        {
            _console = node.Global.console;
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return NoOpDisposable.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= LogLevel.Debug;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var formattedMessage = formatter(state, exception);
            switch (logLevel)
            {
                case LogLevel.Error:
                case LogLevel.Critical:
                    _console.error(formattedMessage);
                    break;
                case LogLevel.Debug:
                    _console.debug(formattedMessage);
                    break;
                case LogLevel.Trace:
                    _console.trace(formattedMessage);
                    break;
                case LogLevel.Warning:
                    _console.warn(formattedMessage);
                    break;
                case LogLevel.Information:
                case LogLevel.None:
                    _console.trace(formattedMessage);
                    break;
                default:
                    throw new ArgumentException("Invalid loglevel", nameof(logLevel));
            }
        }

        private class NoOpDisposable : IDisposable
        {
            public static NoOpDisposable Instance = new NoOpDisposable();

            public void Dispose() { }
        }
    }
}