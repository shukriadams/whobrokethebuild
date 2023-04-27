const concat = require('./concat')

module.exports = async function conc(){
    return new Promise(async function(resolve, reject){
        try {
            console.log('concatenating JS');
            // './components/**/*.js'
            // @microsoft/dist/browser/signalr.js
            await concat(['./node_modules/@microsoft/signalr/dist/browser/signalr.js', './components/**/*.js'], './../wwwroot/js/bundle.js')

        } catch(ex) {
            reject(ex)
        }
    })
}
