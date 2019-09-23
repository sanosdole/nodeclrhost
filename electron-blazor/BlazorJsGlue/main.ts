
import coreclrhosting = require('coreclr-hosting');

global["window"] = window;
global["ToReplace"] = window.document.getElementById("AnimateDiv");
console.log('TATA: ', global["ToReplace"]);
console.log('PATH: ', __dirname + '\\bin\\published');
var result = coreclrhosting.runCoreApp(__dirname + '\\..', 'BlazorApp.dll');
document.write('from net ' + result);