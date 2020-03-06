
var assert = require('assert');
const coreclrhosting = require('.././');

global.registerAsyncTest = function registerAsyncTest(testMethod) {
  describe("async test", function () {
    it("should return promise", function () {
      return testMethod();
    });
    it("should invoke done", function (done) {
      testMethod().then(done);
    });

  });

}

global.setupTestObject = function () {
  global.testObject = {
    integerValue: 42,
    doubleValue: 3.1415,
    stringValue: "Hello world",
    stringArray: [ "A", "B" ],
    mixedArray: [ "A", 42, { iValue: 42, strValue: "strValue"} ],
    objectValue: {
      nestedStringValue: "Nested hello"
    },
    nullValue: null,
    trueValue: true,
    falseValue: false,
    addIntegerValue: function(value) {
      return value + this.integerValue;
    },
    funcThatThrows: function () {
      throw new Error("Test error message");
    },
    invokeCallback: function (arg, cb) {
      cb(arg + 'Pong');
    },
    invokeIntCallback:  function (arg, cb) {
      cb(arg + 42);
    },
    isPromise: function (promise) {
      var isPromise = promise && typeof promise.then === 'function';
      if (isPromise) {
        promise.catch(e => { }); // DM 29.11.2019: This prevents unobserved promise errors
      }
      return isPromise;
    },
    createPromise: function (shouldResolve) {
      return new Promise(function (resolve, reject) {
        setTimeout(function () {
          if (shouldResolve)
            resolve("Resolved");
          else
            reject(new Error("As requested"));
        }, 10);
      });
    },
    awaitPromise: function (promise) {
      return promise;
    },
    TestClass: function (arg) {
      this.value = arg;
    }
  };

  global.testObject.TestClass.staticFunc = function (arg) {
    return arg + 42;
  }
};

console.log("PID: " + process.pid);
var result = coreclrhosting.runCoreApp(__dirname + '/TestApp/bin/Debug/netcoreapp3.0/TestApp.dll');

describe('coreclrhosting', function () {
  it('should return 0', function () {
    assert.strictEqual(result, 0);
  });

});
