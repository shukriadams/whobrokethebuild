const fs = require('fs-extra'),
    { Worker } = require('worker_threads')

module.exports = {

    
    /**
     * Reads a raw log for the given build. This can be extremely system-taxing, use with care.
     * 
     * @param {import('../types/build').Build} build Build to load log for.
     */
    async readRawLogForBuild(build){
        const settings = require(_$+ 'helpers/settings'),
            path = require('path'),
            logPath = path.join(build.jobId, build.build.toString()),
            rawLogPath = path.join(settings.buildLogsDump, logPath)

        if (!await fs.exists(rawLogPath))
            return 'Log for this build does not exist.'

        if (fs.statSync(rawLogPath).size > settings.maxReadableRawLogSize)
            return `File is too large to read`

        return await fs.readFile(rawLogPath, 'utf8')
    },


    /**
     * @param {object} build relative path of log file within local log dump
     * @param {string} logParserType plugin name for log parser
     * 
     * returns parsedLog object
     */
    async parseFromBuild(build, logParserType){
        const settings = require(_$+ 'helpers/settings'),
            path = require('path'),
            logPath = path.join(build.jobId, build.build.toString()),
            rawLogPath = path.join(settings.buildLogsDump, logPath)

        return await this.parseLog(rawLogPath, logParserType)
    },

    
    /**
     * Parses a build log - work is offloaded to a worker thread, as large logs can take many minutes to process, and will completely
     * block the main server thread.
     * @param {object} build relative path of log file within local log dump
     * @param {string} logParserType plugin name for log parser
     * @returns {Promise<Array<object>>} array of log line objects { text : string, type: string error|warning }
     */
    async parseLog(logPath, logParserType){
        return new Promise(async (resolve, reject)=>{
            try {
                const cachedLogPath = `${logPath}.cache`

                if (await fs.pathExists(cachedLogPath)){
                    return resolve( fs.readJson(cachedLogPath))
                }

                if (!await fs.pathExists(logPath)){
                    __log.warn(`Attempting to read log that does not exist : "${logPath}"`)
                    return resolve([{ text : 'Log file does not exist', type : 'error' }])
                }

                __log.debug(`Log parse start for ${logPath}`)
                
                const processStart = new Date(),
                    pluginsManager = require(_$+'helpers/pluginsManager'),
                    logParser = await pluginsManager.get(logParserType),
                    worker = new Worker(_$+'workers/buildLogProcess.js', {
                        workerData : {
                            logPath, 
                            logParserRequirePath : logParser.__wbtb.requirePath
                        }
                    })

                worker.on('message', async function(parsedItems){
                    await fs.outputJson(cachedLogPath, parsedItems)
                    __log.debug(`Log parse ended for ${logPath}, took ${Math.floor((new Date().getTime() - processStart.getTime()) / (1000 * 60))} minutes. `)
                    resolve(parsedItems)
                })

                worker.on('error', reject)
                worker.on('exit', code => {
                    if (code !== 0)
                        reject(`Worker "${worker.constructor.name}" exited unexpectedly with code ${code}`)
                })

            }catch(ex){
                reject(ex)
            }
        })
    }
}
