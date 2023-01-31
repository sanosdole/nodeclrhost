namespace ElectronHostedBlazor.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;
    using NodeHostEnvironment;

    internal sealed class ElectronConsoleWriter
    {
        private readonly object _syncObject = new object();
        private readonly IBridgeToNode _node;
        private readonly dynamic _console;
        private List<Tuple<LogLevel, string>>? _queuedEntries;

        public ElectronConsoleWriter(IBridgeToNode node)
        {
            _node = node;
            _console = node.Global.console;
        }

        public void Write(LogLevel logLevel, string finalMessage)
        {
            if (_node.CheckAccess())
            {
                WriteIntern(logLevel, finalMessage);
                return;
            }

            var entry = Tuple.Create(logLevel, finalMessage);
            lock (_syncObject)
            {
                if (_queuedEntries != null)
                {
                    _queuedEntries.Add(entry);
                    return;
                }

                _queuedEntries = new List<Tuple<LogLevel, string>> { entry };
            }

            _node.Run(WriteQueuedEntries);
        }

        private void WriteIntern(LogLevel logLevel, string finalMessage)
        {
            switch (logLevel)
            {
                case LogLevel.Error:
                case LogLevel.Critical:
                    _console.error(finalMessage);
                    break;
                case LogLevel.Warning:
                    _console.warn(finalMessage);
                    break;
                case LogLevel.Information:
                    _console.info(finalMessage);
                    break;
                case LogLevel.Debug:
                    _console.debug(finalMessage);
                    break;
                case LogLevel.Trace:
                case LogLevel.None:
                default:
                    throw new ArgumentException("Invalid loglevel", nameof(logLevel));
            }
        }

        private void WriteQueuedEntries()
        {
            List<Tuple<LogLevel, string>> entries;
            lock (_syncObject)
            {
                entries = _queuedEntries;
                _queuedEntries = null;
            }

            Debug.Assert(entries != null, "Lost queued log entries");

            foreach (var entry in entries)
                WriteIntern(entry.Item1, entry.Item2);
        }
    }
}
