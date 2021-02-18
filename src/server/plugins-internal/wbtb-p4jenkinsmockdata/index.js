module.exports = {

    validateSettings: async () => {
        return true
    },

    async initialize(){
        
        const path = require('path'),
            fs = require('fs-extra'),
            jenkinsMockRoot = path.join(__dirname, '..', 'wbtb-jenkins', 'mock', 'jobs'),
            perforceMockRoot = path.join(__dirname, '..', 'wbtb-perforce', 'mock', 'revisions')

        if (await fs.exists(jenkinsMockRoot))
            return

        let { uniqueNamesGenerator } = require('unique-names-generator'),
            sample = require('lodash.sample'),
            timebelt = require('timebelt'),
            settings = require(_$+'helpers/settings'),
            jobsCount = settings.p4jenkinsmockdata_jobsCount || 10,
            buildsPerJob = settings.p4jenkinsmockdata_buildsPerJob || 10,
            minCommitsPerBuild = settings.p4jenkinsmockdata_minCommitsPerBuild || 0,
            maxCommitsPerBuild = settings.p4jenkinsmockdata_maxCommitsPerBuild || 3,
            maxLogLines = settings.p4jenkinsmockdata_maxLogLines || 1000,
            globalCommitCounter = 1,
            pluginsManager = require(_$+'helpers/pluginsManager'),
            data = await pluginsManager.getExclusive('dataProvider'),
            users = await data.getAllUsers(),
            vsServers = await data.getAllVCServers(),
            jsonOptions = { spaces : 4 },
            verbs = ['Call of', 'Rage against the', 'Attack on', 'Battle of', 'The War for', 'Operation for', 'Alliance of', 'Creed of', 'League of', 'Quest for'],
            adverbs = ['of', 'of the', 'on', 'in', 'with', 'against the', 'inspite of'],
            adjectives = ['The One', 'The Chosen', 'Modern', 'Perkele', 'Trump', 'Neo', 'Evil', 'Illegal', 'Crossover', 'Ultra', 'Dashing', 'Awkward', 'Farming', 'Euro', 'Legendary', 'Universal', 'Ancient', 'Demonic', 'Instagraph', 'Knownplayer', 'Blackups', 'Counter'],
            nouns = [ 'Ninja', 'Zombie', 'Avenger', 'Alien', 'Duty', 'Justice', 'Punk', 'Assassin', 'Shooter', 'Sniper', 'Cop', 'Blood', 'Commando', 'Star Battle', 'Orcish', 'Cyber', 'Elvish', 'Medieval'],
            nouns2 = [ 'Simulator', 'Craft', 'Raiders', 'Warfare', 'Arena', '- Battle Royale', '- Remastered', '- 4KHD', '- Mobile Edition'],
            // Pass every username to the generator.
            dictionaries = [verbs, adjectives, nouns, nouns2],
            // loop, generate unique job names
            jobNames = [...Array(jobsCount)].map(()=>
                uniqueNamesGenerator({
                    dictionaries,
                    style: 'capital',
                    separator : ' ',
                    length: dictionaries.length
                })
            )
        
        __log.warn('Generating mock data for Jenkins and Perforce ...')
        
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
                const commits = Math.floor(Math.random() * maxCommitsPerBuild) + minCommitsPerBuild

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

                // write perforce commits - there can be multiple tied to a single build event
                for(let j = 0; j < commits ; j ++){
                    const commitId = globalCommitCounter + j

                    // write perforce commit
                    commitTime = timebelt.subtractHours(commitTime, 1.1)

                    const commitText = `Change ${commitId} by ${sample(perforceUserNames)}@workspace on ${timebelt.toShortDate(commitTime)} ${timebelt.toShortTime(commitTime)}\n\n\tlorem changes\n\nAffected files ...\n\n\t... //mydepot/mystream/path/to/file.txt#2 edit`

                    await fs.outputFile(path.join(perforceMockRoot, commitId.toString()), commitText)
                }
                
                globalCommitCounter += commits

                // finally, write log based on current revision, we can use this to test soft linking
                const logText = 'some logs\n' +
                    '#p4-changes........#\n'+
                    `Change ${globalCommitCounter} on etc etc\n`+
                    '/#p4-changes........#\n'+
                    'more logs'

                // write jenkins build log
                await fs.outputFile(path.join(jenkinsMockRoot, jobName, 'logs', jobBuildCounter.toString()), logText)

           }


            // write job list - this contains all builds in job
            await fs.outputJson(path.join(jenkinsMockRoot, jobName, 'builds.json'), {
                _class : 'hudson.model.FreeStyleProject',
                allBuilds : [...Array(buildsPerJob - 1)].map((_ ,buildnumber) =>{
                    // count backwards
                    buildnumber = buildsPerJob - buildnumber - 1

                    // count backwards an hour per build
                    buildTime = timebelt.subtractHours(buildTime, 1)

                    // build should either pass or fail, but last build has a chance to remaining unfinished
                    let result = sample(['FAILURE', 'SUCCESS']) 
                    if (buildnumber === buildsPerJob)
                        result = sample([result, 'PENDING'])

                    return {
                        _class : 'hudson.model.FreeStyleBuild',
                        duration : Math.floor(Math.random() * 10000) + 100,
                        fullDisplayName : `${jobName} #${buildnumber}`,
                        id : buildnumber.toString(),
                        number : buildnumber,
                        result,
                        builtOn : sample(['Agent1', 'Agent2', 'Agent3']),
                        timestamp : buildTime.getTime()
                    }
                })   
            }, jsonOptions)


            __log.debug(`Wrote mock data for job ${jobName}`)
        }
    }

}