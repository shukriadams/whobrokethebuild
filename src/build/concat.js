const glob = require('glob'),
    path = require('path'),
    os = require('os'),
    fs = require('fs-extra')

module.exports = async function(globPath, outPath, options){
    return new Promise((resolve, reject)=>{
        try {
            glob(globPath, options, async (err, files)=>{
                if (err)
                    return reject(err)
                
                let output = ''
                for (const file of files)
                    output += `${await fs.readFile(file)}${os.EOL}`
                
                await fs.ensureDir(path.dirname(path.resolve(outPath)))
                await fs.writeFile(outPath, output)
                resolve()
            })
        } catch(ex){
            reject(ex)
        }

    })

}