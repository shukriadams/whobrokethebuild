let sessionHelper = require(_$+ 'helpers/session'),
    jsonfile = require('jsonfile'),
    fs = require('fs-extra'),
    pluginLinks

/**
 * Attach data required by entire page stack (sub views)
 */
module.exports = async (model, req)=>{
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

    // view @ /partials/session relies on this
    model.session = {
        isLoggedIn : !!currentUser,
        isAdmin : !!currentUser && currentUser.roles.includes('admin'),
        name : currentUser ? currentUser.name : ''
    }

    // view @ /partials/layout relies on this
    model.layout = {
        bundlemode : '',
        pluginLinks
    }

}


