// Provide require function for coreclr when running on nodejs
global.require = require;

module.exports = require('bindings')('coreclr-hosting.node');
