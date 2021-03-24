const path = require('path');
const process = require('process');

// Provide require function for coreclr when running on nodejs
global.require = require;

var isDebug = "DEBUG" in process.env;
var nativeModulePath = path.join(__dirname, 'build', isDebug ? 'Debug' : 'Release', 'coreclr-hosting.node');

if (isDebug)
    console.log("Loading coreclr-hosting.node from " + nativeModulePath);
process.env.CORECLR_HOSTING_MODULE_PATH = nativeModulePath;
var coreclrHosting = require(nativeModulePath);

module.exports = coreclrHosting;
