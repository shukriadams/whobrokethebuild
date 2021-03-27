const BaseDaemon = require(_$+'daemon/base')

/**
 * @extends {BaseDaemon}
 */
module.exports = class MapUsersToRevisions extends BaseDaemon {

    async _work(){
        // try to map map local users to users in vcs for a given build
        const pluginsManager = require(_$+'helpers/pluginsManager'),
            data = await pluginsManager.getExclusive('dataProvider'),
            builds = await data.getBuildsWithUnmappedInvolvements()

        if (builds.length)
            __log.debug(`found ${builds.length} builds with unmapped users`)

        for (const build of builds){
            try {
                const job = await data.getJob(build.jobId, { expected : true })

                for(const buildInvolvement of build.involvements){
                    const user = await data.getUserByExternalName(job.VCServerId, buildInvolvement.externalUsername)
                    
                    if (user){
                        buildInvolvement.userId = user.id      
                        await data.updateBuild(build)
                        __log.debug(`added user ${user.name} to build ${build.id}:${build.build}`)
                    }
                }
            } catch(ex){
                __log.error(`Unexpected error in ${this.constructor.name} : build "${build.id}:${build.build}"`, ex)
            }
        }

    }
}