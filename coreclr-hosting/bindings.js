var coreclrHosting;

if (process.env.DEBUG) {
    coreclrHosting = require(__dirname + '/build/Debug/coreclr-hosting.node');
} else {
    coreclrHosting = require(__dirname + '/build/Release/coreclr-hosting.node');
}

module.exports = coreclrHosting;
