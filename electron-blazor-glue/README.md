# electron-blazor-glue

Necessary JS code to run a Blazor application within an electron renderer process.
It uses  the `coreclr-hosting` native node module to host an Blazor application.
This requires that the Blazor application is written using the `ElectronHostedBlazor` package.

## Installation

```shell
npm install --save @nodeclrhost/celectron-blazor-glue
```

## Usage

Just use the following script in a HTML page loaded in an electron `BrowserWindow`:

```cs
const glue = require('electron-blazor-glue');
window.runBlazorApp("path/to/the/application", "NameOfTheApplication.dll");
```

