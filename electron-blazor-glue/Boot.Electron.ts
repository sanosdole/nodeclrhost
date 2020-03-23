// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

import '@dotnet/jsinterop';
import './GlobalExports';
//import * as Environment from './Environment';
//import { getAssemblyNameFromUrl } from './Platform/Url';
import { renderBatch } from './Rendering/Renderer';
//import { SharedMemoryRenderBatch } from './Rendering/RenderBatch/SharedMemoryRenderBatch';
import { OutOfProcessRenderBatch } from './Rendering/RenderBatch/OutOfProcessRenderBatch';
//import { Pointer } from './Platform/Platform';
//import { fetchBootConfigAsync, loadEmbeddedResourcesAsync, shouldAutoStart } from './BootCommon';
import { setEventDispatcher } from './Rendering/RendererEventDispatcher';
import coreclrhosting = require('coreclr-hosting');

let started = false;
export async function runBlazorApp(assemblyPath: string, ...args: string[]): Promise<void> {

  if (started) {
    throw new Error('Blazor has already started.');
  }
  started = true;

  window['electron'] = require('electron');

  setEventDispatcher((eventDescriptor, eventArgs) => window['Blazor']._internal.HandleRendererEvent(eventDescriptor, JSON.stringify(eventArgs)));
  //DotNet.invokeMethodAsync('Microsoft.AspNetCore.Blazor', 'DispatchEvent', eventDescriptor, JSON.stringify(eventArgs)));

  // Configure environment for execution under Mono WebAssembly with shared-memory rendering
  /*const platform = Environment.setPlatform(monoPlatform);
  window['Blazor'].platform = platform;
  window['Blazor']._internal.renderBatch = (browserRendererId: number, batchAddress: Pointer) => {
    renderBatch(browserRendererId, new SharedMemoryRenderBatch(batchAddress));
  };*/

  // TODO DM 26.08.2019: Navigation is different on trunk compared to preview 8
  // Configure navigation via JS Interop
  /*window['Blazor']._internal.navigationManager.listenForNavigationEvents(async (uri: string, intercepted: boolean): Promise<void> => {
    await DotNet.invokeMethodAsync(
      'Microsoft.AspNetCore.Blazor',
      'NotifyLocationChanged',
      uri,
      intercepted
    );
  });*/

  // DM 21.08.2019: Setting up the renderer
  window['Blazor']._internal.renderBatch = (browserRendererId: number, batchAddress: ArrayBuffer) => {
    try {
      var typedArray = new Uint8Array(batchAddress);
      //console.info("rendering batch of size " + typedArray.byteLength + " and first byte " + typedArray[0]);
      renderBatch(browserRendererId, new OutOfProcessRenderBatch(typedArray));
    } catch (error) {
      console.error(error);
    }
  }  

  // DM 21.08.2019: Start the blazor app
  var result = await coreclrhosting.runCoreApp(assemblyPath, ...args);
  console.info("Main returned: " + result);

  // TODO DM 29.11.2019: Do we need any of this for RCLs to work?
  /*
    // Fetch the boot JSON file
    const bootConfig = await fetchBootConfigAsync();
    const embeddedResourcesPromise = loadEmbeddedResourcesAsync(bootConfig);
  
    if (!bootConfig.linkerEnabled) {
      console.info('Blazor is running in dev mode without IL stripping. To make the bundle size significantly smaller, publish the application or see https://go.microsoft.com/fwlink/?linkid=870414');
    }
  
    // Determine the URLs of the assemblies we want to load, then begin fetching them all
    const loadAssemblyUrls = [bootConfig.main]
      .concat(bootConfig.assemblyReferences)
      .map(filename => `_framework/_bin/${filename}`);
  
    try {
      await platform.start(loadAssemblyUrls);
    } catch (ex) {
      throw new Error(`Failed to start platform. Reason: ${ex}`);
    }
  
    // Before we start running .NET code, be sure embedded content resources are all loaded
    await embeddedResourcesPromise;
  
    // Start up the application
    const mainAssemblyName = getAssemblyNameFromUrl(bootConfig.main);
    platform.callEntryPoint(mainAssemblyName, bootConfig.entryPoint, []);
    */
}
