if (document.querySelector('[data-isBuild]')){
    const btnDelete = document.querySelector('[data-deleteBuild]'),
        btnUndoAlerts = document.querySelector('[data-undoAlerts]'),
        deleteBuild = async()=>{
            if (!confirm('Are you sure you want to delete this build?'))
                return

            await fetchDo({ url : `/build/object/${btnDelete.getAttribute('data-deleteBuild')}`, method: 'DELETE' })
            window.location = `/`
        },
        undoAlerts = async()=>{
            if (!confirm('Are you sure you want to withdraw all alerts sent for this build break?'))
                return

            await fetchDo({ url : `/build/alerts/${btnDelete.getAttribute('data-deleteBuild')}`, method: 'DELETE' })
            window.location = window.location
        }

    btnDelete.addEventListener('click', async ()=>{
        await deleteBuild()
    }, false)

    if (btnUndoAlerts)
        btnUndoAlerts.addEventListener('click', async ()=>{
            await undoAlerts()
        }, false)

}
