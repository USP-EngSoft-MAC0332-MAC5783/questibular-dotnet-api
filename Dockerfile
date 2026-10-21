# ---------- BUILD ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia os .csproj primeiro (aproveita cache do Docker)
COPY ["Portfolio.Api/Portfolio.Api.csproj", "Portfolio.Api/"]
COPY ["Portfolio.ApiBase/Portfolio.ApiBase.csproj", "Portfolio.ApiBase/"]
COPY ["Portfolio.Aws/Portfolio.Aws.csproj", "Portfolio.Aws/"]
COPY ["Portfolio.Domain/Portfolio.Domain.csproj", "Portfolio.Domain/"]

RUN dotnet restore "Portfolio.Api/Portfolio.Api.csproj"

# Copia o resto do código e publica
COPY . .

WORKDIR /src/Portfolio.Api
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

COPY --from=build /app/publish .

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV ASPNETCORE_URLS=http://0.0.0.0:5000


EXPOSE 5000
ENTRYPOINT ["dotnet", "Portfolio.Api.dll"]