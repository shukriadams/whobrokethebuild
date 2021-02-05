module.exports = {

    /**
     * Adds faultchance to files in revision, based on how many times a file appears in error log
     * revision : Revision object, must have .files array, with .file property in each object in that array
     * buildErrors: array of strings, each string is an error line from build log.
     * 
     */
    processRevision(revision, buildErrors){
        let fsUtils = require('madscience-fsUtils')

        for (let file of revision.files){
            file.faultChance = 0

            const fileNameFragments = fsUtils.fullPathWithoutExtension(file.file)
                .split('/')
                .reverse() // we reverse to ensure that filename goes in first, and fails first if not present
                .filter(f => !!f.length) //remove empty entries

            for (const errorLine of buildErrors){
                for (const fileNameFragment of fileNameFragments){
                    if (!errorLine.includes(`/${fileNameFragment}`))
                        break

                    file.faultChance ++                    
                }
            }

        }
    }

}