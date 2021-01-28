const md5File = require('md5-file')

module.exports = {
    // gets the hash of a file
    async file (filePath){
        return new Promise(async (resolve, reject)=>{
            try {
                md5File(filePath,(err, hash)=>{
                    return err ? 
                        reject(err) : 
                        resolve(hash)
                })            
            } catch(ex){
                reject(ex)
            }
        })
    }
}