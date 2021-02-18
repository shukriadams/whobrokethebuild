const fs = require('fs-extra'),
    ParsedBuildLog = require(_$+'types/parsedBuildLog')

module.exports = {

    
    /**
     * @param {string} logPath relative path of log file within local log dump
     * @param {string} logParserType plugin name for log parser
     * @param {string} joint Join for error lines. Normally a unix line return
     * 
     * Attempts to read a log file at the given path and parse out only errors using the given parser. 
     * Errors are returned as a string.
     * 
     * Log file need not exist.
     */ 
    async parseErrorsFromFileToString(logPath, logParserType, joint = '\n'){
        const lines = await this.parseErrorsFromFile(logPath, logParserType)
        return lines.join(joint)
    },


    /**
     * @param {string} logPath relative path of log file within local log dump
     * @param {string} logParserType plugin name for log parser
     *
     * Returns array of strings, errors parsed from log path file
     * Parsers errors from expected log file. Gracefully handles file not existing
     * 
     * Log file need not exist.
     */
    async parseErrorsFromFile(logPath, logParserType){
        let pluginsManager = require(_$+'helpers/pluginsManager'),
            settings = require(_$+ 'helpers/settings'),
            path = require('path'),
            logParser = await pluginsManager.get(logParserType),
            rawLog = null,
            rawLogPath = path.join(settings.buildLogsDump, logPath)

        if (! await fs.exists(rawLogPath))
            return ['Log file does not exist']

        try {
            rawLog = await fs.readFile(rawLogPath, 'utf8')
        }catch(ex){
            __log.error(`unexpected error parseErrorsFromFile, file "${logPath}"`, ex)
            return
        }

        return logParser.parseErrors(rawLog)
    },


    /**
     * @param {string} logPath relative path of log file within local log dump
     * @param {string} logParserType plugin name for log parser
     * 
     * returns parsedLog object
     */
    async parseFromFile(logPath, logParserType){
        let pluginsManager = require(_$+'helpers/pluginsManager'),
            settings = require(_$+ 'helpers/settings'),
            path = require('path'),
            logParser = await pluginsManager.get(logParserType),
            rawLog = null,
            rawLogPath = path.join(settings.buildLogsDump, logPath)

        if (! await fs.exists(rawLogPath)){
            const out = new ParsedBuildLog()
            out.error = 'Log file does not exist'
            return out
        }

        try {
            rawLog = await fs.readFile(rawLogPath, 'utf8')
        }catch(ex){
            __log.error(`unexpected error parseErrorsFromFile, file "${logPath}"`, ex)
            return
        }

        return logParser.parse(rawLog)
    }
}