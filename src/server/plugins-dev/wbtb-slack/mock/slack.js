const Exception = require(_$+'types/exception')

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
    
                console.log(`mock slack sending message to channel ${args.channel} :`)
                console.log(args.text)
                return 'message posted'
            }
        }
    
        this.file = {
            upload(){
                
            }
        }
    }

}