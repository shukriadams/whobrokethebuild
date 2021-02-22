const fs = require('fs-extra'),
    { Worker } = require('worker_threads')

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
                  
                lineReader.on('line', (line) => {
                    // send line to callback, along with callback of our own to resume next line
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
    
    
    async _parseLogBackground(logPath, logParserRequirePath){
        return new Promise((resolve, reject)=>{
            try {
                const worker = new Worker(_$+'workers/buildLogProcess.js', {
                    workerData : {
                        logPath, logParserRequirePath
                    }
                })
                worker.on('message', resolve)
                worker.on('error', reject)
                worker.on('exit', (code) => {
                    if (code !== 0)
                        reject(`Worker stopped with exit code ${code}`)
                    resolve()
                })            
            } catch (ex){
                reject(ex)
            }

        })
    },

    
    /**
     * @param {object} build relative path of log file within local log dump
     * @param {string} logParserType plugin name for log parser
     * @returns {Promise<Array<object>>} array of log line objects { text : string, type: string error|warning }
     */
    async parseLog(logPath, logParserType){

        const cachedLogPath = `${logPath}.cache`

        if (await fs.pathExists(cachedLogPath))
            return fs.readJson(cachedLogPath)

        if (!await fs.pathExists(logPath))
            return [{ text : 'Log file does not exist', type : 'error' }]
            
        const pluginsManager = require(_$+'helpers/pluginsManager'),
            logParser = await pluginsManager.get(logParserType),
            parsedItems = await this._parseLogBackground(logPath, logParser.__wbtb.requirePath )

        await fs.outputJson(cachedLogPath, parsedItems)

        return parsedItems
    }
}