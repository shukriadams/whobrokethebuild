if (document.querySelector('.build')){
    const btnDelete = document.querySelector('.build-delete'),
        deleteBuild = async()=>{
            if (!confirm('Are you sure you want to delete this build?'))
                return

            await fetchDo({ url : `/build/${btnDelete.getAttribute('data-id')}`, method: 'DELETE' })
            window.location = `/`
        }

    btnDelete.addEventListener('click', async ()=>{
        await deleteBuild()
    }, false)

}
