module.exports = {
    // filters an array of buildInvolvement objects, returns unique by user
    filterUniqueUser(buildInvolvements){
        const uniqueCheck = []
        buildInvolvements = buildInvolvements.filter(involvement =>{
            const useridentifier = involvement.externalUsername || involvement.userId
            if (uniqueCheck.indexOf(useridentifier) === -1){
                uniqueCheck.push(useridentifier)
                return involvement 
            } else
                return null
        })

        return buildInvolvements
    }
}