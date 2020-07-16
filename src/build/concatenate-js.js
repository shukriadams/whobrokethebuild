const concat = require('./concat')

module.exports = async function conc(){
    return new Promise(async function(resolve, reject){
        try {
            console.log('concatenating JS');

            await concat('./server/frontend/**/*.js', './public/scripts/scripts.js')

        } catch(ex) {
            reject(ex);
        }
    })
}
