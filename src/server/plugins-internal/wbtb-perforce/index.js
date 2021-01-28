const perforcehelper = require('madscience-perforcehelper'),
    settings = require(_$+ 'helpers/settings'),
    path = require('path'),
    fs = require('fs-extra'),
    encryption = require(_$+'helpers/encryption')

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
     * Does p4 describe on a revision and returns an object with revision info
     * revision : string, revision id
     * vcServer : object, internal vcServer object
     */
    async getRevision(revision, vcServer){
        // assert vcServer.url
        // assert vcServer.password
        // assert vcServer.username
        let password = await encryption.decrypt(vcServer.password),
            rawDescribeText

        if (settings.sandboxMode){
            let mockRevisionFile = path.join(__dirname, `/mock/revisions/${revision}`)
            // if the revision we're looking for isn't mocked, fall back to generic
            if (!await fs.exists(mockRevisionFile))
                mockRevisionFile = path.join(__dirname, `/mock/revisions/generic`)

            rawDescribeText = await fs.readFile(mockRevisionFile, 'utf8')
        } else {
            rawDescribeText = await perforcehelper.getDescribe(vcServer.username, password, vcServer.url, revision )
        }

        let revisionParsed = perforcehelper.parseDescribe(rawDescribeText, false)
    
        // remap values, we expect user on revision, but this lib uses username
        revisionParsed.user = revisionParsed.username

        return revisionParsed
    }
}