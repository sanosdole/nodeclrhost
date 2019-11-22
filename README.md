# nodeclrhost - Hosting .NET core in node and electron

[![Build Status](https://travis-ci.com/sanosdole/nodeclrhost.svg?branch=master)](https://travis-ci.com/sanosdole/nodeclrhost)

This is an experimental project that enables writing node/electron applications with .NET core.
This is achieved by native node module (`coreclr-hosting`) that runs a .NET core application.
The .NET application uses the `NodeHostEnvironment` library to interact with the node runtime.
The .NET application is kept alive until it explicitly ends the hosting.

Currently the application needs to be published as standalone for the addon to find the .NET runtime.

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

