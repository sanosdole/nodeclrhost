// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Logging
{
    using System;
    using Microsoft.Extensions.Logging;

    internal sealed class ElectronConsoleLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly ElectronConsoleWriter _writer;

        public ElectronConsoleLogger(ElectronConsoleWriter writer, string categoryName)
        {
            _writer = writer;
            _categoryName = categoryName;
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return NoOpDisposable.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel > LogLevel.Trace && logLevel < LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var formattedMessage = formatter(state, exception);
            var exceptionString = null != exception ?
                $"\n{exception}" :
                string.Empty;
            var finalMessage = $"{_categoryName}[{eventId.Id}]:\n{formattedMessage}{exceptionString}";
            _writer.Write(logLevel, finalMessage);
        }

        private class NoOpDisposable : IDisposable
        {
            public static NoOpDisposable Instance = new NoOpDisposable();

            public void Dispose() { }
        }
    }
}