# coreclr-hosting

Native node module for hosting the dotnet core runtime.

It utilizes the dotnet core hosting API to load and execute managed code from withing node.

It also provides the native implementation utilized by the `NodeHostEnvironment` package to interact with the node runtime from managed code.

## Installation

```shell
npm install --save coreclr-hosting
```

## Usage

To load and execute a managed assembly a single function is provided:

```js
const coreclrHosting = require('coreclr-hosting');

var exitCode = coreclrHosting.runCoreApp('path/to/published/assembly', 'NameOfAssembly.dll');
```

__Currently only published assemblies containing the dotnet runtime next to them are supported.__
