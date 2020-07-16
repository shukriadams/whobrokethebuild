const timebelt = require('timebelt')

module.exports = Handlebars => {

    Handlebars.registerHelper('shortTime', function(date){
        if (!date)
            return ''
        
        return timebelt.toShortTime(date, 'h:m')
    })
}