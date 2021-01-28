const settings = require(_$+ 'helpers/settings'),
    commonModelHelper = require(_$+ 'helpers/commonModels'),
    fs = require('fs-extra'),
    fsUtils = require('madscience-fsUtils'),
    errorHandler = require(_$+'helpers/errorHandler'),
    handlebars = require(_$+'helpers/handlebars')

module.exports = function(app){

    app.get('/log/:type?', async function(req, res){
        try {
            const view = await handlebars.getView('log'),
                type = req.params.type || 'info',
                model = { }

            let logFiles = await fsUtils.readFilesInDir(settings.logPath)
            logFiles = logFiles.filter(file => file.includes(`${type}.`))
            logFiles = logFiles.sort()

            model.log = `No logs found`

            if (logFiles.length){
                let rawLog = await fs.readFile(logFiles[logFiles.length - 1], 'utf8')
                rawLog = rawLog.includes('\r\n') ? rawLog.split('\r\n') : rawLog.split('\n')
                rawLog = rawLog.filter(line => !!line) // remove empty lines
                model.log =  ''
                
                for (const line of rawLog){
                    const lineParsed = JSON.parse(line)
                    model.log += `${lineParsed.timestamp} : ${lineParsed.message}\n` 
                }
            }
            
            await commonModelHelper(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })

}

