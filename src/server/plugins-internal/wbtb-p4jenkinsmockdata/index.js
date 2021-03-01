module.exports = {

    validateSettings: async () => {
        return true
    },

    async initialize(){
        
        const path = require('path'),
            fs = require('fs-extra'),
            jenkinsMockRoot = path.join(__dirname, '..', 'wbtb-jenkins', 'mock', 'jobs'),
            perforceMockRoot = path.join(__dirname, '..', 'wbtb-perforce', 'mock', 'revisions')

        if (await fs.pathExists(jenkinsMockRoot))
            return

        let { uniqueNamesGenerator } = require('unique-names-generator'),
            thisType = 'wbtb-p4jenkinsmockdata',
            sample = require('lodash.sample'),
            timebelt = require('timebelt'),
            Chance = require('chance'),
            settings = require(_$+'helpers/settings'),
            pluginsManager = require(_$+'helpers/pluginsManager'),
            LoremIpsum = require('lorem-ipsum').LoremIpsum,
            lorem = new LoremIpsum({ wordsPerSentence: { max: 20, min: 10 }}),
            chance = new Chance(),
            jobsCount = settings.plugins[thisType].jobsCount || 10,
            buildsPerJob = settings.plugins[thisType].buildsPerJob || 10,
            minCommitsPerBuild = settings.plugins[thisType].minCommitsPerBuild || 0,
            maxCommitsPerBuild = settings.plugins[thisType].maxCommitsPerBuild || 3,
            minLogLines = settings.plugins[thisType].minLogLines || 100,
            maxLogLines = settings.plugins[thisType].maxLogLines || 10000,
            globalCommitCounter = 1,
            data = await pluginsManager.getExclusive('dataProvider'),
            users = await data.getAllUsers(),
            vsServers = await data.getAllVCServers(),
            jsonOptions = { spaces : 4 },
            verbs = ['Call of', 'Rage against the', 'Attack on', 'Battle of', 'The War for', 'Operation for', 'Alliance of', 'Creed of', 'League of','Quest of', 'Quest for'],
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
                const commitCountInThisBuild = chance.integer({ min : minCommitsPerBuild, max: maxCommitsPerBuild }) 

                // write build file, this can contain 1 or more commits, but there is one per build
                await fs.outputJson(path.join(jenkinsMockRoot, jobName, 'commits', jobBuildCounter.toString()), {
                    _class : 'hudson.model.FreeStyleBuild',
                    changeSet : {
                        _class : 'hudson.scm.SubversionChangeLogSet',
                        items : [...Array(commitCountInThisBuild)].map((_,commit)=>{
                            return {
                                _class : 'hudson.scm.SubversionChangeLogSet$LogEntry',
                                commitId : (globalCommitCounter + commit).toString()
                            }
                        })
                    }
                }, jsonOptions)

                function revisionToString(r){
                    return `Change ${r.revision} by ${r.user}@${r.workspace} on ${timebelt.toShortDate(r.date)} ${timebelt.toShortTime(r.date)}\n\n\t${r.description}\n\nAffected files ...\n\n${r.files.map(file => `... ${file.file}#${file.version} ${file.change}\n`).join('')}\n\nDifferences ...\n\n`
                }

                let commitsInThisBuild = []
                // write perforce commits - there can be multiple tied to a single build event
                for(let j = 0; j < commitCountInThisBuild ; j ++){
                    const commitId = globalCommitCounter + j

                    // write perforce commit
                    commitTime = timebelt.subtractHours(commitTime, 1.1)
                    const commit = {
                        revision : commitId,
                        user : sample(perforceUserNames),
                        workspace : 'workspace',
                        date : commitTime,
                        description : lorem.generateWords(chance.integer({min : 1, max: 20})),
                        files : [...Array(chance.integer({min : 1, max: 5}))].map(()=>{
                            return {
                                file : `//${lorem.generateWords(chance.integer({min : 5, max: 10})).split(' ').join('/')}.${sample(['txt', 'cs', 'cpp', 'obj'])}`,
                                version : chance.integer({min : 5, max: 10}),
                                change : sample(['add', 'edit', 'delete']),
                                differences : ['whatever']
                            }
                        })
                    }

                    commitsInThisBuild.push(commit)

                    await fs.outputFile(path.join(perforceMockRoot, commitId.toString()), revisionToString(commit))
                }
                
                globalCommitCounter += commitCountInThisBuild

                // finally, write log based on current revision, we can use this to test soft linking
                let hasError = false,
                    logLines = chance.integer({ min : minLogLines, max: maxLogLines}),
                        logText = 'some logs\n' +
                            '#p4-changes........#\n'+
                            `Change ${globalCommitCounter} on etc etc\n`+
                            '#/p4-changes........#\n'

                for (let i = 0 ; i < logLines ; i ++){
                    logText += `${lorem.generateWords()}\n` 

                    if (!hasError && (chance.bool() || i === logLines - 1)){
                        hasError = true
                        let commitThatFailed = sample(commitsInThisBuild),
                            fileInCommit = commitThatFailed ? sample(commitThatFailed.files) : null

                        // use unreal error pattern : file-path : some-text error some-code : some-explanation
                        logText += `${fileInCommit ? fileInCommit.file : ''} : ${lorem.generateWords(chance.integer({min : 3, max: 5}))} Error : C1234: ${lorem.generateWords(chance.integer({min : 3, max: 5}))}\n`
                    }
                }

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
                        duration : Math.floor(Math.random() * 1000000) + 100000,
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