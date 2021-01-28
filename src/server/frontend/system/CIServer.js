if (document.querySelector('.ciserver')){
    const submitButton = document.querySelector('.ciserver-post'),
        deleteButton = document.querySelector('.ciserver-delete'),
        ciServerID = document.querySelector('.ciserver-id')

    submitButton.addEventListener('click', async ()=>{
        await submitData()
    }, false)

    deleteButton.addEventListener('click', async ()=>{
        await deleteCiServer()
    }, false)


    // 
    document.addEventListener('click', async (e)=>{
        if (e.target.classList.contains('ciserver-deleteJob'))
            await deleteJob(e.target.getAttribute('data-id'))
    }, false)

    async function deleteCiServer(){
        if (!confirm('Are you sure you want to delete this CI server? All build data and history associated with it will be permanently removed.'))
            return

        await fetchDo({
            method : 'DELETE',
            url : `/settings/ciserver/${ciServerID.value}`
        })

        window.location = '/settings/system'
    }

    async function deleteJob(id){
        if (!confirm('Are you sure you want to delete this job? This will permanently remove all build history for it'))
            return
        
            await fetchDo({
                method : 'DELETE',
                url : `/jobs/${id}`
            })

            window.location = window.location
    }

    async function submitData(){
        await fetchDo({url : '/settings/ciserver', data : {
            id : ciServerID.value,
            name : document.querySelector('.ciserver-name').value,
            type : document.querySelector('.ciserver-type').value,
            username : document.querySelector('.ciserver-username').value,
            password : document.querySelector('.ciserver-password').value,
            url : document.querySelector('.ciserver-url').value
        }})

        window.location = '/settings/system'
    }
}
