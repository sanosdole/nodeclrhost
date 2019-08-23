using Microsoft.AspNetCore.Components;

namespace BlazorApp.Services
{
    internal class NodeComponentContext : IComponentContext
    {
        public bool IsConnected => true;
    }
}