// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Hosting
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ElectronHostedBlazor.Builder;
    using ElectronHostedBlazor.Rendering;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NodeHostEnvironment.BridgeApi;

    internal class NodeBlazorApplicationBuilder : IComponentsApplicationBuilder
    {
        public NodeBlazorApplicationBuilder(IServiceProvider services)
        {
            Entries = new List<(Type componentType, string domElementSelector)>();
            Services = services;
        }

        public List<(Type componentType, string domElementSelector)> Entries { get; }

        public IServiceProvider Services { get; }

        public void AddComponent(Type componentType, string domElementSelector)
        {
            if (componentType == null)
            {
                throw new ArgumentNullException(nameof(componentType));
            }

            if (domElementSelector == null)
            {
                throw new ArgumentNullException(nameof(domElementSelector));
            }

            Entries.Add((componentType, domElementSelector));
        }

        public async Task<NodeRenderer> CreateRendererAsync()
        {
            var loggerFactory = (ILoggerFactory)Services.GetService(typeof(ILoggerFactory));
            var renderer = new NodeRenderer(Services, loggerFactory, Services.GetRequiredService<IBridgeToNode>());
            for (var i = 0; i < Entries.Count; i++)
            {
                var (componentType, domElementSelector) = Entries[i];
                await renderer.AddComponentAsync(componentType, domElementSelector);
            }

            return renderer;
        }
    }
}