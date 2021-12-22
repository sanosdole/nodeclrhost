// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Services
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.Extensions.Logging;

    internal class ElectronErrorBoundaryLogger : IErrorBoundaryLogger
    {
        private readonly ILogger<ErrorBoundary> _errorBoundaryLogger;

        public ElectronErrorBoundaryLogger(ILogger<ErrorBoundary> errorBoundaryLogger)
        {
            _errorBoundaryLogger = errorBoundaryLogger ?? throw new ArgumentNullException(nameof(errorBoundaryLogger)); ;
        }

        public ValueTask LogErrorAsync(Exception exception)
        {
            // For, client-side code, all internal state is visible to the end user. We can just
            // log directly to the console.
            _errorBoundaryLogger.LogError(exception.ToString());
            return ValueTask.CompletedTask;
        }
    }
}
