

let Handlebars = require('handlebars'),
    fs = require('fs-extra'),
    fsUtils = require('madscience-fsUtils'),
    pages = null,
    views = null,
    path = require('path'),
    glob = require('glob'),
    settings = require(_$+ 'helpers/settings'),
    pluginsManager = require(_$+'helpers/pluginsManager'),
    layouts = require('handlebars-layouts'),
    helpers = fsUtils.getFilesAsModulePathsSync(_$+'helpers/handlebars')

// load and register file-based helpers
for (let helperPath of helpers)
    (require(helperPath))(Handlebars)

// register handlebars helpers
Handlebars.registerHelper(layouts(Handlebars))

module.exports = {

    async getView(page){

        if (!pages || !settings.cacheHandlebarViews){

            pages = {}
            views = {}
        
            // core partials
            let root = path.join(_$+'views/partials'),
                partialPaths = await fsUtils.readFilesUnderDir(path.join(__dirname,'./../views/partials'))  // findViews(path.join(__dirname,'./../views/partials'))

            for (let partialPath of partialPaths){
                let content = fs.readFileSync(partialPath, 'utf8'),
                    name = partialPath.replace(root, '').match(/\/(.*).hbs/).pop()

                if (views[name]){
                    console.warn(`The core partial "${name}" (from ${partialPath}) is already taken by another partial.`)
                    continue
                }    

                Handlebars.registerPartial(name, content)
                views[name] = true
            }
        
            // core pages
            root = path.join(_$+'views/pages')
            let pagePaths = await fsUtils.readFilesUnderDir(root)
            for (let pagePath of pagePaths){
                let content = fs.readFileSync(pagePath, 'utf8'),
                    name = pagePath.replace(root, '').match(/\/(.*).hbs/).pop();
                
                if (pages[name]){
                    console.warn(`The core page "${name}" (from ${pagePath}) is already taken by another view.`)
                    continue
                }    
                
                pages[name] = Handlebars.compile(content)
            }

            // plugin pages
            root = pluginsManager.getPluginRootPath()
            pagePaths = glob.sync(`${root}/**/views/**/*.hbs`, { ignore : ['**/node_modules/**', '**/mock/**']})
            for (let pagePath of pagePaths){
                let content = fs.readFileSync(pagePath, 'utf8'),
                    name = pagePath.replace(root, '').match(/\/(.*).hbs/).pop()

                if (pages[name]){
                    console.warn(`The plugin page "${name}" (from ${pagePath}) is already taken by another view.`)
                    continue
                }    
                    
                pages[name] = Handlebars.compile(content)
            }

            // plugin partials
            pagePaths = glob.sync(`${root}/**/partials/**/*.hbs`, { ignore : ['**/node_modules/**', '**/mock/**']})
            for (let pagePath of pagePaths){
                let content = fs.readFileSync(pagePath, 'utf8'),
                    name = pagePath.replace(root, '').match(/\/(.*).hbs/).pop()

                if (views[name]){
                    console.warn(`The plugin partial "${name}" (from ${pagePath}) is already taken by another partial.`)
                    continue
                }    

                Handlebars.registerPartial(name, content)
                views[name] = true
            }

            // add external plugins too!
        }

        return pages[page]
    },
    
}
