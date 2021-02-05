// @ts-check

module.exports = function(){
    return Object.assign({}, {
        publicId : null,        // STRING. required. fixed username of user, on whatever platform the user account is from.
        name : null,            // STRING, optional. For display purposes only, no logic should use this. authMethod contains id-purpose user name.
        email : null,            // STRING. optional. Doesn't mean user wants to be contacted!
        isAuthApproved : false, // BOOL. if true, an admin has approved user for authentication
        avatars : [],           // ARRAY of Avatar objects
        roles : [],             // ARRAY of STRINGS. Use these to flag user for rolls.
        contactMethods : {},    // hashtable of ContactMethod objects
        externalIds : [],       // ARRAY of UserMapping objects
        authMethod : null,      // STRING. Identifier for auth method.
        authData : null,        // OBJECT. Auth object
        
        userMappings: []        // used on mongo etc only 
    });
}