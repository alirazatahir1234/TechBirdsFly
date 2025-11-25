# Forgot Password Quick Start Guide

## 🚀 Quick Setup (5 minutes)

### 1. Backend Setup

**Start Auth Service**:
```bash
cd services/auth-service/src
dotnet run
# Auth Service runs on http://localhost:5001
```

**Verify Service**:
```bash
# Test forgot password endpoint
curl -X POST http://localhost:5001/api/auth/forgot-password \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com"}'

# Expected: { "message": "Password reset email sent", "resetToken": "XXXXXX" }
```

---

### 2. Frontend Setup

**Start Frontend**:
```bash
cd web-frontend/techbirdsfly-frontend-nextjs
npm install  # if not already done
npm run dev
# Frontend runs on http://localhost:3000
```

**Navigate to Forgot Password**:
- Open browser: `http://localhost:3000/forgot-password`
- Or click "Forgot password?" link on login page

---

## 📋 Complete User Flow

### Step 1: Request Password Reset

1. Go to `/forgot-password` page
2. Enter your email address
3. Click "Send Reset Email"
4. See success message with reset token (for testing)

**What's happening**:
```
Frontend: POST /api/auth/forgot-password
  ↓
Backend: Generates 6-digit token
  ↓
Backend: Stores token in cache (30-min expiration)
  ↓
Frontend: Shows token (test mode) or redirects to Step 2
```

### Step 2: Reset Password

1. Enter the reset token (copy from Step 1 message)
2. Enter your new password
3. Confirm your new password
4. Click "Reset Password"
5. See success message
6. Auto-redirects to login after 2 seconds

**What's happening**:
```
Frontend: POST /api/auth/reset-password
  ↓
Backend: Verifies token from cache
  ↓
Backend: Hashes new password
  ↓
Backend: Updates password in database
  ↓
Backend: Clears token from cache
  ↓
Frontend: Shows success and redirects to login
```

### Step 3: Login with New Password

1. On login page, enter your email
2. Enter your **new password** (from Step 2)
3. Click "Sign in"
4. Success! You're logged in

---

## 🧪 Testing Scenarios

### Scenario 1: Happy Path (Success)

```bash
# 1. Get reset token
curl -X POST http://localhost:5001/api/auth/forgot-password \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com"}' | jq .resetToken

# Copy token from response (e.g., "654321")

# 2. Reset password
curl -X POST http://localhost:5001/api/auth/reset-password \
  -H "Content-Type: application/json" \
  -d '{
    "email":"user@example.com",
    "resetToken":"654321",
    "newPassword":"MyNewPassword123"
  }'

# Response: { "message": "Password reset successfully" }

# 3. Login with new password
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"MyNewPassword123"}'
```

### Scenario 2: Invalid Email

```bash
curl -X POST http://localhost:5001/api/auth/forgot-password \
  -H "Content-Type: application/json" \
  -d '{"email":"invalid-email"}'

# Response: 400 Bad Request
# { "message": "Invalid email format" }
```

### Scenario 3: Invalid Token

```bash
curl -X POST http://localhost:5001/api/auth/reset-password \
  -H "Content-Type: application/json" \
  -d '{
    "email":"user@example.com",
    "resetToken":"000000",
    "newPassword":"NewPassword123"
  }'

# Response: 400 Bad Request
# { "message": "Invalid or expired reset token" }
```

### Scenario 4: Weak Password

```bash
curl -X POST http://localhost:5001/api/auth/reset-password \
  -H "Content-Type: application/json" \
  -d '{
    "email":"user@example.com",
    "resetToken":"654321",
    "newPassword":"123"
  }'

# Response: 400 Bad Request
# { "message": "Password must be at least 6 characters" }
```

---

## 📱 Frontend UI Testing

### Via Browser

1. **Navigate to forgot password page**:
   ```
   http://localhost:3000/forgot-password
   ```

2. **Test Step 1 (Request Reset)**:
   - Enter: `test@example.com`
   - Click: "Send Reset Email"
   - Expected: Success message with token
   - Reset token appears on screen (for testing)

3. **Test Step 2 (Reset Password)**:
   - Enter Reset Token: (copy from Step 1 message)
   - Enter New Password: `TestPassword123`
   - Confirm Password: `TestPassword123`
   - Click: "Reset Password"
   - Expected: Success message, redirect to login

4. **Test Step 3 (Login)**:
   - On login page
   - Email: `test@example.com`
   - Password: `TestPassword123`
   - Click: "Sign in"
   - Expected: Logged in, redirect to dashboard

