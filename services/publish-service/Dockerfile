# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Domain/PublishService.Domain.csproj", "Domain/"]
COPY ["src/Application/PublishService.Application.csproj", "Application/"]
COPY ["src/Infrastructure/PublishService.Infrastructure.csproj", "Infrastructure/"]
COPY ["src/WebAPI/PublishService.WebAPI.csproj", "WebAPI/"]

RUN dotnet restore "WebAPI/PublishService.WebAPI.csproj"

COPY ["src/", "."]

RUN dotnet build "WebAPI/PublishService.WebAPI.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "WebAPI/PublishService.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 5025

ENTRYPOINT ["dotnet", "PublishService.WebAPI.dll"]
