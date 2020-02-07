const coreclrhosting = require('coreclr-hosting');

// publish electron api for consumption
global.electron = require('electron');

coreclrhosting.runCoreApp(__dirname + "/LocalService/bin/Debug/netcoreapp3.1/LocalService.dll");
