const fs = require('fs-extra')
const { getBuildsWithoutRevisionObjects } = require('../plugins-internal/wbtb-mongo')

module.exports = {

    
    /**
     * @param {object} build build of which log should be read
     * @param {string} logParserType plugin name for log parser
     * @param {string} joint Join for error lines. Normally a unix line return
     * 
     * Attempts to read a log file at the given path and parse out only errors using the given parser. 
     * Errors are returned as a string.
     * 
     * Log file need not exist.
     */ 
    async parseErrorsFromBuildLogToString(build, logParserType, joint = '\n'){
        let lines = await this.parseErrorsFromBuildLog(build, logParserType)
        return lines.join(joint)
    },


    /**
     * @param {object} build build of which log should be read
     * @param {string} logParserType plugin name for log parser
     *
     * Returns array of strings, errors parsed from log path file
     * Parsers errors from expected log file. Gracefully handles file not existing
     * 
     * Log file need not exist.
     */
    async parseErrorsFromBuildLog(build, logParserType){
        const log = await this._parseBuildLog(build, logParserType)
        return log.filter(r => r.type === 'error').map(r => r.text)
    },


    /**
     * @param {object} build relative path of log file within local log dump
     * @param {string} logParserType plugin name for log parser
     * 
     * returns parsedLog object
     */
    async parseFromBuild(build, logParserType){
        return this._parseBuildLog(build, logParserType)
    },


    /**
     * Reads and returns a section of a textfile. Returns empty string when the file is overrun.
     */
    async readTextFileChunk(path, chunkIndex = 0, chunkSize = 1024){
        return new Promise((resolve, reject)=>{
            try {
                let fs = require('fs'),
                    data = '',
                    readStream = fs.createReadStream(path,{ highWaterMark: chunkIndex * chunkSize, encoding: 'utf8'})
                
                readStream.on('data', function(chunk) {
                    data += chunk;
                }).on('end', function() {
                    resolve(data) 
                })

            } catch(ex){
                reject(ex)
            }
        })        
    },

    async stepThroughFile(path, onLine){
        const fs = require('fs-extra')
        if (!fs.pathExists(path))
            throw `File ${path} does not exist`

        return new Promise((resolve, reject)=>{
            try {
                const lineReader = require('readline').createInterface({
                    input: require('fs').createReadStream(path)
                })
                  
                lineReader.on('line', line => {
                    onLine(line)
                })

                lineReader.on('close', () =>{
                    resolve()
                })

              } catch(ex){
                reject(ex)
            }
        })
    },

    /**
     * @param {object} build relative path of log file within local log dump
     * @param {string} logParserType plugin name for log parser
     * @param {string} parseMethod Function on logParser to run -  parse|parseErrors
     * 
     * @returns {Promise<Array<object>>} array of log line objects { text : string, type: string error|warning|text }
     */
    async _parseBuildLog(build, logParserType){
        let settings = require(_$+ 'helpers/settings'),
            path = require('path'),
            logPath = path.join(build.jobId, build.build.toString()),
            rawLogPath = path.join(settings.buildLogsDump, logPath)

        const parsedItems = this.parseLog(rawLogPath, logParserType)

        return parsedItems        
    },
    

    /**
     * @param {object} build relative path of log file within local log dump
     * @param {string} logParserType plugin name for log parser
     * @returns {Promise<Array<object>>} array of log line objects { text : string, type: string error|warning }
     */
    async parseLog(logPath, logParserType){
        __log.debug(`starting log parse ${logPath}`)
        let pluginsManager = require(_$+'helpers/pluginsManager'),
            logParser = await pluginsManager.get(logParserType),
            cachedLogPath = `${logPath}.cache`,
            parsedItems = []

        if (await fs.pathExists(cachedLogPath))
            return fs.readJson(cachedLogPath)

        if (!await fs.pathExists(logPath))
            return [{ text : 'Log file does not exist', type : 'error' }]

        await this.stepThroughFile(logPath, logLine =>{
            // ignore empty lines, parser will return "empty" warnigs for these
            if (!logLine.length)
                return

            const parsed = logParser.parse(logLine)
            if (parsed.length)
                parsedItems = parsedItems.concat(parsed)
        })

        await fs.outputJson(cachedLogPath, parsedItems)

        __log.debug(`ending log parse ${logPath}`)
        return parsedItems
    }
}