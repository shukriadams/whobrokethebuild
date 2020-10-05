const Exception = require(_$+'types/exception'),
    ContactLog = require(_$+'types/contactLog'),
    constants = require(_$+'types/constants'),
    pluginsManager = require(_$+'helpers/pluginsManager'),
    urljoin = require('urljoin'),
    thisType = 'wbtb-slack',    
    settings = require(_$+'helpers/settings')

module.exports = {

    
    /**
     * required by plugin interface
     */
    async validateSettings() {
        return true
    },


    /**
     * Required by all "contact" plugins
     */
    async canTransmit(){
        const 
            data = await pluginsManager.getExclusive('dataProvider'),
            token = await data.getPluginSetting('wbtb-slack', 'token')
        
        if (!token || !token.value)
            throw new Exception({
                message : `wbtb-slack requires a "token" setting to function`
            })
    },


    /**
     * Gets an array of all available channels in slack. Returns an empty array if slack has not been configured yet.
     */ 
    async getChannels(){
        const data = await pluginsManager.getExclusive('dataProvider'),
            Slack = settings.sandboxMode ? require('./mock/slack') : require('slack'),
            tokenSetting = await data.getPluginSetting('wbtb-slack', 'token')

        if (!tokenSetting)
            return []

        const slack = new Slack({ token : tokenSetting.value }),
            slackQuery = await slack.channels.list({ token : tokenSetting.value }),
            channels = []

        for (let channel of slackQuery.channels)
            channels.push({
                id : channel.id,
                name : channel.name
            })

        return channels
    },
    

    /**
     * slackContactMethod : contactMethod from job, written by this plugin 
     * job : job object 
     * delta : the change (constants.BUILDDELTA_*)
     */
    async alertChannel(slackContactMethod, job, build, delta){
        const data = await pluginsManager.getExclusive('dataProvider'),
            token = await data.getPluginSetting('wbtb-slack', 'token'),
            Slack = settings.sandboxMode ? require('./mock/slack') : require('slack'),
            slack = new Slack({ token : token.value }),
            buildInvolvements = await data.getBuildInvolementsByBuild(build.id),
            context = `build_${delta}_${build.id}`

        // check if channel has already been informed about this build failure
        let contactLog = await data.getContactLogByContext(slackContactMethod.channelId, slackContactMethod.type, context)
        if (contactLog)
            return

        // generate a string of user names involved in build if build broke
        let userString = '',
            userUniqueCheck = []

        if (delta === constants.BUILDDELTA_CAUSEBREAK){
            for (const buildInvolvement of buildInvolvements){
                const user = buildInvolvement.userId ? await data.getUser(buildInvolvement.userId) : null,
                    username = user ? user.name : buildInvolvement.externalUsername

                if (userUniqueCheck.indexOf(username) === -1){
                    userUniqueCheck.push(username)
                    userString += `${username}, `
                }
            }

            if (userString.length){
                userString = userString.substring(0, userString.length - 2) // clip off trailing ', '
                userString = `People involved : ${userString}`
            }
        }

        const buildLink = urljoin(settings.localUrl, `build/${build.id}`)
        const message = delta === constants.BUILDDELTA_CAUSEBREAK ? 
            `Build ${job.name} is broken.\n${userString}. \nMore info : ${buildLink}` : 
            `Build ${job.name} is working again`

        const response = await slack.chat.postMessage({ token : token.value, channel : slackContactMethod.channelId, text : message });

        contactLog = ContactLog()
        contactLog.receiverContext = slackContactMethod.channelId
        contactLog.type = slackContactMethod.type
        contactLog.eventContext = context
        contactLog.created = new Date().getTime()

        await data.insertContactLog(contactLog)
    },


    /**
     * Sends a message to user that that user was involved in build break
     * user : user object
     * slackContactMethod : contactMethod from user object
     * build : build object for failing build
     */
    async alertBrokenBuild(user, build){
        let data = await pluginsManager.getExclusive('dataProvider'),
            token = await data.getPluginSetting('wbtb-slack', 'token'),
            Slack = settings.sandboxMode ? require('./mock/slack') : require('slack'),
            slack = new Slack({ token : token.value }),
            job = await data.getJob(build.jobId),
            logParser = job.logParser ? pluginsManager.get(job.logParser) : null,
            context = `build_fail_${build.id}`,
            buildInvolvements = await data.getBuildInvolementsByBuild(build.id),
            usersInvolved = buildInvolvements.map(involvement => involvement.externalUsername)

        // convert to unique users
        usersInvolved = Array.from(new Set(usersInvolved)) 

        // check if user has already been informed about this build failure
        let contactLog = await data.getContactLogByContext(user.id, thisType, context)
        if (contactLog)
            return console.log(`user ${user} already alerted for ${context}`)

        // there are multiple users involved, determine if the user being targetted was likely responsible for the break
        let isImplicated = false
        if (usersInvolved.length)
            for (const buildInvolvement of buildInvolvements){
                
                // ensure that buildInvolvement has been mapped, if not, abort this send, we'll try later
                if (!buildInvolvement.revisionObject)
                    return

                for (const file of buildInvolvement.revisionObject.files)
                   if (file.faultChance > .5) {
                        isImplicated = true
                        break
                   }
            }


        let conversation = await slack.conversations.open({ token : token.value, users : user.contactMethods[thisType].slackId })
        if (!conversation.channel)
            throw new Exception({
                message : `unable to create conversation channel for user ${user.id}`
            })

        let buildLink = urljoin(settings.localUrl, `build/${build.id}`),
            log = logParser ? `\`\`\`${(await logParser.parseErrors(build.log))}\`\`\`\n` : '',
            message = `You were involved in a build break for ${job.name}, ${build.build}\n${log}`

        if (usersInvolved.length > 1){
            if (isImplicated)
                message += `There were ${usersInvolved.length} people in this break, but it's likely your code broke it.\n`
            else
                message += `There were ${usersInvolved.length} people in this break, but don't worry, it looks like you're an innocent bystander.\n`
        } else {
            message += `You were the only person involved in this break.\n`
        }

        message += `More info : ${buildLink}`
        await slack.chat.postMessage({ token : token.value, channel : conversation.channel.id, text : message })

        contactLog = ContactLog()
        contactLog.receiverContext = user.id
        contactLog.type = thisType
        contactLog.eventContext = context
        contactLog.created = new Date().getTime()

        await data.insertContactLog(contactLog)
    }



}