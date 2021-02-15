FROM shukriadams/whobrokethebuild:latest

# revert to root to install
USER root

RUN apt-get update \
    && apt-get install git -y \
    && apt-get install subversion -y \
    && apt-get install curl -y \ 
    && curl -sL https://cdist2.perforce.com/perforce/r20.1/bin.linux26x86_64/p4 --output /tmp/p4 \
    && cp /tmp/p4 /usr/local/bin/ \
    && chmod +x /usr/local/bin/p4 

# reset user to expected
USER app