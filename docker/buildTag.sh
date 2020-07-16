# tag is required
TAG=$1

if [ -z $TAG ]; then
   echo "Error, tag not set. Use : ./buildTag XXX where XXX is an existing tag that you want to build at.";
   exit 1;
fi

# clone working copy of repo at the latest tag
rm -rf .clone &&
git clone --depth 1 --branch $TAG git@github.com:shukriadams/expreactboilerplate.git .clone &&

# forward ssh-agent to container for building
eval $(ssh-agent)
ssh-add

# kill any existing build container, then start again
docker-compose -f docker-compose-build.yml kill &&
docker-compose -f docker-compose-build.yml up -d &&

# clean out build container if it's been previously used
docker exec buildcontainer sh -c 'rm -rf /tmp/build/*' &&
docker exec buildcontainer sh -c 'rm -rf /tmp/stage/*' &&

# copy src into buildcontainer
docker cp ./.clone/src/. buildcontainer:/tmp/build &&
# install with --no-bin-links to avoid simlinks, this is needed to copy artefacts.
# execute build command here
docker exec buildcontainer sh -c 'cd /tmp/build/ && npm install --no-bin-links && jspm install -y && npm run build' &&
# copy build to stage folder, clean out node_modules and other folders that should not make it to final build
docker exec buildcontainer sh -c 'cp -R /tmp/build/* /tmp/stage' &&
docker exec buildcontainer sh -c 'rm -rf /tmp/stage/node_modules' &&
docker exec buildcontainer sh -c 'rm -rf /tmp/stage/client' &&
docker exec buildcontainer sh -c 'rm -rf /tmp/stage/build' &&
# do a fresh npm install of production-only modules
docker exec buildcontainer sh -c 'cd /tmp/stage/ && npm install --production --no-bin-links' &&
# zip install and copy it out 
docker exec buildcontainer sh -c 'tar -czvf /tmp/build.tar.gz /tmp/stage' &&
docker cp buildcontainer:/tmp/build.tar.gz . &&

# build deploy container, this will pick up zip
docker build -t shukriadams/expreactboilerplate . &&

# tag container. You can add push steps here if needed
docker tag shukriadams/expreactboilerplate:latest shukriadams/expreactboilerplate:$TAG