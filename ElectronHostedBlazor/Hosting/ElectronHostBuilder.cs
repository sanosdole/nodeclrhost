// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Hosting
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text.Json;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Infrastructure;
    using Microsoft.AspNetCore.Components.RenderTree;
    using Microsoft.AspNetCore.Components.Routing;
    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.Json;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.JSInterop;
    using NodeHostEnvironment;

    using Services;
    using Logging;
    using Microsoft.Extensions.Hosting;



    /// <summary>
    /// A builder for configuring and creating a <see cref="ElectronHost"/>.
    /// </summary>
    public sealed class ElectronHostBuilder
    {
        //private readonly JsonSerializerOptions _jsonOptions;
        private Func<IServiceProvider> _createServiceProvider;
        private RootComponentTypeCache? _rootComponentCache;
        private string? _persistedState;

        /// <summary>
        /// Creates an instance of <see cref="ElectronHostBuilder"/> using the most common
        /// conventions and settings.
        /// </summary>
        /// <param name="args">The argument passed to the application's main method.</param>
        /// <returns>A <see cref="ElectronHostBuilder"/>.</returns>        
        public static ElectronHostBuilder CreateDefault(string[]? args = default)
        {
            var node = NodeHost.Instance;
            /*// We don't use the args for anything right now, but we want to accept them
            // here so that it shows up this way in the project templates.
            var jsRuntime = DefaultElectronJSRuntime.Instance;*/
            var builder = new ElectronHostBuilder(node);

            //ElectronCultureProvider.Initialize();

            // Right now we don't have conventions or behaviors that are specific to this method
            // however, making this the default for the template allows us to add things like that
            // in the future, while giving `new ElectronHostBuilder` as an opt-out of opinionated
            // settings.
            return builder;
        }

        /// <summary>
        /// Creates an instance of <see cref="ElectronHostBuilder"/> with the minimal configuration.
        /// </summary>
        internal ElectronHostBuilder(
            /*IJSUnmarshalledRuntime jsRuntime, JsonSerializerOptions jsonOptions*/
            IBridgeToNode node)
        {
            Node = node;
            // Private right now because we don't have much reason to expose it. This can be exposed
            // in the future if we want to give people a choice between CreateDefault and something
            // less opinionated.

            //_jsonOptions = jsonOptions;
            Configuration = new ElectronHostConfiguration();
            RootComponents = new RootComponentMappingCollection();
            Services = new ServiceCollection();
            Logging = new LoggingBuilder(Services);

            // Retrieve required attributes from JSRuntimeInvoker
            //InitializeNavigationManager(jsRuntime);
            //InitializeRegisteredRootComponents(jsRuntime);
            InitializePersistedState(node/*jsRuntime*/);
            InitializeDefaultServices(node);

            var hostEnvironment = InitializeEnvironment(node/*jsRuntime*/);
            HostEnvironment = hostEnvironment;

            _createServiceProvider = () =>
            {
                return Services.BuildServiceProvider(validateScopes: ElectronHostEnvironmentExtensions.IsDevelopment(hostEnvironment));
            };
        }

        /*private void InitializeRegisteredRootComponents(IJSUnmarshalledRuntime jsRuntime)
        {
            var componentsCount = jsRuntime.InvokeUnmarshalled<int>(RegisteredComponentsInterop.GetRegisteredComponentsCount);
            if (componentsCount == 0)
            {
                return;
            }

            var registeredComponents = new WebAssemblyComponentMarker[componentsCount];
            for (var i = 0; i < componentsCount; i++)
            {
                var id = jsRuntime.InvokeUnmarshalled<int, int>(RegisteredComponentsInterop.GetId, i);
                var assembly = jsRuntime.InvokeUnmarshalled<int, string>(RegisteredComponentsInterop.GetAssembly, id);
                var typeName = jsRuntime.InvokeUnmarshalled<int, string>(RegisteredComponentsInterop.GetTypeName, id);
                var serializedParameterDefinitions = jsRuntime.InvokeUnmarshalled<int, object?, object?, string>(RegisteredComponentsInterop.GetParameterDefinitions, id, null, null);
                var serializedParameterValues = jsRuntime.InvokeUnmarshalled<int, object?, object?, string>(RegisteredComponentsInterop.GetParameterValues, id, null, null);
                registeredComponents[i] = new WebAssemblyComponentMarker(WebAssemblyComponentMarker.ClientMarkerType, assembly, typeName, serializedParameterDefinitions, serializedParameterValues, id.ToString(CultureInfo.InvariantCulture));
            }

            var componentDeserializer = ElectronComponentParameterDeserializer.Instance;
            foreach (var registeredComponent in registeredComponents)
            {
                _rootComponentCache = new RootComponentTypeCache();
                var componentType = _rootComponentCache.GetRootComponent(registeredComponent.Assembly!, registeredComponent.TypeName!);
                if (componentType is null)
                {
                    throw new InvalidOperationException(
                        $"Root component type '{registeredComponent.TypeName}' could not be found in the assembly '{registeredComponent.Assembly}'. " +
                        $"This is likely a result of trimming (tree shaking).");
                }

                var definitions = componentDeserializer.GetParameterDefinitions(registeredComponent.ParameterDefinitions!);
                var values = componentDeserializer.GetParameterValues(registeredComponent.ParameterValues!);
                var parameters = componentDeserializer.DeserializeParameters(definitions, values);

                RootComponents.Add(componentType, registeredComponent.PrerenderId!, parameters);
            }
        }*/

        private void InitializePersistedState(/*IJSUnmarshalledRuntime jsRuntime*/IBridgeToNode node)
        {
            //_persistedState = jsRuntime.InvokeUnmarshalled<string>("Blazor._internal.getPersistedState");
            //_persistedState = (string)node.Global.window.Blazor._internal.getPersistedState();
        }

        /*private void InitializeNavigationManager(IJSUnmarshalledRuntime jsRuntime)
        {
            var baseUri = jsRuntime.InvokeUnmarshalled<string>(BrowserNavigationManagerInterop.GetBaseUri);
            var uri = jsRuntime.InvokeUnmarshalled<string>(BrowserNavigationManagerInterop.GetLocationHref);

            ElectronNavigationManager.Instance = new ElectronNavigationManager(baseUri, uri);
        }*/

        private ElectronHostEnvironment InitializeEnvironment(/*IJSUnmarshalledRuntime jsRuntime*/IBridgeToNode node)
        {
            //var applicationEnvironment = jsRuntime.InvokeUnmarshalled<string>("Blazor._internal.getApplicationEnvironment");
            //var applicationEnvironment = (string)node.Global.window.Blazor._internal.getApplicationEnvironment();
            // TODO: Read appEnv using normal dotnet mechanism from Environment!
            //var applicationEnvironment = Environments.Production;
            var applicationEnvironment = "Production";

            //var hostEnvironment = new ElectronHostEnvironment(applicationEnvironment, ElectronNavigationManager.Instance.BaseUri);
            var hostEnvironment = new ElectronHostEnvironment(applicationEnvironment, (string)node.Global.window.Blazor._internal.navigationManager.getBaseURI());

            Services.AddSingleton<IElectronHostEnvironment>(hostEnvironment);
            
            /*var configFiles = new[]
            {
                "appsettings.json",
                $"appsettings.{applicationEnvironment}.json"
            };

            foreach (var configFile in configFiles)
            {
                / *var appSettingsJson = jsRuntime.InvokeUnmarshalled<string, byte[]>(
                    "Blazor._internal.getConfig", configFile);* /
                var appSettingsJson = (byte[])node.Global.window.Blazor._internal.getConfig(configFile);

                if (appSettingsJson != null)
                {
                    // Perf: Using this over AddJsonStream. This allows the linker to trim out the "File"-specific APIs and assemblies
                    // for Configuration, of where there are several.
                    Configuration.Add<JsonStreamConfigurationSource>(s => s.Stream = new MemoryStream(appSettingsJson));
                }
            }*/

            return hostEnvironment;
        }

        /// <summary>
        /// Gets an <see cref="ElectronHostConfiguration"/> that can be used to customize the application's
        /// configuration sources and read configuration attributes.
        /// </summary>
        public ElectronHostConfiguration Configuration { get; }

        /// <summary>
        /// Gets the collection of root component mappings configured for the application.
        /// </summary>
        public RootComponentMappingCollection RootComponents { get; }

        /// <summary>
        /// Gets the service collection.
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Gets information about the app's host environment.
        /// </summary>
        public IElectronHostEnvironment HostEnvironment { get; }

        /// <summary>
        /// Gets the logging builder for configuring logging services.
        /// </summary>
        public ILoggingBuilder Logging { get; }
        internal ElectronJSRuntime? Runtime { get; private set;}
        internal IBridgeToNode? Node { get; }

        /// <summary>
        /// Registers a <see cref="IServiceProviderFactory{TBuilder}" /> instance to be used to create the <see cref="IServiceProvider" />.
        /// </summary>
        /// <param name="factory">The <see cref="IServiceProviderFactory{TBuilder}" />.</param>
        /// <param name="configure">
        /// A delegate used to configure the <typeparamref T="TBuilder" />. This can be used to configure services using
        /// APIS specific to the <see cref="IServiceProviderFactory{TBuilder}" /> implementation.
        /// </param>
        /// <typeparam name="TBuilder">The type of builder provided by the <see cref="IServiceProviderFactory{TBuilder}" />.</typeparam>
        /// <remarks>
        /// <para>
        /// <see cref="ConfigureContainer{TBuilder}(IServiceProviderFactory{TBuilder}, Action{TBuilder})"/> is called by <see cref="Build"/>
        /// and so the delegate provided by <paramref name="configure"/> will run after all other services have been registered.
        /// </para>
        /// <para>
        /// Multiple calls to <see cref="ConfigureContainer{TBuilder}(IServiceProviderFactory{TBuilder}, Action{TBuilder})"/> will replace
        /// the previously stored <paramref name="factory"/> and <paramref name="configure"/> delegate.
        /// </para>
        /// </remarks>
        public void ConfigureContainer<TBuilder>(IServiceProviderFactory<TBuilder> factory, Action<TBuilder>? configure = null) where TBuilder : notnull
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            _createServiceProvider = () =>
            {
                var container = factory.CreateBuilder(Services);
                configure?.Invoke(container);
                return factory.CreateServiceProvider(container);
            };
        }

        /// <summary>
        /// Builds a <see cref="ElectronHost"/> instance based on the configuration of this builder.
        /// </summary>
        /// <returns>A <see cref="ElectronHost"/> object.</returns>
        public ElectronHost Build()
        {
            // Intentionally overwrite configuration with the one we're creating.
            Services.AddSingleton<IConfiguration>(Configuration);

            // A Blazor application always runs in a scope. Since we want to make it possible for the user
            // to configure services inside *that scope* inside their startup code, we create *both* the
            // service provider and the scope here.
            var services = _createServiceProvider();
            var scope = services.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();

            return new ElectronHost(this, services, scope, _persistedState);
        }

        internal void InitializeDefaultServices(IBridgeToNode node)
        {
            Services.AddSingleton<IBridgeToNode>(node);
            Runtime = new ElectronJSRuntime(node);
            Services.AddSingleton<IJSRuntime>(Runtime);            
            Services.AddSingleton<IScrollToLocationHash, ElectronScrollToLocationHash>();
            Services.AddSingleton<NavigationManager, ElectronNavigationManager>();
            Services.AddSingleton<INavigationInterception, ElectronNavigationInterception>();
            

            //Services.AddSingleton(new LazyAssemblyLoader(DefaultElectronJSRuntime.Instance));
            Services.AddSingleton<ComponentStatePersistenceManager>();
            Services.AddSingleton<PersistentComponentState>(sp => sp.GetRequiredService<ComponentStatePersistenceManager>().State);
            Services.AddSingleton<IErrorBoundaryLogger, ElectronErrorBoundaryLogger>();
            Services.AddLogging(builder => builder.AddElectronConsole());
        }
    }
}
