const logoutButton = document.querySelector('.header-logout')

if (logoutButton)
    logoutButton.addEventListener('click', async ()=>{
        await endSession()
    }, false)

async function endSession(){
    if (!confirm('Are you sure you want to logout?'))
        return
        
    await fetchDo({ url : 'session', method: 'DELETE' })
    window.location = window.location
}