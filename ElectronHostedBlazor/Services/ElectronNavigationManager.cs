// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Services
{
    using System;
    using Microsoft.AspNetCore.Components;
    using NodeHostEnvironment;

    public sealed class ElectronNavigationManager : NavigationManager
    {
        private readonly dynamic _blazorInternal;
        private readonly dynamic _navigationManager;

        public ElectronNavigationManager(IBridgeToNode node)
        {
            _blazorInternal = node.Global.window.Blazor._internal;
            _navigationManager = _blazorInternal.navigationManager;
        }

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

            _navigationManager.listenForNavigationEvents(new Action<string, bool>(NotifyLocationChangedFromJs));
            var baseUri = (string)_navigationManager.getBaseURI();
            var uri = (string)_navigationManager.getLocationHref();
            /*string uri = window.location.href;
            string baseUri = window.location.origin;*/

            Initialize(uri, baseUri);
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            _blazorInternal.navigateTo(uri, forceLoad);
            //WebAssemblyJSRuntime.Instance.Invoke<object>(Interop.NavigateTo, uri, forceLoad);
        }

        /// <summary>
        /// For framework use only.
        /// </summary>
        //[JSInvokable(nameof(NotifyLocationChanged))]
        private void NotifyLocationChangedFromJs(string newAbsoluteUri, bool isInterceptedLink)
        {
            SetLocation(newAbsoluteUri, isInterceptedLink);
        }

        public void SetLocation(string uri, bool isInterceptedLink)
        {
            Uri = uri;
            NotifyLocationChanged(isInterceptedLink);
        }

        private static string StringUntilAny(string str, char[] chars)
        {
            var firstIndex = str.IndexOfAny(chars);
            return firstIndex < 0 ? str : str.Substring(0, firstIndex);
        }
    }
}
