const constants = require(_$+'types/constants')

module.exports = Handlebars => {
    /**
     * Translates delta value into user-friendly text. Because delta is not always available but status
     * is always there, we use status as fallback value.
     */
    Handlebars.registerHelper('deltaTranslate', function(delta, status){

        let result = null
        switch(delta){
            case constants.BUILDDELTA_PASS : {
                result = 'Passing'
                break
            }

            case constants.BUILDDELTA_FIX : {
                result = 'Fixed build'
                break
            }

            case constants.BUILDDELTA_CAUSEBREAK : {
                result = 'Broke build'
                break
            }

            case constants.BUILDDELTA_CONTINUEBREAK : {
                result = 'Already broken'
                break
            }

            case constants.BUILDDELTA_CHANGEBREAK : {
                result = 'Changed break'
                break
            }
        }

        // back to status
        if (!result)
            switch(status){
                case constants.BUILDSTATUS_FAILED : {
                    result = 'Broken'
                    break
                }

                case constants.BUILDSTATUS_PASSED : {
                    result = 'Passing'
                    break
                }

                case constants.BUILDSTATUS_INPROGRESS : {
                    result = 'Building'
                    break
                }
            }

        // this is unusual
        if (!result){
            __log.info(`Unhandled delta : "${delta}" and status:`)
            result = 'Unknown'
        }

        return result
    })
    
}