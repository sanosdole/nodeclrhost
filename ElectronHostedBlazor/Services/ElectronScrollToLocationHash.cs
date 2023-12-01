// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Services;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Routing;
using NodeHostEnvironment;

internal sealed class ElectronScrollToLocationHash : IScrollToLocationHash
{
    private readonly dynamic _blazorInternal;
    private readonly dynamic _navigationManager;

    public ElectronScrollToLocationHash(IBridgeToNode node)
        {
            _blazorInternal = node.Global.window.Blazor._internal;
            _navigationManager = _blazorInternal.navigationManager;
        }

        public Task RefreshScrollPositionForHash(string locationAbsolute)
    {
        var hashIndex = locationAbsolute.IndexOf("#", StringComparison.Ordinal);

        if (hashIndex > -1 && locationAbsolute.Length > hashIndex + 1)
        {
            var elementId = locationAbsolute[(hashIndex + 1)..];

            _navigationManager.scrollToElement(elementId);
        }

        return Task.CompletedTask;
    }
}