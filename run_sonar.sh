#!/usr/bin/env bash

SONAR_TOKEN=""
SONAR_PROJECT_KEY="portfolio-dotnet-api"
SONAR_HOST_URL="http://localhost:3000"

echo "====> Limpando relatórios antigos..."
rm -rf ./coverage-results
mkdir -p ./coverage-results

echo "====> Iniciando SonarScanner..."
dotnet sonarscanner begin \
  /k:"${SONAR_PROJECT_KEY}" \
  /d:sonar.host.url="${SONAR_HOST_URL}" \
  /d:sonar.token="${SONAR_TOKEN}" \
  /d:sonar.cs.cobertura.reportsPaths="coverage-results/**/coverage.cobertura.xml" \
  /d:sonar.coverage.exclusions="**/Migrations/**,**/Program.cs,**/Settings/*.cs,**/Events/*.cs,**/Notifications/*.cs,**/Models/*.cs,**/Entities/EntityMetadata.cs,**/obj/**,**/*.g.cs,**/AppBuilderCustom.cs,**/Builders/*.cs,**/Extensions/*.cs,**/*Factory.cs,**/SwaggerConfigurator.cs,**/KestrelConfigurator.cs,**/CorsPolicyExtensions.cs,**/PostgresConnectionStringBuilder.cs"

echo "====> Compilando o projeto..."
dotnet build --no-incremental

echo "====> Rodando testes e coletando cobertura..."
dotnet test --no-build --collect:"XPlat Code Coverage" --results-directory ./coverage-results

echo "====> Finalizando e enviando para o Sonar..."
dotnet sonarscanner end /d:sonar.token="${SONAR_TOKEN}"

echo "====> Processo concluído!"