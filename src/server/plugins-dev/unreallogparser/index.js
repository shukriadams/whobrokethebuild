

module.exports = {
    
    /**
     * Unique CI server identifier associated with this plugin. Cannot be used by another ci plugin 
     */
    getTypeCode(){
        // must be same name as package in package.json
        return 'unreallogparser'
    },


    /**
     * required by plugin interface
     */
    getDescription(){
        return {
            id : 'unreallogparser'
        }
    },


    /**
     * 
     */
    async validateSettings(){
        return true
    },


    /**
     * Parses error out of build log
     */
    parseErrors(raw){
        if (!raw) 
            return ''

        // force unix paths on log, this helps reduce noise when getting distinct lines
        const fullErrorLog = raw.replace(/\\/g,'/')

        // use "error" word in log as marker for errors
        let errors = fullErrorLog.match(/^.*\berror\b.*$/gmi)

        // give up, cant' parse log, use whole thing
        if (!errors || !errors.length)
            errors = [fullErrorLog]

        // remove known noise line with "error" in it
        errors = errors.filter(function(item){
            if (!item.includes('Running UnrealHeaderTool'))
                return item
        })

        // get distinct items in list - thi
        const distinct = {}
        for (let error of errors)
        if (!distinct[error.toLowerCase()])
            distinct[error.toLowerCase()] = error

        errors = []
        for (const key in distinct)
            errors.push(distinct[key])

        return errors && errors.length ? errors.join('\n\n') : 'Non-standard error, unable to parse.'
    }
}