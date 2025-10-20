# Multi-stage build for User Service

# ============================================================================
# BUILD STAGE
# ============================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["services/user-service/src/UserService/UserService.csproj", "UserService/"]

# Restore dependencies
RUN dotnet restore "UserService/UserService.csproj"

# Copy source code
COPY ["services/user-service/src/UserService/", "UserService/"]

# Build application
WORKDIR "/src/UserService"
RUN dotnet build "UserService.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "UserService.csproj" -c Release -o /app/publish

# ============================================================================
# RUNTIME STAGE
# ============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Create non-root user
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

# Expose port
EXPOSE 5002

# Environment variables (can be overridden at runtime)
ENV ASPNETCORE_URLS=http://+:5002
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5002/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "UserService.dll"]
