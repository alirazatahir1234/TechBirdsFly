# 🚀 Quick Test Commands Reference

## Start All Services

```bash
cd /Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly
./start-all-services-gateway.sh
```

**What it does:**
- Kills existing processes on ports 3000, 5001, 5500
- Opens 3 Terminal windows
- Starts Frontend (Next.js)
- Starts Gateway (YARP)
- Starts Auth Service (.NET)
- Auto-waits 30 seconds for initialization
- Verifies all services are online

---

## 🏥 Health Checks

### Auth Service (Port 5001)
```bash
curl http://localhost:5001/health
```

**Expected Response:**
```json
{
  "status": "Healthy"
}
```

### API Gateway (Port 5500)
```bash
curl http://localhost:5500/health
```

**Expected Response:**
```json
{
  "status": "Healthy"
}
```

### Frontend (Port 3000)
```bash
curl http://localhost:3000
```

**Expected Response:** HTML page

---

## 👤 Authentication Tests

### 1️⃣ User Signup/Registration

```bash
curl -X POST http://localhost:5500/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser@example.com",
    "fullName": "Test User",
    "password": "Password123!",
    "confirmPassword": "Password123!"
  }'
```

**Expected Response (200 OK):**
```json
{
  "userId": "147b6f72-9ee2-4367-8b01-21fc13f15340",
  "email": "testuser@example.com"
}
```

**Common Errors:**
- `"Passwords do not match"` - confirmPassword doesn't match password
- `"User with this email already exists"` - Email already registered
- `"Password does not meet requirements"` - Password too weak

---

### 2️⃣ User Login

```bash
curl -X POST http://localhost:5500/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser@example.com",
    "password": "Password123!"
  }'
```

**Expected Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "9Bi1kZbUF+rNEFuxJt0VpzydcxvsE0e8chQfPFcb3kwY8wK1bs4fh9QR6K3Gsd5lJ..."
}
```

**Common Errors:**
- `"Invalid email or password"` - Wrong credentials
- `"User not found"` - Email not registered

---

### 3️⃣ Forgot Password Request

```bash
curl -X POST http://localhost:5500/api/auth/forgot-password \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser@example.com"
  }'
```

**Expected Response (200 OK):**
```json
{
  "message": "Password reset instructions sent to your email"
}
```

---

### 4️⃣ Reset Password

```bash
curl -X POST http://localhost:5500/api/auth/reset-password \
  -H "Content-Type: application/json" \
  -d '{
    "token": "YOUR_RESET_TOKEN_HERE",
    "newPassword": "NewPassword123!",
    "confirmPassword": "NewPassword123!"
  }'
```

**Expected Response (200 OK):**
```json
{
  "message": "Password reset successfully"
}
```

---

## 🔑 Using JWT Tokens

### Store Token from Login Response
```bash
# After login, save the accessToken
BEARER_TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### Use Token for Authenticated Requests
```bash
curl -X GET http://localhost:5500/api/auth/me \
  -H "Authorization: Bearer $BEARER_TOKEN"
```

---

## 🧪 Complete Test Flow

### Step 1: Start All Services
```bash
./start-all-services-gateway.sh
sleep 30
```

### Step 2: Verify Services Are Online
```bash
curl http://localhost:5001/health
curl http://localhost:5500/health
curl http://localhost:3000
```

### Step 3: Create a Test User
```bash
curl -X POST http://localhost:5500/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser@example.com",
    "fullName": "Test User",
    "password": "TestPass123!",
    "confirmPassword": "TestPass123!"
  }'
```

### Step 4: Login with Test User
```bash
curl -X POST http://localhost:5500/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser@example.com",
    "password": "TestPass123!"
  }'
```

### Step 5: Verify JWT Token
```bash
# Extract token from response and decode it
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

# Check token at jwt.io (paste the token there)
# Or use this command to decode:
echo $TOKEN | cut -d'.' -f2 | base64 -D | jq .
```

---

## 📊 Expected Results

### Success Indicators
✅ All 3 services online (health checks respond)
✅ Signup creates user with UUID
✅ Login returns accessToken and refreshToken
✅ JWT tokens are valid and signed
✅ Tokens contain user claims (email, userId)

### Performance Expectations
- Health check response: <100ms
- Signup API response: ~450ms
- Login API response: ~380ms

---

## 🐛 Troubleshooting

### "Connection Refused" on localhost:5500
**Problem:** Gateway not running
**Solution:** Run `./start-all-services-gateway.sh` and wait 30 seconds

### "Passwords do not match"
**Problem:** confirmPassword field missing or doesn't match
**Solution:** Include `confirmPassword` field in signup request

### "User with this email already exists"
**Problem:** Email already registered
**Solution:** Use a different email or check database

### "Port already in use"
**Problem:** Service already running on port
**Solution:** Kill process: `lsof -ti:PORT | xargs kill -9`
- For port 3000: `lsof -ti:3000 | xargs kill -9`
- For port 5001: `lsof -ti:5001 | xargs kill -9`
- For port 5500: `lsof -ti:5500 | xargs kill -9`

---

## 🌐 URLs for Manual Testing

### Frontend (Browser)
```
http://localhost:3000
http://localhost:3000/signup
http://localhost:3000/login
```

### API Gateway
```
http://localhost:5500
http://localhost:5500/health
http://localhost:5500/api/auth/register
http://localhost:5500/api/auth/login
```

### Auth Service (Direct - bypass gateway)
```
http://localhost:5001
http://localhost:5001/health
http://localhost:5001/api/auth/register
http://localhost:5001/api/auth/login
```

---

## 📱 Frontend Testing in Browser

1. Open http://localhost:3000/signup
2. Fill in:
   - Email: `testuser@example.com`
   - Full Name: `Test User`
   - Password: `TestPass123!`
   - Confirm Password: `TestPass123!`
3. Click "Sign Up"
4. Expected: Success message and redirect to login
5. Go to http://localhost:3000/login
6. Fill in:
   - Email: `testuser@example.com`
   - Password: `TestPass123!`
7. Click "Login"
8. Expected: Success message and redirect to dashboard

---

## 📚 Related Documentation

- **Full Test Report:** `TEST_REPORT_FULL_SYSTEM.md`
- **Gateway Integration Guide:** `GATEWAY_SIGNUP_INTEGRATION_FIX.md`
- **Quick Reference:** `GATEWAY_INTEGRATION_QUICK_REF.md`
- **User Schema:** `USER_PROFILE_SCHEMA.md`

---

## 💡 Tips

- Use `jq` to format JSON responses: `curl ... | jq .`
- Save tokens to environment variables: `TOKEN=$(curl ... | jq -r '.accessToken')`
- Test with Postman for GUI: Import the API endpoints
- Use Thunder Client VSCode extension for in-editor testing
- Monitor logs in the open Terminal windows for debugging

---

**Everything is set up and ready to test!** 🚀
