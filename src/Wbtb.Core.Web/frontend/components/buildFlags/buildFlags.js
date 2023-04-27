;
(() => {

    async function deleteBuildflag(id){
        if (!confirm('Are you sure you want to withdraw all alerts sent for this build break?'))
            return

        await fetchDo({ url: `/buildflag/delete/${id}`, method: 'DELETE' })
        window.location = window.location
    }

    for (let deleteLink of document.querySelectorAll('.buildFlags-delete')) 
        deleteLink.addEventListener('click', async () => {
            deleteBuildflag(this.element.getAttribute('buildflagid'))
        }, false)

})();
