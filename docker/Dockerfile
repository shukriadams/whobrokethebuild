FROM alpine:3.9

RUN apk update \
    && apk upgrade \
    && apk add bash \
    && apk add nodejs=10.14.2-r0 \
    && apk add npm=10.14.2-r0 \
    && mkdir -p /var/app \
    && adduser -D -u 1000 app \
    && chmod 700 -R /var/app \
    && chown -R app /var/app

# copy the build zip in, unpack, strips the first two folders when unzipping, getting rid of /tmp/build
# then clean up build.zip
COPY ./build.tar.gz /tmp
RUN tar -xzvf /tmp/build.tar.gz -C /var/app --strip 2
RUN rm /tmp/build.tar.gz

USER app

# cd to app and start, do not use WORKDIR, as this makes it complicated to change start CMD 
# and debug later
CMD cd /var/app && npm start