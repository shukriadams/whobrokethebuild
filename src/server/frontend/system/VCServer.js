if (document.querySelector('.vcserver')){
    const submitButton = document.querySelector('.vcserver-post')

    submitButton.addEventListener('click', async ()=>{
        await submitData()
    }, false)

    async function submitData(){
        let res = await fetchDo({url : '/settings/vcserver', data : {
            id : document.querySelector('.vcserver-id').value,
            name : document.querySelector('.vcserver-name').value,
            vcs : document.querySelector('.vcserver-vcs').value,
            username : document.querySelector('.vcserver-username').value,
            password : document.querySelector('.vcserver-password').value,
            accessToken : document.querySelector('.vcserver-accessToken').value,
            url : document.querySelector('.vcserver-url').value
        }})

        window.location = '/settings/system'
    }
}
