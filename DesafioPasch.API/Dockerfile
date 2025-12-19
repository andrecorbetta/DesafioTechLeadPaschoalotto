# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia tudo e publica
COPY . .
RUN dotnet restore ./DesafioPasch.API/DesafioPasch.API.csproj
RUN dotnet publish ./DesafioPasch.API/DesafioPasch.API.csproj -c Release -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Porta padrão do container
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Copia artefatos publicados
COPY --from=build /app/publish .

# Opcional: define ambiente
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "DesafioPasch.API.dll"]
