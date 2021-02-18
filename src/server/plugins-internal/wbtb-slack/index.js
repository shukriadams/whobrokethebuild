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
        if (!settings.slackAccessToken){
            __log.error(`slack plugin requires "slackAccessToken" property on global settings`)
            return false
        }

        return true
    },


    /**
     * Returns true of all plugins or this plugin is running in sandbox mode
     */
    isSandboxMode(){
        // individual plugin config always wins, if set
        if (this.__wbtb.sandboxMode !== undefined)
            return !!this.__wbtb.sandboxMode

        // if global sandbox mode is explicitly on, allow through
        if (settings.sandboxMode === true)
            return true

        return false
    },


    /**
     * Required by all "contact" plugins
     */
    async canTransmit(){
        const data = await pluginsManager.getExclusive('dataProvider'),
            token = await data.getPluginSetting(thisType, 'token')
        
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
            Slack = this.isSandboxMode() ? require('./mock/slack') : require('slack'),
            tokenSetting = await data.getPluginSetting(thisType, 'token')

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
     * Required by all "contact" plugins
     * @param {object} slackContactMethod contactMethod from job, written by this plugin 
     * @param {object} job job object to alert for
     * @param {object} build build object to alert for
     */
    async alertGroup(slackContactMethod, job, build){
        const data = await pluginsManager.getExclusive('dataProvider'),
            Slack = this.isSandboxMode() ? require('./mock/slack') : require('slack'),
            slack = new Slack({ token : settings.slackAccessToken }),
            buildInvolvements = await data.getBuildInvolementsByBuild(build.id),
            context = `build_${build.status}_${build.id}`

        if (!this.__wbtb.enableMessaging){
            __log.debug(`Slack messaging disabled, blocked send`)
            return
        }

        // allow alerts only on passing or failing builds
        if (build.status !== constants.BUILDSTATUS_FAILED && build.status !== constants.BUILDSTATUS_PASSED)
            return

        // check if channel has already been informed about this build failure
        // generate a string of user names involved in build if build broke
        let userString = '',
            userUniqueCheck = [],
            contactLog = await data.getContactLogByContext(slackContactMethod.channelId, slackContactMethod.type, context)

        if (contactLog)
            return

        if (build.status === constants.BUILDSTATUS_FAILED){
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

        const targetChannelId = settings.slackOverrideChannelId || slackContactMethod.channelId,
            buildLink = urljoin(settings.localUrl, `build/${build.id}`),
            message = build.status === constants.BUILDSTATUS_FAILED ? 
                `Build ${job.name} is broken.\n${userString}. \nMore info : ${buildLink}` : 
                `Build ${job.name} is working again`

        if (settings.slackOverrideChannelId)
            __log.info(`slackOverrideChannelId set, diverting post meant for channel ${slackContactMethod.channelId} to ${settings.slackOverrideChannelId}`)
    
        await slack.chat.postMessage({ token : settings.slackAccessToken, channel : targetChannelId, text : message });

        contactLog = new ContactLog()
        contactLog.receiverContext = slackContactMethod.channelId
        contactLog.type = slackContactMethod.type
        contactLog.eventContext = context
        contactLog.created = new Date().getTime()

        await data.insertContactLog(contactLog)
        __log.info(`alert sent to slack channel ${targetChannelId}`)
    },


    /**
     * @param {object} user  User to contact
     * @param {object} build Build that broke
     * @param {string} messageType type of message to sent
     * @param {boolean} force if true, message will be sent even if it has been sent before
     * 
     * Required by all "contact" plugins
     * Sends a message to user that that user was involved in build break
     * user : user object
     * slackContactMethod : contactMethod from user object
     * build : build object for failing build
     * force : force send the message, ignore if it has already been sent. for testing only
     */
    async alertUser(user, build, messageType = 'implicated', force = false){
        let data = await pluginsManager.getExclusive('dataProvider'),
            messageBuilder = await pluginsManager.getExclusive('messagebuilder'),
            Slack = this.isSandboxMode() ? require('./mock/slack') : require('slack'),
            slack = new Slack({ token : settings.slackAccessToken }),
            context = `build_fail_${build.id}`,
            buildInvolvements = await data.getBuildInvolementsByBuild(build.id),
            usersInvolved = buildInvolvements.map(involvement => involvement.externalUsername)

        // convert to unique users
        usersInvolved = Array.from(new Set(usersInvolved)) 

        if (!this.__wbtb.enableMessaging){
            __log.debug(`Slack messaging disabled, blocked send`)
            return
        }

        if (!build.logPath){
            __log.warn(`Attepting to warn user on build that has no local log, build "${build.id}" to user "${user.id}"`)
            return
        }

        // check if user has already been informed about this build failure
        let contactLog = force ? null : await data.getContactLogByContext(user.id, thisType, context)
        if (contactLog){
            __log.info(`Skipping sending to alert for build "${build.id}" to user "${user.id}", alert has already been sent`)
            return
        }

        const targetSlackId = settings.slackOverrideUserId || (user.pluginSettings[thisType] && user.pluginSettings[thisType].slackId)
        if (settings.slackOverrideUserId)
            __log.info(`slackOverrideUserId set, diverting post to meant for user slackid ${user.id} to override user id ${settings.slackOverrideUserId}`)

        let conversation = await slack.conversations.open({ token : settings.slackAccessToken, users : targetSlackId })
        if (!conversation.channel)
            throw new Exception({
                message : `unable to create conversation channel for user ${user.id}`
            })

        const message = messageType === 'implicated' ? 
            await messageBuilder.buildImplicatedMessage(user, build, '\`\`\`') :
            await messageBuilder.buildInterestedMessage(user, build)
        
        await slack.chat.postMessage({ token : settings.slackAccessToken, channel : conversation.channel.id, text : message })

        // log that message has been sent, this will be used to prevent the same user from being informed of the same build error
        contactLog = new ContactLog()
        contactLog.receiverContext = user.id
        contactLog.type = thisType
        contactLog.eventContext = context
        contactLog.created = new Date().getTime()

        await data.insertContactLog(contactLog)
        __log.info(`alert sent to slack user via personal channel ${conversation.channel.id}`)
    },


}