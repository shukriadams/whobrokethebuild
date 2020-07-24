
module.exports  = (res, error)=>{
    let show = error
    res.status(500)
    
    if (typeof error === 'object'){
        res.json(error)
    } else {
        res.end(error)
    }

    console.log(show)
}