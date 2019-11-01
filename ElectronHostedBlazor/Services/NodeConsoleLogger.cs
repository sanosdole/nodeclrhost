// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

using System;
using Microsoft.Extensions.Logging;

namespace BlazorApp.Services
{
    internal class NodeConsoleLogger<T> : ILogger<T>, ILogger
    {
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
            var console = NodeJSRuntime.Instance.Host.Global.console;
            switch (logLevel)
            {
                case LogLevel.Error:
                case LogLevel.Critical:
                    console.error(formattedMessage);
                    break;
                case LogLevel.Debug:
                    console.debug(formattedMessage);
                    break;
                case LogLevel.Trace:
                    console.trace(formattedMessage);
                    break;
                case LogLevel.Warning:
                    console.warn(formattedMessage);
                    break;
                case LogLevel.Information:
                case LogLevel.None:
                    console.trace(formattedMessage);
                    break;
                default:
                    throw new ArgumentException("Invalid loglevel", nameof(logLevel));
            }
            
            Console.WriteLine($"[{logLevel}] {formattedMessage}");
        }

        private class NoOpDisposable : IDisposable
        {
            public static NoOpDisposable Instance = new NoOpDisposable();

            public void Dispose() { }
        }
    }
}