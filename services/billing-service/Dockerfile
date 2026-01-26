FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project files (billing-service and its dependencies)
COPY services/billing-service/src/BillingService/BillingService.csproj ./services/billing-service/src/BillingService/
COPY services/cache-service/src/CacheClient/CacheClient.csproj ./services/cache-service/src/CacheClient/

# Restore dependencies
WORKDIR /app/services/billing-service/src/BillingService
RUN dotnet restore

# Copy source code
WORKDIR /app
COPY services/billing-service/src/BillingService/ ./services/billing-service/src/BillingService/
COPY services/cache-service/src/CacheClient/ ./services/cache-service/src/CacheClient/

# Build
WORKDIR /app/services/billing-service/src/BillingService
RUN dotnet build -c Release -o /app/out

# Publish
RUN dotnet publish -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5005
ENTRYPOINT ["dotnet", "BillingService.dll"]
