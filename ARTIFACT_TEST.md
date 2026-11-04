# Tutorial: How to run the API via Docker

This quick guide explains how to run the API in a simulated Production environment using Docker, assuming the publication package (`.tar.gz`) has already been generated and downloaded from the GitHub Actions Artifacts.

## Prerequisites

Before starting, make sure you have on your machine:

* Docker and Docker Compose installed.
* The infrastructure files: `published-api-docker-compose.yml` e `Portfolio.Api.Dockerfile`.
* The `.env` file in the parent folder (`../.env`) with the environment variables.
* The `../certs` folder containing the SSL certificates (`fullchain.pem` e `privkey.pem`).

## 1. Folder Structure and Artifact Download

Your directory structure should look like this:

```text
questibular-dotnet-api/
 ├── .env
 ├── certs/
 └── portfolio-api-published/
      ├── published-api-docker-compose.yml
      ├── Portfolio.Api.Dockerfile
      └── publish/            <--  Where the API files should go - copy all the extracted files to this folder
```

**Step-by-step:**

1. Go to the Actions tab on your repository's GitHub.
2. Select the most recent build pipeline (or the Releases tab if you are using tags).
3. Download the `api-v{sua_versao}.tar.gz` file (or the artifact `.zip`).
4. Extract the downloaded content into the `portfolio-api-published/publish/` folder.

*(Note: Make sure the `Portfolio.Api.dll` file is directly inside the  `publish/` folder)*.

---

## 2. Run the Container

Open the terminal, navigate to the folder where the docker-compose file is located (`portfolio-api-published`), and run the command:

```bash
docker compose -f published-api-docker-compose.yml --env-file ../.env up -d --build --force-recreate
```

**What this command does:**
* `--env-file ../.env`: Ensures Docker reads the variables from your  `.env` file (required to build the certificate and JWT paths).
* `--build`: Rebuilds the Docker image in case you have updated the files inside the `publish` folder.
* `-d`: Runs in background (detached mode).
* `--force-recreate`: Ensures the old container is destroyed and a new one is created from scratch, avoiding state conflicts.

*(To follow the logs and see if the application started correctly, run: `docker logs -f portfolio-api`)*.

---

## 3. Verify if it is working

Since the environment is simulated as Production, Swagger is disabled (returning 404) for security reasons. To test if the API is up, access the Health Check:

URL: `https://localhost:5001/health`

If the browser (or the command `curl -k https://localhost:5001/health`) returns `200 OK`, the application started successfully and is ready to receive requests.