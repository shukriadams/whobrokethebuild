
const fs = require('fs-extra'),
    path = require('path'),
    errorHandler = require(_$+'helpers/errorHandler')

module.exports = function(app){

    
    /**
     * Mocks job list api call.
     */
    app.get('/jenkins/mock/api/json', async function(req, res){
        try {
            if (req.query.tree='jobs[name]')
                return res.send(await fs.readFile(path.join(__dirname, 'mock', 'jobs', 'jobs.json'), 'utf8'))
            
            res.end('Unsupported arg for /jenkins/mock/api/json')
        } catch(ex){
            errorHandler(res, ex)
        }
    })


    /**
     * Mocks endpoint that returns a full log for a given jenkins build @ buildNumber
     */
    app.get('/jenkins/mock/job/:jobname/:buildnumber/consoleText', async function(req, res){
        try {
            const logPath = path.join(__dirname, 'mock', 'jobs', req.params.jobname, 'logs', req.params.buildnumber)
            if (!await fs.pathExists(logPath))
                throw `Expected mock log not found @ ${logPath}`

            return res.send(await fs.readFile(logPath, 'utf8'))

        } catch(ex){
            errorHandler(res, ex)
        }
    })


    /**
     * Mocks endpoint that returns a list of commits for a given build
     */
    app.get('/jenkins/mock/job/:jobname/:buildnumber/api/json', async function(req, res){
        try {
            if (req.query.pretty === 'true' && req.query.tree === 'changeSet[items[commitId]]'){
                const commitsPath = path.join(__dirname, 'mock', 'jobs', req.params.jobname, 'commits', req.params.buildnumber)
                if (!await fs.pathExists(commitsPath))
                    throw `Expected commits mock json not found @ ${commitsPath}`

                res.send(await fs.readFile(commitsPath, 'utf8') )
            }
            else
                throw 'unhandled jenkins mock for route /jenkins/mock/job/:jobname/:buildnumber/consoleText'

        } catch(ex){
            errorHandler(res, ex)
        }
    })


    /**
     * Mocks endpoint that gets a list of recent builds for a job
     */
    app.get('/jenkins/mock/job/:jobname/api/json', async function(req, res){
        try {
             if (req.query.pretty === 'true' && req.query.tree === 'allBuilds[fullDisplayName,id,number,timestamp,duration,builtOn,result]'){
                const filepath = path.join(__dirname, 'mock', 'jobs', req.params.jobname, 'builds.json' )
                if (! await fs.pathExists(filepath))
                    throw `Expected mock file ${filepath} not found`

                res.send(await fs.readFile(filepath, 'utf8'))
            }
            else
                throw 'unhandled jenkins mock for route /jenkins/mock/job/:jobname/api/json'

        } catch(ex){
            errorHandler(res, ex)
        }
    })
    

}

