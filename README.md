# Portfolio API

A modern REST API built with **.NET 10**, following **Domain-Driven Design (DDD)** and **Hexagonal Architecture** principles. It features a rich domain model, CQRS-like separation for read/write, asynchronous event processing, and robust security (JWT, 2FA).

For a deep dive into the architectural decisions and code structure, please read the [ARCHITECTURE.md](./ARCHITECTURE.md).

## Tech Stack

- **Framework**: .NET 10, ASP.NET Core
- **Database**: PostgreSQL (Entity Framework Core)
- **Architecture**: Hexagonal, DDD, CQRS
- **Mapping**: Mapster
- **DevOps**: Docker, Docker Compose, SonarQube

---

## Development Setup

### Prerequisites

- [Docker](https://docs.docker.com/get-docker/) and Docker Compose installed
- [.NET SDK](https://dotnet.microsoft.com/download) installed
- A database client such as [DBeaver](https://dbeaver.io/) (optional, for inspecting Postgres)

### 1. Environment file

Copy `.env.example` to `.env` and adjust the values as needed:

```bash
cp .env.example .env
```

### 2. Start the containers

Run `docker-net.sh`. It creates the external Docker network (`db-net`) and starts the `postgres` and `sonarqube` containers.

Verify that everything is up by running:

```bash
docker ps
```

You should see an output similar to:

```text
CONTAINER ID   IMAGE                 COMMAND                  CREATED          STATUS          PORTS                                         NAMES
0d69c2edc29b   sonarqube:community   "/opt/sonarqube/dock…"   6 seconds ago    Up 5 seconds    0.0.0.0:3000->9000/tcp, [::]:3000->9000/tcp   sonarqube
85c16d3f22e9   postgres:15-alpine    "docker-entrypoint.s…"   24 seconds ago   Up 24 seconds   5432/tcp                                      postgres
```

### 3. Start the API

```bash
docker compose up -d --build
```
*The PostgreSQL database will be initialized, and Migrations will be applied automatically on API startup.*

Access Swagger to test the endpoints: [http://localhost:5000/swagger](http://localhost:5000/swagger)

### 4. SonarQube Setup (Optional)

#### 4.1 First access
Open SonarQube in your browser: [http://localhost:3000/](http://localhost:3000/)
Log in using the credentials from your `.env` file, then **change the password** immediately on first login.

#### 4.2 Create the project
1. Select **"Create a local project"**.
2. Fill in the fields:
   | Field            | Value                  |
   | ---------------- | ---------------------- |
   | Display name     | `portfolio-dotnet-api` |
   | Project key      | `portfolio-dotnet-api` |
   | Main branch name | `main`                 |
3. Select **"Is custom"**, then **"Previous version"** — this lets you create a version for each change.
4. Click **"Create project"**.

#### 4.3 Generate a token & Analyze
1. In the **"Project onboarding"** screen, click **"Locally"**.
2. Create a token with **"No expiration"**. **Save both your token and password safely**.
3. Install the scanner: `dotnet tool install --global dotnet-sonarscanner`
4. Run the analysis (replace `<YOUR_TOKEN>`):

```bash
dotnet sonarscanner begin /k:"portfolio-dotnet-api" /d:sonar.host.url="http://localhost:3000" /d:sonar.token="<YOUR_TOKEN>"
dotnet build
dotnet sonarscanner end /d:sonar.token="<YOUR_TOKEN>"
```

### 5. Postgres Connection
Connect to the Postgres instance using the credentials from your `.env` file. Any client that supports PostgreSQL will work (e.g., [DBeaver](https://dbeaver.io/)).

![Postgres connection](./docs/tutorials/PostgresConn.png)