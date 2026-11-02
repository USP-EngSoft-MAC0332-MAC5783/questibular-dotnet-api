#!/usr/bin/env bash


docker network create db-net
docker network create portfolio-network

docker compose -f docker-compose-postgres.yml up -d

docker ps --filter name=postgres

docker compose -f docker-compose-sonar.yml up -d