// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Hosting
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using ElectronHostedBlazor.Rendering;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.JSInterop;

    internal class NodeHost : INodeHost
    {
        private readonly IJSRuntime _runtime;

        private IServiceScope _scope;
        private NodeRenderer _renderer;

        public NodeHost(IServiceProvider services, IJSRuntime runtime)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public IServiceProvider Services { get; }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            // We need to do this as early as possible, it eliminates a bunch of problems. Note that what we do
            // is a bit fragile. If you see things breaking because JSRuntime.Current isn't set, then it's likely
            // that something on the startup path went wrong.
            //
            // We want to the JSRuntime created here to be the 'ambient' runtime when JS calls back into .NET. When
            // this happens in the browser it will be a direct call from Mono. We effectively needs to set the
            // JSRuntime in the 'root' execution context which implies that we want to do as part of a direct
            // call from Program.Main, and before any 'awaits'.
            //JSRuntime.SetCurrentJSRuntime(_runtime);
            //SetBrowserHttpMessageHandlerAsDefault();

            return StartAsyncAwaited();
        }

        private async Task StartAsyncAwaited()
        {
            var scopeFactory = Services.GetRequiredService<IServiceScopeFactory>();
            _scope = scopeFactory.CreateScope();

            try
            {
                var startup = _scope.ServiceProvider.GetService<IBlazorStartup>();
                if (startup == null)
                {
                    var message =
                        $"Could not find a registered Blazor Startup class. " +
                        $"Using {nameof(INodeHost)} requires a call to {nameof(INodeHostBuilder)}.UseBlazorStartup.";
                    throw new InvalidOperationException(message);
                }

                // Note that we differ from the WebHost startup path here by using a 'scope' for the app builder
                // as well as the Configure method.
                var builder = new NodeBlazorApplicationBuilder(_scope.ServiceProvider);
                startup.Configure(builder, _scope.ServiceProvider);

                _renderer = await builder.CreateRendererAsync();
            }
            catch
            {
                _scope.Dispose();
                _scope = null;

                if (_renderer != null)
                {
                    _renderer.Dispose();
                    _renderer = null;
                }

                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (_scope != null)
            {
                _scope.Dispose();
                _scope = null;
            }

            if (_renderer != null)
            {
                _renderer.Dispose();
                _renderer = null;
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            (Services as IDisposable)?.Dispose();
        }
    }

}