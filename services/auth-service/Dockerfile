# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project files (auth-service and its dependencies)
COPY services/auth-service/src/AuthService.csproj ./services/auth-service/src/
COPY services/cache-service/src/CacheClient/CacheClient.csproj ./services/cache-service/src/CacheClient/
COPY src/Shared/TechBirdsFly.Shared.csproj ./src/Shared/

# Restore dependencies
WORKDIR /app/services/auth-service/src
RUN dotnet restore

# Copy source code
WORKDIR /app
COPY services/auth-service/src/ ./services/auth-service/src/
COPY services/cache-service/src/CacheClient/ ./services/cache-service/src/CacheClient/
COPY src/Shared/ ./src/Shared/

# Build and publish
WORKDIR /app/services/auth-service/src
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published files
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 5001

# Health check (port will be determined by PORT env var at runtime)
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:${PORT:-5001}/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "AuthService.dll"]
