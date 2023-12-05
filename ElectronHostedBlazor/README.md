# ElectronHostedBlazor

This project enables writing node/electron applications with .NET.
This is achieved by a native node module (`coreclr-hosting`) that runs a .NET application.
The .NET application uses the `NodeHostEnvironment` library to interact with the node runtime.

Besides running .NET in node, we can also run [.NET Blazor apps](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) in an [Electron](https://electronjs.org/) renderer process without WebAssembly.
This enables access to the DOM and the full .NET core framework at the same time (including full debugger support).
Instructions on how to set this up can be found [here](https://github.com/sanosdole/nodeclrhost/blob/master/docs/electron-blazor-setup.md).

> `ElectronHostedBlazor` only supports the .NET version matching the major version of this project.
