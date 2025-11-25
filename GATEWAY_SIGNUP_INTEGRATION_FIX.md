# 🔧 Gateway SignUp Integration - Connection Error Fix

## ❌ Problem Analysis

**Error**: `Failed to load resource: net::ERR_CONNECTION_REFUSED` on `http://localhost:5500/api/auth/register`

**Root Cause**: 
- Frontend is calling **port 5500** (Gateway)
- But **Gateway is NOT running**
- Therefore, connection is refused

---

## 🎯 Quick Fix (Choose One)

### ✅ Option 1: Start All Services (RECOMMENDED)

This approach uses the gateway as intended.

**Terminal 1 - Auth Service**:
```bash
cd /Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly/services/auth-service/src
dotnet run
```
Output should show:
```
Now listening on: http://localhost:5001
Now listening on: https://localhost:7001
```

**Terminal 2 - Gateway**:
```bash
cd /Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly/gateway/yarp-gateway/src
dotnet run
```
Output should show:
```
🚀 TechBirdsFly API Gateway starting on port 5500
```

**Terminal 3 - Frontend**:
```bash
cd /Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly/web-frontend/techbirdsfly-frontend-nextjs
npm run dev
```
Output should show:
```
▲ Next.js 14.0.0
- Local: http://localhost:3000
```

**Result**: 
- ✅ Frontend → Port 3000
- ✅ Gateway → Port 5500 (reverse proxy)
- ✅ Auth Service → Port 5001 (backend)
- ✅ All connected through gateway

---

### ✅ Option 2: Quick Dev Mode (Direct API)

If you just want to test quickly without gateway.

**Terminal 1 - Auth Service**:
```bash
cd services/auth-service/src
dotnet run
```

**Terminal 2 - Frontend** (with correct API):
```bash
cd web-frontend/techbirdsfly-frontend-nextjs
NEXT_PUBLIC_API_BASE=http://localhost:5001/api npm run dev
```

**Result**:
- ✅ Frontend calls Auth Service directly
- ✅ No gateway needed
- ⚠️ CORS might need adjustment

---

## 📊 Architecture Explanation

### Current Setup (With Gateway - RECOMMENDED)

```
Frontend (localhost:3000)
         ↓ (calls /api/auth/register)
Gateway (localhost:5500)
         ↓ (routes to /api/auth/**)
Auth Service (localhost:5001)
         ↓
Database
```

**File**: `gateway/yarp-gateway/src/appsettings.json`
```json
"ReverseProxy": {
  "Routes": {
    "auth-route": {
      "ClusterId": "auth-cluster",
      "Match": {
        "Path": "/api/auth/{**catch-all}"
      }
    }
  },
  "Clusters": {
    "auth-cluster": {
      "Destinations": {
        "destination1": {
          "Address": "http://localhost:5001"  ← Auth Service
        }
      }
    }
  }
}
```

### Port Summary

| Service | Port | Environment | Purpose |
|---------|------|-------------|---------|
| Frontend | 3000 | `NEXTAUTH_URL=http://localhost:3000` | React/Next.js app |
| Gateway | 5500 | `Kestrel:Endpoints:Http:Url=http://localhost:5500` | API reverse proxy |
| Auth Service | 5001 | Default (Program.cs) | Authentication endpoints |
| User Service | 5005 | (configured in gateway) | User management |
| Admin Service | 5006 | (configured in gateway) | Admin endpoints |
| Image Service | 5007 | (configured in gateway) | Image processing |
| Billing Service | 5177 | (configured in gateway) | Billing endpoints |

---

## ✅ Verification Checklist

### Step 1: Check Auth Service is Running

```bash
# Terminal
curl http://localhost:5001/health
```

**Expected Response**:
```json
{
  "status": "Healthy",
  "timestamp": "2025-11-18T10:30:00Z",
  "services": [
    {
      "name": "Database",
      "status": "Healthy"
    }
  ]
}
```

### Step 2: Check Gateway is Running

```bash
curl http://localhost:5500/health
```

**Expected Response**:
```json
{
  "status": "Healthy",
  "timestamp": "2025-11-18T10:30:00Z",
  "services": [
    {
      "name": "auth-service",
      "status": "Healthy"
    },
    {
      "name": "user-service",
      "status": "Healthy"
    }
  ]
}
```

