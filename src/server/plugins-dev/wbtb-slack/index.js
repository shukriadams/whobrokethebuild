const Exception = require(_$+'types/exception'),
    ContactLog = require(_$+'types/contactLog'),
    constants = require(_$+'types/constants'),
    urljoin = require('urljoin'),
    thisType = 'wbtb-slack',    
    settings = require(_$+'helpers/settings')

module.exports = {

    /**
     * required by plugin interface
     */
    getTypeCode(){
        return 'wbtb-slack'
    },
    
    /**
     * required by plugin interface
     */
    async validateSettings() {
        return true
    },

    /**
     * required by plugin interface
     */
    getDescription(){
        return {
            id : 'wbtb-slack',
            category : 'contact',
            name : 'Slack',
            hasUI: true
        }
    },


    /**
     * Required by all "contact" plugins
     */
    async canTransmit(){
        const data = await pluginsManager.getExclusive('dataProvider'),
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
        let userString = ''
        if (delta === constants.BUILDDELTA_CAUSEBREAK){
            for (const buildInvolvement of buildInvolvements){
                const user = buildInvolvement.userId ? await data.getUser(buildInvolvement.userId) : null
                userString += `${user ? user.name : buildInvolvement.externalUsername}, `
            }

            if (userString.length){
                userString = userString.substring(0, userString.length - 2) // clip off trailing ', '
                userString = `People : ${userString}`
            }
        }

        const message = delta === constants.BUILDDELTA_CAUSEBREAK ? `Build ${job.name} is broken.\n${userString}` : `Build ${job.name} is working again`
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
        const data = await pluginsManager.getExclusive('dataProvider'),
            token = await data.getPluginSetting('wbtb-slack', 'token'),
            Slack = settings.sandboxMode ? require('./mock/slack') : require('slack'),
            slack = new Slack({ token : token.value }),
            job = await data.getJob(build.jobId),
            logParser = job.logParser ? pluginsManager.get(job.logParser) : null,
            context = `build_fail_${build.id}`

        // check if user has already been informed about this build failure
        let contactLog = await data.getContactLogByContext(user.id, thisType, context)
        if (contactLog)
            return

        let conversation = await slack.conversations.open({ token : token.value, users : user.contactMethods[thisType].slackId })
        if (!conversation.channel)
            throw new Exception({
                message : `unable to create conversation channel for user ${user.id}`
            })

        const buildLink = urljoin(settings.localUrl, `builds/${build.id}`)
        log = logParser ? `\`\`\`${(await logParser.parseErrors(build.log))}\`\`\`\n` : ''
        const message = `You were involved in a build break for ${job.name}, ${build.build}\n${log}More info : ${buildLink}`
        await slack.chat.postMessage({ token : token.value, channel : conversation.channel.id, text : message })

        contactLog = ContactLog()
        contactLog.receiverContext = user.id
        contactLog.type = thisType
        contactLog.eventContext = context
        contactLog.created = new Date().getTime()

        await data.insertContactLog(contactLog)
    }



}