const constants = require(_$+'types/constants')

module.exports = Handlebars => {

    Handlebars.registerHelper('deltaTranslate', function(status){

        switch(status){
            case constants.BUILDDELTA_PASS : {
                return ''
            }

            case constants.BUILDDELTA_FIX : {
                return 'fixed the build'
            }

            case constants.BUILDDELTA_CAUSEBREAK : {
                return 'broke the build'
            }

            case constants.BUILDDELTA_CONTINUEBREAK : {
                return 'build already broken'
            }

            case constants.BUILDDELTA_CHANGEBREAK : {
                return 'made a broken build worse'
            }

            default : {
                return 'unknown'
            }
        }

    })
    
}