# TechBirdsFly.AI — Quick Start & Testing Guide

Get the full AI Website Generator running locally in **5 minutes**.

## 🚀 Start Services (Terminal 1-3)

### Terminal 1: Auth Service
```bash
cd /Applications/My\ Drive/TechBirdsFly/services/auth-service/AuthService
dotnet run --urls http://localhost:5001
```

Output should show:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started.
```

### Terminal 2: Generator Service
```bash
cd /Applications/My\ Drive/TechBirdsFly/services/generator-service/GeneratorService
dotnet run --urls http://localhost:5003
```

Output should show:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5003
```

### Terminal 3: Frontend
```bash
cd /Applications/My\ Drive/TechBirdsFly/web-frontend/techbirdsfly-frontend
npm start
```

Browser will open http://localhost:3000 automatically.

---

## ✅ Test End-to-End Flow

### Step 1: Register User

```bash
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "Ali Raza",
    "email": "ali@techbirds.ai",
    "password": "Test@1234"
  }'
```

**Expected Response:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "email": "ali@techbirds.ai",
  "fullName": "Ali Raza"
}
```

**Save the `id` as `USER_ID`:**
```bash
USER_ID="550e8400-e29b-41d4-a716-446655440000"
```

---

### Step 2: Login & Get JWT Token

```bash
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "ali@techbirds.ai",
    "password": "Test@1234"
  }'
```

**Expected Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "Tq3K5..."
}
```

**Save the token:**
```bash
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

### Step 3: Create a Website Generation Project

```bash
curl -X POST http://localhost:5003/api/projects \
  -H "Content-Type: application/json" \
  -H "X-User-Id: $USER_ID" \
  -d '{
    "name": "My Photographer Portfolio",
    "prompt": "Create a modern, dark-themed portfolio website for a professional photographer specializing in landscape photography"
  }'
```

**Expected Response:**
```json
{
  "projectId": "660e8400-e29b-41d4-a716-446655440001",
  "jobId": "760e8400-e29b-41d4-a716-446655440002",
  "status": "pending",
  "message": "Project created and queued for generation"
}
```

**Save the project ID:**
```bash
PROJECT_ID="660e8400-e29b-41d4-a716-446655440001"
```

---

### Step 4: Check Project Status

```bash
curl http://localhost:5003/api/projects/$PROJECT_ID \
  -H "X-User-Id: $USER_ID"
```

**Expected Response:**
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440001",
  "name": "My Photographer Portfolio",
  "status": "pending",
  "previewUrl": null,
  "artifactUrl": null,
  "jobStatus": "queued",
  "createdAt": "2025-10-16T12:00:00Z"
}
```

---

### Step 5: View Swagger Documentation

Open in your browser:

- **Auth Service**: http://localhost:5001/swagger
- **Generator Service**: http://localhost:5003/swagger

---

## 🧪 Automated Test Script

Save as `test.sh` in the project root:

```bash
#!/bin/bash

set -e

echo "🚀 TechBirdsFly.AI — End-to-End Test"
echo "===================================="

# Colors
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Register
echo -e "${BLUE}[1/5] Registering user...${NC}"
REGISTER_RESPONSE=$(curl -s -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "Test User",
    "email": "test@example.com",
    "password": "Test@1234"
  }')

USER_ID=$(echo "$REGISTER_RESPONSE" | jq -r '.id')
echo -e "${GREEN}✓ User created: $USER_ID${NC}"

# Login
echo -e "${BLUE}[2/5] Logging in...${NC}"
LOGIN_RESPONSE=$(curl -s -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test@1234"
  }')

TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.accessToken')
echo -e "${GREEN}✓ Logged in, token received${NC}"

# Create Project
echo -e "${BLUE}[3/5] Creating project...${NC}"
PROJECT_RESPONSE=$(curl -s -X POST http://localhost:5003/api/projects \
  -H "Content-Type: application/json" \
  -H "X-User-Id: $USER_ID" \
  -d '{
    "name": "Test Portfolio",
    "prompt": "Create a simple portfolio website"
  }')

PROJECT_ID=$(echo "$PROJECT_RESPONSE" | jq -r '.projectId')
echo -e "${GREEN}✓ Project created: $PROJECT_ID${NC}"

# Check Status
echo -e "${BLUE}[4/5] Checking project status...${NC}"
STATUS_RESPONSE=$(curl -s http://localhost:5003/api/projects/$PROJECT_ID \
  -H "X-User-Id: $USER_ID")

STATUS=$(echo "$STATUS_RESPONSE" | jq -r '.status')
echo -e "${GREEN}✓ Project status: $STATUS${NC}"

# Verify Services
echo -e "${BLUE}[5/5] Verifying all services...${NC}"
AUTH_HEALTH=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5001/api/auth/login)
GEN_HEALTH=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5003/api/projects)

echo -e "${GREEN}✓ Auth Service: HTTP $AUTH_HEALTH${NC}"
echo -e "${GREEN}✓ Generator Service: HTTP $GEN_HEALTH${NC}"

echo ""
echo -e "${GREEN}✅ All tests passed!${NC}"
echo ""
echo "Project Summary:"
echo "  User ID: $USER_ID"
echo "  Project ID: $PROJECT_ID"
echo "  Status: $STATUS"
```

Run it:
```bash
chmod +x test.sh
./test.sh
```

---

## 🐛 Troubleshooting

### "Port already in use"
Kill existing processes:
```bash
# macOS/Linux
lsof -i :5001  # Find process on port 5001
kill -9 <PID>

# Or just change the port
dotnet run --urls http://localhost:5011
```

### "Database is locked"
Delete the SQLite files and restart:
```bash
rm services/auth-service/AuthService/auth.db
rm services/generator-service/GeneratorService/generator.db
```

### "Connection refused"
Ensure all 3 services are running. Check terminal output for errors.

### "X-User-Id header not found"
Add the header to all Generator Service requests:
```bash
-H "X-User-Id: 550e8400-e29b-41d4-a716-446655440000"
```

---

## 📊 Swagger Testing

Instead of curl, you can use Swagger UI:

1. Go to http://localhost:5001/swagger
2. Click "Try it out" on any endpoint
3. Fill in parameters and click "Execute"
4. See live responses

---

## 🎯 What to Test Next

✅ Register a user  
✅ Login and get token  
✅ Create a project  
✅ Get project status  
⬜ Deploy to Docker (see `/infra/docker-compose.yml`)  
⬜ Integrate Frontend UI  
⬜ Real Azure OpenAI calls  
⬜ Background worker for async processing  

---

## 📚 Documentation

- **Architecture**: `/docs/architecture.md`
- **Auth Service**: `/services/auth-service/README.md`
- **Generator Service**: `/services/generator-service/README.md`
- **Main README**: `/README.md`

---

**Ready to build? Start the services and run the test! 🚀**
