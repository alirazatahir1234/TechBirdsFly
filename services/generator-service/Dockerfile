# TechBirdsFly Generator Service - Cloud Run Compatible
# Multi-stage build for optimized image size

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies (layer caching optimization)
COPY ["src/GeneratorService.csproj", "src/"]
RUN dotnet restore "src/GeneratorService.csproj"

# Copy source code and build
COPY src/ ./src/
WORKDIR /src/src
RUN dotnet publish "GeneratorService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Copy published application
COPY --from=build /app/publish .

# Cloud Run requires PORT env variable
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Health check for container orchestration
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
  CMD curl --fail http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "GeneratorService.dll"]
