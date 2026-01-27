# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

WORKDIR /app

# Copy project files (event-bus-service and its dependencies)
COPY ["services/event-bus-service/src/EventBusService.csproj", "services/event-bus-service/src/"]
COPY ["services/cache-service/src/CacheClient/CacheClient.csproj", "services/cache-service/src/CacheClient/"]
COPY ["src/Shared/TechBirdsFly.Shared.csproj", "src/Shared/"]

# Restore dependencies
WORKDIR /app/services/event-bus-service/src
RUN dotnet restore

# Copy source code
WORKDIR /app
COPY ["services/event-bus-service/src/", "services/event-bus-service/src/"]
COPY ["services/cache-service/src/CacheClient/", "services/cache-service/src/CacheClient/"]
COPY ["src/Shared/", "src/Shared/"]

# Build
WORKDIR /app/services/event-bus-service/src
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM builder AS publish
WORKDIR /app/services/event-bus-service/src
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Install health check tools
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

EXPOSE 5020


ENTRYPOINT ["dotnet", "EventBusService.dll"]
