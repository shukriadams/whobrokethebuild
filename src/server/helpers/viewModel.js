const sessionHelper = require(_$+'helpers/session'),
    SessionViewModel = require(_$+'types/sessionViewModel'),
    LayoutViewModel = require(_$+'types/layoutViewModel'),
    constants = require(_$+'types/constants')

/**
 * Attach data required by entire page stack (sub views)
 * model : required
 * req : required
 * pageOwnerPublicId : optional. If page is owned by a user, user's publicId, normally derived from URL parameter
 */
module.exports = { 

    /**
     * Adds objects to model for 
     */
    async layout (model, req, pageOwnerPublicId){
        const currentUser = await sessionHelper.getCurrentUser(req),
            canViewUserPage = currentUser && (currentUser.roles.includes('admin')
                || currentUser.publicId === pageOwnerPublicId)

        // view @ /partials/session relies on this
        model.session = new SessionViewModel() 
        model.session.isLoggedIn = !!currentUser
        model.session.isAdmin = !!currentUser && currentUser.roles.includes('admin')
        model.session.canViewUserPage = canViewUserPage
        model.session.name = currentUser ? currentUser.name : ''

        model.constants = constants

        // view @ /partials/layout relies on this
        model.layout = new LayoutViewModel()
        model.layout.bundlemode = ''
    },

    async layout_userSettings(model, req, userId){
        
        await sessionHelper.ensureUserOrRole(req, userId, 'admin')

        const pluginsManager = require(_$+'helpers/pluginsManager'),
            data = await pluginsManager.getExclusive('dataProvider'),
            user = await data.getUser(userId)

        model.user = user
        await this.layout(model, req, userId)
    }
}

