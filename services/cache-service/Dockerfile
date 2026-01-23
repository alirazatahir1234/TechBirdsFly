# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

WORKDIR /src

# Copy project files
COPY ["services/cache-service/src/CacheService/CacheService.csproj", "src/CacheService/"]
COPY ["src/Shared/TechBirdsFly.Shared.csproj", "Shared/"]

# Restore dependencies
RUN dotnet restore "src/CacheService/CacheService.csproj"

# Copy source code
COPY ["services/cache-service/src/CacheService/", "src/CacheService/"]
COPY ["src/Shared/", "Shared/"]

# Build
RUN dotnet build "src/CacheService.csproj" -c Release -o /app/build

# Publish stage
FROM builder AS publish
RUN dotnet publish "src/CacheService.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Install health check tools
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

EXPOSE 5021

ENV ASPNETCORE_URLS=http://+:5021

ENTRYPOINT ["dotnet", "CacheService.dll"]
