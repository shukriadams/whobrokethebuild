if (document.querySelector('.login')){

    const loginbutton = document.querySelector('.login-process')
        errorMessage = document.querySelector('.login-error')
        loginForm = document.querySelector('.login')
    
    async function submit(){
        let result = await fetchDo({ 
            url : '/session', 
            data : {
                username : loginForm.querySelector('[name="username"]').value,
                password : loginForm.querySelector('[name="password"]').value
            }
        });

        if (result.error)
            errorMessage.innerHTML = result.error
        else
            window.location = '/'

    }

    loginbutton.addEventListener('click', async ()=>{
        await submit()
    }, false)

}
