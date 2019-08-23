using System;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorApp.Hosting
{
    internal interface INodeServiceFactoryAdapter
    {
        object CreateBuilder(IServiceCollection services);

        IServiceProvider CreateServiceProvider(object containerBuilder);
    }
}