const coreclrhosting = require('.././');
const Runner = require('benchr');

global.dotnetCallbacks = {};
global.dotnetCallbacks.jsVoidToVoid = function () {

};

global.dotnetCallbacks.jsIntIntToInt = function (a,b) {
    return a + b;
};

global.dotnetCallbacks.jsVoidToTask = function () {
    return Promise.resolve();
};

global.dotnetCallbacks.jsIntToTaskDelay = function (delay) {
    return new Promise(r => {
        setTimeout(r, delay);
    });
};

global.dotnetCallbacks.jsVoidToTaskYield1 = function () {
    return new Promise(r => {
        setImmediate(r);
    });
};

global.dotnetCallbacks.jsVoidToTaskYield2 = function () {
    return Promise.resolve().then(() => {});
};

global.dotnetCallbacks.jsVoidToTaskYield3 = function () {
    return new Promise(r => {
        queueMicrotask(r);
    });
};

global.dotnetCallbacks.jsTaskToTask = async function(promise) {
    await promise;
}

global.dotnetCallbacks.jsStringArrayToString = function (array) {
    return array.join('|');
};

global.dotnetCallbacks.jsStringToStringArray = function (s) {
    return s.split("|");
};


var promise = coreclrhosting.runCoreApp(__dirname + '/bin/Debug/netcoreapp3.1/Benchmark.dll');

// Fire up runner
const runner = new Runner({
    
}, [ "benchmark/benchmark.js" ]);

runner.on('run.complete', () => {
    global.closeDotNet();
});

runner.run();
return promise;