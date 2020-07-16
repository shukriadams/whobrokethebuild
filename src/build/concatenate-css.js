const concat = require('./concat')

module.exports = async function conc(){
    return new Promise(async function(resolve, reject){
        try {
            console.log('concatenating CSS')
            
            await concat('./.tmp/css/*.css', './public/css/style.css')
            resolve()

        } catch(ex) {
            reject(ex)
        }
    })
}
