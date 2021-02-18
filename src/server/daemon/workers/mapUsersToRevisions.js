const BaseDaemon = require(_$+'daemon/base')

/**
 * @extends {BaseDaemon}
 */
module.exports = class MapUsersToRevisions extends BaseDaemon {

    constructor(...args){
        super(...args)
    }
    
    async _work(){
        // try to map map local users to users in vcs for a given build
        const pluginsManager = require(_$+'helpers/pluginsManager'),
            data = await pluginsManager.getExclusive('dataProvider'),
            buildInvolvements = await data.getUnmappedBuildInvolvements()

        __log.debug(`found ${buildInvolvements.length} buildInvolvements with unmapped users`)

        for (const buildInvolvement of buildInvolvements){
            try {
                const build = await data.getBuild(buildInvolvement.buildId, { expected : true }),
                    job = await data.getJob(build.jobId, { expected : true }),
                    user = await data.getUserByExternalName(job.VCServerId, buildInvolvement.externalUsername)
                    
                if (user){
                    buildInvolvement.userId = user.id      
                    await data.updateBuildInvolvement(buildInvolvement)
                    
                    __log.debug(`added user ${user.name} to build ${build.id}`)
                }
            } catch(ex){
                __log.error(`Unexpected error in ${this.constructor.name} : buildInvolvement "${buildInvolvement.id}"`, ex)
            }
        }

    }
}