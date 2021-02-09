// tries to find an error an line in a log based on the assumption that an error's anatomy is
// file-path : some-text error some-code : some-explanation
// gmi : errors can be multiline
// ignore case
const errorRegex = /(.?)*:(.?)*( error )(.?)*:(.?)*/gmi

module.exports = {


    /**
     * 
     */
    async validateSettings(){
        return true
    },


    /**
     * Parses error lines out of build log. Returns array of strings
     */
    parseErrors(raw){
        if (!raw) 
            return []

        // force unix paths on log, this helps reduce noise when getting distinct lines
        let fullErrorLog = raw.replace(/\\/g,'/'),
            // use "error" word in log as marker for errors
            errors = fullErrorLog.match(errorRegex)

        if (!errors)
            return []

        // remove known noise line with "error" in it
        errors = errors.filter(function(item){
            if (!item.includes('Running UnrealHeaderTool'))
                return item
        })

        // get distinct items in list
        const distinct = {}
        for (let error of errors)
        if (!distinct[error.toLowerCase()])
            distinct[error.toLowerCase()] = error

        errors = []
        for (const key in distinct)
            errors.push(distinct[key])

        return errors
    },


    /**
     * Returns entire log as an array of lines objects. Lines are marked for errors or warnings.
     * 
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

            if (line.match(errorRegex))
                type = 'error'

            result.lines.push({
                text : line,
                type
            })
        }

        return result
    }
}