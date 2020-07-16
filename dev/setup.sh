docker-compose up -d

docker cp ./init.js mongo:tmp/init.js
docker exec -it mongo bash -c "mongo -u root -pexample --authenticationDatabase admin 127.0.0.1:27017/whobrokethebuild < /tmp/init.js"