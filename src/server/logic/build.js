let 
    pluginsManager = require(_$+'helpers/pluginsManager'),
    Build = require(_$+ 'types/build')

module.exports = {
    
    async page (jobId, index, pageSize){
        const data = await pluginsManager.getByCategory('dataProvider')
        await data.pageBuilds(jobId, index, pageSize)
    },

    async remove (build){
        const data = await pluginsManager.getByCategory('dataProvider')
        await data.removeBuild(build)
    },

    async update  (build){
        const data = await pluginsManager.getByCategory('dataProvider')
        await data.updateBuild(build)
    },

    async create (){
        const data = await pluginsManager.getByCategory('dataProvider')
        let build = Build()
        await data.insertBuild(build)
    }

}