if (document.querySelector('.settings-wipeAllBuilds')){
    const btnWipeAllBuilds = document.querySelector('.settings-wipeAllBuilds'),
        cbWipeAllBuilds = document.querySelector('#settings-wipeAllBuildsConfirm')

    async function wipeAllBuilds(){
        if (!cbWipeAllBuilds.checked)
            return alert('Please confirm wipe')

        try{
           const result = await fetchDo({ url : '/settings/system/builds/', method : 'DELETE' })
           alert(result)

        } catch (ex){
            console.log(ex)
        }
        console.log('builds wiped')
    }

    btnWipeAllBuilds.addEventListener('click', async ()=>{
        await wipeAllBuilds()
    }, false)
}
