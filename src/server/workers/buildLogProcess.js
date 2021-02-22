(async ()=>{

    let { workerData, parentPort } = require('worker_threads'),
        logPath = workerData.logPath,
        logParserRequirePath = workerData.logParserRequirePath,
        /**
         * Walks through a  file one line at a time
         */
        stepThroughFile = async (path, onLine)=>{
            const fs = require('fs-extra')
            if (!fs.pathExists(path))
                throw `File ${path} does not exist`

            return new Promise((resolve, reject)=>{
                try {
                    const lineReader = require('readline').createInterface({
                        input: require('fs').createReadStream(path)
                    })
                    
                    lineReader.on('line', (line) => {
                        // send line to callback, along with callback of our own to resume next line
                        onLine(line, ()=>{
                            lineReader.resume()
                        })

                        lineReader.pause()
                    })

                    lineReader.on('close', () =>{
                        resolve()
                    })

                } catch(ex){
                    reject(ex)
                }
            })
        },
        logParser = require(logParserRequirePath),
        parsedItems = []

    await stepThroughFile(logPath, (logLine, next) =>{
        setImmediate(()=>{
           // ignore empty lines, parser will return "empty" warnigs for these
           if (!logLine.length)
               return
    
           const parsed = logParser.parse(logLine)
           if (parsed.length)
               parsedItems = parsedItems.concat(parsed)
            
            next()
        })
    })
    
    parentPort.postMessage(parsedItems)

})()
