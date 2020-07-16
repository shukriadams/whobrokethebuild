if (document.querySelector('.ciserver')){
    const submitButton = document.querySelector('.ciserver-post')

    submitButton.addEventListener('click', async ()=>{
        await submitData()
    }, false)

    // 
    document.addEventListener('click', async (e)=>{
        if (e.target.classList.contains('ciserver-deleteJob'))
            await deleteJob(e.target.getAttribute('data-id'))
    }, false)

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
        let res = await fetchDo({url : '/settings/ciserver', data : {
            id : document.querySelector('.ciserver-id').value,
            name : document.querySelector('.ciserver-name').value,
            type : document.querySelector('.ciserver-type').value,
            username : document.querySelector('.ciserver-username').value,
            password : document.querySelector('.ciserver-password').value,
            url : document.querySelector('.ciserver-url').value
        }})

        window.location = '/settings/system'
    }
}
