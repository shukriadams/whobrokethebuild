const constants = require(_$+ 'types/constants')

module.exports = () =>{
    return Object.assign( {}, {
        plugin : null,          // STRING. key of contact plugin
        receiverContext : null, // STRING, unique identifier of receipient. This can be an email address, a slack channel / user id, etc
        eventContext : null,    // STRING. unique identifier of message event. Normally a build id.
        created : null          // LONG. datetime in milliseconds when record was created. 
    })
}