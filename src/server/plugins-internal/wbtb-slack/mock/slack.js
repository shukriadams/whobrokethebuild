const Exception = require(_$+'types/exception'),
    settings = require(_$+'helpers/settings'),
    path = require('path'),
    fs = require('fs-extra')

module.exports = class {

    constructor(){

        this.conversations = {

            /**
             *  conversationArgs normally contains users array and slack access token
             */
            open (conversationArgs){
                if (!conversationArgs.token)
                    throw new Exception({
                        message : 'wbtb-slack:open - arg missing .token value. This send would fail on real slack'
                    })
                
                if (!conversationArgs.users)
                    throw new Exception({
                        message : 'wbtb-slack:open - arg missing .users value or empty. This send would fail on real slack'
                    })
    
                return {
                    channel : {
                        id : 'some-channel'
                    }
                }
            },

            list : () => {
                return {
                    channels : [{
                        id : '123',
                        name : 'my channel'
                    },
                    {
                        id : '213',
                        name : 'our alerts channel'
                    }]
                }
            }
        }
        
        this.channels = {
            list : () => {
                return {
                    channels : [{
                        id : '123',
                        name : 'my channel'
                    },
                    {
                        id : '213',
                        name : 'our alerts channel'
                    }]
                }
            }
        }

        this.chat = {
            delete(args){
                if (!args.token)
                    throw new Exception({
                        message : 'wbtb-slack:open - arg missing .token value. This send would fail on real slack'
                    })
    
                if (!args.channel)
                    throw new Exception({
                        message : 'wbtb-slack:open - arg missing .channel value. This send would fail on real slack'
                    })

                if (!args.ts)
                    throw new Exception({
                        message : 'wbtb-slack:open - arg missing .ts value. This send would fail on real slack'
                    })                    

                if (args.as_user !== true)
                    throw new Exception({
                        message : 'wbtb-slack:open - arg missing .as_user value of true. This send would fail on real slack'
                    })                    

                __log.debug(`mock slack deleting message in channel ${args.channel} :`)
                return 'message deleted'
                    
            },

            update(args){
                if (!args.token)
                    throw new Exception({
                        message : 'wbtb-slack:open - arg missing .token value. This send would fail on real slack'
                    })
    
                if (!args.channel)
                    throw new Exception({
                        message : 'wbtb-slack:open - arg missing .channel value. This send would fail on real slack'
                    })

                if (!args.ts)
                    throw new Exception({
                        message : 'wbtb-slack:open - arg missing .ts value. This send would fail on real slack'
                    })                    

                if (args.as_user !== true)
                    throw new Exception({
                        message : 'wbtb-slack:open - arg missing .as_user value of true. This send would fail on real slack'
                    })                    

                __log.debug(`mock slack upating message in channel ${args.channel} :`)
                return 'message updated'
            },

            postMessage(args){
                if (!args.token)
                    throw new Exception({
                        message : 'wbtb-slack:open - arg missing .token value. This send would fail on real slack'
                    })
    
                if (!args.channel)
                    throw new Exception({
                        message : 'wbtb-slack:open - arg missing .channel value. This send would fail on real slack'
                    })
    
                if (!args.text)
                    throw new Exception({
                        message : 'wbtb-slack:open - arg missing .text value. This send would fail on real slack'
                    })
    
                __log.debug(`mock slack sending message to channel ${args.channel} :`)
                const writeFolder = path.join(settings.dataFolder, 'wbtb-slack', 'mockMessages')
                fs.ensureDirSync(writeFolder)
                fs.outputJsonSync(path.join(writeFolder, `${new Date().getTime()}-slack-post.json`), args, { spaces : 4 })
                return { ts : new Date().getTime() } // fake a ts value
            }
        }
    
        this.file = {
            upload(){
                
            }
        }
    }

}