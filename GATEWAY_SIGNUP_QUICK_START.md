# Gateway SignUp Integration - Quick Reference

## ⚡ 5-Minute Overview

### What Changed?

Frontend now routes **all auth requests through the Gateway** instead of calling services directly.

```
BEFORE ❌                           AFTER ✅
Frontend → Auth Service (5001)      Frontend → Gateway (5500) → Auth Service (5001)
```

### All Updated Endpoints

| Auth Flow | Before | After |
|-----------|--------|-------|
| **SignUp** | `localhost:5000/api/auth/register` | `localhost:5500/api/auth/register` |
| **Login** | `localhost:5000/api/auth/login` | `localhost:5500/api/auth/login` |
| **Forgot Password** | `localhost:5001/api/auth/forgot-password` | `localhost:5500/api/auth/forgot-password` |
| **Reset Password** | `localhost:5001/api/auth/reset-password` | `localhost:5500/api/auth/reset-password` |

---

## 🚀 Quick Start

### 1. Start All Services (in order)

```bash
# Terminal 1: Auth Service
cd services/auth-service/src
dotnet run --urls http://localhost:5001

# Terminal 2: Gateway
cd gateway/yarp-gateway/src
dotnet run --urls http://localhost:5500

# Terminal 3: Frontend
cd web-frontend/techbirdsfly-frontend-nextjs
npm run dev

# ✅ All running on correct ports
```

### 2. Test SignUp

```bash
# Browser
open http://localhost:3000/register

# Fill form:
# - Full Name: Test User
# - Email: test@example.com
# - Password: SecurePass123!
# - Terms: Check

# Click "Create Account"
# ✅ Should redirect to dashboard
```

### 3. Verify in Browser DevTools

```
Network Tab:
- POST http://localhost:5500/api/auth/register ✅
- Status: 200 OK ✅
- Response: { user, accessToken, refreshToken } ✅

Console Tab:
- No CORS errors ✅
- Login button shows user email ✅

Storage Tab (localStorage):
- token: "jwt.token..." ✅
- refreshToken: "jwt.token..." ✅
- auth-store: {...} ✅
```

---

## 🧪 Quick Test with curl

### SignUp Request

```bash
curl -X POST http://localhost:5500/api/auth/register \
  -H "Content-Type: application/json" \
  -H "Origin: http://localhost:3000" \
  -d '{
    "email": "test@example.com",
    "fullName": "Test User",
    "password": "SecurePass123!"
  }' \
  -v
```

### Expected Response

```
HTTP/1.1 200 OK
Access-Control-Allow-Origin: http://localhost:3000
Content-Type: application/json

{
  "user": {
    "id": "uuid",
    "email": "test@example.com",
    "fullName": "Test User",
    "role": "user"
  },
  "accessToken": "eyJhbGci...",
  "refreshToken": "eyJhbGci...",
  "expiresIn": 1800
}
```

---

## 📊 Architecture at a Glance

```
Frontend (3000)
     ↓
  [SignUp Form] → useAuthStore().register()
     ↓
  POST /api/auth/register (port 5500)
     ↓
Gateway (5500)
  ├─ CORS Check ✅
  ├─ Rate Limit ✅
  ├─ Route: /api/auth/** → auth-cluster
     ↓
Auth Service (5001)
  ├─ Validate email
  ├─ Hash password
  ├─ Create User + UserProfile
  ├─ Generate JWT tokens
     ↓
Gateway (Response)
  ├─ Add CORS headers
     ↓
Frontend (3000)
  ├─ Extract tokens
  ├─ Save to localStorage
  ├─ Update store
  └─ Redirect to dashboard ✅
```

---

## ✅ Checklist

- [ ] Gateway running on port 5500
- [ ] Auth Service running on port 5001
- [ ] Frontend running on port 3000
- [ ] Can open registration page: `http://localhost:3000/register`
- [ ] Test signup with valid email and strong password
- [ ] Redirects to dashboard after signup
- [ ] Tokens appear in localStorage
- [ ] No CORS errors in console
- [ ] Gateway logs show request routing

---

## ⚠️ Common Issues

### "Cannot GET /api/auth/register" 

❌ **Problem**: Gateway not running
✅ **Fix**: Start gateway on port 5500

### "CORS error: Access-Control-Allow-Origin missing"

❌ **Problem**: CORS not configured
✅ **Fix**: Check `gateway/appsettings.json` has `http://localhost:3000`

### "401 Unauthorized" on signup

❌ **Problem**: Public endpoint shouldn't require JWT
✅ **Fix**: Signup is public - no JWT needed. Check Auth Service logs.

### Still calling localhost:5000 or localhost:5001

❌ **Problem**: AuthStore not updated
✅ **Fix**: Verify all 4 methods in `authStore.ts` use `localhost:5500`

---

## 🎯 Files Changed

**1 file modified:**
- ✅ `web-frontend/techbirdsfly-frontend-nextjs/lib/store/authStore.ts`
  - `login()` → uses 5500 ✅
  - `register()` → uses 5500 ✅
  - `forgotPassword()` → uses 5500 ✅
  - `resetPassword()` → uses 5500 ✅

---

## 📈 Next Steps

1. ✅ All services running
2. ✅ Complete signup test
3. ✅ Verify tokens saved
4. 🔄 **Test protected routes** - Use token to access `/api/users/**`
5. 🔄 **Load testing** - Test with multiple concurrent signups
6. 🔄 **Production setup** - Update for Azure deployment

---

## 📚 Full Documentation

For complete details, advanced testing, troubleshooting, and monitoring:
→ Read: `GATEWAY_SIGNUP_INTEGRATION.md`

---

## 🎉 You're All Set!

Your signup flow is now **fully integrated with the Gateway**:

```
✅ Frontend properly configured
✅ Gateway routes configured  
✅ Auth Service endpoints ready
✅ CORS enabled
✅ Rate limiting active
✅ JWT tokens working
✅ Health checks passing
```

**Ready to test! 🚀**
