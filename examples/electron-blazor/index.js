const coreclrhosting = require('coreclr-hosting');
const path = require("path");

// publish electron api for consumption
global.electron = require('electron');
// publish path to preload script
global.preloadScriptPath = path.resolve(__dirname, "preload.js");

coreclrhosting.runCoreApp(__dirname + "/LocalService/bin/Debug/netcoreapp3.1/LocalService.dll");
