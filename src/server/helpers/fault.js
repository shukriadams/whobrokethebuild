module.exports = {
    
    /**
     * Adds faultchance to files in revision, based on how many times a file appears in error log
     * revision : Revision object, must have .files array, with .file property in each object in that array
     * buildErrors: array of strings, each string is an error line from build log.
     * 
     * @param {import('../types/revision').Revision} revision
     * @param {Array<import('../types/parsedBuildLogLine').ParsedBuildLogLine>} buildErrors
     */    
    processRevision(revision, buildErrors){
        const fsUtils = require('madscience-fsUtils')

        for (const file of revision.files){
            file.faultChance = 0

            const fileNameFragments = fsUtils.fullPathWithoutExtension(file.file)
                .split('/')
                .reverse() // we reverse to ensure that filename goes in first, and fails first if not present
                .filter(f => !!f.length) //remove empty entries

            for (const logLine of buildErrors)
                for (const fileNameFragment of fileNameFragments){
                    try {
                        // ignore warnings etc
                        if (logLine.type !== 'error')
                            continue

                        // need to try/catch this, some filename fragments can lead to invalid regexes.
                        // todo : find a better way to match these, this is kinda janky / error prone just on invalid regex errors
                        if (!logLine.text.match(new RegExp(`/${fileNameFragment}`, 'i')))
                            break                    
                        file.faultChance ++                    

                    } catch (ex){
                        __log.error(`Error trying to match fault to fileNameFragment "${fileNameFragment}"`, ex)
                    }

                }
        }

        let maxFaultScore = Math.max(...revision.files.map(file => file.faultChance))
        if (maxFaultScore > 0)
            revision.files.map(file =>{
                file.isFault = file.faultChance >= maxFaultScore
                return file
            })
    },

    
    /**
     * Gets a list of usernames who confirmed broke the given build. Usernames are either the User object .name, or the string
     * name from version control for a given commit that is known to be at fault.
     * 
     * Throws 'revisions not mapped yet' if the build passed in has not had its revisions mapped
     * 
     * @param {import('../types/build').Build} build A build 
     * @returns {Promise<Array<string>>} 
     */
    async getUsersWhoBrokeBuild(build){
        const usernames = [],
            pluginsManager = require(_$+'helpers/pluginsManager'),
            data = await pluginsManager.getExclusive('dataProvider')

        for (const buildInvolvement of build.involvements){
            const user = buildInvolvement.userId ? await data.getUser(buildInvolvement.userId) : null,
                username = user ? user.name : buildInvolvement.externalUsername
            
            if (!buildInvolvement.revisionObject)
                throw `revisions not mapped yet`

            if (!buildInvolvement.revisionObject.files.find(file => file.isFault === true))
                continue

            if (!usernames.includes(username))
                usernames.push(username)
        }

        return usernames
    }

}