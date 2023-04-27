const maximumConsoleSize = parseInt(document.querySelector('body').getAttribute('data-consoleSize')),
        connection = new signalR.HubConnectionBuilder()
            .withUrl("/console-signalr")  
            .build()  
  
//This method receive the message and Append to our list  
connection.on("ReceiveMessage", (user, message) => {  
    const msg = message.replace(/&/g, "&").replace(/</g, "<").replace(/>/g, ">"),
        encodedMsg = user + " :: " + msg,  
        li = document.createElement("li")
    
    li.textContent = encodedMsg;

    const messageList = document.getElementById("messagesList"),
        overrun = messageList.childElementCount - maximumConsoleSize

    // remove oldest overflow elements from top of list
    if (overrun > 0) {
        console.log(`Removing ${overrun} elements from list`)
        for (let i = 0; i < overrun; i++)
            messageList.removeChild(messageList.children[0])
    }

    // add new elements at bottom of list
    messageList.appendChild(li)

})  
  
connection.start().catch(err => console.error(err.toString()));
