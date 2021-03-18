module.exports = {

    /**
     * Returns a model containing a list of reference logs, or a meaningful error for why the list failed.
     * {
     *      error : string
     *      logs : array of string to log paths
     * }    
     */
    async page(index = 0){
        const settings = require(_$+'helpers/settings'),
            fs = require('fs-extra'),
            fsUtils = require('madscience-fsUtils')

        if (!settings.buildLogsDump)
            return {error : `settings.buildLogsDump not set`}

        if (!await fs.pathExists(settings.buildLogsDump))
            return { error : `log dump folder "${settings.buildLogsDump}" does not exist` }
        
        let items = await fsUtils.readFilesUnderDir(settings.buildLogsDump),
            totalItems = items.length,
            pages = Math.floor(items.length / settings.standardPageSize)

        if (items.length % settings.standardPageSize)
            pages ++

        items = items.slice(index * settings.standardPageSize, (index * settings.standardPageSize) + settings.standardPageSize)
        return { items, pages,index, totalItems }
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
            referenceLogPath = path.join(settings.buildLogsDump, logId),
            rawLogPath = path.join(settings.buildLogsDump, logId, 'log'),
            revisionsPath = path.join(settings.buildLogsDump, logId, 'revisions')

        if (!await fs.pathExists(referenceLogPath))
            return { error : `reference log folder "${referenceLogPath}" does not exist` }

        if (!await fs.pathExists(rawLogPath))
            return { error : `raw log file "${rawLogPath}" does not exist. Please add this` }

        if (!await fs.pathExists(revisionsPath))
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