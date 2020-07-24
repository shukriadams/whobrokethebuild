let 
    pluginsManager = require(_$+'helpers/pluginsManager'),
    Build = require(_$+ 'types/build')

module.exports = {

    /**
     * Gets a build for display purposes, throws exception if not found.
     */
    async get(id){
        const data = await pluginsManager.getExclusive('dataProvider'),
            build = await data.getBuild(id, { expected : true })
            
        build.__job = await data.getJob(build.jobId, { expected : true })
        const vcServer = await data.getVCServer(build.__job.VCServerId, { expected : true }),
            vcPlugin = await pluginsManager.get(vcServer.vcs)

        // parses the build log if a log is set at job level
        if (build.__job.logParser){
            // todo : get should internally log error if plugin not found
            const logParserPlugin = await pluginsManager.get(build.__job.logParser)
            if (logParserPlugin)
                build.__parsedLog = logParserPlugin.parseErrors(build.log)
        }

        // convert revisions array into objects of revisions, mapped to VC-specific partial that can render them
        build.__revisions = []
        for (const revisionId of build.revisions){
            let revisionObject = await vcPlugin.getRevision(revisionId, vcServer)
            // if we can't retrieve an object describing revision, we need to make a placeholder so the view
            // can render something
            if (!revisionObject)
                revisionObject = {
                    description: '--!revision not found!-',
                    revision : revisionId
                }
            revisionObject.__viewName = vcPlugin.getRevisionPartialName()
            build.__revisions.push(revisionObject)
        }

        return build
    },

    async page (jobId, index, pageSize){
        const data = await pluginsManager.getExclusive('dataProvider')
        await data.pageBuilds(jobId, index, pageSize)
    },

    async remove (buildId){
        const data = await pluginsManager.getExclusive('dataProvider')
        await data.removeBuild(buildId)
    },

    async update  (build){
        const data = await pluginsManager.getExclusive('dataProvider')
        await data.updateBuild(build)
    },

    async create (){
        const data = await pluginsManager.getExclusive('dataProvider')
        let build = Build()
        await data.insertBuild(build)
    }

}