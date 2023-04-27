/**
 * This script builds fonts from svgs. It is not supposed to be run on build servers, but rather on local dev systems. Please commit your 
 * fonts to git after generation.
 */
const webfontsGenerator = require('webfonts-generator')
 
webfontsGenerator({
        files: [
            './../components/svg/add.svg',
            './../components/svg/delete.svg',
            './../components/svg/eye.svg',
            './../components/svg/fingerprint.svg',
            './../components/svg/gear.svg',
            './../components/svg/logout.svg',
            './../components/svg/login.svg',
            './../components/svg/log.svg',
            './../components/svg/radar.svg',
            './../components/svg/users.svg',
            './../components/svg/user.svg'
        ],
        dest: './../components/icons/',
        cssFontsUrl : '/fonts',
        types :['woff2', 'woff', 'eot', 'ttf'],
        html : true
    }, function(error) {
        if (error)
            console.log('Fail!', error)
        else
            console.log('Done!')
    })