module.exports = Handlebars => {

    Handlebars.registerHelper('eq', function(value1, value2, options){
        if (value1 === value2)
            return options.fn(this)
    
        return options.inverse(this)
    })
}