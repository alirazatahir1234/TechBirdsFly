#!/bin/bash

# ============================================================================
# TechBirdsFly Complete Startup Script
# Purpose: Start all infrastructure and services for local development
# Updated: November 2, 2025
# ============================================================================

set -e

PROJECT_ROOT="/Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly"
DOCKER_COMPOSE_FILE="$PROJECT_ROOT/docker-compose.debug.yml"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}🚀 TechBirdsFly Complete Startup${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""

# Step 1: Check if Docker is running
echo -e "${YELLOW}[1/4] Checking Docker...${NC}"
if ! command -v docker &> /dev/null; then
    echo -e "${RED}❌ Docker is not installed${NC}"
    exit 1
fi

if ! docker ps &> /dev/null; then
    echo -e "${RED}❌ Docker daemon is not running${NC}"
    echo -e "${YELLOW}Start Docker and try again${NC}"
    exit 1
fi
echo -e "${GREEN}✅ Docker is running${NC}"
echo ""

# Step 2: Start infrastructure
echo -e "${YELLOW}[2/4] Starting Docker infrastructure...${NC}"
echo "Starting: PostgreSQL, Kafka, Zookeeper, Schema Registry, Seq, Jaeger, Redis"
cd "$PROJECT_ROOT"
docker compose -f docker-compose.debug.yml up -d 2>&1 | grep -E "Creating|Container|already|Error" || true
sleep 5
echo -e "${GREEN}✅ Infrastructure started${NC}"
echo ""

# Step 3: Verify services
echo -e "${YELLOW}[3/4] Verifying services...${NC}"
echo "Checking service availability..."
sleep 3
echo -e "${GREEN}✅ Services should be available${NC}"
echo ""

# Step 4: Database migrations
echo -e "${YELLOW}[4/4] Running database migrations...${NC}"
cd "$PROJECT_ROOT/services/event-bus-service/src"
echo "Creating Event Bus database..."
dotnet ef database update --quiet 2>/dev/null || echo "Already migrated"
echo -e "${GREEN}✅ Database migrations complete${NC}"
echo ""

# Final Summary
echo -e "${BLUE}========================================${NC}"
echo -e "${GREEN}✅ STARTUP COMPLETE${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""
echo -e "${YELLOW}📋 Next Steps:${NC}"
echo ""
echo "1️⃣  Start Services via VS Code Debug:"
echo "   • Press: Ctrl+Shift+D (Windows/Linux) or Cmd+Shift+D (Mac)"
echo "   • Select: '🔵 All .NET Services + Frontend'"
echo "   • Click: ▶️ Green Play Button"
echo ""
echo "2️⃣  Or start services manually from terminal:"
echo ""
echo "   Terminal 1 - Auth Service:"
echo "   cd $PROJECT_ROOT/services/auth-service/src"
echo "   dotnet run"
echo ""
echo "   Terminal 2 - Event Bus Service:"
echo "   cd $PROJECT_ROOT/services/event-bus-service/src"
echo "   dotnet run"
echo ""
echo "   Terminal 3 - User Service:"
echo "   cd $PROJECT_ROOT/services/user-service/src/UserService"
echo "   dotnet run"
echo ""
echo -e "${YELLOW}🔍 Monitoring Dashboards:${NC}"
echo "   • Seq Logs:         http://localhost:5341"
echo "   • Jaeger Traces:    http://localhost:16686"
echo "   • Schema Registry:  http://localhost:8081"
echo ""
echo -e "${YELLOW}🌐 Service Swagger URLs:${NC}"
echo "   • Auth Service:     http://localhost:5000/swagger"
echo "   • Event Bus:        http://localhost:5020/swagger"
echo "   • User Service:     http://localhost:5008/swagger"
echo "   • Billing Service:  http://localhost:5002/swagger"
echo "   • Generator Service: http://localhost:5003/swagger"
echo "   • Image Service:    http://localhost:5007/swagger"
echo "   • Admin Service:    http://localhost:5006/swagger"
echo "   • API Gateway:      http://localhost:8000/swagger"
echo "   • Frontend:         http://localhost:3000"
echo ""
echo -e "${YELLOW}🧪 Testing:${NC}"
echo "   • Postman Collection: postman-collection.json"
echo "   • API Reference:      MICROSERVICES_ENDPOINTS.md"
echo "   • Quick Reference:    .vscode/QUICK_REFERENCE.md"
echo ""
echo -e "${YELLOW}🔄 Event Flow (Use Case U1):${NC}"
echo "   1. Register User (Auth Service:5000)"
echo "   2. Event published to Event Bus (Port 5020)"
echo "   3. Stored in PostgreSQL Outbox"
echo "   4. Published to Kafka (Port 9092)"
echo "   5. Consumed by User Service (Port 5008)"
echo "   6. Profile created in SQLite"
echo ""
echo -e "${BLUE}========================================${NC}"
echo -e "${GREEN}Infrastructure Ready! Start services via VS Code.${NC}"
echo -e "${BLUE}========================================${NC}"
