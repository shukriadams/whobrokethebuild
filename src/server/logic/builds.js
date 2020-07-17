let 
    pluginsManager = require(_$+'helpers/pluginsManager'),
    Build = require(_$+ 'types/build')

module.exports = {

    /**
     * Gets a build for display purposes, throws exception if not found.
     */
    async get(id){
        const data = await pluginsManager.getByCategory('dataProvider'),
            build = await data.getBuild(id, { expected : true })
            
        build.__job = await data.getJob(build.jobId, { expected : true })
        
        // parses the build log if a log is set at job level
        if (build.__job.logParser){
            // todo : get should internally log error if plugin not found
            const logParserPlugin = await pluginsManager.get(build.__job.logParser)
            if (logParserPlugin)
                build.__parsedLog = logParserPlugin.parseErrors(build.log)
        }
        // find commits 

        return build
    },

    async page (jobId, index, pageSize){
        const data = await pluginsManager.getByCategory('dataProvider')
        await data.pageBuilds(jobId, index, pageSize)
    },

    async remove (buildId){
        const data = await pluginsManager.getByCategory('dataProvider')
        await data.removeBuild(buildId)
    },

    async update  (build){
        const data = await pluginsManager.getByCategory('dataProvider')
        await data.updateBuild(build)
    },

    async create (){
        const data = await pluginsManager.getByCategory('dataProvider')
        let build = Build()
        await data.insertBuild(build)
    }

}