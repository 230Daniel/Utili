#!/bin/bash

# Script to pull the latest changes.

docker compose -f docker-compose-prebuilt.yaml down
git pull
docker compose -f docker-compose-prebuilt.yaml pull
docker compose -f docker-compose-prebuilt.yaml up -d
