const BaseDaemon = require(_$+'daemon/base')

/**
 * @extends {BaseDaemon}
 */
module.exports = class BuildDeltaCalculator extends BaseDaemon {

    constructor(...args){
        super(...args)
    }
    
    async _work(){
        let constants = require(_$+'types/constants'),
            pluginsManager = require(_$+'helpers/pluginsManager'),
            data = await pluginsManager.getExclusive('dataProvider'),
            builds = await data.getBuildsWithNoDelta()

        __log.debug(`found ${builds.length} builds without delta`)

        // set build deltas - this is a per-build flag showing its relationship to its preceding build. In this way we can query any
        // given build and get its delta without also having to query its predecessor.
        for (const build of builds){
            try {
                const previousBuild = await data.getPreviousBuild(build)

                if (previousBuild) {

                    // previous build is still in progress, no delta on it yet, cannot calculate delta yet, so wait
                    if (!previousBuild.delta){
                        __log.debug(`build ${build.build} can't get delta because previous build ${previousBuild.build} has no delta yet`)
                        continue
                    }
    
                    // note - change break is not assigned here, but we need to take it into account when generating deltas
    
                    // build still passing, previous build passed, nothing to see here
                    if (build.status === constants.BUILDSTATUS_PASSED && (
                        previousBuild.delta === constants.BUILDDELTA_PASS 
                        || previousBuild.delta === constants.BUILDDELTA_FIX ))
                        build.delta = constants.BUILDDELTA_PASS
    
                    // build was first to fail, then it caused the break
                    if (build.status === constants.BUILDSTATUS_FAILED && (
                        previousBuild.delta === constants.BUILDDELTA_PASS 
                        || previousBuild.delta === constants.BUILDDELTA_FIX))
                        {
                            build.delta = constants.BUILDDELTA_CAUSEBREAK
                            build.incidentId = build.id
                        }

                    // build is broken but was already broken, continue the break, not my fault
                    if (build.status === constants.BUILDSTATUS_FAILED && (
                        previousBuild.delta === constants.BUILDDELTA_CAUSEBREAK 
                        || previousBuild.delta === constants.BUILDDELTA_CONTINUEBREAK 
                        || previousBuild.delta === constants.BUILDDELTA_CHANGEBREAK ))
                    {
                        build.delta = constants.BUILDDELTA_CONTINUEBREAK
                    }
    
                    // build is now working but was broken before, I fixed it, go me
                    if (build.status === constants.BUILDSTATUS_PASSED && (
                        previousBuild.delta === constants.BUILDDELTA_CAUSEBREAK 
                        || previousBuild.delta === constants.BUILDDELTA_CONTINUEBREAK 
                        || previousBuild.delta === constants.BUILDDELTA_CHANGEBREAK ))
                            build.delta = constants.BUILDDELTA_FIX

                    // transfer incident from predecessor if broken
                    if ((build.delta === constants.BUILDDELTA_CONTINUEBREAK || build.delta === constants.BUILDDELTA_CHANGEBREAK) && previousBuild.incidentId)
                        build.incidentId = previousBuild.incidentId

                } else {
                    // there is no previous build so no delta, this build's delta will be its own status
                    build.delta = build.status === constants.BUILDSTATUS_PASSED ? constants.BUILDDELTA_PASS : constants.BUILDDELTA_CAUSEBREAK
                    if (build.delta === constants.BUILDDELTA_CAUSEBREAK)
                        build.incidentId = build.id
                }
    
                await data.updateBuild(build)

            } catch(ex){
                __log.error(`Unexpected error in ${this.constructor.name} : build "${build.id}"`, ex)
            }
        }
    }
}