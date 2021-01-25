module.exports = {

    validateSettings: async () => {
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
            rawRevisionText = (await exec.sh({ cmd : `echo ${password} | p4 login -u ${vcServer.username} && p4 describe ${revision}`})).result
        }

        let parsedRevisions = svnhelper.parseSVNLog(rawRevisionText)

        // we've queried only one revision, so array should contain only one item
        return parsedRevisions.length ? parsedRevisions[0] : null
    }
}