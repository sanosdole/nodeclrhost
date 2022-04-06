# nodeclrhost - Hosting .NET core in node and electron

![Build status master](https://github.com/sanosdole/nodeclrhost/actions/workflows/build.yml/badge.svg)

_Latest release_ __v6.0.1__: ![Build status release](https://github.com/sanosdole/nodeclrhost/actions/workflows/release.yml/badge.svg)

This project enables writing node/electron applications with .NET.
This is achieved by a native node module (`coreclr-hosting`) that runs a .NET application.
The .NET application uses the `NodeHostEnvironment` library to interact with the node runtime.

Besides running .NET in node, we can also run [.NET Blazor apps](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) in an [Electron](https://electronjs.org/) renderer process without WebAssembly.
This enables access to the DOM and the full .NET core framework at the same time (including full debugger support).
Instructions on how to set this up can be found [here](docs/electron-blazor-setup.md).

> `ElectronHostedBlazor` only supports the .NET version matching the major version of this project.

## Simple example

To run a .NET application the following JS code is required:

```js
const coreclrHosting = require('coreclr-hosting');

var exitcode = await coreclrHosting.runCoreApp(pathToAssembly, "Hello", "world");
console.log('.NET entry point returned: ' + exitcode);
```

The .NET application can use the `NodeHost.Instance` in its entry point like this:

```cs
class Program
{
    static Task<int> Main(string[] args)
    {
        var tcs = new TaskCompletionSource<int>();
        var host = NodeHost.Instance;
        var console = host.Global.console;
        console.log($"{args[0]} {args[1]}! Starting timeout");
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
Hello world! Starting timeout
Timeout from node
.NET entry point returned: 5
```

## Further examples

The [examples folder](examples) contains examples for:

- [node + dotnet](examples/sample)
- [Running dotnet in Electron browser & renderer processes](examples/electron-sample)
- [Using ElectronHostedBlazor to run a Blazor application in an Electron renderer process](examples/electron-blazor)
- [Running a dotnet MVC app with server side blazor as an Electron application](examples/electron-mvc)