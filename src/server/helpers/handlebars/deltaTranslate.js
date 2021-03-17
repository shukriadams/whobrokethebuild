const constants = require(_$+'types/constants')

module.exports = Handlebars => {

    Handlebars.registerHelper('deltaTranslate', function(delta){

        switch(delta){
            case constants.BUILDDELTA_PASS : {
                return 'Passing'
            }

            case constants.BUILDDELTA_FIX : {
                return 'Fixed build'
            }

            case constants.BUILDDELTA_CAUSEBREAK : {
                return 'Broke build'
            }

            case constants.BUILDDELTA_CONTINUEBREAK : {
                return 'Already broken'
            }

            case constants.BUILDDELTA_CHANGEBREAK : {
                return 'Changed break'
            }

            default : {
                return `Unknown delta ${delta}`
            }
        }

    })
    
}