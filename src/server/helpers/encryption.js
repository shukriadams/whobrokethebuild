let path = require('path'),
    settings = require(_$+'helpers/settings'),
    Exception = require(_$+'types/exception'),
    randomstring = require('randomstring'),
    crypto = require('crypto'),
    key = null,
    algorithm = 'aes256',
    fs = require('fs-extra')

module.exports = {

    /**
     * Gets and if necesssary generates encryption key if one doesn't exist. Obviously not ideal way to handle secrets, but
     * better than storing and display secrets in plain text.
     */
    async getKey(){
        if (!key){
            const keyPath = path.join(settings.dataFolder, 'key')

            if (await fs.pathExists(keyPath))
                key = await fs.promises.readFile(keyPath, 'utf8')
            else{
                key = randomstring.generate(24)
                await fs.outputFile(keyPath, key)
            }
        }

        return key
    },


    /**
     * Decrypts text. TODO : this method is archaic and not secure anymore, update
     * @param {*} text The encrypted text to decrypt
     */
    async decrypt(text){
        const key = await this.getKey()
            decipher = crypto.createDecipher(algorithm, key)

        return decipher.update(text, 'hex', 'utf8') + decipher.final('utf8')
    },


    /**
     * Encrypts text. TODO : this method is archaic and not secure anymore, update
     */
    async encrypt(text){
        const key = await this.getKey()
            cipher = crypto.createCipher(algorithm, key)
            
        return cipher.update(text, 'utf8', 'hex') + cipher.final('hex')
    }
    
}