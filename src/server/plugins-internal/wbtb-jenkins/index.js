const pluginsHelper = require(_$+'helpers/pluginsManager'),
    constants = require(_$+'types/constants'),
    Build = require(_$+'types/build'),
    fs = require('fs-extra'),
    path = require('path'),
    sanitize = require('sanitize-filename'),
    BuildInvolvment = require(_$+'types/buildInvolvement'),
    urljoin = require('urljoin'),
    settings = require(_$+ 'helpers/settings'),
    httputils = require('madscience-httputils'),
    timebelt = require('timebelt'),
    thisType = 'wbtb-jenkins'

module.exports = {
    

    async validateSettings(){
        return true
    },

    
    /**
     * Converts jenkins "result" value to internal "status" value
    */
    resultToStatus(result){
        switch(result){
            case 'FAILURE': {
                return constants.BUILDSTATUS_FAILED
            }
            case 'SUCCESS': {
                return constants.BUILDSTATUS_PASSED
            }
            case 'ABORTED': {
                return constants.BUILDSTATUS_FAILED
            }
            case null : {
                return constants.BUILDSTATUS_INPROGRESS
            }
            default : {
                return constants.BUILDSTATUS_OTHER
            }
        }
    },


    /**
     * @param {object} ciServer
     * @param {object} job
     * @param {object} build
     * 
     * Required by interface for ciserver.
     * Generates a link to the original ci server for a given build
     */
    linkToBuild(ciServer, job, build){
        return urljoin(ciServer.url, 'job', encodeURIComponent(job.name), build.build)
    },


    /**
     * @param {string} baseUrl
     * @param {import('../../types/job').Job} job
     * @param {object} buildNumber string or int
     */
    async downloadBuildLog(baseUrl, job, buildNumber){

        if (settings.sandboxMode)
            baseUrl = urljoin(settings.localUrl, `/jenkins/mock`)

        const isApiCaching = this.__wbtb.cacheAPICalls === true,
            cachePath = path.join(settings.dataFolder, thisType, 'callCache', sanitize(job.name), `${buildNumber}.log`),
            url =  urljoin(baseUrl, 'job', encodeURIComponent(job.name), buildNumber, 'consoleText')

        if (isApiCaching && await fs.exists(cachePath))
            return await fs.readFile(cachePath, `utf8`)

        try {
            let response = await httputils.downloadString(url)
            if (isApiCaching){
                await fs.ensureDir(path.dirname(cachePath))
                await fs.outputFile(cachePath, response.body)
            }

            return response.body
        } catch(ex){
            __log.error(ex)
            return `log retrieval failed : ${ex}`
        }
    },


    /**
     * Gets a list of all jobs on remote server, list is an array of strings.
     */
    async getJobs(baseUrl){
        if (settings.sandboxMode)
            baseUrl = urljoin(settings.localUrl, `/jenkins/mock`)

        let isApiCaching = this.__wbtb.cacheAPICalls === true,
            cachePath = path.join(settings.dataFolder, thisType, 'callCache', 'jobs'),
            url = urljoin(baseUrl, 'api/json?pretty=true&tree=jobs[name]'),
            body = ''        

        try {
            body = (await httputils.downloadString(url)).body
            let data = JSON.parse(body),
                jobs = []

            if (data.jobs){
                for (let job of data.jobs)
                jobs.push(job.name)
            }

            if (isApiCaching){
                await fs.ensureDir(path.dirname(cachePath))
                for (const job of jobs)
                    await fs.outputFile(path.join(cachePath, `${sanitize(job)}.name`), job)
            }

            return jobs
        } catch(ex){
            // return empty, write to log
            __log.error(`failed to download/parse job list : ${ex}. Body was : ${body}`)
            return []
        }
    },


    /**
     * @params {string} baseUrl
     * @params {Job} job
     * @parms {object} buildnumber int or string. Ugh.
     * Gets an array of commit ids involved in a build. If the build has no commits, it was triggered manually.
     */
    async getBuildCommits(baseUrl, job, buildNumber){
        if (settings.sandboxMode)
            baseUrl = urljoin(settings.localUrl, `/jenkins/mock`)

        let isApiCaching = this.__wbtb.cacheAPICalls === true,
            cachePath = path.join(settings.dataFolder, thisType, 'callCache', sanitize(job.name), `commits_${buildNumber}.json`),
            url = urljoin(baseUrl, 'job', encodeURIComponent(job.name), buildNumber,'api/json?pretty=true&tree=changeSet[items[commitId]]'),
            body = ''        

        if (isApiCaching && await fs.exists(cachePath))
            return await fs.readJson(cachePath)


        try {
            body = (await httputils.downloadString(url)).body
            let data = JSON.parse(body),
                items = []

            if (data.changeSet && data.changeSet.items){
                for (let item of data.changeSet.items)
                    items.push(item.commitId)
            }

            if (isApiCaching){
                await fs.ensureDir(path.dirname(cachePath))
                await fs.writeJson(cachePath, items)
            }

            return items
        } catch(ex){
            __log.error(`failed to download/parse commit list : ${ex}. Body was : ${body}`)
            return []
        }
    },
    
    async mapUsers(job, revisions){
        
        const data = await pluginsHelper.getExclusive('dataProvider'),
            vcServer = await data.getVCServer(job.VCServerId, { expected : true }),
            vcs = await pluginsHelper.get(vcServer.vcs)

        const users = []
        for (let revision of revisions){
            const revisionData = await vcs.getRevision(revision, vcServer)
            if (revisionData)
                users.push (revisionData.user)
        }

        return users
        
    },

    /**
     * @param {import('../../types/job').Job} job
     * Gets a list of all builds run on a given job. This method should be run to update WBTB with
     * build history for the given job
     */
    async importBuildsForJob(job){
        let data = await pluginsHelper.getExclusive('dataProvider'),
            ciServer = await data.getCIServer(job.CIServerId, { expected : true }),
            vcServer = await data.getVCServer(job.VCServerId, { expected : true }),
            vcs = await pluginsHelper.get(vcServer.vcs),
            baseUrl = await ciServer.getUrl(),
            isApiCaching = this.__wbtb.cacheAPICalls === true,
            cachePath = path.join(settings.dataFolder, thisType, 'callCache', sanitize(job.name)),
            json = null

        if (!settings.buildLogsDump){
            __log.error(`settings.buildLogsDump not set`)
            return 
        }

        await fs.ensureDir(settings.buildLogsDump)

        // jobname must be url encoded
        if (settings.sandboxMode)
            baseUrl = urljoin(settings.localUrl, `/jenkins/mock`)

        let url = urljoin(baseUrl, `job/${encodeURIComponent(job.name)}/api/json?pretty=true&tree=allBuilds[fullDisplayName,id,number,timestamp,duration,builtOn,result]`),
            response = await httputils.downloadString(url)
        
        if (response.statusCode === 404){
            __log.error(`Build job ${job.name} was not found on Jenkins server ${ciServer.name}.`)
            return
        }

        try {
            json = JSON.parse(response.body)
        } catch(ex){
            __log.error(`Failed to parse JSON from  job ${job.name}`, ex, response.body)
            return
        }

        if (isApiCaching){
            await fs.ensureDir(cachePath)
            for (let remoteBuild of json.allBuilds){
                const filePath = path.join(cachePath, `build_${remoteBuild.number}.json`)
                if (!await fs.exists(filePath))
                    await fs.outputJson(filePath, remoteBuild)
            }
        }

        // limit the number of jobs back in time to import, if necessay.
        let historyLimit = settings.historyLimit || job.historyLimit
        if (!!historyLimit)
            json.allBuilds = json.allBuilds.slice(0, historyLimit)
            
        for (let remoteBuild of json.allBuilds){
            let localBuild = await data.getBuildByExternalId(job.id, remoteBuild.number)
            
            // build exists in db, update it 
            if (localBuild){

                // force update status
                localBuild.status = this.resultToStatus(remoteBuild.result)

                // build end is start + duration in minutes
                if (remoteBuild.duration)
                    localBuild.ended = timebelt.addMilliseconds(localBuild.started, remoteBuild.duration)

                // fetch log if build is complete and log has not be previous fetched
                if (localBuild.logStatus === constants.BUILDLOGSTATUS_NOT_FETCHED && (localBuild.status === constants.BUILDSTATUS_FAILED || localBuild.status === constants.BUILDSTATUS_PASSED)){

                    const foldername = sanitize(job.id),
                        pathFragment = path.join(foldername, sanitize(localBuild.build.toString())),
                        writePath = path.join(settings.buildLogsDump, pathFragment)
                    
                    try {
                        await fs.ensureDir(path.join(settings.buildLogsDump, foldername))
                        // write job name as a file in folder as a convenience
                        await fs.writeFile(path.join(settings.buildLogsDump, foldername, sanitize(job.name)), '')

                        const log = await this.downloadBuildLog(baseUrl, job, localBuild.build)
                        await fs.writeFile(writePath, log)
                        localBuild.logStatus = constants.BUILDLOGSTATUS_UNPROCESSED
                        await data.updateBuild(localBuild)

                    } catch (ex){
                        __log.error(`unexpected error dumping reference log ${writePath}`, ex)
                    }
                }

                continue
            }
            

            // build does not exist in db - create record
            localBuild = new Build()
            localBuild.jobId = job.id
            localBuild.status = this.resultToStatus(remoteBuild.result)
            localBuild.build = remoteBuild.number
            localBuild.host = remoteBuild.builtOn
            localBuild.revisions = await this.getBuildCommits(baseUrl, job, remoteBuild.number)
            localBuild.started = remoteBuild.timestamp

            // Add buildInvolvements if we can read these from CI system. This will normally be in cases where a version control event triggers a build.
            // This will not occur on builds which run on fixed timers, for those builds other ways of determining involved revisions are required.
            for (let revision of localBuild.revisions){
                const revisionData = await vcs.getRevision(revision, vcServer)
                if (revisionData){
                    const buildInvolvment = new BuildInvolvment()
                    buildInvolvment.externalUsername = revisionData.user
                    buildInvolvment.buildId = localBuild.id
                    buildInvolvment.revision = revision
                    buildInvolvment.involvement = constants.BUILDINVOLVEMENT_SOURCECHANGE
                    localBuild.involvements.push(buildInvolvment)
                }
            }

            localBuild = await data.insertBuild(localBuild)
            __log.info(`Imported build ${job.name}:${remoteBuild.number}`)
        }
    },

    async verifyInstance (){
        __log.debug(' not implemented yet')
    }

}