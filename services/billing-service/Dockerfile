FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project file
COPY src/BillingService/BillingService.csproj ./BillingService/
RUN dotnet restore ./BillingService/BillingService.csproj

# Copy source
COPY src/BillingService/ ./BillingService/
RUN dotnet build ./BillingService/BillingService.csproj -c Release -o out

# Publish
RUN dotnet publish ./BillingService/BillingService.csproj -c Release -o publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5005
ENTRYPOINT ["dotnet", "BillingService.dll"]
