FROM ubuntu:20.04

# squelch tzdata prompt
ENV DEBIAN_FRONTEND=noninteractive

RUN apt-get update \
    && apt-get install -y wget \
    && apt-get install -y tzdata \
    && wget -q https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb \
    && sh -c 'dpkg -i packages-microsoft-prod.deb' \
    && apt-get install apt-transport-https -y \
    && apt-get update \
    && apt-get install aspnetcore-runtime-6.0 -y \
    ## clean up
    && rm packages-microsoft-prod.deb \
    && apt-get remove wget -y \
    && apt-get remove apt-transport-https -y \
    && useradd -u 1000 wbtb \
    && mkdir -p /var/wbtb \
    && chmod 700 -R /var/wbtb \
    && chown wbtb /var/wbtb

# this assumes the application has already been built in publish mode
COPY ./bin/Release/net6.0/. /var/wbtb

USER wbtb

# set wbtb default log level
ENV Logging__LogLevel__Microsoft=Warning

CMD sh -c 'cd /var/wbtb && dotnet Wbtb.Core.Web.dll'