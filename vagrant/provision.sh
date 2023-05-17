#!/usr/bin/env bash
sudo apt-get update

sudo apt-get install git -y

# dotnetcore
wget -q https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-6.0 -y

# altecover report generator
dotnet tool install --global dotnet-reportgenerator-globaltool --version 4.1.5

# docker
sudo apt install docker.io -y
sudo apt install docker-compose -y
sudo usermod -aG docker vagrant

# nodejs
curl -sL https://deb.nodesource.com/setup_12.x | sudo -E bash -
sudo apt-get install nodejs -y

# nodejs packages needed by wbtb
npm install yarn -g
npm install uglify-es -g
npm install concat-cli -g

# force startup folder to vagrant project
echo "cd /vagrant/src" >> /home/vagrant/.bashrc

# set hostname, makes console easier to identify
sudo echo "wbtb" > /etc/hostname
sudo echo "127.0.0.1 wbtb" >> /etc/hosts
