#!/usr/bin/env bash


docker network create db-net

docker compose -f docker-compose-postgres.yml up -d

docker ps --filter name=postgres

docker compose -f docker-compose-sonar.yml up -d