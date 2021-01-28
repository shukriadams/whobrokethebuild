# does a "stage1" build, ie, installs npm dev packages, then runs build against frontend and backend assets to generate artefacts which can be run, or packaged into a container
# without that container requiring the npm dev packages or build runtimes
# cwd must be this script's folder
# 
# arguments
#
# --setup : forces yarn aka npm install. Use if running on build server, omit to run locally without overhead of yarn install
# --version : writes current git tag to version file which is exposed in UI. Use if running on build server, omit if running locally.
# 

# capture switches
CLEAN=0
VERSION=0
while [ -n "$1" ]; do 
    case "$1" in
    -s|--clean) CLEAN=1 ;;
    -v|--version) VERSION=1 ;;
    esac 
    shift
done

if [ $CLEAN -eq 1 ]; then
    yarn --no-bin-links --ignore-engines
    npm rebuild node-sass --no-bin-links
fi

# clear out folder
cd ..
rm -rf ./.dist

# get current tag, this will be used to populate a version value that is visible in the client 
if [ $VERSION -eq 1 ]; then
    TAG=$(git describe --abbrev=0 --tags) &&
    echo "{\"version\": \"$TAG\"}" > .version.json
fi

npm run assets