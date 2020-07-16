let 
    pluginsManager = require(_$+'helpers/pluginsManager'),
    Build = require(_$+ 'types/build'),

    create = async function (){
        const data = await pluginsManager.getByCategory('dataProvider')
        let build = Build();
        await data.insertBuild(build);
    },

    remove = async function (build){
        const data = await pluginsManager.getByCategory('dataProvider')
        await data.removeBuild(build);
    },

    page = async function (jobId, index, pageSize){
        const data = await pluginsManager.getByCategory('dataProvider')
        await data.pageBuilds(jobId, index, pageSize);
    },

    update = async function (build){
        const data = await pluginsManager.getByCategory('dataProvider')
        await data.updateBuild(build);
    };

    module.exports = {
        page,
        remove,
        update,
        create
    }