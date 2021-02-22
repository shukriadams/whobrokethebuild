// tries to find an error an line in a log based on the assumption that an error's anatomy is
// file-path : some-text error some-code : some-explanation
// gmi : errors can be multiline
// ignore case

// [ ][A-Z]{1}[0-9]+[:] belows is the error/warning code that appears in all unreal logs. 
// - leading space
// - a single uppercase letter
// - multiple digts
// - trailing :

const ParsedErrorLogItem = require(_$+'types/parsedBuildLogLine'),
    errorRegex = /(.?)*( error)(.?)*[ ][A-Z]{1}[0-9]+[:](.?)*/gmi,
    warnRegex = /(.?)*( warning)(.?)*[ ][A-Z]{1}[0-9]+[:](.?)*/gmi

module.exports = {


    /**
     * 
     */
    async validateSettings(){
        return true
    },


    /**
     * Returns array of lines objects for errors only
     * 
     * {
     *      lines : [{
     *          text: STRING.line text
     *          type: STRING. error|warning|text
     *      }]
     * }     * 
     * 
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
            errors.push({type: 'error', text : distinct[key]})

        return errors
    },


    /**
     * Returns array of lines objects. Lines are marked for errors or warnings.
     * 
     * {
     *      lines : [{
     *          text: STRING.line text
     *          type: STRING. error|warning|text
     *      }]
     * }
     */
    parse(raw){
        if (!raw) 
            return [{ text : 'Log parse error : input is empty', type : 'error' }]
        

        // force unix paths on log, this helps reduce noise when getting distinct lines
        // convert windows to unix line endings
        let result = [],
            fullErrorLog = raw
                .replace(/\\/g,'/')
                .replace(/\r\n/g,'\n')
                .split('\n')

        for (const line of fullErrorLog){
            let type = 'text'

            if (line.match(errorRegex))
                type = 'error'
            else if (line.match(warnRegex))
                type = 'warning'
                
            //if (line.includes('error'))
                console.log(line)

            const lineItem = new ParsedErrorLogItem()
            lineItem.text = line
            lineItem.type = type

            // text types are flooding, if you want the full log, read the raw text
            if (lineItem.type !== `text`)
                result.push(lineItem)
        }

        return result
    }
}