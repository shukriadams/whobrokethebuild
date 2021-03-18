/**
 * @typedef {Object} Avatar
 * @property {string} url External url of avatar
 * @property {string} path internal path of avatar
 * @property {string} type Constant
 */
module.exports = class Avatar {

    constructor(){
        const constants = require(_$+'types/constants')

        this.url = null
        this.path = null
        this.type = constants.AVATARTYPE_OTHER
    }

}