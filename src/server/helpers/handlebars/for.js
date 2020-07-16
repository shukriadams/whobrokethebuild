/**
 * for each child object in object
 */
module.exports = Handlebars => {

    Handlebars.registerHelper('for', function(obj, block){
        let out = ''

        for (const prop in obj){
            console.log('>>', obj[prop])
            out += block.fn(obj[prop])
        }
        
        return out
    })
}