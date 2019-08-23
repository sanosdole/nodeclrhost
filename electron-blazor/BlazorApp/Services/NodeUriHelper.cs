using System;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Services
{
    public sealed class NodeUriHelper : UriHelperBase
    {
        protected override void EnsureInitialized()
        {
            /* WebAssemblyJSRuntime.Instance.Invoke<object>(
                Interop.ListenForNavigationEvents,
                typeof(WebAssemblyUriHelper).Assembly.GetName().Name,
                nameof(NotifyLocationChanged));

            // As described in the comment block above, BrowserUriHelper is only for
            // client-side (Mono) use, so it's OK to rely on synchronicity here.
            var baseUri = WebAssemblyJSRuntime.Instance.Invoke<string>(Interop.GetBaseUri);
            var uri = WebAssemblyJSRuntime.Instance.Invoke<string>(Interop.GetLocationHref);
            */

            string uri = NodeJSRuntime.Instance.Host.Global.window.location.href;
            string baseUri = NodeJSRuntime.Instance.Host.Global.window.location.origin;



            InitializeState(uri, baseUri);
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            throw new NotImplementedException();
        }
    }
}