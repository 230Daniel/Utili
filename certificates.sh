#!/bin/bash

# Interactive script to create or renew SSL certificates.

STATUS="STOPPED"
docker container inspect utili-nginx > /dev/null && STATUS="RUNNING"

if [ $STATUS = "RUNNING" ]; then
    echo -e "\n\nStopping nginx container while Certbot needs port 80...\n\n"
    docker compose stop nginx
fi

sudo docker run -it --rm --name certbot \
    -v "/etc/letsencrypt:/etc/letsencrypt" \
    -v "/var/lib/letsencrypt:/var/lib/letsencrypt" \
    -p 80:80 \
    certbot/certbot certonly

if [ $STATUS = "RUNNING" ]; then
    echo -e "\n\nRestarting nginx container now that Certbot is finished...\n\n"
    docker compose start nginx
fi
