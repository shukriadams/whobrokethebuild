// @ts-check

module.exports = function(){
    return Object.assign({}, {
        userId: null,       // ObjectID.id of user that owns this session
        userAgent: null,    // STRING. 
        ip: null,           // STRINTG.    
        created: null       // long, datetime ticks session was created
    });
}