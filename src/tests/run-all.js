let Mocha = require('mocha'),
    glob = require('glob'),
    tests = glob.sync('./tests/**/*.js'),
    mocha = new Mocha({});

for (let i = 0 ; i < tests.length ; i ++){
    // slice removes .js file extension, which mocha doesn't want
    mocha.addFile(tests[i].slice(0, -3));
}

mocha.run();