### Step 3: Check Gateway can Route to Auth Service

```bash
curl http://localhost:5500/api/auth/health
```

**Should match Auth Service response** (proving gateway is proxying correctly)

### Step 4: Test SignUp Endpoint

```bash
curl -X POST http://localhost:5500/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Password123!",
    "fullName": "Test User"
  }'
```

**Expected Response**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "email": "test@example.com",
  "fullName": "Test User",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

---

## 🚀 Start All Services Script

Create `start-services.sh`:

```bash
#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

BASE_DIR="/Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly"

echo "${GREEN}🚀 Starting TechBirdsFly Services${NC}"
echo ""

# Kill any existing processes on ports 3000, 5001, 5500
echo "${YELLOW}🛑 Cleaning up existing processes...${NC}"
lsof -ti:3000,5001,5500 | xargs kill -9 2>/dev/null || true
sleep 2

# Terminal 1: Auth Service
echo "${GREEN}📌 Starting Auth Service on port 5001...${NC}"
osascript <<EOF
tell application "Terminal"
  do script "cd '$BASE_DIR/services/auth-service/src' && dotnet run"
end tell
EOF
sleep 3

# Terminal 2: Gateway
echo "${GREEN}📌 Starting Gateway on port 5500...${NC}"
osascript <<EOF
tell application "Terminal"
  do script "cd '$BASE_DIR/gateway/yarp-gateway/src' && dotnet run"
end tell
EOF
sleep 3

# Terminal 3: Frontend
echo "${GREEN}📌 Starting Frontend on port 3000...${NC}"
osascript <<EOF
tell application "Terminal"
  do script "cd '$BASE_DIR/web-frontend/techbirdsfly-frontend-nextjs' && npm run dev"
end tell
EOF

echo ""
echo "${GREEN}✅ All services starting!${NC}"
echo ""
echo "Waiting for services to fully start..."
sleep 10

# Verify services
echo "${YELLOW}🔍 Verifying services...${NC}"
echo ""

# Check Auth Service
if curl -s http://localhost:5001/health > /dev/null; then
  echo "${GREEN}✅ Auth Service (port 5001): ONLINE${NC}"
else
  echo "${RED}❌ Auth Service (port 5001): OFFLINE${NC}"
fi

# Check Gateway
if curl -s http://localhost:5500/health > /dev/null; then
  echo "${GREEN}✅ Gateway (port 5500): ONLINE${NC}"
else
  echo "${RED}❌ Gateway (port 5500): OFFLINE${NC}"
fi

# Check Frontend
if curl -s http://localhost:3000 > /dev/null; then
  echo "${GREEN}✅ Frontend (port 3000): ONLINE${NC}"
else
  echo "${RED}❌ Frontend (port 3000): OFFLINE${NC}"
fi

echo ""
echo "${GREEN}🎉 Ready to test!${NC}"
echo ""
echo "Frontend:  http://localhost:3000"
echo "Gateway:   http://localhost:5500"
echo "Auth:      http://localhost:5001"
```

**Usage**:
```bash
chmod +x start-services.sh
./start-services.sh
```

---

## 📝 Environment Configuration

### Current `.env.local`

```bash
NEXTAUTH_URL=http://localhost:3000
NEXTAUTH_SECRET=supersecretkeyfordevelopmentonly123456789012345
NEXT_PUBLIC_API_BASE=http://localhost:5000/api
GOOGLE_CLIENT_ID=
GOOGLE_CLIENT_SECRET=
FACEBOOK_CLIENT_ID=
FACEBOOK_CLIENT_SECRET=
```

### Issue
`NEXT_PUBLIC_API_BASE=http://localhost:5000/api` is NOT being used!

### Solution - Update to

```bash
NEXTAUTH_URL=http://localhost:3000
NEXTAUTH_SECRET=supersecretkeyfordevelopmentonly123456789012345
NEXT_PUBLIC_API_BASE=http://localhost:5500/api
NEXT_PUBLIC_GATEWAY_URL=http://localhost:5500
GOOGLE_CLIENT_ID=
GOOGLE_CLIENT_SECRET=
FACEBOOK_CLIENT_ID=
FACEBOOK_CLIENT_SECRET=
```

