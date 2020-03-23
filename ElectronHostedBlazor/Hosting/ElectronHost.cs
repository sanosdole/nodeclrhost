// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Hosting
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.JSInterop;
    using NodeHostEnvironment;

    internal class ElectronHost : IElectronHost
    {
        private readonly IJSRuntime _runtime;
        private readonly IBridgeToNode _node;

        public ElectronHost(IServiceProvider services, IJSRuntime runtime, IBridgeToNode node)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _node = node ?? throw new ArgumentNullException(nameof(node));
        }

        public IServiceProvider Services { get; }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {            
            var scopeFactory = Services.GetRequiredService<IServiceScopeFactory>();
            using (var _scope = scopeFactory.CreateScope())
            {
                var startup = _scope.ServiceProvider.GetService<IBlazorStartup>();
                if (startup == null)
                {
                    var message =
                        $"Could not find a registered Blazor Startup class. " +
                        $"Using {nameof(IElectronHost)} requires a call to {nameof(IElectronHostBuilder)}.UseBlazorStartup.";
                    throw new InvalidOperationException(message);
                }

                // Note that we differ from the WebHost startup path here by using a 'scope' for the app builder
                // as well as the Configure method.
                var builder = new ElectronBlazorApplicationBuilder(_scope.ServiceProvider);
                startup.Configure(builder, _scope.ServiceProvider);

                using(var _renderer = await builder.CreateRendererAsync())
                {
                    var tcs = new TaskCompletionSource<object>();
                    _node.Global.window.addEventListener("unload",
                        new Action<dynamic>(e => tcs.SetResult(0)));
                    await tcs.Task;
                }
            }
            
        }

        public void Dispose()
        {
            (Services as IDisposable)?.Dispose();
        }
    }

}