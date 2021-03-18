const fs = require('fs-extra'),
    settings = require(_$+ 'helpers/settings'),
    path = require('path'),
    errorHandler = require(_$+'helpers/errorHandler')

module.exports = function(app){

    app.get('/image/:path', async(req, res)=>{
        try {
            res.writeHead(200, { 'content-type' : 'image/jpg' })
            const filePath =  path.join(settings.staticImagesFolder, req.params.path)
            if (!await fs.exists(filePath)){
                res.status(404)
                return res.end()
            }
                
            fs.createReadStream(filePath).pipe(res)
        } catch(ex) {
            errorHandler(res, ex)
        }
    })

}
