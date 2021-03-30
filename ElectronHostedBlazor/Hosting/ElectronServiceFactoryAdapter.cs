// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Hosting
{
    using System;
    using Microsoft.Extensions.DependencyInjection;

    // Equivalent to https://github.com/aspnet/Extensions/blob/master/src/Hosting/Hosting/src/Internal/ServiceFactoryAdapter.cs

    internal class ElectronServiceFactoryAdapter<TContainerBuilder> : IElectronServiceFactoryAdapter
    {
        private IServiceProviderFactory<TContainerBuilder> _serviceProviderFactory;
        private readonly Func<ElectronHostBuilderContext> _contextResolver;
        private Func<ElectronHostBuilderContext, IServiceProviderFactory<TContainerBuilder>> _factoryResolver;

        public ElectronServiceFactoryAdapter(IServiceProviderFactory<TContainerBuilder> serviceProviderFactory)
        {
            _serviceProviderFactory = serviceProviderFactory ?? throw new ArgumentNullException(nameof(serviceProviderFactory));
        }

        public ElectronServiceFactoryAdapter(Func<ElectronHostBuilderContext> contextResolver, Func<ElectronHostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factoryResolver)
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
