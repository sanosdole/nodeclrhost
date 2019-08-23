//index.js
console.log('Start');

const coreclrHosting = require('coreclr-hosting');

//console.log(module);
//console.log('addon',testAddon);
//console.log('gc',global.gc);
//console.log('this', this);

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
    cb();
  }, 1000)
}
// console.log(testAddon.hello());
console.log(__dirname + '/SampleApp/bin/published');
var runResult = coreclrHosting.runCoreApp(__dirname + '/SampleApp/bin/published', 'SampleApp.dll'/*,
function (error, result) {
    
    console.log('AsyncResult=' + result);
}*/);
console.log('RunResult=' + runResult);