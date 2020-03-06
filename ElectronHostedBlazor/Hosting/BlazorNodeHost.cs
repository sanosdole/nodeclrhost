// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Hosting
{

    public static class BlazorNodeHost
    {
        /// <summary>
        /// Creates an instance of <see cref="INodeHostBuilder"/>.
        /// </summary>
        /// <returns>The <see cref="INodeHostBuilder"/>.</returns>
        public static INodeHostBuilder CreateDefaultBuilder()
        {
            return new NodeHostBuilder();
        }
    }
}