const { rm } = require('fs-extra')

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
        const data = await pluginsManager.getExclusive('dataProvider'),
            job = await data.getJob(build.jobId, { expected: true }),
            message = `Hey ${user.name || user.publicId}, ${job.name} is failing - more info can be found at ${urljoin(settings.localUrl, `build/${build.id}`)}`

        return message
    },


    /**
     * @param user User object
     * @param build Build object
     * @return {Promise<string>} 
     */
    async buildImplicatedMessage(user, build, quoteblock = ''){
        let data = await pluginsManager.getExclusive('dataProvider'),
            job = await data.getJob(build.jobId, { expected: true }),
            usersInvolved = build.involvements.map(involvement => involvement.externalUsername),
            isImplicated = false,
            log = build.logData ? 
                build.logData
                    .filter(r => r.type === 'error')
                    .map(r => r.text)
                    .join('\n') :
                `No log data available`

        // ensure log has content, if errors cannot be parsed, it will be blank
        log = log || 'Could not parse error from build log'
        // format for slack so log message is embeddded in quote block
        log = `${quoteblock}${log}${quoteblock}\n`

        // determine if the user being messaged is implicated in the build. This happens only if
        // a file belonging to the user has been directly marked as being "at fault" 
        for (const buildInvolvement of build.involvements){
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
        let message = `Hey ${user.name || user.publicId}, looks like you were involved in a build break for ${job.name}.\n${log}`
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