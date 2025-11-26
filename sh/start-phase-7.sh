#!/bin/bash

# PHASE 7 - Quick Start Script
# Start both backend and frontend

echo "🚀 Starting TechBirdsFly Full Stack..."
echo ""

# Colors
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Start Backend
echo -e "${BLUE}📦 Starting Backend (Generator Service)...${NC}"
cd "$(dirname "$0")/services/generator-service/src"
ASPNETCORE_URLS="http://localhost:5003" dotnet run -c Debug &
BACKEND_PID=$!
echo -e "${GREEN}✓ Backend started (PID: $BACKEND_PID)${NC}"
echo ""

# Wait for backend to be ready
sleep 3

# Start Frontend
echo -e "${BLUE}⚛️  Starting Frontend (Next.js)...${NC}"
cd "$(dirname "$0")/web-frontend/techbirdsfly-frontend-nextjs"
npm run dev &
FRONTEND_PID=$!
echo -e "${GREEN}✓ Frontend started (PID: $FRONTEND_PID)${NC}"
echo ""

# Print URLs
echo -e "${GREEN}════════════════════════════════════════════${NC}"
echo -e "${GREEN}✨ TechBirdsFly is running!${NC}"
echo ""
echo -e "${BLUE}Backend (Generator Service):${NC}"
echo "  Health Check: http://localhost:5003/api/v1/generate/health"
echo "  Swagger: http://localhost:5003/swagger/index.html"
echo ""
echo -e "${BLUE}Frontend (Next.js):${NC}"
echo "  Dashboard: http://localhost:3000/dashboard/create"
echo "  Editor: http://localhost:3000/dashboard/editor"
echo "  Export: http://localhost:3000/dashboard/export"
echo ""
echo -e "${GREEN}════════════════════════════════════════════${NC}"
echo ""
echo "Press Ctrl+C to stop all services"
echo ""

# Wait for processes
wait $BACKEND_PID $FRONTEND_PID
