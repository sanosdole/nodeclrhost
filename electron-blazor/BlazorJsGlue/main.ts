
import coreclrhosting = require('coreclr-hosting');

global["window"] = window;
var result = coreclrhosting.runCoreApp(__dirname + '\\..', 'BlazorApp.dll');
//document.write('from net ' + result);