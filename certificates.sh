#!/bin/bash

# Interactive script to create or renew SSL certificates.

STATUS="STOPPED"
docker container inspect utili-frontend > /dev/null && STATUS="RUNNING"

if [ $STATUS = "RUNNING" ]; then
    echo -e "\n\nStopping frontend container while Certbot needs port 80...\n\n"
    docker compose stop frontend
fi

sudo docker run -it --rm --name certbot \
    -v "/etc/letsencrypt:/etc/letsencrypt" \
    -v "/var/lib/letsencrypt:/var/lib/letsencrypt" \
    -p 80:80 \
    certbot/certbot certonly

if [ $STATUS = "RUNNING" ]; then
    echo -e "\n\nRestarting frontend container now that Certbot is finished...\n\n"
    docker compose start frontend
fi
