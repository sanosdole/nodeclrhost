// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Hosting
{
    using System;
    using Builder;
    using Microsoft.Extensions.DependencyInjection;

    internal interface IBlazorStartup
    {
        void ConfigureServices(IServiceCollection services);

        void Configure(IComponentsApplicationBuilder app, IServiceProvider services);
    }
}
