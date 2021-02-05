const appendCommonViewModel = require(_$+ 'helpers/appendCommonViewModel'),
    pluginsManager = require(_$+'helpers/pluginsManager'),
    vcServerLogic = require(_$+'logic/VCServer'),
    errorHandler = require(_$+'helpers/errorHandler'),
    handlebars = require(_$+'helpers/handlebars')

module.exports = function(app){

    app.get('/settings/vcserver/:id?', async (req, res)=>{
        try {
            const 
                view = await handlebars.getView('settings/vcserver'),
                model = { },
                data = await pluginsManager.getExclusive('dataProvider')

            model.VCServerTypes = await pluginsManager.getTypeCodesOf('vcs')
            model.isCreate = !req.params.id
            
            if (req.params.id)
                model.vcserver = await data.getVCServer(req.params.id, { expected : true })

            await appendCommonViewModel(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })

    app.post('/settings/vcserver', async (req, res)=>{
        try {
            if (req.body.id)
                await vcServerLogic.update(req.body)
            else    
                await vcServerLogic.insert(req.body)

            res.json({
                foo : 'bar'
            })

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}

