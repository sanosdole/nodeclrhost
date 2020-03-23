# nodeclrhost - Hosting .NET core in node and electron

[![Build status master](https://travis-ci.com/sanosdole/nodeclrhost.svg?branch=master)](https://travis-ci.com/sanosdole/nodeclrhost) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

_Latest release_ __v0.4.3-alpha.1__: [![Build status release](https://travis-ci.com/sanosdole/nodeclrhost.svg?branch=v0.4.3-alpha.1)](https://travis-ci.com/sanosdole/nodeclrhost)

_Prebuilt versions:_

- Node
  - 12.13.1
  - 12.15.0
- Electron
  - 7.1.4
  - 8.0.0

This is an experimental project that enables writing node/electron applications with .NET core.
This is achieved by native node module (`coreclr-hosting`) that runs a .NET core application.
The .NET application uses the `NodeHostEnvironment` library to interact with the node runtime.
The .NET application is kept alive until it explicitly ends the hosting.

Besides running .NET in node, we can also run [.NET Blazor apps](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) in a [Electron](https://electronjs.org/) renderer process without WebAssembly.
This enables access to the DOM and the full .NET core framework at the same time (including full debugger support).
Instructions on how to set this up can be found [here](docs/electron-blazor-setup.md).

## Simple example

To run a .NET application the following JS code is required:

```js
const coreclrHosting = require('coreclr-hosting');

var exitcode = await coreclrHosting.runCoreApp(pathToAssembly, "hello", "world");
console.log('.NET entry point returned: ' + exitcode);
```

The .NET application has to set up the hosting environment in its entry point like this:

```cs
class Program
{
    static async Task<int> Main(string[] args)
    {
        var tcs = new TaskCompletionSource<int>();
        var host = NodeHost.Instance;
        var console = host.Global.console;
        console.log("Starting timeout");
        host.Global.setTimeout(new Action(() =>
                                {
                                    console.log("Timeout from node");
                                    tcs.SetResult(5);
                                }),
                                1500);
        return tcs.Task;
    }
}
```

This application will output:

```console
Starting timeout
Timeout from node
.NET entry point returned: 5
```

