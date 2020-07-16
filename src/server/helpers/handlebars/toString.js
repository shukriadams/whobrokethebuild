module.exports = Handlebars => {

    Handlebars.registerHelper('toString', function(obj){
        return JSON.stringify(obj)
    })
    
}