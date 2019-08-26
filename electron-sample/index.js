const { app, BrowserWindow } = require('electron');
const coreclrhosting = require('coreclr-hosting');

console.log('hosting:', coreclrhosting);
console.log('hosting:', BrowserWindow);
global.electron = require('electron');

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


var runResult = coreclrhosting.runCoreApp(__dirname + '/BrowserApp/bin/published', 'BrowserApp.dll');

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