// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

import '@dotnet/jsinterop';
import './GlobalExports';
import { renderBatch, getRendererer, attachRootComponentToElement, attachRootComponentToLogicalElement } from './Rendering/Renderer';
import { OutOfProcessRenderBatch } from './Rendering/RenderBatch/OutOfProcessRenderBatch';
import { setEventDispatcher } from './Rendering/RendererEventDispatcher';

import coreclrhosting = require('coreclr-hosting');

let started = false;
export async function runBlazorApp(assemblyPath: string, ...args: string[]): Promise<void> {

  if (started) {
    throw new Error('Blazor has already started.');
  }
  started = true;

  window['electron'] = require('electron');

  // DM 07.09.2020: Server side blazor uses JSON.parse while WASM uses some Mono internal function. So reviving is probably broken.
  //                We need this as the dotnet/jsinterop does not parse the result json directly.
  DotNet.jsCallDispatcher['endInvokeDotNetFromJSWithJson'] = function (asyncCallId: string, success: boolean, resultOrExceptionMessage: any): void {
    DotNet.jsCallDispatcher.endInvokeDotNetFromJS(asyncCallId, success, success ? JSON.parse(resultOrExceptionMessage) : resultOrExceptionMessage);
  }

  //setEventDispatcher((eventDescriptor, eventArgs) => window['Blazor']._internal.HandleRendererEvent(eventDescriptor, JSON.stringify(eventArgs)));
  setEventDispatcher((eventDescriptor, eventArgs) => {
    // It's extremely unusual, but an event can be raised while we're in the middle of synchronously applying a
    // renderbatch. For example, a renderbatch might mutate the DOM in such a way as to cause an <input> to lose
    // focus, in turn triggering a 'change' event. It may also be possible to listen to other DOM mutation events
    // that are themselves triggered by the application of a renderbatch.
    const renderer = getRendererer(eventDescriptor.browserRendererId);
    if (renderer.eventDelegator.getHandler(eventDescriptor.eventHandlerId)) {
      //monoPlatform.invokeWhenHeapUnlocked(() => DotNet.invokeMethodAsync('Microsoft.AspNetCore.Components.WebAssembly', 'DispatchEvent', eventDescriptor, JSON.stringify(eventArgs)));
      window['Blazor']._internal.HandleRendererEvent(eventDescriptor, JSON.stringify(eventArgs))
    }
  });

  // DM 21.08.2019: Setting up the renderer
  window['Blazor']._internal.renderBatch = (browserRendererId: number, batchAddress: ArrayBuffer, batchLength: number) => {
    try {
      var typedArray = new Uint8Array(batchAddress, 0, batchLength);
      renderBatch(browserRendererId, new OutOfProcessRenderBatch(typedArray));
    } catch (error) {
      console.error(error);
    }
  }

  // Configure navigation via JS Interop
  /*const getBaseUri = window['Blazor']._internal.navigationManager.getBaseURI;
  const getLocationHref = window['Blazor']._internal.navigationManager.getLocationHref;
  window['Blazor']._internal.navigationManager.getUnmarshalledBaseURI = () => BINDING.js_string_to_mono_string(getBaseUri());
  window['Blazor']._internal.navigationManager.getUnmarshalledLocationHref = () => BINDING.js_string_to_mono_string(getLocationHref());*/

  window['Blazor']._internal.navigationManager.listenForNavigationEvents(async (uri: string, intercepted: boolean): Promise<void> => {
    await DotNet.invokeMethodAsync(
      'Microsoft.AspNetCore.Components.WebAssembly',
      'NotifyLocationChanged',
      uri,
      intercepted
    );
  });

  /*
  // Get the custom environment setting if defined
  const environment = options?.environment;

  // Fetch the resources and prepare the Mono runtime
  const bootConfigPromise = BootConfigResult.initAsync(environment);

  // Leverage the time while we are loading boot.config.json from the network to discover any potentially registered component on
  // the document.
  const discoveredComponents = discoverComponents(document, 'webassembly') as WebAssemblyComponentDescriptor[];
  const componentAttacher = new WebAssemblyComponentAttacher(discoveredComponents);
  window['Blazor']._internal.registeredComponents = {
    getRegisteredComponentsCount: () => componentAttacher.getCount(),
    getId: (index) => componentAttacher.getId(index),
    getAssembly: (id) => BINDING.js_string_to_mono_string(componentAttacher.getAssembly(id)),
    getTypeName: (id) => BINDING.js_string_to_mono_string(componentAttacher.getTypeName(id)),
    getParameterDefinitions: (id) => BINDING.js_string_to_mono_string(componentAttacher.getParameterDefinitions(id) || ''),
    getParameterValues: (id) => BINDING.js_string_to_mono_string(componentAttacher.getParameterValues(id) || ''),
  };

  window['Blazor']._internal.attachRootComponentToElement = (selector, componentId, rendererId) => {
    const element = componentAttacher.resolveRegisteredElement(selector);
    if (!element) {
      attachRootComponentToElement(selector, componentId, rendererId);
    } else {
      attachRootComponentToLogicalElement(rendererId, element, componentId);
    }
  };

  const bootConfigResult = await bootConfigPromise;
  const [resourceLoader] = await Promise.all([
    WebAssemblyResourceLoader.initAsync(bootConfigResult.bootConfig, options || {}),
    WebAssemblyConfigLoader.initAsync(bootConfigResult)]);
*/

  // DM 21.08.2019: Start the blazor app
  return await coreclrhosting.runCoreApp(assemblyPath, ...args);
  
  
}
