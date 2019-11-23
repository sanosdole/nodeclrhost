
var assert = require('assert');
const coreclrhosting = require('.././');
var pCache;
//global.assert = assert;
global.registerAsyncTest = function registerAsyncTest(testMethod) {
  console.log("Register async func");
  describe("async test", function() {
    it("should return promise", function() {
      /*return new Promise(function(resolve, reject) {
        testMethod().then(v => resolve(), e => reject(e));
      });*/

      global.testPromise.then(function(v) {
        console.log("Testpromise resolved " + v);

      });

      pCache = testMethod();
      console.log("Callint async method pcache= ", pCache);
      return pCache.then(function(v) {
        //done();
        console.log("Resolved");
      }
        );
      //setTimeout(() => { console.log("TIME"); done();}, 20);

    });

  });

}

global.setupTestObject = function() {
  global.testObject = {
    integerValue: 42,
    doubleValue: 3.1415,
    stringValue: "Hello world",
    objectValue: {
      nestedStringValue: "Nested hello"
    },
    nullValue: null,
    trueValue: true,
    falseValue: false,
    funcThatThrows: function() {
      throw new Error("Test error message");
    },
    invokeCallback: function(arg, cb) {
      cb(arg + 'Pong');
    },
    isPromise: function(promise) {
      promise.then((v) => console.log("Fullfilled " + v), r => console.log("Rejected"));
      return promise && typeof promise.then === 'function';      
    },
    createPromise: function(shouldResolve) {
      return new Promise(function(resolve, reject) {
        setTimeout(function() {
          if (shouldResolve)
            resolve("Resolved");
          else
            reject("As requested");
        }, 100);
      });
    }
  };
};

var result = coreclrhosting.runCoreApp(__dirname + '/bin/published', 'TestApp.dll');

describe('coreclrhosting', function () {
  it('should return 0', function () {    
    assert.strictEqual(result, 0);
  });
});
