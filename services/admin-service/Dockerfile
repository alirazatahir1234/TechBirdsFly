FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project files (admin-service and its dependencies)
COPY src/*.csproj ./admin-service/
COPY ../cache-service/src/CacheClient/*.csproj ./cache-service/src/CacheClient/

# Restore dependencies
WORKDIR /app/admin-service
RUN dotnet restore

# Copy source code
WORKDIR /app
COPY src/ ./admin-service/
COPY ../cache-service/src/CacheClient/ ./cache-service/src/CacheClient/

# Build
WORKDIR /app/admin-service
RUN dotnet build -c Release -o /app/out

# Publish
RUN dotnet publish -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5006
ENTRYPOINT ["dotnet", "TechBirdsFly.AdminService.dll"]
