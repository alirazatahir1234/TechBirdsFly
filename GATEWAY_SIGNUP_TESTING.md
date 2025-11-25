# Gateway SignUp Integration - Testing & Validation

## 🧪 Complete Test Suite

---

## Test 1: Basic SignUp Flow

### Test Case: Successful User Registration

**Prerequisites**:
- ✅ Gateway running on port 5500
- ✅ Auth Service running on port 5001
- ✅ Frontend running on port 3000
- ✅ PostgreSQL database accessible

**Test Steps**:

1. Open browser: `http://localhost:3000/register`
2. Fill form:
   - Full Name: `John Doe`
   - Email: `john.doe@testbirds.com`
   - Password: `SecurePass123!`
   - Confirm: `SecurePass123!`
   - Terms: ✓
3. Click "Create Account"
4. Wait for loading spinner to disappear

**Expected Results**:

| Step | Expected | Actual | Status |
|------|----------|--------|--------|
| Form opens | Registration form visible | | ☐ |
| Form fills | All fields populate | | ☐ |
| Submit | Loading spinner shows | | ☐ |
| Network | POST to 5500 (gateway) | | ☐ |
| Response | 200 OK with tokens | | ☐ |
| Storage | Tokens in localStorage | | ☐ |
| Redirect | Navigate to dashboard | | ☐ |
| User info | User name displayed | | ☐ |

**Pass/Fail**: ☐ PASS | ☐ FAIL

---

## Test 2: CORS Preflight

### Test Case: Browser CORS Validation

**Prerequisites**: Browser with DevTools open

**Test Steps**:

1. Open `http://localhost:3000/register`
2. Open DevTools → Network tab
3. Fill and submit form
4. Look for preflight request

**Expected CORS Headers**:

```http
Request Headers:
- Origin: http://localhost:3000 ✅
- Access-Control-Request-Method: POST ✅
- Access-Control-Request-Headers: content-type ✅

Response Headers:
- Access-Control-Allow-Origin: http://localhost:3000 ✅
- Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS ✅
- Access-Control-Allow-Headers: Content-Type, Authorization ✅
```

**Verification**:
- [ ] Preflight returns 200 OK
- [ ] CORS headers present
- [ ] Actual POST request follows
- [ ] No CORS errors in console

---

## Test 3: Rate Limiting

### Test Case: Rate Limit Enforcement

**Prerequisites**: Tested SignUp flow works

**Test Steps**:

1. Open DevTools → Console
2. Run script to send 15 rapid requests:

```javascript
// Paste in console
const urls = Array.from({length: 15}, (_, i) => i);

async function testRateLimit() {
  for (const i of urls) {
    try {
      const response = await fetch('http://localhost:5500/api/auth/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          email: `user${i}@example.com`,
          fullName: `User ${i}`,
          password: 'Pass123!'
        })
      });
      console.log(`Request ${i+1}: ${response.status} ${response.statusText}`);
    } catch (e) {
      console.error(`Request ${i+1}: Error - ${e.message}`);
    }
  }
}

testRateLimit();
```

**Expected Results**:

```
Request 1: 200 OK ✅
Request 2: 200 OK ✅
...
Request 10: 200 OK ✅
Request 11: 429 Too Many Requests ✅
Request 12: 429 Too Many Requests ✅
...
Request 15: 429 Too Many Requests ✅
```

**Verification**:
- [ ] First 10 requests succeed (200)
- [ ] Requests 11+ are rate limited (429)
- [ ] Wait 60 seconds and retry
- [ ] New batch allowed after reset

---

## Test 4: Gateway Routing

### Test Case: Request Routing Through Gateway

**Prerequisites**: Services running

**Test Steps**:

1. Terminal 1 - Watch Auth Service logs:
```bash
cd services/auth-service/src
dotnet run --urls http://localhost:5001 2>&1 | grep -E "POST|register|auth"
```

2. Terminal 2 - Send request through gateway:
```bash
curl -X POST http://localhost:5500/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "routing-test@example.com",
    "fullName": "Routing Test",
    "password": "RoutePass123!"
  }' -v
```

**Expected Results**:

✅ **Gateway Output**:
```
200 OK
Routing to: auth-cluster → http://localhost:5001
```