---

## 🔧 Update Auth Store to Use Environment Variable

**File**: `web-frontend/techbirdsfly-frontend-nextjs/lib/store/authStore.ts`

**Change from**:
```typescript
const response = await fetch('http://localhost:5500/api/auth/register', {
```

**Change to**:
```typescript
const apiBase = process.env.NEXT_PUBLIC_API_BASE || 'http://localhost:5500/api';
const response = await fetch(`${apiBase}/auth/register`, {
```

---

## 🎯 Complete SignUp Flow

### Step 1: User enters email & password on frontend
```
Frontend: http://localhost:3000/signup
```

### Step 2: Frontend calls Gateway
```
POST http://localhost:5500/api/auth/register
{
  "email": "user@example.com",
  "password": "Password123!",
  "fullName": "John Doe"
}
```

### Step 3: Gateway routes to Auth Service
```
Gateway (5500) → Auth Service (5001)
```

### Step 4: Auth Service processes
```
1. Validates email/password
2. Hashes password
3. Creates User entity
4. Creates UserProfile entity
5. Generates JWT token
6. Returns token to frontend
```

### Step 5: Frontend stores token
```
Zustand store → localStorage (with NextAuth)
```

### Step 6: Future requests include token
```
Authorization: Bearer <token>
```

---

## 🛠️ Troubleshooting

### Q: `ERR_CONNECTION_REFUSED on 5500`
**A**: Gateway not running. Run: `cd gateway/yarp-gateway/src && dotnet run`

### Q: `ERR_CONNECTION_REFUSED on 5001`
**A**: Auth Service not running. Run: `cd services/auth-service/src && dotnet run`

### Q: CORS errors in console
**A**: Check `gateway/yarp-gateway/src/appsettings.json` has `http://localhost:3000` in `Cors:AllowedOrigins`

### Q: Gateway says "auth-service OFFLINE"
**A**: Auth Service not running on 5001. Check it started correctly.

### Q: Frontend still calling 5500 after env update
**A**: 
1. Kill frontend process
2. Clear `.next` folder: `rm -rf .next`
3. Restart: `npm run dev`

### Q: 404 on `/api/auth/register`
**A**: Check Auth Service is running AND gateway health shows auth-service is ONLINE

---

## 📚 Quick Reference

### All Terminal Commands

**Terminal 1** (Auth Service):
```bash
cd services/auth-service/src && dotnet run
```

**Terminal 2** (Gateway):
```bash
cd gateway/yarp-gateway/src && dotnet run
```

**Terminal 3** (Frontend):
```bash
cd web-frontend/techbirdsfly-frontend-nextjs && npm run dev
```

### Quick Tests

```bash
# Check Auth Service
curl http://localhost:5001/health

# Check Gateway
curl http://localhost:5500/health

# Test SignUp
curl -X POST http://localhost:5500/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"Pass123!","fullName":"Test"}'

# Check Frontend
curl http://localhost:3000
```

---

## ✅ Success Indicators

When everything is working:

✅ All terminals show services running
✅ `/health` endpoints return healthy status
✅ Gateway shows auth-service as ONLINE
✅ Frontend signup form appears
✅ Clicking signup sends request to gateway
✅ Response includes JWT token
✅ Token stored in localStorage

---

## 🎓 Architecture Lessons

1. **Gateway Pattern**: Single entry point (5500) proxies to multiple services
2. **Port Routing**: Each service on different port for isolation
3. **Health Checks**: Gateway monitors downstream service health
4. **CORS**: Gateway handles cross-origin from frontend (3000)
5. **JWT**: Token-based auth across all services
6. **Environment Variables**: Config changes without code modifications

---

## 📞 Summary

| Issue | Fix | Time |
|-------|-----|------|
| `ERR_CONNECTION_REFUSED` | Start Gateway on 5500 | 1 min |
| Wrong API port | Update `.env.local` | 1 min |
| Auth store hardcoded URLs | Use `process.env.NEXT_PUBLIC_API_BASE` | 2 min |
| Services not communicating | Run all 3 services, verify health | 5 min |
| CORS errors | Check gateway CORS config | 2 min |

**Total Fix Time**: ~5 minutes ✅
