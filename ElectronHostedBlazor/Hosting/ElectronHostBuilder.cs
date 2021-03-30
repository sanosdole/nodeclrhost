// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Hosting
{
    using System;
    using System.Collections.Generic;
    using Logging;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.JSInterop;
    using NodeHostEnvironment;
    using Services;

    internal sealed class ElectronHostBuilder : IElectronHostBuilder
    {
        private readonly List<Action<ElectronHostBuilderContext, IServiceCollection>> _configureServicesActions = new List<Action<ElectronHostBuilderContext, IServiceCollection>>();
        private readonly List<Action<ElectronHostBuilderContext, ILoggingBuilder>> _configureLoggingActions = new List<Action<ElectronHostBuilderContext, ILoggingBuilder>>();
        private bool _hostBuilt;
        private ElectronHostBuilderContext _BrowserHostBuilderContext;
        private IElectronServiceFactoryAdapter _serviceProviderFactory = new ElectronServiceFactoryAdapter<IServiceCollection>(new DefaultServiceProviderFactory());
        private IServiceProvider _appServices;

        /// <summary>
        /// A central location for sharing state between components during the host building process.
        /// </summary>
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();

        /// <summary>
        /// Adds services to the container. This can be called multiple times and the results will be additive.
        /// </summary>
        /// <param name="configureDelegate"></param>
        /// <returns>The same instance of the <see cref="IElectronHostBuilder"/> for chaining.</returns>
        public IElectronHostBuilder ConfigureServices(Action<ElectronHostBuilderContext, IServiceCollection> configureDelegate)
        {
            _configureServicesActions.Add(configureDelegate ??
                                          throw new ArgumentNullException(nameof(configureDelegate)));
            return this;
        }

        public IElectronHostBuilder ConfigureLogging(Action<ElectronHostBuilderContext, ILoggingBuilder> configureDelegate)
        {
            _configureLoggingActions.Add(configureDelegate ??
                                         throw new ArgumentNullException(nameof(configureDelegate)));
            return this;
        }

        /// <summary>
        /// Overrides the factory used to create the service provider.
        /// </summary>
        /// <returns>The same instance of the <see cref="IElectronHostBuilder"/> for chaining.</returns>
        public IElectronHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
        {
            _serviceProviderFactory = new ElectronServiceFactoryAdapter<TContainerBuilder>(factory ??
                                                                                           throw new ArgumentNullException(nameof(factory)));
            return this;
        }

        /// <summary>
        /// Overrides the factory used to create the service provider.
        /// </summary>
        /// <returns>The same instance of the <see cref="IElectronHostBuilder"/> for chaining.</returns>
        public IElectronHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<ElectronHostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
        {
            _serviceProviderFactory = new ElectronServiceFactoryAdapter<TContainerBuilder>(() => _BrowserHostBuilderContext,
                                                                                           factory ??
                                                                                           throw new ArgumentNullException(nameof(factory)));
            return this;
        }

        /// <summary>
        /// Run the given actions to initialize the host. This can only be called once.
        /// </summary>
        /// <returns>An initialized <see cref="IElectronHost"/></returns>
        public IElectronHost Build()
        {
            if (_hostBuilt)
            {
                throw new InvalidOperationException("Build can only be called once.");
            }

            _hostBuilt = true;

            CreateBrowserHostBuilderContext();
            CreateServiceProvider();

            return _appServices.GetRequiredService<IElectronHost>();
        }

        private void CreateBrowserHostBuilderContext()
        {
            _BrowserHostBuilderContext = new ElectronHostBuilderContext(Properties);
        }

        private void CreateServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddSingleton(_BrowserHostBuilderContext);

            // Could use `Properties` to configure path
            var node = NodeHost.Instance;
            services.AddSingleton(node);

            var jsRuntime = new ElectronJSRuntime(node);
            services.AddSingleton<IJSRuntime>(jsRuntime);
            services.AddSingleton<IJSInProcessRuntime>(jsRuntime);

            services.AddSingleton<IElectronHost, ElectronHost>();

            services.AddSingleton<NavigationManager, ElectronNavigationManager>();
            services.AddSingleton<INavigationInterception, ElectronNavigationInterception>();

            // DM 19.08.2019: We do not need an HttpClient like WebAssembly does as we have the full framework
            /*services.AddSingleton<HttpClient>(s =>
            {
                // Creating the URI helper needs to wait until the JS Runtime is initialized, so defer it.
                var navigationManager = s.GetRequiredService<NavigationManager>();
                return new HttpClient
                {
                    BaseAddress = new Uri(navigationManager.BaseUri)
                };
            });*/

            // Needed for authorization
            // However, since authorization isn't on by default, we could consider removing these and
            // having a separate services.AddBlazorAuthorization() call that brings in the required services.
            services.AddOptions();

            // Apply user customization
            foreach (var configureServicesAction in _configureServicesActions)
                configureServicesAction(_BrowserHostBuilderContext, services);

            // Add Logging and apply user customizations to it
            services.AddLogging(loggingBuilder =>
                                {
                                    loggingBuilder.AddElectronConsole();
                                    foreach (var configureLoggingAction in _configureLoggingActions)
                                        configureLoggingAction(_BrowserHostBuilderContext, loggingBuilder);
                                });

            var builder = _serviceProviderFactory.CreateBuilder(services);
            _appServices = _serviceProviderFactory.CreateServiceProvider(builder);
        }
    }
}
