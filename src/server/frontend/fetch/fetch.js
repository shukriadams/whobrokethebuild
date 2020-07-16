/**
 * Expects json response!
 */
async function fetchDo(options){
    return new Promise((resolve, reject)=>{
        fetch(options.url, {
            method: options.method || 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(options.data) 
        }).then(response => {
            
            response.text().then(data => {
                try {
                    let json = JSON.parse(data) 
                    resolve(data)
                }catch(ex){
                    reject(data)
                }
            })
        }).catch( err => {
            err.text().then( errorMessage => {
                reject({
                    code : -1,
                    status : -1,
                    error : errorMessage
                })
            })
        })
    });

}