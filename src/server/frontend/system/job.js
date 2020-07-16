if (document.querySelector('.jobSettings')){
    const submitButton = document.querySelector('.jobSettings-post')

    submitButton.addEventListener('click', async ()=>{
        await submitData()
    }, false)

    async function submitData(){
        const CIServerId = document.querySelector('.jobSettings-ciServerId').value
        let res = await fetchDo({url : '/settings/job', data : {
            id : document.querySelector('.jobSettings-id').value,
            name : document.querySelector('.jobSettings-name').value,
            tags : document.querySelector('.jobSettings-tags').value,
            VCServerId : document.querySelector('.jobSettings-vcServerId').value,
            CIServerId,
            logParser : document.querySelector('.jobSettings-logParser').value,
            isPublic : document.querySelector('.jobSettings-isPublic').checked
        }})
        
        window.location = `/settings/ciserver/${CIServerId}`
    }
}