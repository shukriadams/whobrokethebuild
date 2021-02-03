const commonModelHelper = require(_$+ 'helpers/commonModels'),
    pluginsManager = require(_$+'helpers/pluginsManager'),
    ciServerLogic = require(_$+'logic/CIServer'),
    errorHandler = require(_$+'helpers/errorHandler'),
    handlebars = require(_$+'helpers/handlebars')

module.exports = app => {

    app.get('/settings/ciserver/:id?', async (req, res) =>{
        try {
            const view = await handlebars.getView('settings/ciserver'),
                model = { },
                data = await pluginsManager.getExclusive('dataProvider')
    
            model.CIServerTypes = await pluginsManager.getTypeCodesOf('ciserver')
            model.isCreate = !req.params.id

            if (req.params.id){
                model.ciserver = await data.getCIServer(req.params.id)

                let ciServerPlugin = await pluginsManager.get(model.ciserver.type)
                    url = await model.ciserver.getUrl(),
                    availableJobs = [],
                    existingJobs = []
                
                try 
                {
                    existingJobs = await data.getAllJobsByCIServer(req.params.id)
                    availableJobs = await ciServerPlugin.getJobs(url)
                }catch(ex){
                    __log.error(ex)
                    model.error = JSON.stringify(ex)
                }

                model.jobs = []

                // flag jobs that are no longer available
                for (const job of existingJobs){
                    model.jobs.push(job)
                    job.__isImported = true
                    if (!availableJobs.find(available => available === job.name))
                        job.__noLongerAvailable = true
                }

                for (const availableJobName of availableJobs){
                    if(existingJobs.find(existingJob => existingJob.name === availableJobName))
                        continue

                    model.jobs.push({
                        name : availableJobName,
                        __nameUrlFriendly : encodeURI(availableJobName),
                        __isImported : false
                    })
                }

                model.jobs.sort((a,b)=>{
                    return a.name > b.name ? 1 :
                        b.name > a.name ? -1 :
                        0
                })
            }

            await commonModelHelper(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res,ex)
        }
    })

    app.delete('/settings/ciserver/:id', async (req, res) =>{
        try {
            const id = req.params.id,
                data = await pluginsManager.getExclusive('dataProvider')

            await data.removeCIServer(id)
            res.json({
                foo : 'bar'
            })

        } catch(ex){
            errorHandler(res,ex)
        }
    })

    app.post('/settings/ciserver', async function(req, res){
        try {
            if (req.body.id)
                await ciServerLogic.update(req.body)
            else    
                await ciServerLogic.insert(req.body)

            res.json({
                foo : 'bar'
            })

        } catch(ex){
            errorHandler(res,ex)
        }
    })
}

