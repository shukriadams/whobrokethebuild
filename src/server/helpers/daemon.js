let CronJob = require('cron').CronJob,
    stringSimilarity = require('string-similarity'),
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
                    const data = await pluginsManager.getExclusive('dataProvider'),
                        jobs = await data.getAllJobs()

                        
                    // import latest builds
                    for (let job of jobs){
                        const ciserver = await data.getCIServer(job.CIServerId)
                        if (!ciserver)
                            throw `CIServer record ${job.CIServerId} defined in job ${job.id} not found`
                        
                        const ciServerPlugin = await pluginsManager.get(ciserver.type)
                        await ciServerPlugin.importBuildsForJob(job.id)
                    }


                    // try to map map local users to users in vcs for a given build
                    const buildInvolvements = await data.getUnmappedBuildInvolvements()
                    for (const buildInvolvement of buildInvolvements){
                        const build = await data.getBuild(buildInvolvement.buildId, { expected : true }),
                            job = await data.getJob(build.jobId, { expected : true }),
                            user = await data.getUserByExternalName(job.VCServerId, buildInvolvement.externalUsername)
                            
                        if (user){
                            buildInvolvement.userId = user.id      
                            await data.updateBuildInvolvement(buildInvolvement)
                            logger.info.info(`added user ${user.name} to build ${build.id} ` )
                        }
                    }
                    
                    
                    // parse build logs 
                    const unprocessedBuilds = await data.getBuildsWithUnparsedLogs()
                    for (const build of unprocessedBuilds){
                        // try to parse build log for all builds
                        const job = await data.getJob(build.jobId)
                        if (!job)
                            continue

                        const logParser = job.logParser ? await pluginsManager.get(job.logParser) : null
                        if (!logParser)
                            continue

                        build.logParsed = logParser.parseErrors(build.log)
                        build.isLogParsed = true
                        await data.updateBuild(build)
                        logger.info.info(`parsed log for build ${build.id}`)
                    }
                   

                    // map revision codes to revision objects
                    const unprocessedBuildInvolvements = await data.getBuildInvolvementsWithoutRevisionObjects()
                    for (const buildInvolvement of unprocessedBuildInvolvements){
                        
                        const build = await data.getBuild(buildInvolvement.buildId)
                        if (!build)
                            continue

                        const job = await data.getJob(build.jobId)
                        if(!job)
                            continue

                        const vcServer = await data.getVCServer(job.VCServerId)
                        if (!vcServer)
                            continue

                        const vcPlugin = await pluginsManager.get(vcServer.vcs)
                        if (!vcPlugin)
                            continue

                        buildInvolvement.revisionObject = await vcPlugin.getRevision(buildInvolvement.revision, vcServer)  
                        // force placeholder revision object if lookup to vc fails to retrieve it
                        if (!buildInvolvement.revisionObject)
                            buildInvolvement.revisionObject = {
                                revision : `${buildInvolvement.revision} lookup failed`,
                                user : buildInvolvement.externalUsername,
                                description : '', 
                                files : [] 
                            }

                        // calculate fail chance of each file in revision
                        for (const file of buildInvolvement.revisionObject.files)
                            file.faultChance = stringSimilarity.compareTwoStrings(file.file, build.log) 

                        logger.info.info(`Mapped revision ${buildInvolvement.revision} in buildInvolvement ${buildInvolvement.id}`)
                        await data.updateBuildInvolvement(buildInvolvement)
                    }



                    // for each job, check if current build is broken, and if so send message to people what broke it
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
                                logger.info.info(`throw exception here! - expected user ${buildInvolvement.userId} in buildInvolvement ${buildInvolvement.id} not found`)
                                continue
                            }

                            for (const contactMethod in user.contactMethods){
                                const contactPlugin = await pluginsManager.get(contactMethod)
                                if (!contactPlugin){
                                    logger.info.info(`throw exception here! - expected plugin ${contactMethod.type} not found`)
                                    continue
                                }
                                
                                await contactPlugin.alertBrokenBuild(user, breakingBuild)
                            }
                        }
                    }


                    // set build deltas - this is a per-build flag showing its relationship to its preceding build. In this way we can query any
                    // given build and get its delta without also having to query its predecessor.
                    let builds = await data.getBuildsWithNoDelta(),
                        lastDelta = null

                    for (const build of builds){
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
                                console.log('cont from > ', lastDelta, previousBuild)
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

                        /*
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
                        */
                    }

                    // alert on build status changed
                    for (let job of jobs){
                        const latestBuild = await data.getLatestBuild(job.id)

                        if (!latestBuild || !latestBuild.ended)
                            continue

                        if ((latestBuild.status === constants.BUILDSTATUS_PASSED && !job.isPassing) ||
                            (latestBuild.status === constants.BUILDSTATUS_FAILED && job.isPassing)){
                                for (jobContactMethodKey in job.contactMethods){
                                    // todo : log that require plugin not found
                                    const plugin = await pluginsManager.get(jobContactMethodKey)
                                    if (!plugin) 
                                        continue
                                
                                    await plugin.alertChannel(job.contactMethods[jobContactMethodKey], job, latestBuild)
                                }

                                job.isPassing = latestBuild.status === constants.BUILDSTATUS_PASSED
                                await data.updateJob(job)
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