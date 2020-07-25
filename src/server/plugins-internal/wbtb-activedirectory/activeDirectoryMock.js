module.exports = class {
    async loginUser (username, password){
        return {
            success : username === 'admin' ? false : true
        }
    }
    
    static createUserObj(){
        return {
            guid: "a184e39a-c6ff-44ba-a46b-5a53bc6cc6c3"
        }
    }

    async getAllUsers(){
        return {
            success : true, 
            users : [
                {
                    groups: ["My Group","Our Admins"],
                    phone: "",
                    name: "Rexor Haxor",
                    mail: "test@example.com",
                    guid:"a184e39a-c6ff-44ba-a46b-5a53bc6cc6c3",
                    dn : "CN=SupportMe,OU=Users,OU=mycompany,DC=mycompany,DC=local"
                },

                {
                    groups: ["My Group","Our Admins"],
                    phone: "",
                    name: "Bob McBobface",
                    mail: "",
                    guid:"f184e39a-c6ff-44ba-b46b-5a53bc6cc6c3",
                    dn : "CN=SupportMe,OU=Users,OU=mycompany,DC=mycompany,DC=local"
                }            ]
        }
    }
}