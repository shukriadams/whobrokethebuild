module.exports = Handlebars => {

    Handlebars.registerHelper('renderPartialByName', function(partialName, context){
        if (!partialName || ! context)
            return console.log('renderPartialByName received invalid partial name or context')
    
        let fn,
            template = Handlebars.partials[partialName]
    
        if (!template){
            template = Handlebars.partials['templateNotFound']
            context = {
                name : partialName
            }
        }
    
        if (typeof template === 'function')
            fn = template
        else
            fn = Handlebars.compile(template)
    
        return fn(context).replace(/^\s+/, '')
    })
    
}

