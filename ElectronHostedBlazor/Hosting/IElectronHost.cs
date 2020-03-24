// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Hosting
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IElectronHost : IDisposable
    {
        /// <summary>
        /// The programs configured services.
        /// </summary>
        IServiceProvider Services { get; }

        /// <summary>
        /// Run the program.
        /// </summary>
        /// <param name="cancellationToken">Used to abort program run.</param>
        /// <returns></returns>
        Task RunAsync(CancellationToken cancellationToken = default);
    }
}