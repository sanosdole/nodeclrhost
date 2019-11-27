# nodeclrhost - Hosting .NET core in node and electron

[![Build status master](https://travis-ci.com/sanosdole/nodeclrhost.svg?branch=master)](https://travis-ci.com/sanosdole/nodeclrhost) [![Build status v0.1.0-alpha.10](https://travis-ci.com/sanosdole/nodeclrhost.svg?branch=v0.1.0-alpha.10)](https://travis-ci.com/sanosdole/nodeclrhost) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

This is an experimental project that enables writing node/electron applications with .NET core.
This is achieved by native node module (`coreclr-hosting`) that runs a .NET core application.
The .NET application uses the `NodeHostEnvironment` library to interact with the node runtime.
The .NET application is kept alive until it explicitly ends the hosting.

Currently the application needs to be published as standalone for the addon to load the .NET runtime.

Besides running .NET in node, we can also run [.NET Blazor apps](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) in a [Electron](https://electronjs.org/) renderer process without WebAssembly.
This enables access to the DOM and the full .NET core framework at the same time (including full debugger support).
Instructions on how to set this up can be found [here](docs/electron-blazor-setup.md).

## Simple example

To run a .NET application the following JS code is required:

```js
const coreclrHosting = require('coreclr-hosting');

var exitcode = coreclrHosting.runCoreApp(pathToPublishedApplication, nameOfTheApplicationAssembly);
console.log('.NET entry point returned: ' + exitcode);
```

The .NET application has to set up the hosting environment in its entry point like this:

```cs
class Program
{
    static int Main(string[] args)
    {
        var host = NodeHost.InProcess(); // This will initialize the bridge
        var console = host.Global.console;
        console.log("Starting timeout");
        host.Global.setTimeout(new Action(() =>
                                {
                                    console.log("Timeout from node");
                                    host.Dispose(); // This will allow the node application to exit
                                }),
                                1500);
        return 5;
    }
}
```

This application will output:

```console
Starting timeout
.NET entry point returned: 5
Timeout from node
```

