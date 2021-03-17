const strip = require('strip-pre-indent')

module.exports = Handlebars => {
    Handlebars.registerHelper('stripLeadingSpace', function(options){
        let markup = options.fn(this)
        console.log('>>>>', markup )
        markup = strip(options.fn(this))
        console.log('!!!!', markup )
        return new Handlebars.SafeString(markup)
    })
    
}