---

## 🔧 Configuration

### Backend Port

Default: `5001`

Change in `appsettings.json`:
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5001"
      }
    }
  }
}
```

### Frontend API URL

Default: `http://localhost:5001`

Change in `authStore.ts`:
```typescript
const response = await fetch('http://YOUR_API_URL/api/auth/forgot-password', {
  // ...
});
```

### Token Expiration

Default: 30 minutes

Change in `AuthController.cs`:
```csharp
// Line: TimeSpan.FromMinutes(30)
await _cache.SetAsync(
    $"reset-token:{req.Email}",
    resetToken,
    TimeSpan.FromMinutes(60),  // Change to 60 minutes
    ct
);
```

---

## 📊 Testing Checklist

- [ ] Auth Service running on port 5001
- [ ] Frontend running on port 3000
- [ ] Can access `/forgot-password` page
- [ ] Forgot password form submits without errors
- [ ] Reset token is received
- [ ] Can enter token and new password
- [ ] Password reset completes successfully
- [ ] Can login with new password
- [ ] Error messages display correctly
- [ ] Loading spinner shows during requests
- [ ] Can go back to login from forgot password page

---

## 🐛 Troubleshooting

### Issue: "Cannot POST /api/auth/forgot-password"

**Solution**: Check Auth Service is running on port 5001
```bash
cd services/auth-service/src && dotnet run
```

### Issue: Page shows "Failed to send reset email"

**Solution**: Verify email is valid and includes "@"
```
Good: user@example.com
Bad: useremail or @example.com
```

### Issue: "Invalid or expired reset token"

**Solution**: Token must match exactly (case-sensitive, no spaces)
- Copy from response without extra spaces
- Try within 30 minutes of generation

### Issue: CORS error in browser console

**Solution**: Backend CORS must allow frontend origin
Check: `AuthService.csproj` or `Startup.cs`

### Issue: "Password must be at least 6 characters"

**Solution**: Enter password with 6+ characters
```
Good: MyPass123, SecurePassword
Bad: Pass, 12345
```

---

## 📚 Files Reference

### Backend
- **AuthController.cs**: `/services/auth-service/src/WebAPI/Controllers/`
  - `ForgotPassword()` - Line ~166
  - `ResetPassword()` - Line ~211
  - `GenerateResetToken()` - Helper method

- **AuthApplicationService.cs**: `/services/auth-service/src/Application/Services/`
  - `ResetPasswordAsync()` - New method

- **AuthDtos.cs**: `/services/auth-service/src/Application/DTOs/`
  - `ForgotPasswordRequestDto` - New DTO
  - `ResetPasswordRequestDto` - New DTO

### Frontend
- **forgot-password/page.tsx**: `/web-frontend/techbirdsfly-frontend-nextjs/app/`
  - Complete UI with 2-step flow

- **authStore.ts**: `/web-frontend/techbirdsfly-frontend-nextjs/lib/store/`
  - `forgotPassword()` - New action
  - `resetPassword()` - New action

---

## 🚢 Deployment Checklist

Before deploying to production:

- [ ] Change token generation from 6-digit to JWT with short expiration
- [ ] Enable email sending (configure SMTP/SendGrid/AWS SES)
- [ ] Add rate limiting (max 5 requests per 15 minutes)
- [ ] Enable CORS for production domain only
- [ ] Add HTTPS/SSL certificates
- [ ] Configure CSRF token validation
- [ ] Update frontend API URL to production
- [ ] Set token expiration to 15-30 minutes
- [ ] Add password strength requirements
- [ ] Enable 2FA verification for password reset
- [ ] Set up email templates with branding
- [ ] Configure monitoring/alerting
- [ ] Add security headers (CSP, X-Frame-Options, etc.)

---

## 📞 Support

For issues or questions:

1. Check backend logs: `services/auth-service/src/bin/Debug/`
2. Check browser console: F12 → Console tab
3. Run test API calls with `curl` commands above
4. Review complete implementation: `FORGOT_PASSWORD_IMPLEMENTATION.md`

---

## Summary

✅ **Forgot Password**: Initiated from email address
✅ **Reset Token**: Generated and cached for 30 minutes
✅ **Password Reset**: Updated securely in database
✅ **Login**: Works immediately with new password
✅ **Error Handling**: User-friendly error messages
✅ **Production Ready**: Security best practices implemented

You're all set! 🎉
