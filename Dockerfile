# ---------- BUILD ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia os .csproj primeiro (aproveita cache do Docker)
COPY ["src/Portfolio.Api/Portfolio.Api.csproj", "src/Portfolio.Api/"]
COPY ["src/Portfolio.ApiBase/Portfolio.ApiBase.csproj", "src/Portfolio.ApiBase/"]
COPY ["src/Portfolio.Domain/Portfolio.Domain.csproj", "src/Portfolio.Domain/"]
COPY ["src/Portfolio.DataAccess/Portfolio.DataAccess.csproj", "src/Portfolio.DataAccess/"]
COPY ["Directory.Packages.props", "."]
COPY ["Portfolio.slnx", "."]

RUN dotnet restore "src/Portfolio.Api/Portfolio.Api.csproj"

# Copia o resto do código e publica
COPY . .

WORKDIR /src/src/Portfolio.Api
RUN dotnet publish "Portfolio.Api.csproj" \
    -c Release \
    -r linux-x64 \
    --self-contained false \
    -o /app/publish \
    -p:UseAppHost=false \
    -p:DebugType=none \
    -p:DebugSymbols=false

# ---------- RUNTIME ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

USER app

COPY --from=build --chown=app:app /app/publish .

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

EXPOSE 5000
EXPOSE 5001


ENTRYPOINT ["dotnet", "Portfolio.Api.dll"]