const 
    ago = require('s-ago')

module.exports = Handlebars => {

    Handlebars.registerHelper('ago', function(date){
        if (!date)
            return ''
        
        return ago(new Date(date))
    })
}