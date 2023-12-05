// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// Modified by Daniel Martin for nodeclrhost

import { DotNet } from './JsInterop/Microsoft.JSInterop';
import { Blazor } from './GlobalExports';
import { renderBatch, attachRootComponentToElement, attachRootComponentToLogicalElement } from './Rendering/Renderer';
import { OutOfProcessRenderBatch } from './Rendering/RenderBatch/OutOfProcessRenderBatch';
import { WebAssemblyComponentAttacher } from './Platform/WebAssemblyComponentAttacher';
import { discoverComponents, WebAssemblyComponentDescriptor } from './Services/ComponentDescriptorDiscovery';
import { fetchAndInvokeInitializers } from './JSInitializers/JSInitializers.Electron';
import { InitialRootComponentsList } from './Services/InitialRootComponentsList';
import { JSEventRegistry } from './Services/JSEventRegistry';

import coreclrhosting = require('coreclr-hosting');

let started = false;

export let dispatcher: DotNet.ICallDispatcher;
window['DotNet'] = DotNet;

export async function runBlazorApp(assemblyPath: string, ...args: string[]): Promise<void> {
  if (started) {
    throw new Error('Blazor has already started.');
  }
  started = true;

  window['electron'] = require('electron');

  //const jsInitializer = await fetchAndInvokeInitializers();

  JSEventRegistry.create(Blazor);
  const webAssemblyComponents = discoverComponents(document, 'webassembly') as WebAssemblyComponentDescriptor[];
  const components = new InitialRootComponentsList(webAssemblyComponents);

  const componentAttacher = new WebAssemblyComponentAttacher(components);
  window['Blazor']._internal.attachRootComponentToElement = (selector, componentId, rendererId) => {
    const element = componentAttacher.resolveRegisteredElement(selector);
    if (!element) {
      attachRootComponentToElement(selector, componentId, rendererId);
    } else {
      attachRootComponentToLogicalElement(rendererId, element, componentId, false);
    }
  };

  //setEventDispatcher((eventDescriptor, eventArgs) => window['Blazor']._internal.HandleRendererEvent(eventDescriptor, JSON.stringify(eventArgs)));
  /*setEventDispatcher((eventDescriptor, eventArgs) => {
    // It's extremely unusual, but an event can be raised while we're in the middle of synchronously applying a
    // renderbatch. For example, a renderbatch might mutate the DOM in such a way as to cause an <input> to lose
    // focus, in turn triggering a 'change' event. It may also be possible to listen to other DOM mutation events
    // that are themselves triggered by the application of a renderbatch.
    const renderer = getRendererer(eventDescriptor.browserRendererId);
    if (renderer.eventDelegator.getHandler(eventDescriptor.eventHandlerId)) {
      //monoPlatform.invokeWhenHeapUnlocked(() => DotNet.invokeMethodAsync('Microsoft.AspNetCore.Components.WebAssembly', 'DispatchEvent', eventDescriptor, JSON.stringify(eventArgs)));
      window['Blazor']._internal.HandleRendererEvent(eventDescriptor, JSON.stringify(eventArgs))
    }
  });*/

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
/*
  window['Blazor']._internal.navigationManager.listenForNavigationEvents(async (uri: string, intercepted: boolean): Promise<void> => {
    await DotNet.invokeMethodAsync(
      'Microsoft.AspNetCore.Components.WebAssembly',
      'NotifyLocationChanged',
      uri,
      intercepted
    );
  });*/

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

  
  //await jsInitializer.invokeAfterStartedCallbacks(Blazor);

  // DM 21.08.2019: Start the blazor app
  return await coreclrhosting.runCoreApp(assemblyPath, ...args);
  
  
}
