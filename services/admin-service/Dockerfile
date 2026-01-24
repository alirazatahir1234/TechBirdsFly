FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project files (admin-service and its dependencies)
COPY services/admin-service/src/*.csproj ./services/admin-service/src/
COPY services/cache-service/src/CacheClient/*.csproj ./services/cache-service/src/CacheClient/
COPY src/Shared/*.csproj ./src/Shared/

# Restore dependencies
WORKDIR /app/services/admin-service/src
RUN dotnet restore

# Copy source code
WORKDIR /app
COPY services/admin-service/src/ ./services/admin-service/src/
COPY services/cache-service/src/CacheClient/ ./services/cache-service/src/CacheClient/
COPY src/Shared/ ./src/Shared/

# Build
WORKDIR /app/services/admin-service/src
RUN dotnet build -c Release -o /app/out

# Publish
RUN dotnet publish -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5006
ENTRYPOINT ["dotnet", "TechBirdsFly.AdminService.dll"]
