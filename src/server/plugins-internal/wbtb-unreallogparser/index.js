// tries to find an error an line in a log based on the assumption that an error's anatomy is
// file-path : some-text error some-code : some-explanation
// gmi : errors can be multiline
// ignore case

// [ ][A-Z]{1}[0-9]+[:] belows is the error/warning code that appears in all unreal logs. 
// - leading space
// - a single uppercase letter
// - multiple digts
// - trailing :

const errorRegex = /(.?)*( error)(.?)*[ ][A-Z]{1}[0-9]+[:](.?)*/gmi,
    warnRegex = /(.?)*( warning)(.?)*[ ][A-Z]{1}[0-9]+[:](.?)*/gmi,
    find = (text, regex) =>{
        const lookup = text.match(regex)
        return lookup ? lookup.pop() : null
    }

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
        errors = errors.filter(item => {
            if (!item.includes('Running UnrealHeaderTool'))
                return item
        })

        // get distinct items in list
        const distinct = {}
        for (const error of errors)
            if (!distinct[error.toLowerCase()])
                distinct[error.toLowerCase()] = error

        errors = []
        for (const key in distinct)
            errors.push({type: 'error', text : distinct[key]})

        return errors
    },


    /**
     * Parses an error/warning line from a log, tries to find file, line number and code. Always
     * returns an object, but with null properties of any of these values are not found.
     * 
     * @param {*} line 
     * @returns Object with line, lineNumber and error/warning code
     */
    parseLine(line){
        if (!line)
            return null
        
        let file = find(line, /^(.*?)\(.*\): /),
            lineNumber = find(line, /.*\((.*?)\): /),
            code = find(line, /(error|warning) C([\d]+): /)

        if (lineNumber)
            lineNumber = parseInt(lineNumber)

        if (code)
            code = `C${code}`

        return {
            file,
            lineNumber,
            code
        }
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

            const lineItem = {
                text : line,
                type : type
            }

            // text types are flooding, if you want the full log, read the raw text
            if (lineItem.type !== `text`)
                result.push(lineItem)
        }

        return result
    }
}