// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Services
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Options;
    using NodeHostEnvironment;

    public sealed class ElectronNavigationManager : NavigationManager
    {
        private readonly IBridgeToNode _node;
        private readonly dynamic _blazorInternal;
        private readonly dynamic _navigationManager;

        public ElectronNavigationManager(IBridgeToNode node)
        {
            _node = node;
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

            //locationChangingCallback: (callId: number, uri: string, state: string | undefined, intercepted: boolean) => Promise<void>

            _navigationManager.listenForNavigationEvents(0,
                                                         new Func<string, string, bool, Task>(NotifyLocationChangedFromJs),
                                                         new Func<long, string, string, bool, Task>(NotifyLocationChangingFromJs) );
            var baseUri = (string)_navigationManager.getBaseURI();
            var uri = (string)_navigationManager.getLocationHref();
            /*string uri = window.location.href;
            string baseUri = window.location.origin;*/

            Initialize(uri, baseUri);
        }

        protected override void NavigateToCore(string uri, NavigationOptions options)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            _ = PerformNavigationAsync();

            async Task PerformNavigationAsync()
            {
                try
                {
                    var shouldContinueNavigation = await NotifyLocationChangingAsync(uri, options.HistoryEntryState, false);

                    if (!shouldContinueNavigation)
                    {
                        //Log.NavigationCanceled(_logger, uri);
                        return;
                    }

                    var marshalled = _node.New();
                    marshalled.forceLoad = options.ForceLoad;
                    marshalled.replaceHistoryEntry = options.ReplaceHistoryEntry;
                    if (options.HistoryEntryState != null)
                        marshalled.historyEntryState = options.HistoryEntryState;
                    _blazorInternal.navigateTo(uri, options);
                    //DefaultWebAssemblyJSRuntime.Instance.InvokeVoid(Interop.NavigateTo, uri, options);
                }
                catch (Exception ex)
                {
                    // We shouldn't ever reach this since exceptions thrown from handlers are handled in HandleLocationChangingHandlerException.
                    // But if some other exception gets thrown, we still want to know about it.
                    //Log.NavigationFailed(_logger, uri, ex);
                }
            }

            
        }

        private Task NotifyLocationChangedFromJs(string newAbsoluteUri, string state, bool isInterceptedLink)
        {
            SetLocation(newAbsoluteUri, state, isInterceptedLink);
            return Task.CompletedTask;
        }

        private async Task NotifyLocationChangingFromJs(long callId, string uri, string state, bool intercepted)
        {
            await NotifyLocationChangingAsync(uri, state, intercepted);
        }

        public void SetLocation(string uri, string? state, bool isInterceptedLink)
        {
            Uri = uri;
            HistoryEntryState = state;
            NotifyLocationChanged(isInterceptedLink);
        }
    }
}
