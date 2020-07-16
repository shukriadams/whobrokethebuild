module.exports = Handlebars => {

    Handlebars.registerHelper('sum', function(value1, value2, options){
        return parseInt(value1) + parseInt(value2)
    })
    
}