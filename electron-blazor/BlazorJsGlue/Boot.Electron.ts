import '@dotnet/jsinterop'; 
import './GlobalExports';
import * as Environment from './Environment';
import { getAssemblyNameFromUrl } from './Platform/Url';
import { renderBatch } from './Rendering/Renderer';
import { SharedMemoryRenderBatch } from './Rendering/RenderBatch/SharedMemoryRenderBatch';
import { OutOfProcessRenderBatch } from './Rendering/RenderBatch/OutOfProcessRenderBatch';
import { Pointer } from './Platform/Platform';
import { fetchBootConfigAsync, loadEmbeddedResourcesAsync, shouldAutoStart } from './BootCommon';
import { setEventDispatcher } from './Rendering/RendererEventDispatcher';
import coreclrhosting = require('coreclr-hosting');

let started = false;

async function boot(options?: any): Promise<void> {

  if (started) {
    throw new Error('Blazor has already started.');
  }
  started = true;

  setEventDispatcher((eventDescriptor, eventArgs) => DotNet.invokeMethodAsync('Microsoft.AspNetCore.Blazor', 'DispatchEvent', eventDescriptor, JSON.stringify(eventArgs)));

  // Configure environment for execution under Mono WebAssembly with shared-memory rendering
  /*const platform = Environment.setPlatform(monoPlatform);
  window['Blazor'].platform = platform;
  window['Blazor']._internal.renderBatch = (browserRendererId: number, batchAddress: Pointer) => {
    renderBatch(browserRendererId, new SharedMemoryRenderBatch(batchAddress));
  };*/

  // Configure navigation via JS Interop
  window['Blazor']._internal.navigationManager.listenForNavigationEvents(async (uri: string, intercepted: boolean): Promise<void> => {
    await DotNet.invokeMethodAsync(
      'Microsoft.AspNetCore.Blazor',
      'NotifyLocationChanged',
      uri,
      intercepted
    );
  });

// DM 21.08.2019: Setting up the renderer
window['Blazor']._internal.renderBatch = (browserRendererId: number, batchAddress: ArrayBuffer) => {
  var typedArray = new Uint8Array(batchAddress);
  console.info("rendering batch of size " + typedArray.byteLength + " and first byte " + typedArray[0]);
  renderBatch(browserRendererId, new OutOfProcessRenderBatch(typedArray));
}

// DM 21.08.2019: Start the blazor app
console.info("Starting from " + __dirname + '\\..');
var result = coreclrhosting.runCoreApp(__dirname + '\\..', 'BlazorApp.dll');
console.info("Main returned: " + result);
//console.info(window["Blazor"]._internal);

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

window['Blazor'].start = boot;
if (shouldAutoStart()) {
  boot();
}

