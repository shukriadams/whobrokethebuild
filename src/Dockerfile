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
    # set up user+directories
    && useradd -u 1000 wbtb \
    && mkdir -p /var/wbtb/External \
    && chmod 700 -R /var/wbtb \
    && chown wbtb -R /var/wbtb

# this assumes the application has already been built in publish mode
COPY ./Wbtb.Core.Web/bin/Release/net6.0/. /var/wbtb/Wbtb.Core.Web
# include messageQueue
COPY ./MessageQueue/bin/Release/net6.0/. /var/wbtb/MessageQueue
# include built-in plugins WBTB always ships with
COPY ./Wbtb.Extensions.Data.FileSystem/bin/Release/net6.0/. /var/wbtb/Wbtb.Extensions.Data.FileSystem
COPY ./Wbtb.Extensions.Data.Postgres/bin/Release/net6.0/. /var/wbtb/Wbtb.Extensions.Data.Postgres
COPY ./Wbtb.Extensions.BuildServer.JenkinsSandbox/bin/Release/net6.0/. /var/wbtb/Wbtb.Extensions.BuildServer.JenkinsSandbox
COPY ./Wbtb.Extensions.SourceServer.PerforceSandbox/bin/Release/net6.0/. /var/wbtb/Wbtb.Extensions.SourceServer.PerforceSandbox

USER wbtb

# set wbtb default log level
ENV Logging__LogLevel__Microsoft=Warning

CMD sh -c 'dotnet /var/wbtb/MessageQueue/MessageQueue.dll & cd /var/wbtb/Wbtb.Core.Web && dotnet Wbtb.Core.Web.dll && fg'