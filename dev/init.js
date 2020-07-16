var exists = db.system.users.find({ user: "dev" }).count() > 0;
if (!exists){
    db.createUser({user : "dev", pwd : "dev", roles : [ { role : "readWrite", db : "whobrokethebuild" } ]});
}


