const constants = require(_$+'types/constants')

module.exports = Handlebars => {

    Handlebars.registerHelper('statusAndDeltaTranslate', function(status, delta){

        if (status)
        switch(status){
            case constants.BUILDSTATUS_FAILED : {
                switch(delta){
                    case constants.BUILDDELTA_CONTINUEBREAK : {
                        return 'Broken by earlier build'
                    }
                    case constants.BUILDDELTA_CHANGEBREAK : {
                        return 'Broken but changed break'
                    }
                    default : {
                        return 'Broke the build'
                    }
                }
            }
            case constants.BUILDSTATUS_PASSED : {
                switch(delta){
                    case constants.BUILDDELTA_FIX : {
                        return 'Passing, fixed the build'
                    }
                    default : {
                        return 'Passing'
                    }
                }                
            }
            default : {
                return 'Still building'
            }
        }

    })
    
}