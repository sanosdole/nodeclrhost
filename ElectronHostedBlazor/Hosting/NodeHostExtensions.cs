// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Hosting
{
    using System;

    /// <summary>
    /// Extension methods for <see cref="INodeHost"/>.
    /// </summary>
    public static class NodeHostExtensions
    {
        /// <summary>
        /// Runs the application.
        /// </summary>
        /// <param name="host">The <see cref="INodeHost"/> to run.</param>
        /// <remarks>
        /// Currently, Blazor applications running in the browser don't have a lifecycle - the application does not
        /// get a chance to gracefully shut down. For now, <see cref="Run(INodeHost)"/> simply starts the host
        /// and allows execution to continue.
        /// </remarks>
        public static void Run(this INodeHost host)
        {
            // Behave like async void, because we don't yet support async-main properly on Node.
            // However, don't actualy make this method async, because we rely on startup being synchronous
            // for things like attaching navigation event handlers.
            host.StartAsync().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    Console.WriteLine(task.Exception);
                }
                //host.StopAsync();
                //host.Dispose();
            });
        }
    }
}