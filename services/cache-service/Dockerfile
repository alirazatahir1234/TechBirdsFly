# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

WORKDIR /src

# Copy project files
COPY ["src/CacheService.csproj", "src/"]

# Restore dependencies
RUN dotnet restore "src/CacheService.csproj"

# Copy source code
COPY ["src/", "src/"]

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
