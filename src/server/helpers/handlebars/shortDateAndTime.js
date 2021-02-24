const timebelt = require('timebelt')

module.exports = Handlebars => {

    Handlebars.registerHelper('shortDateAndTime', function(date){
        if (!date)
            return ''
        
        return `${timebelt.toShortDate(date)} ${timebelt.toShortTime(date, 'h:m')}` 
    })
}