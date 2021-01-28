FROM ubuntu:20.04

# squelch idiotic tzdata prompt
ENV DEBIAN_FRONTEND=noninteractive

RUN apt-get update \
    && apt-get install bash -y \
    && apt-get install git -y \
    && apt-get install nodejs -y \
    && apt-get install npm -y \
    && apt-get install subversion -y \
    && mkdir -p /usr/app \
    && adduser -u 1000 app \
    && chmod 700 -R /usr/app \
    && chown -R app /usr/app

# copy the build zip in, unpack, stripping the first folder level when unzipping, then clean up build.zip
COPY ./.stage.tar.gz /tmp
RUN tar -xzvf /tmp/.stage.tar.gz -C /usr/app --strip 1 \
    && rm /tmp/.stage.tar.gz

USER app

CMD cd /usr/app && npm start
