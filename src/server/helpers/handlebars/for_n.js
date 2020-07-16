/**
 * for n loop (for int = 0; int < n ; int ++)
 */
module.exports = Handlebars => {

    Handlebars.registerHelper('for_n', function(n, block){
        let out = ''

        for (let i = 0 ; i < n ; i ++)
            out += block.fn(i)
        
        return out
    })
}