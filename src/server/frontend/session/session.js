if (document.querySelector('.session')){
    const logoutButton = document.querySelector('.session-end');

    if (logoutButton)
        logoutButton.addEventListener('click', async ()=>{
            await endSession();
        }, false);

    async function endSession(){
        await fetchDo({url : 'session', method: 'DELETE'});
        window.location = window.location;
    }
}
