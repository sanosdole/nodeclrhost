const { app, BrowserWindow } = require('electron');
const coreclrhosting = require('coreclr-hosting');

console.log('hosting:', coreclrhosting);
console.log('hosting:', BrowserWindow);
global.electron = require('electron');
global.appRoot = __dirname;
/*
global.createBrowserWindow = function(options) {
    //return new BrowserWindow(options);
    var mainWindow = new BrowserWindow({webPreferences: {
        nodeIntegration: true,
        additionalArguments: [ "--allow-sandbox-debugging" ]
    }});
    mainWindow.loadFile("renderer.html");
    return mainWindow;
}*/

console.log("PID:" + process.pid);
var runResult = coreclrhosting.runCoreApp(__dirname + '/BrowserApp/bin/Debug/net6.0/BrowserApp.dll', 'asd');

/*
electron = global.electron;
let window;
electron.app.on("ready", launchInfo => {
    window = new BrowserWindow({ 
        title: "FromElectron",
        webPreferences: {
            nodeIntegration: true,
            devTools: false,
            contextIsolation: false,
            sandbox: false
        }});
    window.loadFile("renderer.html");
});
*/


/*
app.on('ready', function (launchInfo) {
    mainWindow = new BrowserWindow({webPreferences: {
        nodeIntegration: true
    }});
    mainWindow.loadFile("renderer.html");
});*/