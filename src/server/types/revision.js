module.exports = function(){
    return Object.assign({}, {
        revision : null,    // string. Revision number / code / id, depending on source control 
        date : null,        // DATE. date revision was created.
        user : null,        // string. user name
        description : null, // string 
        files : [ ]         // array of stings, or RevisionFile with revisions that have been fault tested
    })
}


