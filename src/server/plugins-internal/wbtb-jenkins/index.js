const
    pluginsHelper = require(_$+'helpers/pluginsManager'),
    encryption = require(_$+'helpers/encryption'),
    constants = require(_$+'types/constants'),
    Build = require(_$+'types/build'),
    BuildInvolvment = require(_$+'types/buildInvolvement'),
    urljoin = require('urljoin'),
    settings = require(_$+ 'helpers/settings'),
    logger = require('winston-wrapper').new(settings.logPath),
    httputils = require('madscience-httputils'),
    timebelt = require('timebelt')

module.exports = {
    

    validateSettings: async () => {
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
     * 
     */
    downloadBuildLog : async (baseUrl, jobName, buildNumber)=>{

        if (settings.sandboxMode)
            baseUrl = urljoin(settings.localUrl, `/jenkins/mock`)

        const url =  urljoin(baseUrl, 'job', encodeURIComponent(jobName), buildNumber, 'consoleText')

        try {
            let response = await httputils.downloadString(url)
            return response.body
        } catch(ex){
            logger.error.error(ex)
            return `log retrieval failed : ${ex}`
        }
    },


    /**
     * Gets a list of all jobs on remote server, list is an array of strings
     */
    getJobs : async(baseUrl)=>{
        if (settings.sandboxMode)
            baseUrl = urljoin(settings.localUrl, `/jenkins/mock`)

        let url = urljoin(baseUrl, 'api/json?pretty=true&tree=jobs[name]'),
            body = ''        

        try {
            body = (await httputils.downloadString(url)).body
            let data = JSON.parse(body),
                jobs = []

            if (data.jobs){
                for (let job of data.jobs)
                jobs.push(job.name)
            }

            return jobs
        } catch(ex){
            // return empty, write to log
            logger.error.error(`failed to download/parse job list : ${ex}. Body was : ${body}`)
            return []
        }
    },


    /**
     * Gets an array of commit ids involved in a build. If the build has no commits, it was triggered manually.
     */
    getBuildCommits : async(baseUrl, jobName, buildNumber)=>{
        if (settings.sandboxMode)
            baseUrl = urljoin(settings.localUrl, `/jenkins/mock`)

        let url = urljoin(baseUrl, 'job', encodeURIComponent(jobName), buildNumber,'api/json?pretty=true&tree=changeSet[items[commitId]]'),
            body = ''        

        try {
            body = (await httputils.downloadString(url)).body
            let data = JSON.parse(body),
                items = []

            if (data.changeSet && data.changeSet.items){
                for (let item of data.changeSet.items)
                    items.push(item.commitId)
            }

            return items
        } catch(ex){
            logger.error.error(`failed to download/parse commit list : ${ex}. Body was : ${body}`)
            return []
        }
    },
    
    async mapUsers(job, revisions){
        
        const 
            data = await pluginsHelper.getExclusive('dataProvider'),
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
     * Gets a list of all builds run on a given job. This method should be run to update WBTB with
     * build history for the given job
     */
    async importBuildsForJob(jobId){
        const 
            data = await pluginsHelper.getExclusive('dataProvider'),
            job = await data.getJob(jobId, { expected : true}),
            ciServer = await data.getCIServer(job.CIServerId, { expected : true}),
            vcServer = await data.getVCServer(job.VCServerId, { expected : true }),
            vcs = await pluginsHelper.get(vcServer.vcs)

        // jobname must be url encoded
        let baseUrl = ciServer.url
        if (ciServer.username) 
            baseUrl = ciServer.url.replace('://', `://${ciServer.username}:${await encryption.decrypt(ciServer.password)}@`) 

        if (settings.sandboxMode)
            baseUrl = urljoin(settings.localUrl, `/jenkins/mock`)

        let url = urljoin(baseUrl, `job/${encodeURIComponent(job.name)}/api/json?pretty=true&tree=allBuilds[fullDisplayName,id,number,timestamp,duration,builtOn,result]`)
        const response = await httputils.downloadString(url)
        let json = null

        try {
            json = JSON.parse(response.body)
        } catch(ex){
            logger.error.error(ex, response.body);
            return
        }
        
        for (let remoteBuild of json.allBuilds){
            let localBuild = await data.getBuildByExternalId(jobId, remoteBuild.number)
            
            // updated build
            if (localBuild){

                // force update status
                localBuild.status = this.resultToStatus(remoteBuild.result)

                // build end is start + duration in minutes
                if (remoteBuild.duration)
                    localBuild.ended = timebelt.addMinutes(localBuild.started, remoteBuild.duration).getTime()

                // fetch log if build is complete
                if (!localBuild.log && (localBuild.status === constants.BUILDSTATUS_FAILED || localBuild.status === constants.BUILDSTATUS_PASSED))
                    localBuild.log = await this.downloadBuildLog(baseUrl, job.name, localBuild.build)

                // bad : this will constantly update records, even if not dirty
                await data.updateBuild(localBuild)
                continue
            }
            
            // insert build
            localBuild = Build()
            localBuild.jobId = jobId
            localBuild.status = this.resultToStatus(remoteBuild.result)
            localBuild.build = remoteBuild.number
            localBuild.host = remoteBuild.builtOn
            localBuild.revisions = await this.getBuildCommits(baseUrl, job.name, remoteBuild.number)
            localBuild.started = remoteBuild.timestamp
            localBuild = await data.insertBuild(localBuild)

            // insert buildInvolvement
            for (let revision of localBuild.revisions){
                const revisionData = await vcs.getRevision(revision, vcServer)
                if (revisionData){
                    let buildInvolvment = await data.getBuildInvolvementByRevision(localBuild.id, revisionData.user)
                    if (buildInvolvment)
                        continue 

                    buildInvolvment = BuildInvolvment()
                    buildInvolvment.externalUsername = revisionData.user
                    buildInvolvment.buildId = localBuild.id
                    buildInvolvment.revision = revision
                    buildInvolvment.involvement = constants.BUILDINVOLVEMENT_SOURCECHANGE
                    await data.insertBuildInvolvement(buildInvolvment)
                }
            }
            
            logger.info.info(`Imported build ${job.name}:${remoteBuild.number}`)
        }
    },

    async verifyInstance (){
        console.log(' not implemented yet')
    }

}