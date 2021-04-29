// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

//import '@microsoft/dotnet-js-interop';
import './JsInterop/Microsoft.JSInterop';

export const domFunctions = {
  focus,
};

function focus(element: HTMLElement): void {
  if (element instanceof HTMLElement) {
    element.focus();
  } else {
    throw new Error('Unable to focus an invalid element.');
  }
}
