using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Routing;

namespace BlazorApp.Services
{
    internal sealed class NodeNavigationInterception : INavigationInterception
    {
        public static readonly NodeNavigationInterception Instance = new NodeNavigationInterception();

        public Task EnableNavigationInterceptionAsync()
        {
            // TODO DM 19.08.2019: Implement
            //NodeJSRuntime.Instance.Invoke<object>(Interop.EnableNavigationInterception);
            return Task.CompletedTask;
        }
    }
}