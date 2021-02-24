/**
 * Parses build logs. Runs as a worker thread.
 */ 
(async ()=>{

    let { workerData, parentPort } = require('worker_threads'),
        fsUtils = require('madscience-fsUtils'),
        logPath = workerData.logPath,
        logParserRequirePath = workerData.logParserRequirePath,
        logParser = require(logParserRequirePath),
        parsedItems = []

    await fsUtils.lineStepThroughFile (logPath, (logLine, next) =>{
        setImmediate(()=>{

            // ignore empty lines, parser will return "empty" warnings for these, which are unnecessary
            if (logLine.length){
                const parsed = logParser.parse(logLine)
                if (parsed.length)
                    parsedItems = parsedItems.concat(parsed)
            }
            
            next()
        })
    })
    
    parentPort.postMessage(parsedItems)

})()