✅ **Auth Service Log** (should show):
```
POST /api/auth/register
Request received
User entity created
UserProfile entity created
JWT tokens generated
Response sent
```

✅ **Response**:
```json
{
  "user": {...},
  "accessToken": "...",
  "refreshToken": "..."
}
```

**Verification**:
- [ ] Gateway receives request
- [ ] Auth Service logs show incoming request
- [ ] Response contains user + tokens
- [ ] No direct connection to Auth Service needed

---

## Test 5: Token Storage & Usage

### Test Case: JWT Token Management

**Prerequisites**: Successful signup completed

**Test Steps**:

1. After signup, open DevTools → Application → Storage
2. Check `auth-store` in localStorage

**Expected localStorage Content**:

```javascript
// localStorage['auth-store']
{
  "state": {
    "user": {
      "id": "uuid",
      "email": "test@example.com",
      "fullName": "Test User",
      "role": "user",
      "avatar": null,
      "createdAt": "2025-11-17T10:00:00Z"
    },
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "isAuthenticated": true,
    "isLoading": false,
    "error": null
  }
}
```

**Decode Token**:
```javascript
// Paste in console
const token = JSON.parse(localStorage['auth-store']).state.token;
const payload = token.split('.')[1];
console.log(JSON.parse(atob(payload)));
```

**Expected Payload**:
```javascript
{
  "sub": "user-id",
  "email": "test@example.com",
  "fullName": "Test User",
  "iat": 1700213400,
  "exp": 1700215200,
  "iss": "TechBirdsFly.AuthService",
  "aud": ["techbirdsfly-frontend-nextjs", "techbirdsfly-gateway"]
}
```

**Verification**:
- [ ] localStorage contains auth-store
- [ ] Token is valid JWT format
- [ ] Token contains correct claims
- [ ] Expiration time reasonable (30 min)
- [ ] Can decode and verify

---

## Test 6: Protected Route Access

### Test Case: Using Token on Protected Route

**Prerequisites**: Signup completed, tokens stored

**Test Steps**:

1. After signup (user logged in), access dashboard
2. Dashboard calls `/api/users/{userId}` (protected route)
3. DevTools → Network tab

**Expected Flow**:

```
Dashboard loads
  ↓
Fetch /api/users/{userId}
  ├─ Endpoint: http://localhost:5500/api/users/...
  ├─ Headers: Authorization: Bearer {token}
  └─ Method: GET
  ↓
Gateway receives request
  ├─ Validates JWT ✅
  ├─ Routes to User Service
  └─ Adds Authorization header
  ↓
User Service processes request
  ├─ Finds user
  └─ Returns profile data
  ↓
Response returns to frontend
  └─ Dashboard displays user info
```

**Network Tab Verification**:

| Request | URL | Status | Purpose |
|---------|-----|--------|---------|
| GET | `/api/users/...` | 200 OK | ✅ Works with token |
| Without token | `/api/users/...` | 401 Unauthorized | ✅ JWT required |

---

## Test 7: Error Handling

### Test Case: Invalid Inputs

**Test 7a: Duplicate Email**

**Steps**:
1. Register: `duplicate@example.com`
2. Register again with same email
3. Check error message

**Expected**:
```json
{
  "statusCode": 400,
  "message": "Email already exists"
}
```

**Verification**: ☐ Error message shown to user

---

### Test 7b: Weak Password

**Steps**:
1. Try password: `123` (too short)
2. Check form validation

**Expected**: Client-side validation shows error before sending

**Form Message**:
```
❌ Password must be at least 8 characters
❌ Must contain at least one uppercase letter
❌ Must contain at least one number
❌ Must contain at least one special character
```

**Verification**: ☐ Form validation prevents submission

---

### Test 7c: Invalid Email Format

**Steps**:
1. Enter email: `not-an-email`
2. Try to submit

**Expected**: Client-side validation error

**Verification**: ☐ Form rejects invalid format

---

### Test 7d: Network Error

**Steps**:
1. Stop Gateway service
2. Try to signup
3. Check error handling

**Expected**:
```
Signup fails with network error
User sees: "Connection failed. Please try again."
```

**Verification**: ☐ Graceful error handling, user can retry

---

## Test 8: Concurrent Requests

### Test Case: Multiple Users Signing Up Simultaneously

**Prerequisites**: Services running

**Test Steps**:

