const fs = require('fs-extra'),
    concatenateCss = require('./concatenate-css'),
    concatenateJs = require('./concatenate-js'),
    runner = require('node-sass-runner');

(async function(){
    await fs.ensureDir('./.tmp/css');

    try {

        await runner({
            scssPath : './client/app/**/*.scss',
            cssOutFolder : './.tmp/css'
        });

        await runner({
            scssPath : './server/frontend/**/*.scss',
            cssOutFolder : './.tmp/css'
        });

        await concatenateCss();
        await concatenateJs();
    } catch(ex){
        console.log(`failed with ${ex}`);
    }
})()
