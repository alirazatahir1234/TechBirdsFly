# Media Service - Quick Start Guide ⚡

## One-Minute Setup

```bash
# 1. Create database
createdb techbirdsfly_media

# 2. Run migrations
cd services/media-service/src
dotnet ef database update

# 3. Start service
dotnet run

# ✅ Service running on http://localhost:5009
```

## Essential Commands

| Command | Effect |
|---------|--------|
| `dotnet run` | Start Media Service (port 5009) |
| `dotnet build` | Compile service |
| `dotnet ef database update` | Apply migrations |
| `curl http://localhost:5009/api/media/health` | Health check |

## 5 Core Endpoints

```bash
# 1. UPLOAD IMAGE
curl -X POST http://localhost:5009/api/media/upload -F "file=@image.jpg"

# 2. GENERATE AI IMAGE
curl -X POST http://localhost:5009/api/media/generate \
  -H "Content-Type: application/json" \
  -d '{"prompt": "beautiful sunset", "style": "watercolor"}'

# 3. GET IMAGE INFO
curl http://localhost:5009/api/media/{id}

# 4. DELETE IMAGE
curl -X DELETE http://localhost:5009/api/media/{id}

# 5. HEALTH CHECK
curl http://localhost:5009/api/media/health
```

## Integration with Gateway

```bash
# Via Gateway (recommended)
curl http://localhost:5500/api/media/health

# Swagger Docs
# Gateway: http://localhost:5500/swagger
# Direct: http://localhost:5009/swagger
```

## Configuration

**appsettings.json Key Settings:**
```json
{
  "Kestrel": { "Http": { "Url": "http://localhost:5009" } },
  "ConnectionStrings": { "DefaultConnection": "Host=localhost;...;Database=techbirdsfly_media" },
  "Storage": { "LocalPath": "./uploads" },
  "AI": { "LlamaBaseUrl": "http://localhost:11434" }
}
```

## Architecture at a Glance

```
Request → MediaController → MediatR Handler → Application Logic
  ↓                                            ↓
  MediaFile ← Repository ← EF Core → PostgreSQL
                              ↓
                         Storage Service (local/cloud)
                              ↓
                         AI Service (Ollama/Llama)
```

## File Locations

| Layer | Location | Files |
|-------|----------|-------|
| Domain | `Domain/` | 6 files (Entity, Interfaces, Exceptions) |
| Application | `Application/Features/` | 8 files (Commands, Handlers, Queries) |
| Infrastructure | `Infrastructure/` | 4 files (DbContext, Repository, Storage, AI) |
| WebAPI | `WebAPI/Controllers/` | 2 files (Controller, DTO) |
| Config | `src/` | 3 files (Program.cs, appsettings.json, .csproj) |

## Troubleshooting

**Service won't start?**
- Check PostgreSQL is running: `pg_isready -h localhost`
- Check port 5009 is free: `lsof -i :5009`
- Check database exists: `psql -l | grep techbirdsfly_media`

**AI generation not working?**
- Ensure Ollama running: `ollama serve`
- Check Ollama URL in appsettings.json
- Verify llava model: `ollama list`

**File uploads fail?**
- Check `./uploads` directory exists and is writable
- Verify file size limit in Program.cs
- Check available disk space

## Next: Gateway Integration

1. Add to gateway `appsettings.json`:
```json
"media": {
  "Destinations": { "media/destination1": { "Address": "http://localhost:5009" } },
  "HealthCheck": { "Active": { "Enabled": true, "Path": "/api/media/health" } }
}
```

2. Add route: `"ClusterId": "media", "Match": { "Path": "/api/media/**" }`

3. Restart gateway

## Done! 🎉

Media Service is now ready. Move to Phase 9 for final deployment package.

