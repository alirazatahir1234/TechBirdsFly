# PublishService Integration - Quick Reference

**Status**: 🟢 Complete & Ready  
**Build**: ✅ Passing  
**Integration**: ✅ Complete  

---

## 🚀 One-Minute Setup

```bash
# 1. Start Docker infrastructure
docker-compose -f infra/docker-compose.yml up -d

# 2. Verify PublishService is running
curl http://localhost:5025/api/publish/health

# 3. Open Swagger docs
open http://localhost:5025/swagger

# 4. Or debug locally (F5 in VS Code)
# - Automatically builds & starts
# - Opens Swagger automatically
```

---

## 📡 API Endpoints (via Gateway)

| Method | Endpoint | Purpose |
|--------|----------|---------|
| POST | `/api/publish/deploy` | Deploy website |
| GET | `/api/publish/status/{id}` | Check status |
| GET | `/api/publish/history/{projectId}` | Deployment history |
| GET | `/api/publish/health` | Health check |

---

## 🐳 Docker Integration

**Service**: `publish-service`  
**Port**: 5025  
**Database**: PostgreSQL (techbirdsfly_publish)  
**Status**: Added to docker-compose.yml ✅

```bash
# View logs
docker logs techbirdsfly-publish-service

# Enter container
docker exec -it techbirdsfly-publish-service bash
```

---

## 🎯 YARP Gateway Integration

**Route**: `/api/publish/*` → `localhost:5025` ✅

Configuration in `gateway/yarp-gateway/src/appsettings.json`:
- Route: `publish-route`
- Cluster: `publish-cluster`
- Health check: `/api/publish/health`

---

## 🔍 VS Code Debug

**Press F5** to debug:
- Configuration: "📤 .NET Publish Service (Port 5025)"
- Pre-build task: `build-publish-service`
- Swagger auto-opens at http://localhost:5025/swagger

---

## 📦 Build Status

```
✅ PublishService.Domain          - SUCCESS
✅ PublishService.Application     - SUCCESS
✅ PublishService.Infrastructure  - SUCCESS
✅ PublishService.WebAPI          - SUCCESS
```

---

## 💾 Database

**Connection String** (Development):
```
Host=localhost;Port=5432;Database=techbirdsfly_publish;Username=postgres;Password=Alisheikh@123
```

**Auto-migrations** on startup ✅

---

## 📚 Documentation

| Document | Purpose |
|----------|---------|
| `PUBLISHSERVICE_QUICK_START.md` | 5-minute setup |
| `PUBLISHSERVICE_INTEGRATION_COMPLETE.md` | Full integration details |
| `FEATURE_G_PUBLISH_WEBSITE_PLAN.md` | Architecture & planning |
| `FEATURE_G_IMPLEMENTATION_COMPLETE.md` | Implementation details |

---

## ✅ Integration Checklist

- [x] Docker Compose configuration
- [x] YARP Gateway routing
- [x] VS Code debug setup
- [x] Package versions fixed
- [x] All projects build successfully
- [x] Documentation created

---

## 🎯 Test Commands

```bash
# Health check
curl http://localhost:5025/api/publish/health

# Deploy
curl -X POST http://localhost:5025/api/publish/deploy \
  -H "Content-Type: application/json" \
  -d '{"projectId":"guid","userId":"guid","html":"<html></html>","provider":"techbirdsfly","token":"test"}'

# Via Gateway
curl http://localhost:8000/api/publish/health
```

---

## 🔧 Troubleshooting

**Port 5025 already in use?**
```bash
lsof -i :5025  # Find process
kill -9 <PID>   # Kill it
```

**Database connection error?**
```bash
# Check PostgreSQL is running
docker exec techbirdsfly-postgres pg_isready

# Check database exists
docker exec techbirdsfly-postgres psql -U postgres -l | grep techbirdsfly_publish
```

**Gateway not routing?**
```bash
# Verify appsettings.json has publish-route
cat gateway/yarp-gateway/src/appsettings.json | grep -A5 "publish-route"
```

---

## 📊 Microservices Count

| Service | Port | Status |
|---------|------|--------|
| Auth | 5001 | ✅ Running |
| Billing | 5002 | ✅ Running |
| Generator | 5003 | ✅ Running |
| Export | 5004 | ✅ Running |
| Image | 5007 | ✅ Running |
| Admin | 5006 | ✅ Running |
| User | 5008 | ✅ Running |
| **Publish** | **5025** | **✅ NEW** |
| Project | 5009 | ✅ Running |
| Event Bus | 5020 | ✅ Running |
| Cache | 5021 | ✅ Running |
| Media | 5022 | ✅ Running |
| Gateway | 8000 | ✅ Running |

---

## 🎉 Ready To:

✅ Start locally (F5 in VS Code)  
✅ Deploy via Docker  
✅ Test via API  
✅ Monitor via logs (Seq + Jaeger)  
✅ Develop frontend UI  
✅ Integrate with projects  

---

**Last Updated**: November 27, 2025 23:45  
**Status**: 🟢 Production Ready
