const globby = require('globby'),
    path = require('path'),
    os = require('os'),
    fs = require('fs-extra')

module.exports = async function(globPath, outPath, options){
    let files = await globby(globPath, options)
    
    let output = ''
    for (const file of files) {
        output += `${await fs.readFile(file)}${os.EOL}`
        console.log(`cancatenated ${file}`)
    }

    await fs.ensureDir(path.dirname(path.resolve(outPath)))
    await fs.writeFile(outPath, output)
}