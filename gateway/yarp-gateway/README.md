# YARP Gateway - TechBirdsFly

Production-ready API Gateway built with Microsoft YARP (Yet Another Reverse Proxy) featuring JWT authentication, rate limiting, CORS handling, and comprehensive service health monitoring.

## 🚀 Features

### Core Features
- **YARP Reverse Proxy**: High-performance HTTP reverse proxy
- **JWT Authentication**: Bearer token validation at gateway level
- **Rate Limiting**: Multi-tier throttling (per-user, per-IP, global)
- **CORS Protection**: Configurable cross-origin resource sharing
- **Service Health Checks**: Active health monitoring of all downstream services
- **Request Logging**: Comprehensive HTTP request/response logging with Serilog
- **Load Balancing**: Automatic distribution across multiple service instances
- **Swagger/OpenAPI**: Interactive API documentation

### Security
- ✅ JWT Bearer token validation before routing
- ✅ Rate limiting: 100 requests/min per user, 50 requests/30s per IP
- ✅ CORS policies for frontend origins
- ✅ Automatic token expiration handling
- ✅ Secure headers and error responses

### Performance
- ✅ Sliding window rate limiting for DDoS protection
- ✅ Health check-based routing (removes unhealthy instances)
- ✅ Connection pooling and HTTP/2 support
- ✅ Minimal overhead (<1ms typical latency)

## 📋 Prerequisites

- .NET 8 SDK
- Access to all downstream services (ports 5001, 5003, 5006, 5007, 5008)
- JWT signing key configured

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    YARP API Gateway (5000)                   │
│  ┌─────────────┐  ┌──────────────┐  ┌──────────────┐       │
│  │     JWT     │  │     Rate     │  │     CORS     │       │
│  │ Validation  │→ │   Limiting   │→ │   Handling   │       │
│  └─────────────┘  └──────────────┘  └──────────────┘       │
└─────────────────────────────────────────────────────────────┘
                            │
        ┌───────────────────┼───────────────────┐
        ↓                   ↓                   ↓
  ┌─────────┐         ┌─────────┐         ┌─────────┐
  │  Auth   │         │  User   │         │  Image  │
  │ Service │         │ Service │         │ Service │
  │  :5001  │         │  :5008  │         │  :5007  │
  └─────────┘         └─────────┘         └─────────┘
        ↓                   ↓                   ↓
  ┌─────────┐         ┌─────────┐         ┌─────────┐
  │Generator│         │ Billing │         │  Admin  │
  │ Service │         │ Service │         │ Service │
  │  :5003  │         │  :5005  │         │  :5006  │
  └─────────┘         └─────────┘         └─────────┘
