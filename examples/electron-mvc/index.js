const coreclrhosting = require('coreclr-hosting');

var promise = coreclrhosting.runCoreApp(__dirname + '/MvcApp/bin/Debug/net8.0/MvcApp.dll', __dirname + '/MvcApp');

promise.then(function (exitCode) {
    process.exit(exitCode);
}, function (error) {
    console.log(error);
    process.exit(-1);
});
