// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

import { navigateTo, internalFunctions as navigationManagerInternalFunctions } from './Services/NavigationManager';
import { internalFunctions as httpInternalFunctions } from './Services/Http';
import { attachRootComponentToElement } from './Rendering/Renderer';

// Make the following APIs available in global scope for invocation from JS
window['Blazor'] = {
  navigateTo,

  _internal: {
    attachRootComponentToElement,
    http: httpInternalFunctions,
    navigationManager: navigationManagerInternalFunctions,
  },
};