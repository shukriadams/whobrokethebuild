
module.exports  = (res, error)=>{
    let show = error
    res.status(500)
    
    if (typeof error === 'object'){
        show = error.toString()
        res.json(error)
    } else {
        res.end(error)
    }

    console.log(show)
}