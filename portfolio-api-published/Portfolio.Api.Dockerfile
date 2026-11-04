# Portfolio.Api.Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

# Copy the published output (everything in the build context)
COPY publish/ .

# Environment + port (ASP.NET Core 8+ image defaults to 8080)
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

# .NET 10 aspnet image already runs as non-root "app" user
ENTRYPOINT ["dotnet", "Portfolio.Api.dll"]