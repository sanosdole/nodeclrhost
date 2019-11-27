const path = require('path');

var nativeModulePath = path.join(__dirname, 'build', process.env.DEBUG ? 'Debug' : 'Release', 'coreclr-hosting.node');
console.log("Loading coreclr-hosting.node from " + nativeModulePath);
var coreclrHosting = require(nativeModulePath);

module.exports = coreclrHosting;
