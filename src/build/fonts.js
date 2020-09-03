/**
 * This script builds fonts from svgs. It is not supposed to be run on build servers, but rather on local dev systems. Please commit your 
 * fonts to git after generation.
 */
const webfontsGenerator = require('webfonts-generator')
 
webfontsGenerator({
        files: [
            'svgs/add.svg',
            'svgs/delete.svg',
            'svgs/gear.svg',
            'svgs/logout.svg',
            'svgs/login.svg',
            'svgs/log.svg',
            'svgs/users.svg',
            'svgs/user.svg'
        ],
        dest: './../public/css/icons/',
        cssFontsUrl : '/css/icons',
        types :['woff2', 'woff', 'eot', 'ttf'],
        html : true
    }, function(error) {
        if (error)
            console.log('Fail!', error)
        else
            console.log('Done!')
    })