# Phase 3.2 Services - Quick Deployment Guide

## 🚀 Services Summary

### Image Service (Port 5007)
- **Purpose**: AI-powered image generation and storage
- **Framework**: .NET 8.0 Web API
- **Database**: SQLite (user.db)
- **Key Endpoints**: 
  - `POST /api/image/generate` - Generate images
  - `POST /api/image/upload` - Upload images
  - `GET /api/image/list` - List user images
  - `GET /api/image/health` - Health check

### User Service (Port 5008)
- **Purpose**: User profile and subscription management
- **Framework**: .NET 8.0 Web API
- **Database**: SQLite (user.db)
- **Key Endpoints**:
  - `GET /api/users/me` - Get current user
  - `GET /api/users/{id}/subscription` - Get subscription
  - `POST /api/users/{id}/subscription/upgrade` - Upgrade plan
  - `GET /api/users/health` - Health check

---

## 📦 Local Build & Run

### Build Both Services
```bash
cd services/image-service/src/ImageService && dotnet build
cd services/user-service/src/UserService && dotnet build
```

### Run Individually

**Image Service:**
```bash
cd services/image-service/src/ImageService
dotnet run

# Access: https://localhost:5007 or http://localhost:5006 (Swagger)
```

**User Service:**
```bash
cd services/user-service/src/UserService
dotnet run

# Access: https://localhost:5008 or http://localhost:5007 (Swagger)
```

### Docker Compose (Full Stack)

```bash
cd /Applications/My Drive/TechBirdsFly

# Build and run all services
docker-compose up -d

# View logs
docker-compose logs -f user-service
docker-compose logs -f image-service

# Stop all services
docker-compose down
```

---

## 🔌 Integration Points

### Image Service ← User Service
```
Image Service calls User Service to:
1. Check subscription plan limits
2. Report usage statistics
3. Verify user has active subscription
```

### Example Request from Image Service to User Service:
```http
POST /api/users/{userId}/usage
Authorization: Bearer {service_token}
Content-Type: application/json

{
  "generationCount": 1,
  "storageUsedGb": 0.5
}
```

---

## 🧪 Quick Testing

### Test Image Service
```bash
# Health check
curl http://localhost:5007/api/image/health

# Generate image (requires JWT token)
curl -X POST http://localhost:5007/api/image/generate \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "A beautiful sunset over mountains",
    "size": "1024x1024"
  }'
```

### Test User Service
```bash
# Health check
curl http://localhost:5008/api/users/health

# Get current user (requires JWT token)
curl http://localhost:5008/api/users/me \
  -H "Authorization: Bearer {token}"

# Get subscription
curl http://localhost:5008/api/users/{userId}/subscription \
  -H "Authorization: Bearer {token}"
```

---

## 📋 Configuration Checklist

- [ ] Copy `.env.example` to `.env`
- [ ] Set `JWT_SECRET` in `.env`
- [ ] Verify database connection strings
- [ ] Set CORS allowed origins
- [ ] Configure storage path for images
- [ ] Set up logging levels
- [ ] Test health check endpoints
- [ ] Verify JWT token validation

---

## 🚢 Production Deployment Steps

1. **Build Docker Images**
   ```bash
   docker build -t techbirdsfly/image-service:latest services/image-service/
   docker build -t techbirdsfly/user-service:latest services/user-service/
   ```

2. **Push to Registry** (if using Docker Hub)
   ```bash
   docker push techbirdsfly/image-service:latest
   docker push techbirdsfly/user-service:latest
   ```

3. **Deploy with docker-compose**
   ```bash
   docker-compose -f docker-compose.yml up -d
   ```

4. **Verify Services**
   ```bash
   curl https://yourdomain.com/api/image/health
   curl https://yourdomain.com/api/users/health
   ```

---

## 📊 Service Comparison

| Feature | Image Service | User Service |
|---------|---------------|--------------|
| Purpose | AI image generation | User management |
| Endpoints | 7 | 11 |
| Database | SQLite/PostgreSQL | SQLite/PostgreSQL |
| Authentication | JWT Bearer | JWT Bearer |
| Status | ✅ Ready | ✅ Ready |
| Docker | ✅ Multi-stage | ✅ Multi-stage |

---

## 🔐 Environment Variables

### Image Service
```env
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Data Source=image.db
JwtSettings__SecretKey=your-secret-key-here
JwtSettings__Issuer=TechBirdsFly
JwtSettings__Audience=TechBirdsFly-Users
Storage__Type=local
Storage__LocalPath=/app/storage
OpenAi__ApiKey=sk-your-key-here
```

### User Service
```env
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Data Source=user.db
JwtSettings__SecretKey=your-secret-key-here
JwtSettings__Issuer=TechBirdsFly
JwtSettings__Audience=TechBirdsFly-Users
Cors__AllowedOrigins__0=http://localhost:3000
Cors__AllowedOrigins__1=http://localhost:3001
```

---

## 📚 Documentation Links

- **Image Service README**: `services/image-service/README.md`
- **User Service README**: `services/user-service/README.md`
- **User Service Guide**: `services/user-service/IMPLEMENTATION_GUIDE.md`
- **Phase Completion**: `PHASE3_2_COMPLETION.md`
- **Architecture**: `docs/architecture.md`

---

## 🆘 Troubleshooting

### Services Won't Connect
1. Verify both services are running: `docker ps`
2. Check docker network: `docker network ls`
3. Verify service names in docker-compose.yml

### Database Lock Error
1. Stop all services
2. Delete `.db` files
3. Restart services (databases will be recreated)

### JWT Token Issues
1. Verify `JwtSettings__SecretKey` matches across all services
2. Check token hasn't expired
3. Verify Authorization header format: `Bearer {token}`

### Port Already in Use
```bash
# Check what's using port 5007
lsof -i :5007

# Kill process using port
kill -9 <PID>
```

---

## ✅ Verification Checklist

After deployment, verify:

- [ ] Image Service health check returns 200
- [ ] User Service health check returns 200
- [ ] Can get current user (with valid JWT)
- [ ] Can get user subscription (with valid JWT)
- [ ] Can generate image (with valid JWT)
- [ ] Usage tracking works
- [ ] Swagger docs accessible
- [ ] Logging output shows no errors

---

## 🎯 Next Steps

1. **Phase 3.3**: React Admin Dashboard Integration
2. **Phase 3.4**: End-to-end testing
3. **Phase 3.5**: Performance tuning and optimization
4. **Phase 3.6**: Security audit and hardening
5. **Phase 3.7**: Production deployment

---

**Status**: ✅ Ready for Deployment  
**Last Updated**: October 17, 2025  
**Version**: 1.0.0
