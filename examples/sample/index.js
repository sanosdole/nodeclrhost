//index.js
console.log('Start');

const coreclrHosting = require('coreclr-hosting');

global.TestClass = function(name) {
    this.name = name;

};

global.testAddon = { a: 5, b: 'string' };
global.testCallback = function(arg1, arg2, arg3) {
    console.log('testCallback called with:', arg1, arg2, arg3);
    var cbResult = arg1(null, "Another argument passed back to given callback");
    console.log('result from cb ', cbResult);

    try {
        if (global.gc) {global.gc();}
      } catch (e) {
        console.log("Use `node --expose-gc index.js`");        
      }

};

global.callLater = function(cb) {
  setTimeout(function() {
    console.log("Calling later");
    try {
      cb();
    } catch (e) {
      console.log(e);
    }
    
  }, 1000)
}

var runResult = coreclrHosting.runCoreApp(__dirname + '/SampleApp/bin/Debug/netcoreapp3.1/SampleApp.dll');
console.log('RunResult=' + runResult);