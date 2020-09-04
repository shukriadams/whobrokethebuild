let sessionHelper = require(_$+ 'helpers/session'),
    jsonfile = require('jsonfile'),
    fs = require('fs-extra'),
    pluginLinks

/**
 * Attach data required by entire page stack (sub views)
 * model : required
 * req : required
 * pageOwnerPublicId : optional. If page is owned by a user, user's publicId, normally derived from URL parameter
 */

module.exports = async (model, req, pageOwnerPublicId)=>{
    if (!pluginLinks){

        if (await fs.exists('./.plugin.conf'))
            pluginLinks = jsonfile.readFileSync('./.plugin.conf')
        else
            pluginLinks = {}

        const pluginsArray = []
        for (const prop in pluginLinks)
            pluginsArray.push(pluginLinks[prop])
    }

    const currentUser = await sessionHelper.getCurrentUser(req)

    const canViewUserPage = currentUser && currentUser.roles.includes('admin')
        || currentUser.publicId === pageOwnerPublicId

    // view @ /partials/session relies on this
    model.session = {
        isLoggedIn : !!currentUser,
        isAdmin : !!currentUser && currentUser.roles.includes('admin'),
        canViewUserPage,
        name : currentUser ? currentUser.name : ''
    }

    // view @ /partials/layout relies on this
    model.layout = {
        bundlemode : '',
        pluginLinks
    }

}


