# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

WORKDIR /src

# Copy project files
COPY ["src/EventBusService.csproj", "src/"]

# Restore dependencies
RUN dotnet restore "src/EventBusService.csproj"

# Copy source code
COPY ["src/", "src/"]

# Build
RUN dotnet build "src/EventBusService.csproj" -c Release -o /app/build

# Publish stage
FROM builder AS publish
RUN dotnet publish "src/EventBusService.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Install health check tools
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

EXPOSE 5020

ENV ASPNETCORE_URLS=http://+:5020

ENTRYPOINT ["dotnet", "EventBusService.dll"]
