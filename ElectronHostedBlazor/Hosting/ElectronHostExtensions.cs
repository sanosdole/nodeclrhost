// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Hosting
{
    using System.Threading.Tasks;
    using System;

    /// <summary>
    /// Extension methods for <see cref="IElectronHost"/>.
    /// </summary>
    public static class ElectronHostExtensions
    {
        /// <summary>
        /// Runs the application.
        /// </summary>
        /// <param name="host">The <see cref="IElectronHost"/> to run.</param>
        /// <remarks>
        /// Currently, Blazor applications running in the browser don't have a lifecycle - the application does not
        /// get a chance to gracefully shut down. For now, <see cref="Run(IElectronHost)"/> simply starts the host
        /// and allows execution to continue.
        /// </remarks>
        public static async Task<int> Run(this IElectronHost host)
        {
            using(host)
            {
                await host.RunAsync();
                return 0;
            }
        }

        /// <summary>
        /// Adds an handler for rendering exceptions.
        /// </summary>
        public static IElectronHost UseExceptionHandler(this IElectronHost host, UnhandledExceptionEventHandler handler)
        {
            host.UnhandledException += handler;
            return host;
        }
    }
}