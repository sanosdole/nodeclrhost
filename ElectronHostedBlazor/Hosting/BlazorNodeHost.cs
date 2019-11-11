// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

namespace BlazorApp.Hosting
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    
    public static class BlazorNodeHost
    {
        /// <summary>
        /// Creates an instance of <see cref="IWebAssemblyHostBuilder"/>.
        /// </summary>
        /// <returns>The <see cref="IWebAssemblyHostBuilder"/>.</returns>
        public static INodeHostBuilder CreateDefaultBuilder()
        {
            return new NodeHostBuilder();
        }
    }
}