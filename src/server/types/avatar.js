// @ts-check

/**
 * @typedef {Object} Avatar
 * @property {string} url External url of avatar
 * @property {string} type Constant
 */
module.exports = class Avatar {

    constructor(){
        // @ts-ignore
        const constants = require(_$+'types/constants')

        this.url = null
        this.type = constants.AVATARTYPE_OTHER
    }

}