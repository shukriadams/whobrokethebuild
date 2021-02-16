const urljoin = require('urljoin'),
    settings = require(_$+'helpers/settings'),
    pluginsManager = require(_$+'helpers/pluginsManager')

module.exports = {
    
    async validateSettings(){
        return true
    },


    /**
     * @param user User object
     * @param build Build object
     */
    async buildInterestedMessage(user, build){
        let message = `Build ${build.name} is failing. More info can be found at ${urljoin(settings.localUrl, `build/${build.id}`)}`
        return message
    },


    /**
     * @param user User object
     * @param build Build object
     * @return {Promise<string>} 
     */
    async buildImplicatedMessage(user, build, quoteblock = ''){
        let logHelper = require(_$+'helpers/log'),
            data = await pluginsManager.getExclusive('dataProvider'),
            job = await data.getJob(build.jobId, { expected: true }),
            buildInvolvements = await data.getBuildInvolementsByBuild(build.id),
            usersInvolved = buildInvolvements.map(involvement => involvement.externalUsername),
            isImplicated = false,
            log = job.logParser ? 
                await logHelper.parseErrorsFromFileToString(build.logPath, job.logParser) :
                `No log parser set for ${job.name}`

        // ensure log has content, if errors cannot be parsed, it will be blank
        log = log || 'Could not parse error from build log'
        // format for slack so log message is embeddded in quote block
        log = `${quoteblock}${log}${quoteblock}\n`

        // determine if the user being messaged is implicated in the build. This happens only if
        // a file belonging to the user has been directly marked as being "at fault" 
        for (const buildInvolvement of buildInvolvements){
            //
            if (!buildInvolvement.userId !== user.id)
                continue

            // ensure that buildInvolvement has been mapped, if not, abort this send, we'll try later
            if (!buildInvolvement.revisionObject)
                continue

            for (const file of buildInvolvement.revisionObject.files)
                if (file.isFault) {
                    isImplicated = true
                    break
                }
        }

        // get unique users
        usersInvolved = Array.from(new Set(usersInvolved)) 
        let message = `You were involved in a build break for ${job.name}, ${build.build}\n${log}`
        if (usersInvolved.length > 1){
            if (isImplicated)
                message += `There were ${usersInvolved.length} people in this break, but it's likely your code broke it.\n`
            else
                message += `There were ${usersInvolved.length} people in this break, and your code was likely not the cause of the break.\n`
        } else {
            message += `You were the only person involved in this break.\n`
        }
        
        message += `More info : ${urljoin(settings.localUrl, `build/${build.id}`)}`

        return message
    }
    
}