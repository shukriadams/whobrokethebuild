let concatenateCss = require('./concatenate-css'),
    concatenateJs = require('./concatenate-js'),
    fs = require('fs-extra'),
    chokidar = require('chokidar'),
    spawn = require('cross-spawn'),
    process = require('process'),
    argv = require('minimist')(process.argv.slice(2)),
    isFast = !!argv['fast'],
    path = require('path'),
    sass = require('node-sass'),
    _expressProcess = null,
    _triggerFile = null

/** 
 * Converts a Sass file map to its destination compiled css path in ./tmp folder
 */
function mapSassToCss(file){
    return path.join(
        './.tmp/css',
        path.basename(file).substr(0, path.basename(file).length - 5) + '.css'); // remove .scss extension
}


/** 
 * Compiles a Sass file to Css. CSS is written to ./tmp/css folder.
 */
async function compileSassFile(file){
    return new Promise(function(resolve, reject){

        try {
            sass.render({
                file: file,
                sourceComments: true
            }, function(err, result){
                if (err){
                    console.log(err)
                    return resolve(err)
                }

                const outfile = mapSassToCss(file)
                            
                fs.writeFileSync(outfile, result.css)
                console.log(`compiled ${outfile}`)
                resolve()
        
            })
        }catch(ex){
            reject(ex)
        }
    })
}


async function handleJSEvent(){
    await concatenateJs()
}

/** 
 * Called by SassWatcher when a sass file is added or changed. Compiles the sass that triggered
 * event, then concats all css files in ./tmp and places it in Express public folder. To improve
 * performance, if multiple Sass files trigger simultaneously concating is done only after the last
 * Sass file is compiled.
 */
async function handleSassEvent(file){
    _triggerFile = file
    
    await compileSassFile(file)
    
    if (_triggerFile === file){
        await concatenateCss()
        console.log(`concatenated css after last change to ${file}`)
    }
}

/** 
 * Starts Express server. If Express is already started, kills the existing process.
 */
function startExpress(){
    if (_expressProcess){
        console.log('stopping existing express process')
        _expressProcess.kill('SIGINT')
    }
    
    const breakSwitch = process.argv.includes('--brk') ? '-brk' : ''
    if (breakSwitch)
        console.log(`BREAK enabled`)

    _expressProcess = spawn('node', [`--inspect${breakSwitch}=0.0.0.0:3001`, 'index'], { cwd : process.cwd(), env: process.env })

    _expressProcess.stdout.on('data', function (data) {
        console.log(data.toString('utf8'))
    })
     
    _expressProcess.stderr.on('data', function (data) {
        console.log(data.toString('utf8'))
    })
}


(async function(){
    // set up required paths
    if(!isFast)
        await fs.remove('./.tmp/css')
        
    await fs.ensureDir('./.tmp/css')

    // start watching sass files
    let sassWatcher = chokidar.watch(['./components/**/*.scss'], {
        persistent: true,
        usePolling: true,
        ignoreInitial : isFast
    })

    sassWatcher
        .on('add', async function(file) {
            await handleSassEvent(file)
        })
        .on('change', async function(file){
            await handleSassEvent(file)
        })
        .on('unlink', async function(file){
            const outfile = mapSassToCss(file)
            await fs.remove(outfile)
            await concatenateCss()
            console.log(`processed delete ${file}`)
        })

            // start watching sass files
    let jsWatcher = chokidar.watch(['./components/**/*.js'], {
        persistent: true,
        usePolling: true,
        ignoreInitial : isFast
    })

    jsWatcher
        .on('add', async function() {
            await handleJSEvent()
        })
        .on('change', async function(){
            await handleJSEvent()
        })
        .on('unlink', async function(){
            await handleJSEvent()
        })

    console.log('Watching...')
})()
