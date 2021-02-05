let sessionHelper = require(_$+'helpers/session'),
    pluginsManager = require(_$+'helpers/pluginsManager'),
    SessionViewModel = require(_$+'types/sessionViewModel'),
    LayoutViewModel = require(_$+'types/layoutViewModel'),
    pluginLinks

/**
 * Attach data required by entire page stack (sub views)
 * model : required
 * req : required
 * pageOwnerPublicId : optional. If page is owned by a user, user's publicId, normally derived from URL parameter
 */
module.exports = async (model, req, pageOwnerPublicId)=>{

    if (!pluginLinks){
        // get plugins config, if not present force empty structure
        pluginLinks = pluginsManager.getPluginConf() || { }

        const pluginsArray = []
        for (const prop in pluginLinks)
            pluginsArray.push(pluginLinks[prop])
    }

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
    model.pluginLinks = pluginLinks

}


