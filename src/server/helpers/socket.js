let Socketio = require('socket.io'),
    socket = null

module.exports = {

    initialize(express){
        socket = Socketio.listen(express)
    },

    get(){
        return socket;
    }
}