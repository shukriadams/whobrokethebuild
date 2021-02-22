const fs = require('fs-extra')

module.exports = {


    /**
     * @param {object} build relative path of log file within local log dump
     * @param {string} logParserType plugin name for log parser
     * 
     * returns parsedLog object
     */
    async parseFromBuild(build, logParserType){
        return await this._parseBuildLog(build, logParserType)
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

    
    /**
     * Walks through a  file one line at a time
     */
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
                    onLine(line, ()=>{
                        lineReader.resume()
                    })
                    lineReader.pause()
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
        const settings = require(_$+ 'helpers/settings'),
            path = require('path'),
            logPath = path.join(build.jobId, build.build.toString()),
            rawLogPath = path.join(settings.buildLogsDump, logPath)

        return await this.parseLog(rawLogPath, logParserType)
    },
    

    /**
     * @param {object} build relative path of log file within local log dump
     * @param {string} logParserType plugin name for log parser
     * @returns {Promise<Array<object>>} array of log line objects { text : string, type: string error|warning }
     */
    async parseLog(logPath, logParserType){

        let pluginsManager = require(_$+'helpers/pluginsManager'),
            logParser = await pluginsManager.get(logParserType),
            cachedLogPath = `${logPath}.cache`,
            parsedItems = []

        if (await fs.pathExists(cachedLogPath))
            return fs.readJson(cachedLogPath)

        if (!await fs.pathExists(logPath))
            return [{ text : 'Log file does not exist', type : 'error' }]

        __log.debug(`log parse start : ${logPath}`)
        await this.stepThroughFile(logPath, (logLine, next) =>{
            setImmediate(()=>{
               // ignore empty lines, parser will return "empty" warnigs for these
               if (!logLine.length)
                   return

               const parsed = logParser.parse(logLine)
               if (parsed.length)
                   parsedItems = parsedItems.concat(parsed)
                
                next()
            })
        })
        __log.debug(`log parse end : ${logPath}`)

        await fs.outputJson(cachedLogPath, parsedItems)

        return parsedItems
    }
}