namespace BlazorApp.Hosting
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BlazorApp.Builder;
    using BlazorApp.Rendering;
    using Microsoft.Extensions.Logging;

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
            var renderer = new NodeRenderer(Services, loggerFactory);
            for (var i = 0; i < Entries.Count; i++)
            {
                var (componentType, domElementSelector) = Entries[i];
                await renderer.AddComponentAsync(componentType, domElementSelector);
            }

            return renderer;
        }
    }
}