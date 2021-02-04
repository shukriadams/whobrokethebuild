module.exports = {

    /**
     * Returns a model containing a list of reference logs, or a meaningful error for why the list failed.
     * {
     *      error : string
     *      logs : array of string to log paths
     * }    
     */
    async list(){
        const settings = require(_$+'helpers/settings'),
            fs = require('fs-extra'),
            path = require('path'),
            fsUtils = require('madscience-fsUtils'),
            referenceLogsPath = path.join(settings.dataFolder, 'referenceLogs')

        if (!await fs.exists(referenceLogsPath))
            return { error : `reference log folder "${referenceLogsPath}" does not exist` }

        return {
            logs :  await fsUtils.getChildDirs(referenceLogsPath, false)
        }
    },


    /**
     * Returns a model for a specific reference log, or a meaningful error for why the log load failed
     * {
     *      error: string,
     *      log : string. the raw log
     *      revisions : array of strings containing revision text
     * }
     */
    async load(logId){
        const settings = require(_$+'helpers/settings'),
            path = require('path'),
            fs = require('fs-extra'),
            fsUtils = require('madscience-fsUtils'),
            referenceLogPath = path.join(settings.dataFolder, 'referenceLogs', logId),
            rawLogPath = path.join(settings.dataFolder, 'referenceLogs', logId, 'log'),
            revisionsPath = path.join(settings.dataFolder, 'referenceLogs', logId, 'revisions')

        if (!await fs.exists(referenceLogPath))
            return { error : `reference log folder "${referenceLogPath}" does not exist` }

        if (!await fs.exists(rawLogPath))
            return { error : `raw log file "${rawLogPath}" does not exist. Please add this` }

        if (!await fs.exists(revisionsPath))
            return { error : `revisions folder "${revisionsPath}" does not exist. Please add this` }

        
        const revisions = [],
            revisionPaths = await fsUtils.readFilesUnderDir(revisionsPath)

        for (const revisionPath of revisionPaths)
            revisions.push(await fs.readFile(revisionPath, 'utf8'))

        return {
            log : await fs.readFile(rawLogPath, 'utf8'),
            revisions
        }
    }
}