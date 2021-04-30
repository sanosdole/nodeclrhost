const path = require('path');
const process = require('process');
var fs = require('fs');

window.require = require;
global.appRoot = window.appRoot = __dirname;
global.nodeModulesPath = window.nodeModulesPath = path.resolve(__dirname, "node_modules");

const clrDumpPath = path.join(__dirname, "ClrDumps");
if (!fs.existsSync(clrDumpPath)){
    fs.mkdirSync(clrDumpPath);
}

process.env['COMPlus_DbgEnableMiniDump'] = "1";
//process.env['COMPlus_DbgMiniDumpType'] = "2";
const dumpPathPattern = path.join(clrDumpPath, "%e_%p_%t.dmp");
console.log(dumpPathPattern);
process.env['COMPlus_DbgMiniDumpName'] = dumpPathPattern;