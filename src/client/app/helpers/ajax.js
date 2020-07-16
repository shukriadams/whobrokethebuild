async function getJsonAsync(url){
    return new Promise((resolve, reject)=>{
        try {
            fetch(url)
                .then((response)=>{
                    response.json().then((data)=>{
                        resolve(data);
                    });
                }) 
                .catch(function(err) {
                    reject(err);
                });
        } catch(ex) {
            reject(ex);
        }
    })
}

/**
 * 
 */
function getAsJson(url, callback){
    fetch(url)
        .then((response)=>{
            response.json().then((data)=>{
                callback(null, data);
            });
        }) 
        .catch(function(err) {
            callback(err);
        });
}

export {
    getJsonAsync, 
    getAsJson
}