const constants = require(_$+'types/constants')

module.exports = Handlebars => {

    Handlebars.registerHelper('statusTranslate', function(status){

        switch(status){
            case constants.BUILDSTATUS_FAILED : {
                return 'broken'
            }
            case constants.BUILDSTATUS_PASSED : {
                return 'passing'
            }
            default : {
                return 'building'
            }
        }

    })
    
}