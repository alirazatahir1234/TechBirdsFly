# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

WORKDIR /src

# Copy project files (event-bus-service and its dependencies)
COPY ["src/EventBusService.csproj", "event-bus-service/src/"]
COPY ["../cache-service/src/CacheClient/CacheClient.csproj", "cache-service/src/CacheClient/"]

# Restore dependencies
RUN dotnet restore "event-bus-service/src/EventBusService.csproj"

# Copy source code
COPY ["src/", "event-bus-service/src/"]
COPY ["../cache-service/src/CacheClient/", "cache-service/src/CacheClient/"]

# Build
RUN dotnet build "event-bus-service/src/EventBusService.csproj" -c Release -o /app/build

# Publish stage
FROM builder AS publish
RUN dotnet publish "event-bus-service/src/EventBusService.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Install health check tools
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

EXPOSE 5020

ENV ASPNETCORE_URLS=http://+:5020

ENTRYPOINT ["dotnet", "EventBusService.dll"]
