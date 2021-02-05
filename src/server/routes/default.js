const settings = require(_$+ 'helpers/settings'),
    appendCommonViewModel = require(_$+ 'helpers/appendCommonViewModel'),
    handlebars = require(_$+ 'helpers/handlebars'),
    errorHandler = require(_$+'helpers/errorHandler'),
    buildInvolvementLogic = require(_$+'logic/buildInvolvements'),
    pluginsManager = require(_$+'helpers/pluginsManager')

module.exports = function(express){
    

    /**
     * Simple logic-free HTTP get alive check 
     */
    express.get('/isalive', async (req, res)=>{
        res.send('1')
    })


    /**
     * 
     */
    express.get('/', async function (req, res) {
        try {

            const data = await pluginsManager.getExclusive('dataProvider'),
                view = await handlebars.getView('default'),
                model = {
                    default : {
                        bundlemode : settings.bundlemode,
                        bundle : settings.bundle
                    }
                }
            
            model.jobs = await data.getAllJobs()
                
            // add latest and breaking build to job
            for (let job of model.jobs){
                // extend 
                job.__breakingBuild = await data.getCurrentlyBreakingBuild(job.id)

                if (job.__breakingBuild){
                    
                    // extend
                    job.__breakingBuild.__buildInvolvements = buildInvolvementLogic.filterUniqueUser( await data.getBuildInvolementsByBuild(job.__breakingBuild.id))

                    for (const buildInvolvement of job.__breakingBuild.__buildInvolvements)
                        if (buildInvolvement.userId)
                            // extend 
                            buildInvolvement.__user = await data.getUser(buildInvolvement.userId)
                }

                job.__latestBuild = await data.getLatestBuild(job.id)
            }

            // sort jobs with breaking first
            model.jobs.sort((a, b)=> {
                return a.__breakingBuild && !b.__breakingBuild ? -1 :
                    !a.__breakingBuild && b.__breakingBuild ? 1 :
                    0
            })

            await appendCommonViewModel(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}
