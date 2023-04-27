const concat = require('./concat')

module.exports = async function conc(){
    return new Promise(async function(resolve, reject){
        try {
            console.log('concatenating CSS')
            
            await concat([
                './.tmp/css/**/*.css',
                '!./.tmp/css/**/bootstrip*',
                './.tmp/css/components/bootstrip/bootstrip.css',
                './.tmp/css/components/bootstrip/bootstrip-theme-default.css',
                './.tmp/css/components/bootstrip/bootstrip-theme-darkmoon.css',
                './.tmp/css/components/bootstrip-overrides/bootstrip-overrides.css'
                ], './../wwwroot/css/bundle.css')

            resolve()

        } catch(ex) {
            reject(ex)
        }
    })
}
