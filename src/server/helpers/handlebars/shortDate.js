const timebelt = require('timebelt')

module.exports = Handlebars => {

    Handlebars.registerHelper('shortDate', function(date){
        if (!date)
            return ''
        
        return timebelt.toShortDate(date)
    })
}