let pluginsManager = require(_$+'helpers/pluginsManager'),
    Build = require(_$+'types/build')

module.exports = {

    /**
     * Gets a build for display purposes, throws exception if not found.
     */
    async getById(id){
        const data = await pluginsManager.getExclusive('dataProvider'),
            build = await data.getBuild(id, { expected : true })
            
        build.__job = await data.getJob(build.jobId, { expected : true })
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
        let build = new Build()
        await data.insertBuild(build)
    }

}