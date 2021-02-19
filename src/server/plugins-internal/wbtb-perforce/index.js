const perforcehelper = require('madscience-perforcehelper'),
    settings = require(_$+'helpers/settings'),
    Revision = require(_$+'types/revision'),
    RevisionFile = require(_$+'types/revisionFile'),
    path = require('path'),
    fs = require('fs-extra'),
    fsUtils = require('madscience-fsUtils'),
    encryption = require(_$+'helpers/encryption')

module.exports = {

    async validateSettings(){
        return true
    },

    async testCredentials(url, name, password){
        throw 'not implemented yet'
    },

    /**
     * required by interface
     */
    getRevisionPartialName(){
        return 'wbtb-perforce/partials/revision'
    },


    /**
     * Required by interface.
     */
    async parseRawRevision(rawRevisionText){
        return perforcehelper.parseDescribe(rawRevisionText, true)
    },


    /**
     * @param {object} vcServer
     * @param {number} revision
     */
    async getRevisionsBefore(vcServer, revision){
        // ensure number
        revision = parseInt(revision.toString())

        let cachedPath = path.join(settings.dataFolder, 'wbtb-perforce', 'cache', 'revisions.json'),
            allRevisions, 
            fetch = false

        if (await fs.exists(cachedPath)){
            allRevisions = await fs.readJson(cachedPath)
            if (!allRevisions.find(rev => rev.revision === revision))
                fetch = true
        } else
            fetch = true

        if (fetch){
            const allChanges = []

            if (settings.sandboxMode){
                const mockRevisionsPath = path.join(__dirname, 'mock/revisions')

                if (await fs.exists(mockRevisionsPath)){
                    const revisionFiles = await fsUtils.readFilesInDir(mockRevisionsPath)
                    for (let revisionFile of revisionFiles){
                        const content = await fs.readFile(revisionFile, 'utf8')
                        allChanges.push(perforcehelper.parseDescribe(content, false))
                    }
                } else {
                    __log.warn('No mock data detected, please generate')
                }

            } else {
                let password = await encryption.decrypt(vcServer.password),
                    allChanges = await perforcehelper.getChanges(vcServer.username, password, vcServer.url)

                allChanges = perforcehelper.parseChanges(allChanges)
            }

            allRevisions = {
                date : new Date(),
                revisions : allChanges
            }

            await fs.outputJson(cachedPath, allRevisions)
        }

        return allRevisions.filter(revision => revision.revision < revision)
    },


    /**
     * Required by interface.
     * Does p4 describe on a revision and returns an object with revision info
     * revision : string, revision id
     * vcServer : object, internal vcServer object
     */
    async getRevision(revision, vcServer){
        // assert vcServer.url
        // assert vcServer.password
        // assert vcServer.username
        let password = await encryption.decrypt(vcServer.password),
            rawDescribeText,
            cachedPath = path.join(settings.dataFolder, 'wbtb-perforce', 'cache'),
            itemPath = path.join(cachedPath, revision)

        // lookup cache
        await fs.ensureDir(cachedPath)
        if (await fs.exists(itemPath))
            return await fs.readJson(itemPath)
        
        if (settings.sandboxMode){
            let mockRevisionFile = path.join(__dirname, `/mock/revisions/${revision}`)
            // if the revision we're looking for isn't mocked, fall back to generic
            if (await fs.exists(mockRevisionFile))
                rawDescribeText = await fs.readFile(mockRevisionFile, 'utf8')
            else
                rawDescribeText = `Change 0000 by p4bob@wors-space on 2021/01/25 14:38:07\n\n\tDid some things to change some stuff.\n\nAffected files ...\n\n\t... //mydepot/mystream/path/to/file.txt#2 edit\n\nDifferences ...\n\n==== //mydepot/mystream/path/to/file.txt#2 (text) ====\n\n65c65,68\n<       henlo thar\n---\n>       this is way more serious\n>       please don't use meme text, thanks\n`
        } else {
            rawDescribeText = await perforcehelper.getDescribe(vcServer.username, password, vcServer.url, revision )
        }
        
        // prevent massive commits from flooding system
        if (this.__wbtb.maxCommitSize && rawDescribeText.length > this.__wbtb.maxCommitSize)
            rawDescribeText = rawDescribeText.substring(0, this.__wbtb.maxCommitSize)

        const revisionParsed = perforcehelper.parseDescribe(rawDescribeText, false),
            revisionFinal = new Revision()

        revisionFinal.user = revisionParsed.username
        revisionFinal.revision = revisionParsed.revision
        revisionFinal.date = revisionParsed.date
        revisionFinal.description = revisionParsed.description
        revisionFinal.files = []
        
        for (const revFile of revisionParsed.files){
            const revisionFile = new RevisionFile()
            revisionFile.file = revFile.file
            revisionFile.change = revFile.change
            revisionFinal.files.push(revisionFile)
        }

        await fs.outputJson(itemPath, revisionFinal, { spaces : 4 })
        return revisionFinal
    }
}