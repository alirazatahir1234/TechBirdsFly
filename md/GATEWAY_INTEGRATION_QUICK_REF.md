# ⚡ Gateway SignUp Integration - Quick Reference

## 🎯 The Problem
```
❌ ERR_CONNECTION_REFUSED on http://localhost:5500/api/auth/register
```
**Why?** Gateway not running OR hardcoded URLs not using correct port.

---

## ✅ The Solution (3 Quick Steps)

### Step 1️⃣ - Make the Script Executable
```bash
chmod +x start-all-services-gateway.sh
```

### Step 2️⃣ - Run the Script
```bash
./start-all-services-gateway.sh
```

### Step 3️⃣ - Test in Browser
```
Navigate to: http://localhost:3000/signup
Fill form and click submit
```

---

## 📊 What Gets Started

| Service | Port | Status | URL |
|---------|------|--------|-----|
| Frontend | 3000 | ✅ | http://localhost:3000 |
| Gateway | 5500 | ✅ | http://localhost:5500 |
| Auth API | 5001 | ✅ | http://localhost:5001 |

---

## 🔄 Request Flow

```
User Browser (localhost:3000)
        ↓ clicks "Sign Up"
Frontend form submission
        ↓ calls /api/auth/register
API Gateway (localhost:5500)
        ↓ routes /api/auth/** to Auth Service
Auth Service (localhost:5001)
        ↓ validates & processes
Returns JWT token to Frontend
        ↓
Stores in localStorage
```

---

## 🧪 Manual Testing (Without Script)

**Terminal 1 - Auth Service:**
```bash
cd services/auth-service/src && dotnet run
# Output: Now listening on: http://localhost:5001
```

**Terminal 2 - Gateway:**
```bash
cd gateway/yarp-gateway/src && dotnet run
# Output: 🚀 TechBirdsFly API Gateway starting on port 5500
```

**Terminal 3 - Frontend:**
```bash
cd web-frontend/techbirdsfly-frontend-nextjs && npm run dev
# Output: ▲ Next.js 14.0.0 - Local: http://localhost:3000
```

---

## ✅ Verification Checklist

```bash
# 1. Check Auth Service
curl http://localhost:5001/health
# Should return: {"status":"Healthy"...}

# 2. Check Gateway
curl http://localhost:5500/health
# Should return: {"status":"Healthy"...}

# 3. Test SignUp Endpoint
curl -X POST http://localhost:5500/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Pass123!","fullName":"Test User"}'
# Should return: JWT token in response
```

---

## 🔧 What Was Fixed

### ❌ Before
```typescript
// authStore.ts - HARDCODED PORT
const response = await fetch('http://localhost:5500/api/auth/register', {
```

### ✅ After
```typescript
// authStore.ts - USES ENVIRONMENT VARIABLE
const API_BASE = process.env.NEXT_PUBLIC_API_BASE || 'http://localhost:5500/api';
const response = await fetch(`${API_BASE}/auth/register`, {
```

### ✅ Environment Config
```bash
# .env.local - UPDATED
NEXT_PUBLIC_API_BASE=http://localhost:5500/api
NEXT_PUBLIC_GATEWAY_URL=http://localhost:5500
```

---

## 🚨 Common Issues & Fixes

| Issue | Fix |
|-------|-----|
| `ERR_CONNECTION_REFUSED` | Run: `./start-all-services-gateway.sh` |
| Auth Service won't start | Check port 5001 not in use: `lsof -i:5001` |
| Gateway won't start | Check port 5500 not in use: `lsof -i:5500` |
| Frontend showing 404 | Ensure gateway is running AND routed correctly |
| CORS errors | Gateway handles CORS, should be transparent |
| Token not stored | Check localStorage in browser DevTools |

---

## 🎯 Architecture

```
┌─────────────────────────────────────┐
│  Frontend (React + Next.js)         │
│  Port: 3000                         │
│  .env: API_BASE = :5500/api         │
└────────────┬────────────────────────┘
             │ HTTP Requests
             │ GET/POST/PUT/DELETE
             │ Content-Type: JSON
             ▼
┌─────────────────────────────────────┐
│  API Gateway (YARP)                 │
│  Port: 5500                         │
│  ✅ JWT Validation                  │
│  ✅ Rate Limiting                   │
│  ✅ CORS Handling                   │
│  ✅ Request Logging                 │
│  ✅ Health Checks                   │
└────────────┬────────────────────────┘
             │ Proxies to: /api/auth/**
             │            /api/users/**
             │            /api/projects/**
             ▼
┌─────────────────────────────────────┐
│  Auth Service                       │
│  Port: 5001                         │
│  Endpoints:                         │
│  • POST /api/auth/register          │
│  • POST /api/auth/login             │
│  • POST /api/auth/forgot-password   │
│  • POST /api/auth/reset-password    │
└─────────────────────────────────────┘
```

---

## 📝 Files Modified

1. **authStore.ts**
   - Changed: Hardcoded URLs → Environment variable
   - Used: `process.env.NEXT_PUBLIC_API_BASE`
   - All 4 auth methods updated

2. **.env.local**
   - Added: `NEXT_PUBLIC_API_BASE=http://localhost:5500/api`
   - Added: `NEXT_PUBLIC_GATEWAY_URL=http://localhost:5500`

3. **start-all-services-gateway.sh** (NEW)
   - Starts all 3 services in separate terminals
   - Verifies ports are free
   - Tests health endpoints
   - Shows URLs and next steps

---

## 🔐 Security Features

**Gateway provides:**
- ✅ JWT token validation
- ✅ Rate limiting (100 req/min per user)
- ✅ CORS protection
- ✅ Request logging
- ✅ Health monitoring
- ✅ Service isolation

---

## 🚀 Next Steps

1. ✅ Fix connection error (done above)
2. ✅ Start all services
3. ✅ Test signup flow
4. ✅ Verify JWT token received
5. ⬜ Store token in localStorage (Zustand handles this)
6. ⬜ Use token in subsequent API calls

---

## 📞 Debugging Tips

### Check Service Ports
```bash
# See what's using ports 3000, 5001, 5500
lsof -i:3000,5001,5500
```

### Check Service Logs
```bash
# Follow auth service logs in real-time
tail -f ~/.local/share/TechBirdsFly/logs/auth-service.log
```

### Test API Directly
```bash
# Test gateway proxy routing
curl -v http://localhost:5500/api/auth/health
```

### Check Frontend API Config
```javascript
// In browser console
console.log(process.env.NEXT_PUBLIC_API_BASE)
```

---

## ✨ Success Indicators

✅ All 3 terminals show services running
✅ No errors in any console
✅ Gateway shows health status: ONLINE
✅ Browser opens signup page
✅ Signup request hits Gateway
✅ Response includes JWT token
✅ Token saved in localStorage

---

## 📚 Documentation

- Full details: `GATEWAY_SIGNUP_INTEGRATION_FIX.md`
- Architecture: See diagram above
- API Endpoints: `services/auth-service/README.md`
- Gateway Config: `gateway/yarp-gateway/src/appsettings.json`

---

**🎉 You're all set! Follow the 3 steps above to get running.**
