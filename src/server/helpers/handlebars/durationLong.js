const timebelt = require('timebelt')

module.exports = Handlebars => {

    Handlebars.registerHelper('durationLong', function(end, start){
        if (!end || !start)
            return ''
        
        return `${timebelt.timespanString(end, start, ' days', ' hours', ' minutes', ' less than a minute')}`
    })
}