let Socketio = require('socket.io'),
    socket = null

module.exports = {

    initialize : function(express){
        socket = Socketio.listen(express)
    },

    get : function(){
        return socket;
    }
}