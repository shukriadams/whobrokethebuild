const fs = require('fs-extra'),
    ParsedBuildLog = require(_$+'types/parsedBuildLog')

module.exports = {

    async parseErrorsFromFileToString(logPath, logParserType, joint = '\n'){
        const lines = await this.parseErrorsFromFile(logPath, logParserType)
        return lines.join(joint)
    },

    /**
     * Returns array of strings, errors parsed from log path file
     * Parsers errors from expected log file. Gracefully handles file not existing
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