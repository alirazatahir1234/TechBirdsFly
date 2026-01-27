# Multi-stage build for Media Service

# ============================================================================
# BUILD STAGE
# ============================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/MediaService.csproj", "MediaService/"]

# Restore dependencies
RUN dotnet restore "MediaService/MediaService.csproj"

# Copy source code
COPY ["src/", "MediaService/"]

# Build application
WORKDIR "/src/MediaService"
RUN dotnet build "MediaService.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "MediaService.csproj" -c Release -o /app/publish

# ============================================================================
# RUNTIME STAGE
# ============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create storage directory
RUN mkdir -p /app/storage/media

# Copy published application
COPY --from=build /app/publish .

# Create non-root user
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

# Expose port
EXPOSE 5007

# Environment variables (can be overridden at runtime)
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5007/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "MediaService.dll"]
