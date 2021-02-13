const constants = require(_$+'types/constants')

module.exports = Handlebars => {

    Handlebars.registerHelper('deltaTranslate', function(status){

        switch(status){
            case constants.BUILDDELTA_PASS : {
                return ''
            }

            case constants.BUILDDELTA_FIX : {
                return 'fixed build'
            }

            case constants.BUILDDELTA_CAUSEBREAK : {
                return 'broke build'
            }

            case constants.BUILDDELTA_CONTINUEBREAK : {
                return 'already broken'
            }

            case constants.BUILDDELTA_CHANGEBREAK : {
                return 'changed break'
            }

            default : {
                return 'unknown'
            }
        }

    })
    
}