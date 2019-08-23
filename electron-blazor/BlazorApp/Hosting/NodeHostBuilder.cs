namespace BlazorApp.Hosting
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using BlazorApp.Services;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.JSInterop;

    internal sealed class NodeHostBuilder : INodeHostBuilder
    {
        private List<Action<NodeHostBuilderContext, IServiceCollection>> _configureServicesActions = new List<Action<NodeHostBuilderContext, IServiceCollection>>();
        private bool _hostBuilt;
        private NodeHostBuilderContext _BrowserHostBuilderContext;
        private INodeServiceFactoryAdapter _serviceProviderFactory = new NodeServiceFactoryAdapter<IServiceCollection>(new DefaultServiceProviderFactory());
        private IServiceProvider _appServices;

        /// <summary>
        /// A central location for sharing state between components during the host building process.
        /// </summary>
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();

        /// <summary>
        /// Adds services to the container. This can be called multiple times and the results will be additive.
        /// </summary>
        /// <param name="configureDelegate"></param>
        /// <returns>The same instance of the <see cref="INodeHostBuilder"/> for chaining.</returns>
        public INodeHostBuilder ConfigureServices(Action<NodeHostBuilderContext, IServiceCollection> configureDelegate)
        {
            _configureServicesActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
            return this;
        }

        /// <summary>
        /// Overrides the factory used to create the service provider.
        /// </summary>
        /// <returns>The same instance of the <see cref="INodeHostBuilder"/> for chaining.</returns>
        public INodeHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
        {
            _serviceProviderFactory = new NodeServiceFactoryAdapter<TContainerBuilder>(factory ?? throw new ArgumentNullException(nameof(factory)));
            return this;
        }

        /// <summary>
        /// Overrides the factory used to create the service provider.
        /// </summary>
        /// <returns>The same instance of the <see cref="INodeHostBuilder"/> for chaining.</returns>
        public INodeHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<NodeHostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
        {
            _serviceProviderFactory = new NodeServiceFactoryAdapter<TContainerBuilder>(() => _BrowserHostBuilderContext, factory ?? throw new ArgumentNullException(nameof(factory)));
            return this;
        }

        /// <summary>
        /// Run the given actions to initialize the host. This can only be called once.
        /// </summary>
        /// <returns>An initialized <see cref="INodeHost"/></returns>
        public INodeHost Build()
        {
            if (_hostBuilt)
            {
                throw new InvalidOperationException("Build can only be called once.");
            }
            _hostBuilt = true;

            CreateBrowserHostBuilderContext();
            CreateServiceProvider();

            return _appServices.GetRequiredService<INodeHost>();
        }

        private void CreateBrowserHostBuilderContext()
        {
            _BrowserHostBuilderContext = new NodeHostBuilderContext(Properties);
        }

        private void CreateServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddSingleton(_BrowserHostBuilderContext);
            services.AddSingleton<INodeHost, NodeHost>();
            services.AddSingleton<IJSRuntime>(NodeJSRuntime.Instance);
            // TODO DM 19.08.2019: NavigationManager only exists on master not in the preview
            //services.AddSingleton<NavigationManager>(NodeNavigationManager.Instance);
            services.AddSingleton<INavigationInterception>(NodeNavigationInterception.Instance);
            services.AddSingleton<ILoggerFactory, NodeLoggerFactory>();

            services.AddSingleton<AuthenticationStateProvider, NodeAuthenticationStateProvider>();

            services.AddSingleton<IUriHelper, NodeUriHelper>();
            services.AddSingleton<IComponentContext, NodeComponentContext>();

            // DM 19.08.2019: We do not need an HttpClient like WebAssembly does
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
            services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(NodeConsoleLogger<>)));

            foreach (var configureServicesAction in _configureServicesActions)
            {
                configureServicesAction(_BrowserHostBuilderContext, services);
            }

            var builder = _serviceProviderFactory.CreateBuilder(services);
            _appServices = _serviceProviderFactory.CreateServiceProvider(builder);
        }
    }
}