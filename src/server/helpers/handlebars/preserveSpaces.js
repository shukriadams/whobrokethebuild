module.exports = Handlebars => {

    Handlebars.registerHelper('preserveSpaces', function(content){
    
        if (!content || typeof content !== 'string')
            return content
    
        return content.replace(/\n/g, '<br />').replace(/\r\n/g, '<br />')
    })
    
}