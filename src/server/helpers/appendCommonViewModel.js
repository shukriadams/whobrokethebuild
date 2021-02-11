let sessionHelper = require(_$+'helpers/session'),
    SessionViewModel = require(_$+'types/sessionViewModel'),
    LayoutViewModel = require(_$+'types/layoutViewModel')

/**
 * Attach data required by entire page stack (sub views)
 * model : required
 * req : required
 * pageOwnerPublicId : optional. If page is owned by a user, user's publicId, normally derived from URL parameter
 */
module.exports = async (model, req, pageOwnerPublicId)=>{

    const currentUser = await sessionHelper.getCurrentUser(req),
        canViewUserPage = currentUser && (currentUser.roles.includes('admin')
            || currentUser.publicId === pageOwnerPublicId)

    // view @ /partials/session relies on this
    model.session = new SessionViewModel() 
    model.session.isLoggedIn = !!currentUser
    model.session.isAdmin = !!currentUser && currentUser.roles.includes('admin')
    model.session.canViewUserPage = canViewUserPage
    model.session.name = currentUser ? currentUser.name : ''

    // view @ /partials/layout relies on this
    model.layout = new LayoutViewModel()
    model.layout.bundlemode = ''

}


