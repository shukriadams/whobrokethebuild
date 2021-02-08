let path = require('path'),
    crypto = require('crypto'),
    settings = require(_$+'helpers/settings'),
    randomstring = require('randomstring'),
    key = null,
    algorithm = 'aes256',
    fs = require('fs-extra')

module.exports = {

    /**
     * @return {Promise<number>}
     * Gets and if necesssary generates encryption key if one doesn't exist. Obviously not ideal way to handle secrets, but
     * better than storing and display secrets in plain text.
     */
    async _getKey(){
        if (!key){
            let keyPath = path.join(settings.dataFolder, 'key'),
                keyCreated = false

            if (await fs.pathExists(keyPath))
                key = await fs.readFile(keyPath, 'utf8')
            else {
                key = randomstring.generate(24)
                await fs.outputFile(keyPath, key)
                keyCreated = true
                __log.info(`Generating master auth key`)
            }

            const testPath = path.join(settings.dataFolder, 'key.test')
            if (keyCreated || !await fs.pathExists(testPath)){
                const encryptedText = await this.encrypt('a-test-string')
                await fs.outputFile(testPath, encryptedText)
                __log.info(`Generating key test file`)
            }
        }

        return key
    },


    /**
     * Tests local key, this primarily a workaround due to stability issues with keys right now.
     */
    async testKey(){
        const testPath = path.join(settings.dataFolder, 'key.test')
        if (await fs.pathExists(testPath)){
            const encryptedTest = await fs.readFile(testPath, 'utf8')
            try {
                const decryped = await this.decrypt(encryptedTest)
                if (decryped === 'a-test-string')
                    __log.debug('Encryption key test passed')
            } catch (ex) {
                __log.error('Encryption key test failed', ex)
            }
        } else 
            __log.warn('No key test file found, restart app to ensure it exists.')
    },


    /**
     * @return {Promise<string>}
     * Decrypts text. TODO : this method is archaic and not secure anymore, update
     * @param {string} text The encrypted text to decrypt
     */
    async decrypt(text){
        const key = await this._getKey(),
            // @ts-ignore
            decipher = crypto.createDecipher(algorithm, key)

        try {
            return decipher.update(text, 'hex', 'utf8') + decipher.final('utf8')
        } catch(ex){
            console.log(`Decrypt failed, recreate original text`, ex)
            return ''
        }
    },


    /**
     * @return {Promise<string>}
     * Encrypts text. TODO : this method is archaic and not secure anymore, update
     * @param {string} text The text to encrypt
     */
    async encrypt(text){
        const key = await this._getKey(),
            // @ts-ignore
            cipher = crypto.createCipher(algorithm, key)
            
        return cipher.update(text, 'utf8', 'hex') + cipher.final('hex')
    }
    
}