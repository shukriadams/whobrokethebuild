module.exports = Handlebars => {

    Handlebars.registerHelper('pager', function(baseUrl, pageObject){
        let html = '<ul class="pager">'
        
        if (pageObject.pages > 1)
            for (let i = 0 ; i < pageObject.pages ; i ++)
                html += `<li class="pager-item"><a class="pager-link" href="${baseUrl}?page=${i+1}">${i +1}</a></li>`

        html += '</ul>'

        return html
    })
    
}
