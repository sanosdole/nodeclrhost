const { app, BrowserWindow } = require('electron');
const coreclrhosting = require('coreclr-hosting');

console.log('hosting:', coreclrhosting);
console.log('hosting:', BrowserWindow);
global.electron = require('electron');

coreclrhosting.runCoreApp(__dirname + '/bin/published/LocalService', 'LocalService.dll');
