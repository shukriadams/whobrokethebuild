let
    CronJob = require('cron').CronJob,
    settings = require(_$+'helpers/settings'),
    logger = require('winston-wrapper').new(settings.logPath),
    pluginsManager = require(_$+'helpers/pluginsManager'),
    constants = require(_$+'types/constants'),
    commonBusy = false,
    systemBusy = false,
    commonDaemon = null,
    systemDaemon = null,
    isRunning = false

module.exports = {
    
    isRunning(){
        return isRunning
    },

    start : ()=>{

        if (!commonDaemon)
            commonDaemon = new CronJob(settings.daemonInterval, async ()=>{
                
                if (commonBusy)
                    return

                    commonBusy = true
    
                try {
                    const 
                        data = await pluginsManager.getExclusive('dataProvider')
                        jobs = await data.getAllJobs()

                    // import latest builds
                    for (let job of jobs){
                        const ciserver = await data.getCIServer(job.CIServerId)
                        if (!ciserver)
                            throw `CIServer record ${job.CIServerId} defined in job ${job.id} not found`
                        
                        const ciServerPlugin = await pluginsManager.get(ciserver.type)
                        await ciServerPlugin.importBuildsForJob(job.id)
                    }

                    // map local users to users in vcs for a given build
                    const buildInvolvements = await data.getUnmappedBuildInvolvements()
                    for (const buildInvolvement of buildInvolvements){
                        // try to find user object based on external user id
                        const build = await data.getBuild(buildInvolvement.buildId, { expected : true })
                        const job = await data.getJob(build.jobId, { expected : true })
                        
                        const user = await data.getUserByExternalName(job.VCServerId, buildInvolvement.externalUsername)
                        if (user){
                            buildInvolvement.userId = user.id      
                            await data.updateBuildInvolvement(buildInvolvement)
                            console.log(`added user ${user.name} to build ${build.id} ` )
                        }
                    }
                    

                    // for each job, get the currently breaking build (if broken). Then send message to people what broke it
                    for (let job of jobs){
                        const breakingBuild = await data.getCurrentlyBreakingBuild(job.id) 
                        if (!breakingBuild)
                            continue

                        // inform culprits they've been caught red-handed
                        const buildInvolvements = await data.getBuildInvolementsByBuild(breakingBuild.id)
                        for (const buildInvolvement of buildInvolvements){
                            // no local user found for build, don't worry we'll get them next time
                            if (!buildInvolvement.userId)
                                continue
                            
                            const user = await data.getUser(buildInvolvement.userId)
                            if (!user){
                                console.log(`throw exception here! - expected user ${buildInvolvement.userId} in buildInvolvement ${buildInvolvement.id} not found`)
                                continue
                            }

                            for (const contactMethod in user.contactMethods){
                                const contactPlugin = await pluginsManager.get(contactMethod)
                                if (!contactPlugin){
                                    console.log(`throw exception here! - expected plugin ${contactMethod.type} not found`)
                                    continue
                                }
                                
                                await contactPlugin.alertBrokenBuild(user, breakingBuild)
                            }
                        }
                    }

                    
                    // set build deltas - this is a per-build flag showing its relationship to previous build, so we
                    // we don't have to query all builds again to determine relationship
                    let builds = await data.getBuildsWithNoDelta(),
                        lastDelta = null

                    for (const build of builds){
                        // if build already has delta, it's here only to start sequence, don't assign anything to it
                        if (build.delta){
                            lastDelta = build.delta
                            continue
                        }

                        if (lastDelta) {

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
                                build.delta = constants.BUILDDELTA_CONTINUEBREAK

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
                        lastDelta = build.delta

                        // if this build broke or fixed the build, broadcast to public channels
                        if (lastDelta === constants.BUILDDELTA_PASS || lastDelta === constants.BUILDDELTA_CAUSEBREAK){
                            const job = await data.getJob(build.jobId)
                            for (jobContactMethodKey in job.contactMethods){
                                // todo : log that require plugin not found
                                const plugin = await pluginsManager.get(jobContactMethodKey)
                                if (!plugin) 
                                    continue
                            
                                await plugin.alertChannel(job.contactMethods[jobContactMethodKey], job, build, lastDelta)
                            }
                        }
                    }

                    // for all builds which are done, broken and has not already been blamed
                    // for all revisions in builds
                    // mark revision user as person-of-interest
                    // include all files which were involved in commit
                    // parse build logs, try to find which specific files were involved in error
                    // of build log entry can be matched to a file, mark committing user a suspect
                    // if no suspect can be found, mark break as hot-mess and mark everyone has "hot-mess-suspect"

                    // send out warrants
                    // for all build error suspects who have not yet received a warning, send them one, suspects get special ones
                    // if a break is hot mess, alert sentinels
                    // if a build contains a specific kind of error, alert sentinel for that type

                    // calculate state of the nation
                    // is there a build that has been broken for too long?
                    // are too many builds broken?


                } catch (ex) {

                    logger.error.error(ex)

                } finally {

                    commonBusy = false

                }
            }, 
            null,  // oncomplete
            false, 
            null, 
            null, 
            true /* runonitit */ )

        if (!systemDaemon)
            systemDaemon = new CronJob(settings.systemDaemonInterval, async()=>{
                if (systemBusy)
                    return

                systemBusy = true

                try {

                    const data = await pluginsManager.getExclusive('dataProvider')
                    await data.clean()

                } catch (ex) {

                    logger.error.error(ex)

                } finally {

                    systemBusy = false

                }
            },
            null, 
            false, 
            null, 
            null, 
            true /* runonitit */ )

        commonDaemon.start()
        systemDaemon.start()
        isRunning = true
    },

    stop(){
        if (commonDaemon)
            commonDaemon.stop()

        if (systemDaemon)
            systemDaemon.stop()

        isRunning = false
    }
}