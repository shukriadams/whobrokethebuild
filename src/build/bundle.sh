# This script builds the javascript for client-side section of project, and places artefacts in /public folder.
# It does not build Sass.

# clean and set up work directories
rm -rf ./.tmp/bundles
mkdir -p ./.tmp/bundles
mkdir -p ./public/scripts

# bundle core app and libs with jspm
cd ./client/app/
jspm bundle-sfx main/main.js ./../../.tmp/bundles/main.js
cd -


# bundle all system.js, jspm's config, and main.js, int that order. System.js must be bound first as the other scripts depend on it
concat-cli -f ./client/lib/system.js -f ./client/app/config.js -f ./.tmp/bundles/main.js -o ./.tmp/bundles/app.js
# create a minified version of app
uglifyjs -c -v -o ./.tmp/bundles/app.min.js -- ./.tmp/bundles/app.js


# finally, copy the minified and unminified script to a public scripts folder, the next stage of the higher level build script can
# deal with this.
cp ./.tmp/bundles/app.js ./public/scripts/app.js
cp ./.tmp/bundles/app.min.js ./public/scripts/app.min.js
