
var assert = require('assert');
const coreclrhosting = require('.././');

//global.assert = assert;

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
    }
  };
};

var result = coreclrhosting.runCoreApp(__dirname + '/bin/published', 'TestApp.dll');

describe('coreclrhosting', function () {
  it('should return 0', function () {    
    assert.strictEqual(result, 0)
  });
});
