#!/bin/bash

# Script to download Hastebin and configure it for use with Docker

mkdir hastebin-data
chmod 777 hastebin-data
cd src/Hastebin || git clone https://github.com/230Daniel/haste-server src/Hastebin && cd src/Hastebin
git pull
