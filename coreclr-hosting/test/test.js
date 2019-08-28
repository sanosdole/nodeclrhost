var assert = require('assert');

global.assert = require('assert');

const coreclrhosting = require('.././');

var result = coreclrhosting.runCoreApp(__dirname + '/TestApp/bin/published', 'TestApp.dll');
console.log('pid', process.pid);
global.describe('coreclrhosting', function() {

    console.log('pid desc0', process.pid);
    global.it('should return 0', function() {
        console.log('pid it0', process.pid);
        global.assert.equal(result, 0)
    });

    describe(global.test1.description, function() {
        console.log('pid desc', process.pid);
        it(global.test1.it1.description, function() {
            console.log('pid it', process.pid);
            console.log(global.test1.it1);
            global.test1.it1.doIt();

        });

    });
});

/*
describe('Array', function() {
    
  describe('#indexOf()', function() {
    it('should return -1 when the value is not present', function() {
      assert.equal([1, 2, 3].indexOf(4), -1);
    });
  });
});*/