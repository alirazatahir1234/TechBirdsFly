# YARP Gateway - Quick Start Guide

## ⚡ 5-Minute Setup

### Prerequisites
- .NET 8 SDK installed
- Auth Service, User Service, Image Service running (or use mock mode)

### 1. Navigate to Gateway
```bash
cd gateway/yarp-gateway/src
```

### 2. Configure JWT Key (First Time Only)
Edit `appsettings.Development.json`:
```json
{
  "Jwt": {
    "Key": "dev-secret-key-minimum-32-characters-long-for-development"
  }
}
```

⚠️ **Important**: Use the same JWT key across all services (Auth, User, Image, Admin).

### 3. Start Gateway
```bash
dotnet run --urls http://localhost:5000
```

Expected output:
```
🚀 TechBirdsFly API Gateway starting on port 5000
✅ JWT Authentication: Enabled
✅ Rate Limiting: 100 requests/min per user, 50 requests/30s per IP
✅ CORS: Configured for frontend origins
✅ Health Checks: Monitoring 5 downstream services
```

### 4. Verify Gateway
```bash
# Check health
curl http://localhost:5000/health

# Check info
curl http://localhost:5000/info

# Open Swagger
open http://localhost:5000/swagger
```

---

## 🧪 Test the Gateway

### Test 1: Public Route (No Auth)
```bash
# Register a new user
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!",
    "fullName": "Test User"
  }'

# Login to get JWT token
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!"
  }'
```

### Test 2: Protected Route (JWT Required)
```bash
# Save the token from login response
TOKEN="your_jwt_token_here"

# Get user profile
curl http://localhost:5000/api/users/me \
  -H "Authorization: Bearer $TOKEN"
```

### Test 3: Rate Limiting
```bash
# Run 110 requests quickly (should get 429 after 100)
for i in {1..110}; do
  curl -s http://localhost:5000/info | grep -o "version"
  echo " - Request $i"
done
```

Expected: First 100 succeed, then get 429 Too Many Requests

### Test 4: CORS
```bash
curl -X OPTIONS http://localhost:5000/api/auth/login \
  -H "Origin: http://localhost:3000" \
  -H "Access-Control-Request-Method: POST" \
  -v
```

Expected: See `Access-Control-Allow-Origin: http://localhost:3000` in response

---

## 🐳 Docker Quick Start

### Build and Run
```bash
# Build
cd gateway/yarp-gateway
docker build -t yarp-gateway .

# Run
docker run -d \
  --name yarp-gateway \
  -p 5000:5000 \
  -e JWT__KEY="dev-secret-key-minimum-32-characters-long" \
  yarp-gateway:latest

# Check logs
docker logs -f yarp-gateway

# Test health
curl http://localhost:5000/health
```

---

## 📊 Monitoring

### Real-time Logs
```bash
# Terminal 1: Start gateway
dotnet run --urls http://localhost:5000

# Terminal 2: Monitor logs
tail -f /path/to/logs

# Watch for:
# - [INFO] Incoming Request
# - [INFO] JWT Token validated
# - [INFO] Completed Request
# - [WARN] Rate limit exceeded
```

### Health Check
```bash
# Check all services
curl http://localhost:5000/health | jq

# Expected response:
{
  "status": "Healthy",
  "services": [
    {"name": "auth-service", "status": "Healthy"},
    {"name": "user-service", "status": "Healthy"},
    ...
  ]
}
```

---

## 🔧 Configuration

### Change Port
```bash
# Default: 5000
dotnet run --urls http://localhost:8080
```

### Enable Production Mode
```bash
export ASPNETCORE_ENVIRONMENT=Production
dotnet run
```

### Custom CORS Origins
Edit `appsettings.json`:
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "https://your-domain.com"
    ]
  }
}
```

### Adjust Rate Limits
Edit `Program.cs`:
```csharp
options.AddPolicy("PerUserRateLimit", context =>
{
    return RateLimitPartition.GetFixedWindowLimiter(
        username,
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 200,  // Change from 100 to 200
            Window = TimeSpan.FromMinutes(1)
        });
});
```

---

## ⚠️ Troubleshooting

### Problem: "401 Unauthorized" on protected routes
**Solution**: Check JWT token is valid and matches the key in config
```bash
# Verify token hasn't expired
echo $TOKEN | cut -d'.' -f2 | base64 -d | jq
```

### Problem: "429 Too Many Requests"
**Solution**: Wait for rate limit window to reset (60 seconds for user limit)
```bash
# Check Retry-After header
curl -I http://localhost:5000/info
```

### Problem: CORS errors in browser
**Solution**: Verify origin is in allowed list
```json
{
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000"]
  }
}
```

### Problem: Gateway can't connect to services
**Solution**: Ensure all services are running
```bash
# Check Auth Service
curl http://localhost:5001/health

# Check User Service
curl http://localhost:5008/health

# Check Image Service
curl http://localhost:5007/health
```

---

## 📚 Next Steps

1. **Integration**: Connect React frontend to gateway (port 3000 → 5000)
2. **Security**: Configure HTTPS with valid certificates
3. **Monitoring**: Set up log aggregation and dashboards
4. **Scaling**: Deploy multiple gateway instances with load balancer
5. **Testing**: Run load tests to verify performance

---

## 🎯 Common Use Cases

### Use Case 1: Frontend Authentication Flow
```javascript
// 1. Login
const response = await fetch('http://localhost:5000/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ email: 'user@example.com', password: 'pass123' })
});
const { token } = await response.json();

// 2. Use token for protected requests
const profile = await fetch('http://localhost:5000/api/users/me', {
  headers: { 'Authorization': `Bearer ${token}` }
});
```

### Use Case 2: Mobile App Integration
```swift
// Configure base URL
let baseURL = "http://localhost:5000"

// Add JWT to all requests
let headers = ["Authorization": "Bearer \(token)"]

// Make request through gateway
URLSession.shared.dataTask(with: request)
```

### Use Case 3: Third-party API Integration
```python
import requests

# Gateway base URL
BASE_URL = "http://localhost:5000"

# Authenticate
response = requests.post(f"{BASE_URL}/api/auth/login", json={
    "email": "api@example.com",
    "password": "apikey123"
})
token = response.json()["token"]

# Use gateway for all API calls
headers = {"Authorization": f"Bearer {token}"}
users = requests.get(f"{BASE_URL}/api/users", headers=headers)
```

---

## 📖 Documentation

- **Full Documentation**: `gateway/yarp-gateway/README.md`
- **Implementation Details**: `gateway/GATEWAY_IMPLEMENTATION_COMPLETE.md`
- **Test Examples**: `gateway/yarp-gateway/src/YarpGateway.http`
- **YARP Official Docs**: https://microsoft.github.io/reverse-proxy/

---

## ✅ Checklist

- [ ] Gateway running on port 5000
- [ ] JWT authentication working
- [ ] Rate limiting active
- [ ] CORS configured for frontend
- [ ] Health checks passing
- [ ] Downstream services reachable
- [ ] Swagger accessible
- [ ] Logs showing requests

**Status**: 🟢 Ready for development
