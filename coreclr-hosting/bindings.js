var coreclrHosting;

if (process.env.DEBUG) {
    coreclrHosting = require('./build/Debug/coreclr-hosting.node');
} else {
    coreclrHosting = require('./build/Release/coreclr-hosting.node');
}

module.exports = coreclrHosting;
