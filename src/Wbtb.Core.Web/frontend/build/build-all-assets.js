// cwd needs to be parent folder

const fs = require('fs-extra'),
    concatenateCss = require('./concatenate-css'),
    concatenateJs = require('./concatenate-js'),
    runner = require('node-sass-runner');

(async function(){
    await fs.ensureDir('./.tmp/css')

    try {

        await runner.renderAll({
            scss: './components/**/*.scss',
            css: './.tmp/css',
            sassOptions: {
                sourceComments: false
            }
        })

        await fs.copy('./components/icons/iconfont.css', './.tmp/css/icons/iconfont.css')

        await fs.ensureDir('./../wwwroot/fonts/')
        await fs.copy('./components/icons/iconfont.ttf', './../wwwroot/fonts/iconfont.ttf')
        await fs.copy('./components/icons/iconfont.woff', './../wwwroot/fonts/iconfont.woff')
        await fs.copy('./components/icons/iconfont.woff2', './../wwwroot/fonts/icons/iconfont.woff2')
        await fs.copy('./components/icons/iconfont.svg', './../wwwroot/fonts/iconfont.svg')
        await fs.copy('./components/icons/iconfont.eot', './../wwwroot/fonts/iconfont.eot')

        console.log('copied fonts')


        await concatenateCss()
        await concatenateJs()

        
    } catch(ex){
        console.log(`failed with ${ex}`)
    }
    
})()
