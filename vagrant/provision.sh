#!/usr/bin/env bash
sudo apt-get update

sudo apt-get install git -y
curl -sL https://deb.nodesource.com/setup_12.x | sudo -E bash -
sudo apt-get install nodejs -y

# docker
sudo apt install docker.io -y
sudo apt install docker-compose -y
sudo usermod -aG docker vagrant

# nodejs packages
sudo npm install yarn -g
sudo npm install uglify-es -g
sudo npm install concat-cli -g
# reguired by webfonts-generator package
sudo apt-get install build-essential -y

# perforce
curl -sL https://cdist2.perforce.com/perforce/r20.1/bin.linux26x86_64/p4 --output /tmp/p4 
sudo cp /tmp/p4 /usr/local/bin/ 
sudo chmod +x /usr/local/bin/p4 
# Note : if accessing an ssl-protected p4 instance, you will have to trust it in this dev env with
# p4 trust -i ssl:yourserver:1666
# p4 trust -f -y

# 
sudo apt-get install subversion -y

# force github into known hosts so build script can clone without prompt. yes, this is
# insecure because it opens for MITM attack, but I don't have anything better right now.
ssh-keyscan github.com >> ~/.ssh/known_hosts

# force startup folder to vagrant project
echo "cd /vagrant/src" >> /home/vagrant/.bashrc

# set hostname, makes console easier to identify
sudo echo "whobrokethebuild" > /etc/hostname
sudo echo "127.0.0.1 whobrokethebuild" >> /etc/hosts

sudo chmod 700 -R /home/vagrant/.ssh
