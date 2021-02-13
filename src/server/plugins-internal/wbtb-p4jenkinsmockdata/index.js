module.exports = {

    validateSettings: async () => {
        return true
    },

    async initialize(){
        
        const path = require('path'),
            fs = require('fs-extra'),
            jenkinsMockRoot = path.join(__dirname, '..', 'wbtb-jenkins', 'mock', 'lol_jobs'),
            perforceMockRoot = path.join(__dirname, '..', 'wbtb-perforce', 'mock', 'lol_revisions')

        if (await fs.exists(jenkinsMockRoot))
            return

        let { uniqueNamesGenerator } = require('unique-names-generator'),
            sample = require('lodash.sample'),
            timebelt = require('timebelt'),
            jobsCount = 1,
            buildsPerJob = 10,
            minCommitsPerBuild = 0,
            maxCommitsPerBuild = 3,
            maxLogLines = 1000,
            globalCommitCounter = 1,
            pluginsManager = require(_$+'helpers/pluginsManager'),
            data = await pluginsManager.getExclusive('dataProvider'),
            users = await data.getAllUsers(),
            vsServers = await data.getAllVCServers(),
            jsonOptions = { spaces : 4 },
            verbs = ['Call', 'Rage', 'Attack', 'Battle', 'War', 'Operation', 'Alliance'],
            adverbs = ['in', 'of', 'from', 'with', 'against', 'between', 'among', 'despite'],
            buzzwords =[
                'Asssassin', 'Duty', 'Craft', 'Ninja', 'Royale', 'Unknown', 'Star', 'Alien', 'Zombie', 'Avenger',
                'Modern', 'Shooter', 'Sniper', 'Survivor', 'Backops', 'Counter', 'Field', 'Arena', 'Cold', 'Universe', 'Sword', 'Legend',
                'League', 'Orc', 'Ultra', 'Blood', 'Cop', 'Quest', 'Commando'
            ],
            // Pass every username to the generator.
            dictionaries = [verbs, adverbs, buzzwords, buzzwords, buzzwords],
            // loop, generate unique job names
            jobNames = [...Array(jobsCount)].map(()=>
                uniqueNamesGenerator({
                    dictionaries,
                    style: 'capital',
                    separator : ' ',
                    length: dictionaries.length
                })
            )
        
        __log.warn('Generating mock data for Jenkins, this can take a while ...')
        
        // work out which users are bound to perforce, we'll create commits for those
        const perforceUserNames = []
        for (const user of users)
            for (const vcServer of vsServers)
                user.userMappings.map(mapping =>{ 
                    if (mapping.VCServerId === vcServer.id)
                        perforceUserNames.push(mapping.name)  
                })

        // no prebound users found, add a random name
        if (!perforceUserNames.length)
            perforceUserNames.push('some-user')

        // write perforce data

        const subtractHours = function(datetime, hours){
            if (typeof datetime === 'number' || typeof datetime === 'string')
                datetime = new Date(datetime)

            return new Date( 
                datetime.setHours(datetime.getHours() - hours)
            )
        }

        const addHours = function(datetime, hours){
            if (typeof datetime === 'number' || typeof datetime === 'string')
                datetime = new Date(datetime)

            return new Date( 
                datetime.setHours(datetime.getHours() + hours)
            )
        }

        // generate endpoint for list of jobs
        await fs.outputJson(path.join(jenkinsMockRoot, 'jobs.json'), {
            _class : 'hudson.model.Hudson',
            jobs : jobNames.map(name => { return { _class : 'hudson.model.FreeStyleProject', name } })
        }, jsonOptions)

        // generate job files - commit (build) events, and logs per build
        for (let jobName of jobNames){

            let buildTime = new Date(),
                commitTime = new Date()
            
            for (let jobBuildCounter = 1 ; jobBuildCounter < buildsPerJob ; jobBuildCounter ++) {

                // generate nr of commits per commit event
                const commits = Math.floor(Math.random() * maxCommitsPerBuild) + minCommitsPerBuild,
                    logText = 'muh logs'

                // write build file, this can contain 1 or more commits, but there is one per build
                await fs.outputJson(path.join(jenkinsMockRoot, jobName, 'commits', jobBuildCounter.toString()), {
                    _class : 'hudson.model.FreeStyleBuild',
                    changeSet : {
                        _class : 'hudson.scm.SubversionChangeLogSet',
                        items : [...Array(commits)].map((_,commit)=>{
                            return {
                                _class : 'hudson.scm.SubversionChangeLogSet$LogEntry',
                                commitId : (globalCommitCounter + commit).toString()
                            }
                        })
                    }
                }, jsonOptions)

                // write jenkins build log
                await fs.outputFile(path.join(jenkinsMockRoot, jobName, 'logs', jobBuildCounter.toString()), logText)

                // write perforce commits - there can be multiple tied to a single build event
                for(let j = 0; j < commits ; j ++){
                    const commitId = globalCommitCounter + j

                    // write perforce commit
                    commitTime = subtractHours(commitTime, 1.1)

                    const commitText = `Change ${commitId} by ${sample(perforceUserNames)}@workspace on ${timebelt.toShortDate(commitTime)} ${timebelt.toShortTime(commitTime)}\n\n\tlorem changes\n\nAffected files ...\n\n\t... //mydepot/mystream/path/to/file.txt#2 edit`

                    await fs.outputFile(path.join(perforceMockRoot, commitId.toString()), commitText)
                }

                globalCommitCounter += commits

            }

            // write build list for job
            await fs.outputJson(path.join(jenkinsMockRoot, jobName, 'builds.json'), {
                _class : 'hudson.model.FreeStyleProject',
                allBuilds : [...Array(buildsPerJob)].map((_ ,buildnumber) =>{
                    // count backwards
                    buildnumber = buildsPerJob - buildnumber

                    // count backwards an hour per build
                    buildTime = subtractHours(buildTime, 1)

                    // build should either pass or fail, but last build has a chance to remaining unfinished
                    let result = sample(['FAILURE', 'SUCCESS']) 
                    if (buildnumber === buildsPerJob)
                        result = sample([result, 'PENDING'])

                    return {
                        _class : 'hudson.model.FreeStyleBuild',
                        duration : 0,
                        fullDisplayName : `${jobName} #${buildnumber}`,
                        id : buildnumber.toString(),
                        number : buildnumber,
                        result,
                        builtOn : 'Agent1',
                        timestamp : buildTime.getTime()
                    }
                })   
            }, jsonOptions)

            __log.debug(`Wrote mock data for job ${jobName}`)
        }
    }

}