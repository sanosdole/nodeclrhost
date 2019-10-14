using System;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Services
{
    public sealed class NodeNavigationManager : NavigationManager
    {
        /// <summary>
        /// Gets the instance of <see cref="WebAssemblyUriHelper"/>.
        /// </summary>
        public static readonly NodeNavigationManager Instance = new NodeNavigationManager();

        protected override void EnsureInitialized()
        {
            /* WebAssemblyJSRuntime.Instance.Invoke<object>(
                //Interop.ListenForNavigationEvents,
                "Blazor._internal.uriHelper.listenForNavigationEvents,
                typeof(WebAssemblyUriHelper).Assembly.GetName().Name,
                nameof(NotifyLocationChanged));

            // As described in the comment block above, BrowserUriHelper is only for
            // client-side (Mono) use, so it's OK to rely on synchronicity here.
            var baseUri = WebAssemblyJSRuntime.Instance.Invoke<string>(Interop.GetBaseUri);
            var uri = WebAssemblyJSRuntime.Instance.Invoke<string>(Interop.GetLocationHref);
            */

            var window = NodeJSRuntime.Instance.Host.Global.window;
            var blazor = window.Blazor._internal;

            // TODO DM 26.08.2019: Use boolean once callbacks can convert Number to Boolean
            blazor.navigationManager.listenForNavigationEvents(new Action<string, double>(NotifyLocationChangedFromJs));
            var baseUri = (string)blazor.navigationManager.getBaseURI();
            var uri = (string)blazor.navigationManager.getLocationHref();
            /*string uri = window.location.href;
            string baseUri = window.location.origin;*/
            NodeJSRuntime.Instance.Host.Global.console.info("Uri: " + uri);
            NodeJSRuntime.Instance.Host.Global.console.info("baseUri: " + baseUri);



            Initialize(uri, baseUri);
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            var window = NodeJSRuntime.Instance.Host.Global.window;
            var blazor = window.Blazor._internal;

            blazor.navigateTo(uri, forceLoad);
            //WebAssemblyJSRuntime.Instance.Invoke<object>(Interop.NavigateTo, uri, forceLoad);
        }

        /// <summary>
        /// For framework use only.
        /// </summary>
        //[JSInvokable(nameof(NotifyLocationChanged))]
        private static void NotifyLocationChangedFromJs(string newAbsoluteUri, double isInterceptedLink)
        {
            Instance.SetLocation(newAbsoluteUri, isInterceptedLink > 0.0);
        }

        public void SetLocation(string uri, bool isInterceptedLink)
        {
            Uri = uri;
            NotifyLocationChanged(isInterceptedLink);
        }
        private static string StringUntilAny(string str, char[] chars)
        {
            var firstIndex = str.IndexOfAny(chars);
            return firstIndex < 0
                ? str
                : str.Substring(0, firstIndex);
        }


    }
}