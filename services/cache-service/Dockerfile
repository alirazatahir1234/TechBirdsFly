# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

WORKDIR /app

# Copy project files
COPY ["services/cache-service/src/CacheService/CacheService.csproj", "services/cache-service/src/CacheService/"]
COPY ["src/Shared/TechBirdsFly.Shared.csproj", "src/Shared/"]

# Restore dependencies
WORKDIR /app/services/cache-service/src/CacheService
RUN dotnet restore

# Copy source code
WORKDIR /app
COPY ["services/cache-service/src/CacheService/", "services/cache-service/src/CacheService/"]
COPY ["src/Shared/", "src/Shared/"]

# Build
WORKDIR /app/services/cache-service/src/CacheService
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM builder AS publish
WORKDIR /app/services/cache-service/src/CacheService
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Install health check tools
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

EXPOSE 5021


ENTRYPOINT ["dotnet", "CacheService.dll"]
