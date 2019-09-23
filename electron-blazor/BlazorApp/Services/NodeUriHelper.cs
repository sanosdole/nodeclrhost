using System;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Services
{
    public sealed class NodeUriHelper : UriHelperBase
    {
        /// <summary>
        /// Gets the instance of <see cref="WebAssemblyUriHelper"/>.
        /// </summary>
        public static readonly NodeUriHelper Instance = new NodeUriHelper();

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
            blazor.uriHelper.listenForNavigationEvents(new Action<string,double>(NotifyLocationChanged));
            var baseUri = (string)blazor.uriHelper.getBaseURI();
            var uri = (string)blazor.uriHelper.getLocationHref();            
            /*string uri = window.location.href;
            string baseUri = window.location.origin;*/
            NodeJSRuntime.Instance.Host.Global.console.info("Uri: " + uri);
            NodeJSRuntime.Instance.Host.Global.console.info("baseUri: " + baseUri);



            InitializeState(uri, baseUri);
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
        private static void NotifyLocationChanged(string newAbsoluteUri, double isInterceptedLink)
        {
            Instance.SetAbsoluteUri(newAbsoluteUri);
            Instance.TriggerOnLocationChanged(isInterceptedLink > 0.0);

 // TODO DM 26.08.2019: Remove test code
            var _baseUri = Instance.GetBaseUri();
        var _locationAbsolute = Instance.GetAbsoluteUri();
        var LocationPath = Instance.ToBaseRelativePath(_baseUri, _locationAbsolute);
        LocationPath = StringUntilAny(LocationPath, new[] { '?', '#' });     
        NodeJSRuntime.Instance.Host.Global.window.console.info("New location: " + LocationPath);
        NodeJSRuntime.Instance.Host.Global.console.info("New location glob: " + LocationPath);
        }

        private  static string StringUntilAny(string str, char[] chars)
    {
        var firstIndex = str.IndexOfAny(chars);
        return firstIndex < 0
            ? str
            : str.Substring(0, firstIndex);
    }

        /// <summary>
        /// Given the document's document.baseURI value, returns the URI
        /// that can be prepended to relative URI paths to produce an absolute URI.
        /// This is computed by removing anything after the final slash.
        /// Internal for tests.
        /// </summary>
        /// <param name="absoluteBaseUri">The page's document.baseURI value.</param>
        /// <returns>The URI prefix</returns>
        internal static string ToBaseUri(string absoluteBaseUri)
        {
            if (absoluteBaseUri != null)
            {
                var lastSlashIndex = absoluteBaseUri.LastIndexOf('/');
                if (lastSlashIndex >= 0)
                {
                    return absoluteBaseUri.Substring(0, lastSlashIndex + 1);
                }
            }

            return "/";
        }
    }
}