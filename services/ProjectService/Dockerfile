# Multi-stage build for Project Service

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

WORKDIR /src

# Copy project files
COPY ["src/ProjectService.Domain/ProjectService.Domain.csproj", "ProjectService.Domain/"]
COPY ["src/ProjectService.Application/ProjectService.Application.csproj", "ProjectService.Application/"]
COPY ["src/ProjectService.Infrastructure/ProjectService.Infrastructure.csproj", "ProjectService.Infrastructure/"]
COPY ["src/ProjectService.Api/ProjectService.Api.csproj", "ProjectService.Api/"]

# Restore dependencies
RUN dotnet restore "ProjectService.Api/ProjectService.Api.csproj"

# Copy source code
COPY src/ .

# Build application
RUN dotnet build "ProjectService.Api/ProjectService.Api.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "ProjectService.Api/ProjectService.Api.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=builder /app/publish .

# Expose port
EXPOSE 5004

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5004/health || exit 1

# Run application
ENTRYPOINT ["dotnet", "ProjectService.Api.dll"]
