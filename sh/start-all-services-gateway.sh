#!/bin/bash

# ============================================================================
# TechBirdsFly - Start All Services Script
# ============================================================================
# This script starts Auth Service, Gateway, and Frontend in separate terminals
# on macOS using osascript
# ============================================================================

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

BASE_DIR="/Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly"

# Banner
clear
echo -e "${BLUE}"
echo "╔════════════════════════════════════════════════════════════════╗"
echo "║                                                                ║"
echo "║   🚀 TechBirdsFly - Multi-Service Startup Script              ║"
echo "║                                                                ║"
echo "║   This script will start:                                     ║"
echo "║   ✅ Auth Service (port 5001)                                 ║"
echo "║   ✅ API Gateway (port 5500)                                  ║"
echo "║   ✅ Frontend (port 3000)                                     ║"
echo "║                                                                ║"
echo "╚════════════════════════════════════════════════════════════════╝"
echo -e "${NC}\n"

# ============================================================================
# STEP 1: Kill any existing processes
# ============================================================================
echo -e "${YELLOW}🛑 Cleaning up existing processes...${NC}"
echo "   Checking ports: 3000, 5001, 5500"

# Kill processes on ports 3000, 5001, 5500 silently
lsof -ti:3000,5001,5500 2>/dev/null | xargs -r kill -9 2>/dev/null || true

sleep 2
echo -e "${GREEN}✅ Cleanup complete${NC}\n"

# ============================================================================
# STEP 2: Start Auth Service (Port 5001)
# ============================================================================
echo -e "${YELLOW}📦 Starting Auth Service...${NC}"
echo "   Location: services/auth-service/src"
echo "   Port: 5001"
echo "   URL: http://localhost:5001"

osascript <<'APPLESCRIPT'
tell application "Terminal"
  do script "cd '/Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly/services/auth-service/src' && clear && echo '🔐 Auth Service Starting on port 5001...' && dotnet run"
  activate
end tell
APPLESCRIPT

sleep 3
echo -e "${GREEN}✅ Auth Service started${NC}\n"

# ============================================================================
# STEP 3: Start API Gateway (Port 5500)
# ============================================================================
echo -e "${YELLOW}📡 Starting API Gateway...${NC}"
echo "   Location: gateway/yarp-gateway/src"
echo "   Port: 5500"
echo "   URL: http://localhost:5500"
echo "   Routes: /api/auth/** → Auth Service"

osascript <<'APPLESCRIPT'
tell application "Terminal"
  do script "cd '/Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly/gateway/yarp-gateway/src' && clear && echo '🔄 API Gateway Starting on port 5500...' && dotnet run"
  activate
end tell
APPLESCRIPT

sleep 3
echo -e "${GREEN}✅ API Gateway started${NC}\n"

# ============================================================================
# STEP 4: Start Frontend (Port 3000)
# ============================================================================
echo -e "${YELLOW}⚛️  Starting Frontend...${NC}"
echo "   Location: web-frontend/techbirdsfly-frontend-nextjs"
echo "   Port: 3000"
echo "   URL: http://localhost:3000"
echo "   API: http://localhost:5500/api (via gateway)"

osascript <<'APPLESCRIPT'
tell application "Terminal"
  do script "cd '/Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly/web-frontend/techbirdsfly-frontend-nextjs' && clear && echo '⚛️  Frontend Starting on port 3000...' && npm run dev"
  activate
end tell
APPLESCRIPT

sleep 3
echo -e "${GREEN}✅ Frontend started${NC}\n"

# ============================================================================
# STEP 5: Wait for services to fully start
# ============================================================================
echo -e "${YELLOW}⏳ Waiting for services to fully initialize...${NC}"
echo "   (This may take 10-15 seconds)"
sleep 12

# ============================================================================
# STEP 6: Verify services
# ============================================================================
echo -e "\n${YELLOW}🔍 Verifying services...${NC}\n"

verify_service() {
  local name=$1
  local url=$2
  local port=$3

  if curl -s "$url" > /dev/null 2>&1; then
    echo -e "${GREEN}✅ $name (port $port)${NC}"
    echo "   URL: $url"
    echo "   Status: ONLINE"
  else
    echo -e "${RED}❌ $name (port $port)${NC}"
    echo "   URL: $url"
    echo "   Status: OFFLINE (still starting or error)"
  fi
  echo ""
}

verify_service "Auth Service" "http://localhost:5001/health" "5001"
verify_service "API Gateway" "http://localhost:5500/health" "5500"
verify_service "Frontend" "http://localhost:3000" "3000"

# ============================================================================
# STEP 7: Display summary
# ============================================================================
echo -e "${BLUE}╔════════════════════════════════════════════════════════════════╗"
echo "║                    🎉 Services Started!                           ║"
echo "╚════════════════════════════════════════════════════════════════╝${NC}\n"

echo -e "${GREEN}📋 Service URLs:${NC}"
echo "   Frontend:  ${BLUE}http://localhost:3000${NC}"
echo "   Gateway:   ${BLUE}http://localhost:5500${NC}"
echo "   Auth API:  ${BLUE}http://localhost:5001${NC}\n"

echo -e "${GREEN}📡 API Endpoints:${NC}"
echo "   Register:  ${BLUE}POST http://localhost:5500/api/auth/register${NC}"
echo "   Login:     ${BLUE}POST http://localhost:5500/api/auth/login${NC}"
echo "   Forgot:    ${BLUE}POST http://localhost:5500/api/auth/forgot-password${NC}"
echo "   Reset:     ${BLUE}POST http://localhost:5500/api/auth/reset-password${NC}\n"

echo -e "${GREEN}🧪 Quick Tests:${NC}"
echo "   # Check Auth Service"
echo "   ${BLUE}curl http://localhost:5001/health${NC}"
echo ""
echo "   # Check Gateway"
echo "   ${BLUE}curl http://localhost:5500/health${NC}"
echo ""
echo "   # Test SignUp"
echo "   ${BLUE}curl -X POST http://localhost:5500/api/auth/register \\${NC}"
echo "   ${BLUE}  -H 'Content-Type: application/json' \\${NC}"
echo "   ${BLUE}  -d '{\"email\":\"test@example.com\",\"password\":\"Pass123!\",\"fullName\":\"Test User\"}'${NC}\n"

echo -e "${GREEN}📚 Useful Commands:${NC}"
echo "   # View Auth Service logs"
echo "   ${BLUE}Cmd+2 (or click Auth terminal)${NC}"
echo ""
echo "   # View Gateway logs"
echo "   ${BLUE}Cmd+3 (or click Gateway terminal)${NC}"
echo ""
echo "   # View Frontend logs"
echo "   ${BLUE}Cmd+4 (or click Frontend terminal)${NC}\n"

echo -e "${YELLOW}⚠️  Notes:${NC}"
echo "   • Three new Terminal windows have opened"
echo "   • Each service runs in its own window"
echo "   • Close any window to stop that service"
echo "   • Press Ctrl+C to stop a service gracefully"
echo "   • Frontend hot-reloads on code changes"
echo "   • API calls go through Gateway (port 5500)"
echo "   • Gateway proxies to Auth Service (port 5001)\n"

echo -e "${BLUE}🎯 Next Steps:${NC}"
echo "   1. Open http://localhost:3000 in your browser"
echo "   2. Navigate to signup/register"
echo "   3. Fill in email, password, and full name"
echo "   4. Click Submit"
echo "   5. Check browser console for requests/responses\n"

echo -e "${GREEN}✨ Ready to develop!${NC}\n"
