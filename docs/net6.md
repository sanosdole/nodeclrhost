# Net6

## Notes

- JSInterop is no longer an external module
- CultureProvider is unnecessary
- LazyAssemblyLoaded is unnecessary

- We do not support WebAssemblyComponentMarkers from MVC
  - PrerenderingComponentStore is missing in ElectronHost
  - Should we support pre-rendering use-case?

- UnhandledRendererException still necessary or are they replaced by boundaries?

## TODOs

- AppEnvironment and appsettings.json reading
- No implementation for JSInProcessObjectReference using a converter from ElectronJsRuntime
- Test receiveByteArray & retrieveByteArray (Streams)
