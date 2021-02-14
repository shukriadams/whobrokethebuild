const pluginsManager = require(_$+'helpers/pluginsManager'),
    Job = require(_$+ 'types/job')

module.exports = {

    async getById(id){
        const data = await pluginsManager.getExclusive('dataProvider')
        return await data.getJob(id)
    },

    async getAll(job){
        const data = await pluginsManager.getExclusive('dataProvider')
        await data.getAllJobs(job);
    },

    async delete(id){
        const data = await pluginsManager.getExclusive('dataProvider')
        await data.removeJob(id)
    },

    async update (properties){
        const data = await pluginsManager.getExclusive('dataProvider'),
            job = await data.getJob(properties.id)

        if (!job)
            throw 'not found'
        
        job.name = properties.name
        job.tags = properties.tags
        job.VCServerId = properties.VCServerId
        job.CIServerId = properties.CIServerId
        job.isPublic = properties.isPublic
        job.logParser = properties.logParser
        
        return await data.updateJob(job)
    },

    async insert(properties){
        const data = await pluginsManager.getExclusive('dataProvider'),
            job = new Job()

        job.name = properties.name
        job.tags = properties.tags
        job.CIServerId = properties.CIServerId
        job.VCServerId = properties.VCServerId
        job.vcs = properties.vcs
        job.isPublic = properties.isPublic

        return await data.insertJob(job)
    }
}