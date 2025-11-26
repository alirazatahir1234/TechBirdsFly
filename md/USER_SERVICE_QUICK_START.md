# User Service - Quick Start Guide ⚡

## 🚀 30-Second Setup

```bash
# 1. Create database
createdb TBF_User

# 2. Run migrations
cd services/user-service/src/UserService
dotnet ef database update

# 3. Run service
dotnet run
```

Service available at: **http://localhost:5002**

---

## 📍 Essential Commands

### Build
```bash
dotnet build services/user-service/src/UserService/UserService.csproj --configuration Debug
```

### Run
```bash
cd services/user-service/src/UserService && dotnet run
```

### Test Endpoints
```bash
# Register
curl -X POST http://localhost:5002/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!",
    "username": "testuser",
    "fullName": "Test User"
  }'

# Login
curl -X POST http://localhost:5002/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!"
  }'

# Get Profile (replace TOKEN with JWT from login)
curl -X GET http://localhost:5002/api/profile \
  -H "Authorization: Bearer TOKEN"
```

---

## 🗄️ Database Setup

```bash
# Verify database exists
psql -l | grep TBF_User

# Drop database (if needed)
dropdb TBF_User

# Run migrations
dotnet ef database update --project services/user-service/src/UserService/UserService.csproj
```

---

## 📊 API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/auth/register` | POST | Register new user |
| `/api/auth/login` | POST | Login & get JWT |
| `/api/profile` | GET | Get current profile |
| `/api/profile/{userId}` | GET | Get user profile |
| `/api/profile` | PUT | Update profile |
| `/api/profile/change-password` | POST | Change password |

---

## 🔑 Authentication Header

All protected endpoints require:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 🌍 Via API Gateway (Recommended)

```bash
# Through gateway port 5500
curl -X POST http://localhost:5500/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "test@example.com", "password": "Test123!"}'
```

---

## 📝 Configuration Files

**appsettings.json** - Database & JWT config
**Program.cs** - Service configuration
**UserService.csproj** - Dependencies

---

## ✅ Verification Checklist

- [ ] PostgreSQL running on localhost:5432
- [ ] Database `TBF_User` created
- [ ] Migrations applied
- [ ] Service running on port 5002
- [ ] Can register new user
- [ ] Can login & get JWT token
- [ ] Can access protected endpoints

---

## 🆘 Common Issues

| Issue | Fix |
|-------|-----|
| Connection refused | Start PostgreSQL |
| Database doesn't exist | Run `createdb TBF_User` |
| Port already in use | Change port in appsettings.json |
| JWT validation fails | Verify JWT Key in config |

---

## 🔄 Quick Commands

```bash
# Build all services
dotnet build /Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly/TechBirdsFly.sln

# Build User Service only
dotnet build services/user-service/src/UserService/UserService.csproj

# Run via dotnet
dotnet run --project services/user-service/src/UserService

# Access Swagger
open http://localhost:5002/swagger

# Via gateway
open http://localhost:5500/swagger/index.html
```

---

## 📚 Next Steps

1. ✅ Start User Service
2. ✅ Test authentication endpoints
3. → Integrate with frontend
4. → Add email verification (SendGrid)
5. → Deploy to Azure

---

**Status:** Production Ready ✅

