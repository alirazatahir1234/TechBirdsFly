FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/ExportService.Api/ExportService.Api.csproj", "src/ExportService.Api/"]
COPY ["src/ExportService.Application/ExportService.Application.csproj", "src/ExportService.Application/"]
COPY ["src/ExportService.Infrastructure/ExportService.Infrastructure.csproj", "src/ExportService.Infrastructure/"]
COPY ["src/ExportService.Domain/ExportService.Domain.csproj", "src/ExportService.Domain/"]

# Restore dependencies
RUN dotnet restore "src/ExportService.Api/ExportService.Api.csproj"

# Copy source code
COPY . .

# Build
WORKDIR "/src/src/ExportService.Api"
RUN dotnet build "ExportService.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "ExportService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create exports directory
RUN mkdir -p /app/exports

EXPOSE 8200
ENV ASPNETCORE_URLS=http://+:8200

ENTRYPOINT ["dotnet", "ExportService.Api.dll"]
