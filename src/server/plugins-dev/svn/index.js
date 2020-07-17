const 
    exec = require('madscience-node-exec'),
    svnhelper = require('madscience-svnhelper'),
    settings = require(_$+ 'helpers/settings'),
    path = require('path'),
    fs = require('fs-extra'),
    encryption = require(_$+'helpers/encryption')

module.exports = {

    getTypeCode(){
        return 'svn'
    },
    
    validateSettings: async () => {
        // check if svn is installed locally, we need this for talking to remote server
        try {
            await exec.sh({ cmd : `svn help`})
            return true
        } catch(ex){
            console.log(`svn check failed with ${ex}. Please install subversion locally`)
        }
    },

    // required by plugin interface
    getDescription(){
        return {
            id : 'svn',
            name : null
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
        return 'svn/partials/revision'
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
            rawRevisionText = (await exec.sh({ cmd : `svn log -r ${revision} ${vcServer.url} --username ${vcServer.username} --password ${password}`})).result
        }

        let parsedRevisions = svnhelper.parseSVNLog(rawRevisionText)

        // we've queried only one revision, so array should contain only one item
        return parsedRevisions.length ? parsedRevisions[0] : null
    }
}