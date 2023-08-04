FROM ubuntu:20.04

# squelch tzdata prompt
ENV DEBIAN_FRONTEND=noninteractive

RUN apt-get update \
    && apt-get install wget -y \
    && apt-get install git -y \
    && apt-get install tzdata -y \
    && wget -q https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb \
    && sh -c 'dpkg -i packages-microsoft-prod.deb' \
    && apt-get install apt-transport-https -y \
    && apt-get update \
    && apt-get install aspnetcore-runtime-6.0 -y \
    # p4 only (REMOVE)
    && wget https://www.perforce.com/downloads/perforce/r22.1/bin.linux26x86_64/helix-core-server.tgz -O /tmp/p4client.tgz \
    && mkdir -p /usr/bin/p4 \
    && tar zxvf /tmp/p4client.tgz -C /usr/bin/p4 \
    && rm /tmp/p4client.tgz \
    && chmod +x /usr/bin/p4/p4 \
    && chmod +x /usr/bin/p4/p4broker \
    && chmod +x /usr/bin/p4/p4d \
    && chmod +x /usr/bin/p4/p4p \
    ## clean up
    && rm packages-microsoft-prod.deb \
    && apt-get remove wget -y \
    && apt-get remove apt-transport-https -y \
    # set up user+directories
    && mkdir -p /var/wbtb/External 

# WARNING : The following assumes the application has already been built in publish mode. See /build/docker/build.sh in project source to do this.
# core web server
COPY ./Wbtb.Core.Web/bin/Release/net6.0/. /var/wbtb/Wbtb.Core.Web
COPY ./Wbtb.Core.Web/wwwroot/. /var/wbtb/Wbtb.Core.Web/wwwroot
# include messageQueue
COPY ./MessageQueue/bin/Release/net6.0/. /var/wbtb/MessageQueue
# include CLI
COPY ./Wbtb.Core.CLI/bin/Release/net6.0/linux-x64/. /var/wbtb/Wbtb.Core.CLI
COPY ./Wbtb.Core.CLI/wbtb.sh /usr/bin/wbtb
# include built-in plugins WBTB always ships with
COPY ./Wbtb.Extensions.Data.FileSystem/bin/Release/net6.0/. /var/wbtb/Wbtb.Extensions.Data.FileSystem
COPY ./Wbtb.Extensions.Data.Postgres/bin/Release/net6.0/. /var/wbtb/Wbtb.Extensions.Data.Postgres
COPY ./Wbtb.Extensions.BuildServer.JenkinsSandbox/bin/Release/net6.0/. /var/wbtb/Wbtb.Extensions.BuildServer.JenkinsSandbox
COPY ./Wbtb.Extensions.SourceServer.PerforceSandbox/bin/Release/net6.0/. /var/wbtb/Wbtb.Extensions.SourceServer.PerforceSandbox
# testing only, these need to be spun before proper release
COPY ./Wbtb.Extensions.BuildServer.Jenkins/bin/Release/net6.0/. /var/wbtb/Wbtb.Extensions.BuildServer.Jenkins
COPY ./Wbtb.Extensions.SourceServer.Perforce/bin/Release/net6.0/. /var/wbtb/Wbtb.Extensions.SourceServer.Perforce
COPY ./Wbtb.Extensions.Messaging.Slack/bin/Release/net6.0/. /var/wbtb/Wbtb.Extensions.Messaging.Slack
COPY ./Wbtb.Extensions.LogParsing.Cpp/bin/Release/net6.0/. /var/wbtb/Wbtb.Extensions.LogParsing.Cpp
COPY ./Wbtb.Extensions.LogParsing.BasicErrors/bin/Release/net6.0/. /var/wbtb/Wbtb.Extensions.LogParsing.BasicErrors
COPY ./Wbtb.Extensions.LogParsing.JenkinsSelfFailing/bin/Release/net6.0/. /var/wbtb/Wbtb.Extensions.LogParsing.JenkinsSelfFailing
COPY ./Wbtb.Extensions.LogParsing.Unreal/bin/Release/net6.0/. /var/wbtb/Wbtb.Extensions.LogParsing.Unreal

COPY ./Wbtb.Extensions.PostProcessing.AcmeGamesBlamer/bin/Release/net6.0/. /var/wbtb/Wbtb.Extensions.PostProcessing.AcmeGamesBlamer
COPY ./Wbtb.Extensions.PostProcessing.JenkinsSelfBlame/bin/Release/net6.0/. /var/wbtb/Wbtb.Extensions.PostProcessing.JenkinsSelfBlame

Wbtb.Extensions.LogParsing.Unreal

RUN chmod +x /usr/bin/wbtb 

# p4 only (REMOVE)
ENV PATH="${PATH}:/usr/bin/p4" 

# set wbtb default log level
ENV Logging__LogLevel__Microsoft=Warning
# declare config location for wbtb CLI app
ENV WBTB_CONFIGPATH=/var/wbtb/Wbtb.Core.Web/config.yml

CMD sh -c 'dotnet /var/wbtb/MessageQueue/MessageQueue.dll & cd /var/wbtb/Wbtb.Core.Web && dotnet Wbtb.Core.Web.dll && fg'
