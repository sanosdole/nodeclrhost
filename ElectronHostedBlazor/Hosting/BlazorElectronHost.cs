// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Hosting
{
    public static class BlazorElectronHost
    {
        /// <summary>
        /// Creates an instance of <see cref="IElectronHostBuilder"/>.
        /// </summary>
        /// <returns>The <see cref="IElectronHostBuilder"/>.</returns>
        public static IElectronHostBuilder CreateDefaultBuilder()
        {
            return new ElectronHostBuilder();
        }
    }
}