```

## 🚦 Route Configuration

### Public Routes (No Authentication)
| Route | Target Service | Port | Description |
|-------|---------------|------|-------------|
| `/api/auth/**` | Auth Service | 5001 | Registration, login, token refresh |
| `/health` | Gateway | 5000 | Gateway health status |
| `/info` | Gateway | 5000 | Gateway information and routes |
| `/swagger` | Gateway | 5000 | API documentation |

### Protected Routes (JWT Required)
| Route | Target Service | Port | Description |
|-------|---------------|------|-------------|
| `/api/users/**` | User Service | 5008 | User profiles and subscriptions |
| `/api/projects/**` | Generator Service | 5003 | Project generation and management |
| `/api/images/**` | Image Service | 5007 | AI image generation |
| `/api/admin/**` | Admin Service | 5006 | Admin operations |

## ⚙️ Configuration

### JWT Settings (`appsettings.json`)
```json
{
  "Jwt": {
    "Key": "your-super-secret-key-min-32-chars-change-in-production!",
    "Issuer": "TechBirdsFly",
    "Audience": "TechBirdsFly"
  }
}
```

### CORS Settings
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "https://your-production-domain.com"
    ]
  }
}
```

### Rate Limiting
- **Per User**: 100 requests/minute (authenticated users)
- **Per IP**: 50 requests/30 seconds (sliding window)
- **Anonymous**: 10 requests/minute (unauthenticated)
- **Global**: 100 requests/minute per user or IP

### Health Checks
- **Interval**: 30 seconds
- **Timeout**: 5 seconds
- **Policy**: Consecutive failures
- **Endpoint**: `/health` on each service

## 🛠️ Development Setup

### 1. Install Dependencies
```bash
cd gateway/yarp-gateway/src
dotnet restore
```

### 2. Configure Settings
Create or edit `appsettings.Development.json`:
```json
{
  "Jwt": {
    "Key": "dev-secret-key-minimum-32-characters-long-for-development",
    "Issuer": "TechBirdsFly",
    "Audience": "TechBirdsFly"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000"]
  }
}
```

### 3. Run Gateway
```bash
dotnet run --urls http://localhost:5000
```

### 4. Verify Gateway
```bash
# Check health
curl http://localhost:5000/health

# Check info
curl http://localhost:5000/info

# Access Swagger
open http://localhost:5000/swagger
```

## 🐳 Docker Deployment

### Build Image
```bash
cd gateway/yarp-gateway
docker build -t techbirdsfly-gateway:latest .
```

### Run Container
```bash
docker run -d \
  --name yarp-gateway \
  -p 5000:5000 \
  -e JWT__KEY="your-production-jwt-key-here" \
  -e JWT__ISSUER="TechBirdsFly" \
  -e JWT__AUDIENCE="TechBirdsFly" \
  -e CORS__ALLOWEDORIGINS__0="https://yourdomain.com" \
  techbirdsfly-gateway:latest
```

### Docker Compose
```yaml
services:
  gateway:
    build: ./gateway/yarp-gateway
    ports:
      - "5000:5000"
    environment:
      - JWT__KEY=${JWT_SECRET_KEY}
      - CORS__ALLOWEDORIGINS__0=http://localhost:3000
    depends_on:
      - auth-service
      - user-service
      - generator-service
      - image-service
      - admin-service
```

## 📊 Testing

### Test JWT Authentication
```bash
# 1. Login to get JWT token
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!"}'

# 2. Use token for protected routes
TOKEN="your_jwt_token_here"

curl http://localhost:5000/api/users/me \
  -H "Authorization: Bearer $TOKEN"
```

### Test Rate Limiting
```bash
# Run multiple requests quickly (should get 429 after 100 requests)
for i in {1..110}; do
  curl http://localhost:5000/info
  echo "Request $i"
done
```

### Test CORS
```bash
# Preflight OPTIONS request
curl -X OPTIONS http://localhost:5000/api/auth/login \
  -H "Origin: http://localhost:3000" \
  -H "Access-Control-Request-Method: POST" \
  -H "Access-Control-Request-Headers: content-type" \
  -v
```

### Test Health Checks
```bash
# Gateway health (includes all downstream services)
curl http://localhost:5000/health | jq

# Expected response:
# {
#   "status": "Healthy",
#   "timestamp": "2024-01-20T10:30:00Z",
#   "services": [
#     { "name": "auth-service", "status": "Healthy" },
#     { "name": "user-service", "status": "Healthy" },
#     ...
#   ]
# }
```

## 🔒 Security Best Practices

### Production Checklist
- [ ] Change JWT secret key (minimum 32 characters)
- [ ] Configure production CORS origins (no wildcards)
- [ ] Enable HTTPS/TLS encryption
- [ ] Set up firewall rules
- [ ] Configure rate limits based on your SLA
- [ ] Enable request/response logging
- [ ] Set up monitoring and alerts
- [ ] Implement API versioning
- [ ] Add request size limits
- [ ] Configure timeout policies

### Environment Variables
```bash
# Required
export JWT__KEY="production-secret-key-min-32-chars"
export JWT__ISSUER="TechBirdsFly"
export JWT__AUDIENCE="TechBirdsFly"

# Optional
export CORS__ALLOWEDORIGINS__0="https://app.techbirdsfly.com"
export ASPNETCORE_ENVIRONMENT="Production"
```

## 📈 Monitoring

### Logs
The gateway uses Serilog for structured logging:
```json
{
  "timestamp": "2024-01-20T10:30:00Z",
  "level": "Information",
  "message": "Incoming Request: GET /api/users/me from 192.168.1.100",
  "properties": {
    "Method": "GET",
    "Path": "/api/users/me",
    "IP": "192.168.1.100"
  }
}
```

### Metrics to Monitor
- Request rate (requests/second)
- Response time (p50, p95, p99)
- Error rate (4xx, 5xx)
- Rate limit rejections
- Service health status
- JWT validation failures

## 🐛 Troubleshooting

### Issue: "401 Unauthorized" on protected routes
**Solution**: Verify JWT token is valid and not expired
```bash
# Decode JWT to check expiration
echo $TOKEN | cut -d'.' -f2 | base64 -d | jq
```

### Issue: "429 Too Many Requests"
**Solution**: Rate limit exceeded, wait for the retry-after period
```bash
# Check Retry-After header
curl -I http://localhost:5000/info
```

### Issue: CORS errors in browser
**Solution**: Verify allowed origins in configuration
```json
{
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000"]
  }
}
```

### Issue: Service health check failing
**Solution**: Ensure downstream services are running and accessible
```bash
# Test individual service health
curl http://localhost:5001/health  # Auth Service
curl http://localhost:5008/health  # User Service
curl http://localhost:5007/health  # Image Service
```

## 📚 API Documentation

Access interactive Swagger documentation at:
```
http://localhost:5000/swagger
```

## 🤝 Related Services

- [Auth Service](/services/auth-service/README.md) - Authentication and JWT token generation
- [User Service](/services/user-service/README.md) - User profiles and subscriptions
- [Image Service](/services/image-service/README.md) - AI image generation
- [Generator Service](/services/generator-service/README.md) - Project generation
- [Admin Service](/services/admin-service/README.md) - Admin operations

## 📄 License

Part of TechBirdsFly microservices architecture.

## 🆘 Support

For issues or questions:
1. Check this README and troubleshooting section
2. Review YARP documentation: https://microsoft.github.io/reverse-proxy/
3. Check service-specific health endpoints
4. Review gateway logs for detailed error information
