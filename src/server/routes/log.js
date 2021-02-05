const settings = require(_$+ 'helpers/settings'),
    commonModelHelper = require(_$+ 'helpers/commonModels'),
    fs = require('fs-extra'),
    fsUtils = require('madscience-fsUtils'),
    errorHandler = require(_$+'helpers/errorHandler'),
    handlebars = require(_$+'helpers/handlebars')

module.exports = function(app){

    app.get('/log/:type?', async function(req, res){
        try {
            let view = await handlebars.getView('log'),
                model = { },
                logFiles = await fsUtils.readFilesInDir(settings.logPath)

            logFiles = logFiles.sort()
            model.log = `No logs found`

            // show the last log found, this should be the most recent
            if (logFiles.length){
                let rawLog = await fs.readFile(logFiles[logFiles.length - 1], 'utf8')
                rawLog = rawLog.replace(/\r\n/g,'\n') // force unix line ends
                rawLog = rawLog.split('\n') // divide into lines
                rawLog = rawLog.filter(line => !!line) // remove empty lines
                rawLog = rawLog.reverse()
                model.log = ''
                
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

