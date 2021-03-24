const settings = require(_$+ 'helpers/settings'),
    viewModelHelper = require(_$+'helpers/viewModel'),
    handlebars = require(_$+ 'helpers/handlebars'),
    errorHandler = require(_$+'helpers/errorHandler'),
    jobLogic = require(_$+'logic/job'),
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
            
            model.jobs = await jobLogic.getAllJobs()
                
            // add latest and breaking build to job
            for (let job of model.jobs){
                // extend 
                job.__breakingBuild = await data.getBuildThatBrokeJob(job.id)

                if (job.__breakingBuild){
                    
                    job.__brokenSince = job.__breakingBuild.ended

                    // extend
                    for (const buildInvolvement of job.__breakingBuild.involvements){
                        if (!buildInvolvement.revisionObject)
                            continue

                        buildInvolvement.__isFault = !!buildInvolvement.revisionObject.files.find(file => file.isFault === true)
                    }
                }

                job.__latestBuild = await data.getLatestBuild(job.id)
            }

            // sort jobs with breaking first
            model.jobs.sort((a, b)=> {
                return !!a.__breakingBuild && !b.__breakingBuild ? -1 :
                    !a.__breakingBuild && !!b.__breakingBuild ? 1 :
                    0
            })

            await viewModelHelper.layout(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}
