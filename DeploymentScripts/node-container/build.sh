#!/bin/env bash
sudo docker build -t monitron/node-container .
echo "Exporting image"
sudo docker save monitron/node-container > node-container-image.tar
sudo chown $USER:$USER node-container-image.tar
