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
        if (!!settings.sandboxMode && !!settings.plugins[thisType].sandboxMode && !settings.plugins[thisType].accessToken){
            __log.error(`Plugin "${thisType}" requires "accessToken" property. This is an official Slack API token string.`)
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
        return !!settings.plugins[thisType].accessToken
    },


    /**
     * Gets an array of all available channels in slack. Returns an empty array if slack has not been configured yet.
     */ 
    async getChannels(){
        const Slack = this.isSandboxMode() ? require('./mock/slack') : require('slack'),
            token = settings.plugins[thisType].accessToken

        if (!token){
            __log.warn('Cannot list slack channels, no token defined')
            return []
        }

        let slack = new Slack({ token }),
            slackQuery = await slack.conversations.list({ token, limit: 9999, types : 'public_channel,private_channel,mpim,im' }),
            channels = []

        for (let channel of slackQuery.channels)
            if (channel.name)
                channels.push({
                    id : channel.id,
                    name : channel.name
                })

        channels = channels.sort((a, b)=>{
            a = a && a.name ? a.name.toLowerCase() : ''
            b = b && b.name ? b.name.toLowerCase() : ''
            return a > b ? 1 :
                b > a ? -1 :
                0
        })

        return channels
    },
    

    /**
     * Sends an alert to a channel defined in slackContactMethod that the job is broken at the given build.
     * This does not check if the job is actually broken, it assumes that the client code calling this has
     * determined that.
     * 
     * Required by all "contact" plugins
     * 
     * @param {object} slackContactMethod contactMethod from job, written by this plugin 
     * @param {object} job job object to alert for
     * @param {object} breakingBuild build that broke job. Null if build is working
     */
    async alertGroup(slackContactMethod, job, breakingBuild, force = false){
        const data = await pluginsManager.getExclusive('dataProvider'),
            Slack = this.isSandboxMode() ? require('./mock/slack') : require('slack'),
            slack = new Slack({ token : settings.plugins[thisType].accessToken }),
            faultHelper = require(_$+'helpers/fault'),
            context = `slack_group_alert_${job.id}_${(job.lastBreakIncidentId || '')}_${breakingBuild ? 'fail':'pass'}`

        if (!this.__wbtb.enableMessaging){
            __log.debug(`Slack messaging disabled, blocked send`)
            return
        }

        if (settings.plugins[thisType].overrideChannelId)
            __log.info(`slackOverrideChannelId set, diverting post meant for channel ${slackContactMethod.channelId} to ${settings.plugins[thisType].overrideChannelId}`)

        // check if channel has already been informed about this build failure
        // generate a string of user names involved in build if build broke
        let contactLog = await data.getContactLogByContext(slackContactMethod.channelId, slackContactMethod.type, context)
        if (!force && contactLog)
            return

        const usersThatBrokeBuild = breakingBuild ? await faultHelper.getUsersWhoBrokeBuild(breakingBuild) : [],
            targetChannelId = settings.plugins[thisType].overrideChannelId || slackContactMethod.channelId,
            title_link = breakingBuild ? urljoin(settings.localUrl, `build/${breakingBuild.id}`) : urljoin(settings.localUrl, `job/${job.id}`),
            color = breakingBuild ? '#D92424' : '#007a5a',
            text = usersThatBrokeBuild.length ? `People involved : ${usersThatBrokeBuild.join(',')}` : '',
            title = breakingBuild ? 
                `${job.name} broke @ build #${breakingBuild.build}.` : 
                `${job.name} is working again.`,
            attachments = [
                {
                    fallback : title,
                    color,
                    title,
                    title_link,
                    text 
                }
            ]

        await slack.chat.postMessage({ token : settings.plugins[thisType].accessToken, channel : targetChannelId, text: title, attachments });

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
     * @param {string} context calling can supply its own context, else make one up from build. On system with multiple CI jobs on a single code base
     *                         there can be multiple job failures for a single revision, so it is best to use revision id as the context
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
    async alertUser(user, build, context = null, messageType = 'implicated', force = false){
        let data = await pluginsManager.getExclusive('dataProvider'),
            messageBuilder = await pluginsManager.getExclusive('messagebuilder'),
            Slack = this.isSandboxMode() ? require('./mock/slack') : require('slack'),
            slack = new Slack({ token : settings.plugins[thisType].accessToken }),
            usersInvolved = build.involvements.map(involvement => involvement.externalUsername)
            
        // if no context given, we force context to build id, that is, a user cannot be alerted for a build event more than once
        if (!context)
            context = build.id

        // convert to unique users
        usersInvolved = Array.from(new Set(usersInvolved)) 

        if (!this.__wbtb.enableMessaging){
            __log.debug(`Slack messaging disabled, blocked send`)
            return
        }

        if (build.logStatus === constants.BUILDLOGSTATUS_NOT_FETCHED || build.logStatus === constants.BUILDLOGSTATUS_UNPROCESSED){
            __log.warn(`Attepting to warn user on unprocessed log, build "${build.build}" to user "${user.name}"`)
            return
        }

        // check if user has already been informed about this build failure
        let contactLog = force ? null : await data.getContactLogByContext(user.id, thisType, context)
        if (contactLog)
            return

        let targetSlackId = user.pluginSettings[thisType] ? user.pluginSettings[thisType].slackId : null
        if (!force && settings.plugins[thisType].overrideUserId){
            targetSlackId = settings.plugins[thisType].overrideUserId
            __log.info(`slackOverrideUserId set, diverting post to meant for user slackid ${user.id} to override user id ${settings.plugins[thisType].overrideUserId}`)
        }
            
        if (!targetSlackId){
            __log.warn(`No slack token set for user "${user.id}:${user.name}". Cannot alert on build failure for "${build.id}:${build.build}".`)
            return
        }

        let conversation = await slack.conversations.open({ token : settings.plugins[thisType].accessToken, users : targetSlackId })
        if (!conversation.channel)
            throw new Exception({
                message : `unable to create conversation channel for user ${user.id}`
            })

        const message = messageType === 'implicated' ? 
            await messageBuilder.buildImplicatedMessage(user, build, '\`\`\`') :
            await messageBuilder.buildInterestedMessage(user, build)
        
        await slack.chat.postMessage({ token : settings.plugins[thisType].accessToken, channel : conversation.channel.id, text : message })

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