FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project file
COPY src/EditorService.csproj ./EditorService/

# Restore dependencies
WORKDIR /app/EditorService
RUN dotnet restore

# Copy source code
WORKDIR /app
COPY src/ ./EditorService/

# Build
WORKDIR /app/EditorService
RUN dotnet build -c Release -o /app/out

# Publish
RUN dotnet publish -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5007
ENTRYPOINT ["dotnet", "EditorService.dll"]
