const path = require('path');

window.require = require;
global.appRoot = window.appRoot = __dirname;
global.nodeModulesPath = window.nodeModulesPath = path.resolve(__dirname, "node_modules");
