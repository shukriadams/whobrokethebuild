if (document.querySelector('.userSettings')){
    const addUserButton = document.querySelector('[data=vcServer-addUser]'),
        saveBindingsButton = document.querySelector('.userSetting-saveVCServerBindings')

    addUserButton.addEventListener('click', async ()=>{
        await addUser()
    }, false)

    saveBindingsButton.addEventListener('click', async ()=>{
        await saveBindings()
    }, false)

    async function saveBindings(){

        const data = { items : [] },
            bindings = document.querySelectorAll('.userSettings-vcServerBinding')
        
        data.user = document.querySelector('.userSettings-userId').value

        for (const binding of bindings){
            const name = binding.querySelector('.userSettings-vcServerBindingName')
            data.items.push({
                VCServerId : name.getAttribute('data-vcServerId'),
                name: name.value
            })
        }

        let res = await fetchDo({ url : '/settings/user/updateVCServerMappings', data })
        window.location = window.location
        
    }

    async function addUser(){
        let res = await fetchDo({url : '/settings/userSettings/addVCServerMapping', data : {
            vcServer : document.querySelector('.userSettings-vcServerAddUserServer').value,
            user : document.querySelector('.userSettings-userId').value
        }})

        window.location = window.location
    }
}


