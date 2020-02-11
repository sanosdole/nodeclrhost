const path = require('path');
const process = require('process');

var nativeModulePath = path.join(__dirname, 'build', process.env.DEBUG ? 'Debug' : 'Release', 'coreclr-hosting.node');
//console.log("Loading coreclr-hosting.node from " + nativeModulePath);
process.env.CORECLR_HOSTING_MODULE_PATH = nativeModulePath;
var coreclrHosting = require(nativeModulePath);

module.exports = coreclrHosting;
