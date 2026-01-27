# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files
COPY TemplateService.sln .
COPY src/TemplateService.Domain/TemplateService.Domain.csproj src/TemplateService.Domain/
COPY src/TemplateService.Application/TemplateService.Application.csproj src/TemplateService.Application/
COPY src/TemplateService.Infrastructure/TemplateService.Infrastructure.csproj src/TemplateService.Infrastructure/
COPY src/TemplateService.Api/TemplateService.Api.csproj src/TemplateService.Api/

# Restore dependencies
RUN dotnet restore TemplateService.sln

# Copy source code
COPY src/ src/

# Restore again to ensure all dependencies are available after source copy
RUN dotnet restore TemplateService.sln

# Build the application
RUN dotnet build TemplateService.sln -c Release --no-restore

# Publish the application
RUN dotnet publish src/TemplateService.Api/TemplateService.Api.csproj -c Release --no-build -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy from build stage
COPY --from=build /app/publish .

# Expose ports
EXPOSE 8080
EXPOSE 8443

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=10s --timeout=3s --start-period=5s --retries=3 \
  CMD dotnet-health-check || exit 1

# Run the application
ENTRYPOINT ["dotnet", "TemplateService.Api.dll"]
