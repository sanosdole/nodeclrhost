namespace BlazorApp.Hosting
{
    using System;
    using Microsoft.Extensions.DependencyInjection;

    // Equivalent to https://github.com/aspnet/Extensions/blob/master/src/Hosting/Hosting/src/Internal/ServiceFactoryAdapter.cs

    internal class NodeServiceFactoryAdapter<TContainerBuilder> : INodeServiceFactoryAdapter
    {
        private IServiceProviderFactory<TContainerBuilder> _serviceProviderFactory;
        private readonly Func<NodeHostBuilderContext> _contextResolver;
        private Func<NodeHostBuilderContext, IServiceProviderFactory<TContainerBuilder>> _factoryResolver;

        public NodeServiceFactoryAdapter(IServiceProviderFactory<TContainerBuilder> serviceProviderFactory)
        {
            _serviceProviderFactory = serviceProviderFactory ?? throw new ArgumentNullException(nameof(serviceProviderFactory));
        }

        public NodeServiceFactoryAdapter(Func<NodeHostBuilderContext> contextResolver, Func<NodeHostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factoryResolver)
        {
            _contextResolver = contextResolver ?? throw new ArgumentNullException(nameof(contextResolver));
            _factoryResolver = factoryResolver ?? throw new ArgumentNullException(nameof(factoryResolver));
        }

        public object CreateBuilder(IServiceCollection services)
        {
            if (_serviceProviderFactory == null)
            {
                _serviceProviderFactory = _factoryResolver(_contextResolver());

                if (_serviceProviderFactory == null)
                {
                    throw new InvalidOperationException("The resolver returned a null IServiceProviderFactory");
                }
            }
            return _serviceProviderFactory.CreateBuilder(services);
        }

        public IServiceProvider CreateServiceProvider(object containerBuilder)
        {
            if (_serviceProviderFactory == null)
            {
                throw new InvalidOperationException("CreateBuilder must be called before CreateServiceProvider");
            }

            return _serviceProviderFactory.CreateServiceProvider((TContainerBuilder)containerBuilder);
        }
    }
}