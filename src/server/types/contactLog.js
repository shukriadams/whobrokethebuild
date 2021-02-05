// @ts-check

/**
 * @typedef {Object} ContactLog
 * @property {string} plugin key of contact plugin
 * @property {string} receiverContext unique identifier of receipient. This can be an email address, a slack channel / user id, etc
 * @property {string} eventContext unique identifier of message event. Normally a build id.
 * @property {number} created datetime in milliseconds when record was created. 
 */
module.exports = class ContactLog {
    constructor(){
        this.plugin = null
        this.receiverContext = null
        this.eventContext = null
        this.created = null
    }
}
