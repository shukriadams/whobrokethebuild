

module.exports = {


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
    },


    /**
     * Returns log as an array of lines
     * {
     *      error : STRING. if not null, error for why log coudln't be parsed
     *      lines : [{
     *          text: STRING.line text
     *          type: STRING. error|warning|text
     *      }]
     * }
     */
    parse(raw){
        let result = {
            error: null,
            lines : []
        }

        if (!raw) {
            result.error = 'input is empty'
            return result
        }

        // force unix paths on log, this helps reduce noise when getting distinct lines
        // convert windows to unix line endings
        let fullErrorLog = raw
            .replace(/\\/g,'/')
            .replace(/\r\n/g,'\n')
            .split('\n')

        for (const line of fullErrorLog){
            let type = 'text'

            if (line.match(/warning/i))
                type = 'warning'

            if (line.match(/error/i))
                type = 'error'

            result.lines.push({
                text : line,
                type
            })
        }

        return result
    }
}