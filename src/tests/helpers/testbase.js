/**
 * Simple scaffold to run mocha tests on an express server instance.
 */
let path = require('path');
// bind globals for easy module importing
// I have no idea how this is getting loaded before the files which depend on it.
// This is likely a circular dependency error waiting to happen!
global._$ = path.resolve(`${__dirname}/../../server`) + '/';
global._$t = path.resolve(`${__dirname}/../`) + '/';

module.exports = function(testName, tests){
    describe(testName, function() {
        this.timeout(5000);

        // run tests importing this file. Pass variables to test as needed
        tests({

        });

        beforeEach(function(done) {
            (async ()=>{
                // set shortcut global for easier module imports relative to server root directory
                done();
            })();
        });

        afterEach(function(done){
            (async ()=>{
                done();
            })();
        });
    });
};