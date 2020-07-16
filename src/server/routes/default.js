const 
    settings = require(_$+ 'helpers/settings'),
    commonModelHelper = require(_$+ 'helpers/commonModels'),
    handlebars = require(_$+ 'helpers/handlebars'),
    errorHandler = require(_$+'helpers/errorHandler'),
    pluginsManager = require(_$+'helpers/pluginsManager')

module.exports = function(express){
    
    /**
     * 
     */
    express.get('/', async function (req, res) {
        try {

            const
                data = await pluginsManager.getByCategory('dataProvider'),
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
                    job.__breakingBuild.__buildInvolvements = await data.getBuildInvolementsByBuild(job.__breakingBuild.id)

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

            await commonModelHelper(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}
