const BaseDaemon = require(_$+'daemon/base')

module.exports = class extends BaseDaemon {

    constructor(...args){
        super(...args)
    }
    
    async _work(){

        __log.debug(`buildDeltaCalculator daemon doing work ....`)

        let pluginsManager = require(_$+'helpers/pluginsManager'),
            data = await pluginsManager.getExclusive('dataProvider'),
            builds = await data.getBuildsWithNoDelta(),
            lastDelta = null

        // set build deltas - this is a per-build flag showing its relationship to its preceding build. In this way we can query any
        // given build and get its delta without also having to query its predecessor.
        for (const build of builds){
            try {
                const previousBuilds = data.getPreviousBuild(build)

                if (build) {
                    
                    // previous build is still in progress, cannot calculate delta yet, so wait
                    if (!previousBuilds.ended)
                        continue
    
                    // note - change break is not assigned here, but we need to take it into account when generating deltas
    
                    // build still passing, nothing to see here
                    if (build.status === constants.BUILDSTATUS_PASSED && lastDelta === constants.BUILDDELTA_PASS)
                        build.delta = constants.BUILDDELTA_PASS
    
                    // build was first to fail, then it caused the break
                    if (build.status === constants.BUILDSTATUS_FAILED && lastDelta === constants.BUILDDELTA_PASS)
                        build.delta = constants.BUILDDELTA_CAUSEBREAK
    
                    // build is broken but was already broken, continue the break, not my fault
                    if (build.status === constants.BUILDSTATUS_FAILED && 
                        (lastDelta === constants.BUILDDELTA_CAUSEBREAK 
                            || lastDelta === constants.BUILDDELTA_CONTINUEBREAK 
                            || lastDelta === constants.BUILDDELTA_CHANGEBREAK)
                        )
                    {
                        build.delta = constants.BUILDDELTA_CONTINUEBREAK
                    }
                        
    
                    // build is now working but was broken before, I fixed it, go me
                    if (build.status === constants.BUILDDELTA_PASS && 
                            (lastDelta === constants.BUILDDELTA_CAUSEBREAK 
                                || lastDelta === constants.BUILDDELTA_CONTINUEBREAK 
                                || lastDelta === constants.BUILDDELTA_CHANGEBREAK)
                            )
                            build.delta = constants.BUILDDELTA_FIX
    
                } else {
                    // there is no previous build so no delta, this build's delta will be its own status
                    build.delta = build.status === constants.BUILDSTATUS_PASSED ? constants.BUILDDELTA_PASS : constants.BUILDDELTA_CAUSEBREAK
                }
    
                await data.updateBuild(build)

            } catch(ex){
                __log.error(`Unexpected error trying to calculate delta for build "${build.id}"`, ex)
            }
        }
    }
}