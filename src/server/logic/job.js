const
    pluginsManager = require(_$+'helpers/pluginsManager'),
    Job = require(_$+ 'types/job')

module.exports = {
    getById : async id => {
        const data = await pluginsManager.getExclusive('dataProvider')
        return await data.getJob(id)
    },

    getAll : async job => {
        const data = await pluginsManager.getExclusive('dataProvider')
        await data.getAllJobs(job);
    },

    delete : async id => {
        const data = await pluginsManager.getExclusive('dataProvider')
        await data.removeJob(id)
    },

    update : async properties => {
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

    insert : async  properties => {
        const data = await pluginsManager.getExclusive('dataProvider'),
            job = Job()

        job.name = properties.name
        job.tags = properties.tags
        job.CIServerId = properties.CIServerId
        job.VCServerId = properties.VCServerId
        job.vcs = properties.vcs
        job.isPublic = properties.isPublic

        return await data.insertJob(job)
    }
}