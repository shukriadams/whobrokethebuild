import io from 'socket.io-client';

export default function initialize(){
    let socket = io.connect('/', { reconnect: true });
    
    socket.on('some-action', async function(){
        // respond
    });
};
