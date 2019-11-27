
var assert = require('assert');
const coreclrhosting = require('.././');

global.registerAsyncTest = function registerAsyncTest(testMethod) {
  describe("async test", function() {
    it("should return promise", function() {
      return testMethod();
    });
    it("should invoke done", function(done) {
      testMethod().then(done);
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
      return promise && typeof promise.then === 'function';      
    },
    createPromise: function(shouldResolve) {
      return new Promise(function(resolve, reject) {
        setTimeout(function() {
          if (shouldResolve)
            resolve("Resolved");
          else
            reject(new Error("As requested"));
        }, 10);        
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
