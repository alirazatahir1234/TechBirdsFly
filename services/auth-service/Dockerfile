# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files (auth-service and its dependencies)
COPY services/auth-service/src/AuthService.csproj ./auth-service/
COPY services/cache-service/src/CacheClient/CacheClient.csproj ./cache-service/src/CacheClient/

# Restore dependencies
WORKDIR /src/auth-service
RUN dotnet restore

# Copy source code
WORKDIR /src
COPY services/auth-service/src/ ./auth-service/
COPY services/cache-service/src/CacheClient/ ./cache-service/src/CacheClient/

# Build and publish
WORKDIR /src/auth-service
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published files
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5001
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 5001

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5001/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "AuthService.dll"]
