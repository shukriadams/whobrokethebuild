const pluginsHelper = require(_$+'helpers/pluginsManager'),
    constants = require(_$+'types/constants'),
    Build = require(_$+'types/build'),
    fs = require('fs-extra'),
    path = require('path'),
    BuildInvolvment = require(_$+'types/buildInvolvement'),
    urljoin = require('urljoin'),
    settings = require(_$+ 'helpers/settings'),
    httputils = require('madscience-httputils'),
    { uniqueNamesGenerator, adjectives, colors, animals } = require('unique-names-generator'),
    timebelt = require('timebelt')

module.exports = {

    validateSettings: async () => {
        return true
    },

    async initialize(){
        
        const jenkinsMockRoot = path.join(__dirname, '..', 'wbtb-jenkins', 'mock', 'lol_jobs'),
            perforceMockRoot = path.join(__dirname, '..', 'wbtb-peforce', 'mock', 'lol_jobs')

        if (await fs.exists(jenkinsMockRoot))
            return

        let jobsCount = 20,
            buildsPerJob = 100,
            minCommitsPerBuild = 0,
            maxCommitsPerBuild = 3,
            maxLogLines= 1000,
            commitEventId = 0,
            commitId = 0,
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

        await fs.outputJson(path.join(jenkinsMockRoot, 'jobs.json'), {
            _class : 'hudson.model.Hudson',
            jobs : jobNames.map(name => { return { _class : 'hudson.model.FreeStyleProject', name } })
        }, jsonOptions)

        for (let jobName of jobNames){
            for (let i = 1 ; i < buildsPerJob ; i ++) {
                commitEventId ++

                // generate nr of commits per commit event
                const commits = Math.floor(Math.random() * maxCommitsPerBuild) + minCommitsPerBuild,
                    logText = 'muh logs'

                // write commit event
                for(let j = 0; j < commits ; j ++){
                    await fs.outputJson(path.join(jenkinsMockRoot, jobName, 'commits', commitId.toString()), {
                        _class : 'hudson.model.FreeStyleBuild',
                        changeSet : {
                            _class : 'hudson.scm.SubversionChangeLogSet',
                            items : [...Array(commits)].map(()=>{
                                commitId ++
                                return {
                                    _class : 'hudson.scm.SubversionChangeLogSet$LogEntry',
                                    commitId : commitId.toString()
                                }
                            })
                        }
                    }, jsonOptions)

                    await fs.outputFile(path.join(jenkinsMockRoot, jobName, 'logs', commitId.toString()), logText)
                }

                __log.debug(`Wrote mock data for job ${jobName} - ${Math.floor((i/buildsPerJob)*100)}%`)
            }
        }
    }

}