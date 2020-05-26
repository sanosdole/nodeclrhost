/*process.env.COREHOST_TRACE = 1;
process.env.COREHOST_TRACE_VERBOSITY= 3;*/

const coreclrhosting = require('coreclr-hosting');

global.electron = require('electron');



console.log("PID:" + process.pid);
var runResult = coreclrhosting.runCoreApp(__dirname + '/MvcApp/bin/Debug/netcoreapp3.1/MvcApp.dll', __dirname + '/MvcApp');
