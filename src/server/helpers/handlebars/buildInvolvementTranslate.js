const constants = require(_$+'types/constants')

module.exports = Handlebars => {

    Handlebars.registerHelper('buildInvolvementTranslate', function(involvement){

        switch(involvement){

            case constants.BUILDINVOLVEMENT_ASSISTING : {
                return 'Assisting'
            }

            case constants.BUILDINVOLVEMENT_SOURCECHANGE : {
                return 'Confirmed source change'
            }

            case constants.BUILDINVOLVEMENT_SUSPECTED_SOURCECHANGE : {
                return 'Likely involvement'
            }

            default : {
                return 'Unknown'
            }
        }

    })
    
}