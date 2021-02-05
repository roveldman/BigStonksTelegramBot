#!/bin/bash
mkdir backups
mv hello.db ./backups/hello.$(date +%s).db
docker ps --all
read -p "Copy From: " answer
docker cp $answer:/app/hello.db hello.db
docker stop $answer
docker rm $answer
docker build -t stonk-dev .
docker run -d --restart unless-stopped  stonk-dev 
