
module.exports  = (res, error)=>{
    res.status(500)
    let out =typeof error === 'object' ? error.toString() : error
    if (error.stack)
        out += `\n${error.stack} `

    res.end(out)
}