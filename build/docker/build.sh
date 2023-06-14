set -e
DOCKERPUSH=0
SMOKETEST=0

while [ -n "$1" ]; do 
    case "$1" in
    --push|-p) DOCKERPUSH=1 ;;
    --smoketest|-t) SMOKETEST=1 ;;
    esac 
    shift
done

TAG=$(git describe --tags --abbrev=0)
HASH=$(git rev-parse --short HEAD)
if [ -z "$TAG" ]; then
    echo "TAG not set, exiting"
    exit 1;
fi

# write hhash + tag to currentVersion.txt in source, this will be displayed by web ui
echo "$HASH $TAG" > ./../../src/Wbtb.Core.Web/currentVersion.txt 

echo "Building CLI ..."
docker run \
    -e TAG=$TAG \
    -v $(pwd)./../../src:/tmp/wbtb \
    mcr.microsoft.com/dotnet/sdk:6.0 \
    sh -c "cd /tmp/wbtb && \
        dotnet restore Wbtb.Core.CLI && \
        dotnet publish Wbtb.Core.CLI --configuration Release"

echo "Building web server ..."
docker run \
    -e TAG=$TAG \
    -v $(pwd)./../../src:/tmp/wbtb \
    mcr.microsoft.com/dotnet/sdk:6.0 \
    sh -c "cd /tmp/wbtb && \
        dotnet restore Wbtb.Core.Web && \
        dotnet publish Wbtb.Core.Web /property:PublishWithAspNetCoreTargetManifest=false --configuration Release"

echo "Building web static assets ..."
docker run \
    -v $(pwd)./../../src:/tmp/wbtb \
    shukriadams/node12build:0.0.4 \
    sh -c "cd /tmp/wbtb/Wbtb.Core.Web/frontend && \
        sh ./setup.sh
        npm install && \
        npm run icons && \
        npm run build"

echo "Building Message queue ..."
docker run \
    -v $(pwd)./../../src:/tmp/wbtb \
    mcr.microsoft.com/dotnet/sdk:6.0 \
    sh -c "cd /tmp/wbtb && \
        dotnet restore MessageQueue && \
        dotnet publish MessageQueue /property:PublishWithAspNetCoreTargetManifest=false --configuration Release"

# build hosting container
echo "Building container ..."
cd ./../../src
docker build -t shukriadams/wbtb . 
docker tag shukriadams/wbtb:latest shukriadams/wbtb:$TAG 
cd -

if [ $SMOKETEST -eq 1 ]; then
    docker-compose -f docker-compose-test.yml down --remove-orphans
    docker-compose -f docker-compose-test.yml up -d 
    sleep 5  # wait a few seconds to make sure app in container has started
    STATUS=$(curl -s -o /dev/null -w "%{http_code}" localhost:49022) 
    docker-compose -f docker-compose-test.yml down 
    if [ "$STATUS" != "200" ]; then
        echo "test container returned unexpected value ${STATUS}"
        exit 1
    else
        echo "smoke test passed"
    fi
fi

if [ $DOCKERPUSH -eq 1 ]; then
    docker login -u $DOCKER_USER -p $DOCKER_PASS 
    docker push shukriadams/wbtb:$TAG  
fi
