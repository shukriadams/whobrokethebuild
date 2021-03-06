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

    areGroupAlertsDeletable(){
        return true
    },

    async groupAlertSent(slackContactMethod, job, incidentId){
        const data = await pluginsManager.getExclusive('dataProvider'),
            context = `${thisType}_group_alert_${job.id}_${incidentId}`,
            contactLog = await data.getContactLogByContext(slackContactMethod.channelId, slackContactMethod.type, context)

        return contactLog != null && contactLog.data.withdrawn !== true
    },


    /**
     * @param {object} slackContactMethod
     * @param {import('../../types/job').Job} job
     * @param {string} buildId Id of the build to withdraw message for
     * @returns {string} 
     */
    async deleteGroupAlert(slackContactMethod, job, buildId){
        const data = await pluginsManager.getExclusive('dataProvider'),
            Slack = this.isSandboxMode() ? require('./mock/slack') : require('slack'),
            slack = new Slack({ token : settings.plugins[thisType].accessToken }),
            context = `${thisType}_group_alert_${job.id}_${(buildId)}`

        let contactLog = await data.getContactLogByContext(slackContactMethod.channelId, slackContactMethod.type, context)
        if (!contactLog){
            console.warn(`could not find contactLog record for alert on build ${buildId}`)
            return 'alert not found'
        }

        const ts = contactLog.data.ts
        if (!ts){
            console.warn(`contactLog record for alert on build ${buildId} does not have a data.ts value, delete not possible`)
            return 'untraceable alert'
        }

        const targetChannelId = settings.plugins[thisType].overrideChannelId || slackContactMethod.channelId

        await slack.chat.delete({ token : settings.plugins[thisType].accessToken, channel : targetChannelId, ts, as_user : true });
        contactLog.data.withdrawn = true
        await data.updateContactLog(contactLog)
        return 'alert deleted'
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
     * @param {object} breakingBuild build that broke job
     */
    async alertGroupBuildBreaking(slackContactMethod, job, incidentId, force = false){
        const data = await pluginsManager.getExclusive('dataProvider'),
            Slack = this.isSandboxMode() ? require('./mock/slack') : require('slack'),
            slack = new Slack({ token : settings.plugins[thisType].accessToken }),
            faultHelper = require(_$+'helpers/fault'),
            context = `${thisType}_group_alert_${job.id}_${(incidentId)}`

        // Do this as early as possible in method, we will be spamming this method
        // check if channel has already been informed about this build failure
        // generate a string of user names involved in build if build broke
        let contactLog = await data.getContactLogByContext(slackContactMethod.channelId, slackContactMethod.type, context),
            usersThatBrokeBuild

        if (!force && contactLog)
            return

        // if we're forcing and contact log exists, we need to remove the previous log, duplicates are not allowed in db
        if (contactLog)
            await data.removeContactLog(contactLog.id)

        if (!this.__wbtb.enableMessaging){
            __log.debug(`Slack messaging disabled, blocked send`)
            return
        }

        if (settings.plugins[thisType].overrideChannelId)
            __log.info(`slackOverrideChannelId set, diverting post meant for channel ${slackContactMethod.channelId} to ${settings.plugins[thisType].overrideChannelId}`)

        let breakingBuild = await data.getBuild(incidentId, { expected : true })

        try {
            usersThatBrokeBuild = await faultHelper.getUsersWhoBrokeBuild(breakingBuild)
        } catch (ex){
            if (ex === 'revisions not mapped yet')
                return
                
            throw ex
        }

        const targetChannelId = settings.plugins[thisType].overrideChannelId || slackContactMethod.channelId,
            title_link = urljoin(settings.localUrl, `build/${breakingBuild.id}`),
            color = '#D92424', //'#007a5a',
            text = usersThatBrokeBuild.length ? `People involved : ${usersThatBrokeBuild.join(',')}` : '',
            title = `${job.name} broke @ build #${breakingBuild.build}.`, 
            attachments = [
                {
                    fallback : title,
                    color,
                    title,
                    title_link,
                    text 
                }
            ]

        let result = await slack.chat.postMessage({ 
            token : settings.plugins[thisType].accessToken, 
            channel : targetChannelId, 
            text: title, 
            attachments 
        })

        contactLog = new ContactLog()
        contactLog.receiverContext = slackContactMethod.channelId
        contactLog.type = slackContactMethod.type
        contactLog.eventContext = context
        contactLog.created = new Date().getTime()
        contactLog.data = {
            ts : result.ts
        }

        await data.insertContactLog(contactLog)
        __log.info(`fail alert sent to slack channel ${targetChannelId} for buildId ${incidentId}, result : `, result)
    },
    

    async alertGroupBuildPassing(slackContactMethod, job, incidentId){
        const data = await pluginsManager.getExclusive('dataProvider'),
            Slack = this.isSandboxMode() ? require('./mock/slack') : require('slack'),
            slack = new Slack({ token : settings.plugins[thisType].accessToken }),
            context = `${thisType}_group_alert_${job.id}_${(incidentId)}`

        let contactLog = await data.getContactLogByContext(slackContactMethod.channelId, slackContactMethod.type, context)
        if (!contactLog){
            console.warn(`could not find contactLog record for alert on build ${incidentId}`)
            return 'alert not found'
        }

        if (!contactLog.data.ts){
            console.warn(`contactLog record for alert on build ${incidentId} does not have a data.ts value, delete not possible`)
            return 'untraceable alert'
        }

        // alert has already been updated, exit silently
        if (contactLog.data.updated)
            return

        const targetChannelId = settings.plugins[thisType].overrideChannelId || slackContactMethod.channelId,
            title = `${job.name} is passing again`,
            title_link = urljoin(settings.localUrl, `build/${incidentId}`),
            attachments = [
                {
                    fallback : title,
                    color : '#007a5a',
                    title,
                    title_link
                }
            ]

        const result = await slack.chat.update({ 
            token : settings.plugins[thisType].accessToken, 
            channel : targetChannelId, 
            ts : contactLog.data.ts, 
            text : title,
            attachments,
            as_user : true 
        })

        contactLog.data.updated = true
        await data.updateContactLog(contactLog)

        __log.info(`passing alert updated to slack channel ${targetChannelId} for buildId ${incidentId}, result :`, result)
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

        // check if user has already been informed about this build failure. Do this as early as possible, as most times this method runs
        // we will be retrying an alert that has already been sent
        let contactLog = force ? null : await data.getContactLogByContext(user.id, thisType, context)
        if (contactLog)
            return

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
        
        const result = await slack.chat.postMessage({ token : settings.plugins[thisType].accessToken, channel : conversation.channel.id, text : message })

        // log that message has been sent, this will be used to prevent the same user from being informed of the same build error
        contactLog = new ContactLog()
        contactLog.receiverContext = user.id
        contactLog.type = thisType
        contactLog.eventContext = context
        contactLog.created = new Date().getTime()

        await data.insertContactLog(contactLog)
        __log.info(`alert sent to slack user via personal channel ${conversation.channel.id}, result : `, result)
    },


}