```bash
# Send 5 parallel signup requests
parallel_count=5
for i in $(seq 1 $parallel_count); do
  curl -X POST http://localhost:5500/api/auth/register \
    -H "Content-Type: application/json" \
    -d "{
      \"email\": \"concurrent$i@example.com\",
      \"fullName\": \"Concurrent User $i\",
      \"password\": \"ConcPass123!\"
    }" &
done
wait
```

**Expected Results**:

```
✅ All 5 requests complete successfully
✅ Each gets unique user ID
✅ Database has 5 new users
✅ Each has own tokens
✅ No conflicts or race conditions
```

**Verification**: ☐ Concurrent access works reliably

---

## Test 9: Performance

### Test Case: Response Time Measurement

**Prerequisites**: All services running

**Test Steps**:

```bash
# Measure signup response time
time curl -X POST http://localhost:5500/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "perf@example.com",
    "fullName": "Performance Test",
    "password": "PerfPass123!"
  }'
```

**Expected**: Response time < 2 seconds (total latency)

**Breakdown**:
- Gateway → Auth Service: < 100ms
- Password hashing: ~200-500ms
- Database insert: ~100-200ms
- JWT generation: ~50ms
- Return response: ~100ms
- **Total**: < 2 seconds ✅

**Verification**:
- [ ] Total time < 2 seconds
- [ ] Acceptable user experience
- [ ] No timeout errors

---

## Test 10: Browser Console Validation

### Test Case: Frontend Error Handling

**Prerequisites**: Successfully signed up

**Test Steps**:

1. Open DevTools → Console
2. Check for any errors/warnings
3. Logout
4. Try to access protected route

**Expected Console Output**:

```javascript
// On successful signup
✅ No errors
✅ No warnings
✅ No CORS issues
✅ Tokens successfully saved

// On logout
✅ Tokens cleared
✅ Redirect to login

// On protected route without token
❌ 401 Unauthorized (expected)
✅ Redirected to login
```

**Verification**: ☐ Clean console with proper error handling

---

## Test Summary Table

| Test | Purpose | Status | Notes |
|------|---------|--------|-------|
| 1 | Basic signup flow | ☐ | Happy path test |
| 2 | CORS validation | ☐ | Browser security |
| 3 | Rate limiting | ☐ | DDoS protection |
| 4 | Gateway routing | ☐ | Request forwarding |
| 5 | Token management | ☐ | JWT handling |
| 6 | Protected routes | ☐ | Auth on other services |
| 7a | Duplicate email | ☐ | Error handling |
| 7b | Weak password | ☐ | Validation |
| 7c | Invalid email | ☐ | Input validation |
| 7d | Network error | ☐ | Resilience |
| 8 | Concurrent requests | ☐ | Load testing |
| 9 | Performance | ☐ | Response time |
| 10 | Browser console | ☐ | Frontend health |

---

## 🎯 Sign-Off Checklist

- [ ] All 10 test cases pass
- [ ] No console errors
- [ ] Response times acceptable (< 2s)
- [ ] Database populated correctly
- [ ] Tokens working on protected routes
- [ ] Rate limiting active
- [ ] Error handling graceful
- [ ] CORS working properly
- [ ] Gateway routing verified
- [ ] Ready for production

---

## 📊 Regression Testing

After any changes, re-run these critical tests:

1. ✅ **Signup** - Can register new user
2. ✅ **Login** - Can login with registered account
3. ✅ **Dashboard** - Can access protected route
4. ✅ **Logout** - Tokens cleared and redirect works
5. ✅ **Rate Limit** - Still enforced at 10/min

---

## 🚀 Ready for Production?

Once all tests pass:

- [ ] Deploy Gateway to Azure
- [ ] Deploy Auth Service to Azure
- [ ] Update DNS/CNAME records
- [ ] Update frontend configuration
- [ ] Configure production database
- [ ] Enable HTTPS/TLS
- [ ] Set up monitoring/alerts
- [ ] Monitor for 24 hours
- [ ] Document any issues
- [ ] Create runbook for support team

---

## 📞 Support

If tests fail, check:

1. Are all 3 services running on correct ports?
2. Check logs for error messages
3. Verify database connectivity
4. Check JWT configuration in all services
5. See troubleshooting guide in main documentation

**Ready to test? Let's go! 🎉**
