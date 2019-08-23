namespace BlazorApp.Hosting
{
    using System;
    using BlazorApp.Builder;
    using Microsoft.Extensions.DependencyInjection;

    internal interface IBlazorStartup
    {
        void ConfigureServices(IServiceCollection services);

        void Configure(IComponentsApplicationBuilder app, IServiceProvider services);
    }
}