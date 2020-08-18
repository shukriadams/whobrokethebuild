module.exports = Handlebars => {

    Handlebars.registerHelper('gistOf', function(text, maxLength = 20, shortener = '...', options){
        if (!text || text.length < maxLength)
            return text

        return `${text.substring(0, maxLength)}${shortener}`
    })
}