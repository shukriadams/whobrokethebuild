module.exports = Handlebars => {

    Handlebars.registerHelper('markSelected', function(selectedValue, currentValue){
        return selectedValue === currentValue ? 'selected' : ''
    })
    
}