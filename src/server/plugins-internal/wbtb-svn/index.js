const exec = require('madscience-node-exec'),
    svnhelper = require('madscience-svnhelper'),
    settings = require(_$+ 'helpers/settings'),
    Revision = require(_$+'types/revision'),
    RevisionFile = require(_$+'types/revisionFile'),
    path = require('path'),
    fs = require('fs-extra'),
    encryption = require(_$+'helpers/encryption')

module.exports = {
    
    validateSettings: async () => {
        // check if svn is installed locally, we need this for talking to remote server
        try {
            await exec.sh({ cmd : `svn help`})
            return true
        } catch(ex){
            __log.error(`svn check failed with ${ex}. Please install subversion locally`)
        }
    },


    async testCredentials(url, name, password){
        throw 'not implemented yet'
    },

    /**
     * Required by interface.
     * Gets name of local partial view that will render revision on generic pages
     */
    getRevisionPartialName(){
        return 'wbtb-svn/partials/revision'
    },


    /**
     * Required by interface.
     */
    async parseRawRevision(rawRevisionText){
        return svnhelper.parseSVNLog(rawRevisionText)
    },

    /**
     * Required by interface.
     * Does SVN log on a revision and returns an object with revision info
     * revision : string, revision id
     * vcServer : object, internal vcServer object
     */
    async getRevision(revision, vcServer){
        // assert vcServer.url
        // assert vcServer.password
        // assert vcServer.username
        let password = await encryption.decrypt(vcServer.password),
            rawRevisionText

        if (settings.sandboxMode){
            let mockRevisionFile = path.join(__dirname, `/mock/revisions/${revision}`)
            // if the revision we're looking for isn't mocked, fall back to generic
            if (!await fs.exists(mockRevisionFile))
                mockRevisionFile = path.join(__dirname, `/mock/revisions/generic`)

            rawRevisionText = await fs.readFile(mockRevisionFile, 'utf8')
        } else {
            rawRevisionText = (await exec.sh({ cmd : `svn log --verbose -r ${revision} ${vcServer.url} --username ${vcServer.username} --password ${password}`})).result
        }

        let parsedRevisions = svnhelper.parseSVNLog(rawRevisionText)
        if (!parsedRevisions.length)
            return null
        
        const revisionFinal = new Revision()

        // we've queried only one revision, so array should contain only one item
        revisionFinal.user = parsedRevisions[0].user
        revisionFinal.revision = parsedRevisions[0].revision
        revisionFinal.date = parsedRevisions[0].date
        revisionFinal.description = parsedRevisions[0].description
        revisionFinal.files = []
        
        for (const revFile of parsedRevisions[0].files){
            const revisionFile = new RevisionFile()
            revisionFile.file = revFile.file
            revisionFile.change = revFile.change
            revisionFinal.files.push(revisionFile)
        }

        return revisionFinal
    }
}