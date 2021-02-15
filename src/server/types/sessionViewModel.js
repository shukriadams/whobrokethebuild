/**
 * @typedef {Object} SessionViewModel
 * @property {boolean} isLoggedIn 
 * @property {boolean} isAdmin
 * @property {boolean} canViewUserPage
 * @property {string} name
 */

module.exports = class SessionViewModel {
    constructor(){
        this.isLoggedIn = false
        this.isAdmin = false
        this.canViewUserPage = false
        this.name = null
    }
}