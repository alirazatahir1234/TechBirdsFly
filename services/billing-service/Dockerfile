FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project file
COPY src/*.csproj ./
RUN dotnet restore

# Copy source
COPY src/ ./
RUN dotnet build -c Release -o out

# Publish
RUN dotnet publish -c Release -o publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5005
ENTRYPOINT ["dotnet", "BillingService.dll"]
