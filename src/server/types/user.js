/**
 * @typedef {Object} User
 * @property {string} publicId required. fixed username of user, on whatever platform the user account is from.
 * @property {string} name optional. For display purposes only, no logic should use this. authMethod contains id-purpose user name.
 * @property {string} password Used to carry plaintext password to database, never persisted
 * @property {string} email optional. Doesn't mean user wants to be contacted!
 * @property {boolean} isAuthApproved if true, an admin has approved user for authentication
 * @property {Array<import("./avatar").Avatar>} avatars 
 * @property {Array<string>} roles Use these to flag user for rolls.
 * @property {import("./contactMethod").ContactMethod} contactMethods hashtable of ContactMethod objects
 * @property {Array<object>} externalIds Array of UserMapping objects
 * @property {string} authMethod Identifier for auth method.
 * @property {object} authData Auth object(?)
 * @property {object} userMappings used on mongo etc only (?)
 */
module.exports = class User {

    constructor(){
        this.publicId = null
        this.name = null
        this.password = null
        this.email = null
        this.isAuthApproved = false
        this.avatars = []
        this.roles = []
        this.contactMethods = {}
        this.externalIds = []
        this.authMethod = null
        this.authData = null
        this.userMappings = []
    }

}
