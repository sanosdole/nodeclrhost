using System;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Services
{
    // TODO DM 19.08.2019: NavigationManager only exists on master not in the preview
    /*
    /// <summary>
    /// Default client-side implementation of <see cref="NavigationManager"/>.
    /// </summary>
    internal class NodeNavigationManager : NavigationManager
    {
        /// <summary>
        /// Gets the instance of <see cref="NodeNavigationManager"/>.
        /// </summary>
        public static readonly NodeNavigationManager Instance = new NodeNavigationManager();

        // For simplicity we force public consumption of the BrowserNavigationManager through
        // a singleton. Only a single instance can be updated by the browser through
        // interop. We can construct instances for testing.
        internal NodeNavigationManager()
        {
        }

        protected override void EnsureInitialized()
        {
            // As described in the comment block above, BrowserNavigationManager is only for
            // client-side (Mono) use, so it's OK to rely on synchronicity here.
            var baseUri = NodeJSRuntime.Instance.Invoke<string>(Interop.GetBaseUri);
            var uri = NodeJSRuntime.Instance.Invoke<string>(Interop.GetLocationHref);
            Initialize(baseUri, uri);
        }

        public void SetLocation(string uri, bool isInterceptedLink)
        {
            Uri = uri;
            NotifyLocationChanged(isInterceptedLink);
        }

        /// <inheritdoc />
        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            NodeJSRuntime.Instance.Invoke<object>(Interop.NavigateTo, uri, forceLoad);
        }
    }
    */
}