module.exports = {
    _key(jobId){
        return `job_stats_${jobId}`
    },

    /**
     * This should be called every time job build data changes
     */
    async reset(job){
        const key = this._key(job.id),
            cache = require(_$+'helpers/cache')

        await cache.remove(key)
    },

    async getStats(jobId){

        let cache = require(_$+'helpers/cache'),
            key = this._key(jobId),
            stats = await cache.get(key)
        
        if (stats)
            return stats

        const jobLogic = require(_$+'logic/job'),
            userLogic = require(_$+'logic/users'),
            buildLogic = require(_$+'logic/builds'),
            faultHelper = require(_$+'helpers/fault'),
            job = await jobLogic.getJob(jobId, { expected : true })

        stats = {
            totalBreaks : 0,
            totalRuns : 0,
            daysActive : 0,
            runsPerDay: 0,
            breaksPerDay: 0,
            breakRatio: 0, // raw runs-to-fails ratio
            brokenSince : null,
            brokenByUsers : [], // user objects
            brokenBy: [] // strings
        }

        if (!job.isPassing){
            let build = null
            if (job.lastBreakIncidentId){
                build = await buildLogic.getBuild(job.lastBreakIncidentId)
                stats.brokenSince = build.ended
                stats.brokenBy = await faultHelper.getUsersWhoBrokeBuild(build)
            }

            for (const userId of stats.brokenBy){
                const user = await userLogic.getByExternalName(job.VCServerId, userId)
                if (user)
                    stats.brokenByUsers.push(user)
            }
        }

        await cache.add(key, stats)

        return stats
        
    }
}