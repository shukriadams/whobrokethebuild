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

    async get(jobId){

        let cache = require(_$+'helpers/cache'),
            key = this._key(jobId),
            stats = await cache.get(key)
        
        if (stats)
           return stats

        const pluginsManager = require(_$+'helpers/pluginsManager'),
            data = await pluginsManager.getExclusive('dataProvider')

        stats = await data.getJobStats(jobId)
        await cache.add(key, stats)
        return stats
    }
}