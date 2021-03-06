const pluginsManager = require(_$+'helpers/pluginsManager'),
    Avatar = require(_$+'types/avatar'),
    settings = require(_$+'helpers/settings'),
    Job = require(_$+'types/job')

module.exports = {

    applyDefaultAvatar(job){
        if (!settings.defaultJobAvatar)
            return job

        if (!job.avatar){
            job.avatar = new Avatar()
            job.avatar.path = settings.defaultJobAvatar
        }

        return job
    },

    async getJob(id, options){
        let data = await pluginsManager.getExclusive('dataProvider'),
            job = await data.getJob(id, options)

        job = this.applyDefaultAvatar(job)
        
        return job
    },

    async getAllJobs(){
        let data = await pluginsManager.getExclusive('dataProvider'),
            jobs = await data.getAllJobs()

        jobs = jobs.map(job => this.applyDefaultAvatar(job) )

        return jobs
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
        job.logParser = properties.logParser
        
        return await data.insertJob(job)
    }
}