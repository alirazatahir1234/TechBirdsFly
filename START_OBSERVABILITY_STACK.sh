#!/bin/bash

# TechBirdsFly Phase 1: Observability Stack - Quick Start
# ============================================================

set -e

echo "🚀 TechBirdsFly Observability Stack Startup"
echo "============================================="
echo ""

# Colors
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Step 1: Navigate to infra directory
cd "$(dirname "$0")/infra"
echo -e "${BLUE}📍 Working directory: $(pwd)${NC}"
echo ""

# Step 2: Start infrastructure
echo -e "${BLUE}📦 Starting Docker Compose services...${NC}"
docker-compose up -d redis rabbitmq seq jaeger

echo ""
echo -e "${GREEN}✅ Infrastructure services started!${NC}"
echo ""

# Step 3: Wait for services to be healthy
echo -e "${BLUE}⏳ Waiting for services to be healthy...${NC}"
sleep 5

# Check Seq
if docker-compose exec -T seq curl -s http://localhost:5341 > /dev/null 2>&1; then
  echo -e "${GREEN}✅ Seq is healthy${NC}"
else
  echo -e "${YELLOW}⚠️  Seq not ready yet (normal, might take a few seconds)${NC}"
fi

# Check Jaeger
if docker-compose exec -T jaeger curl -s http://localhost:16686 > /dev/null 2>&1; then
  echo -e "${GREEN}✅ Jaeger is healthy${NC}"
else
  echo -e "${YELLOW}⚠️  Jaeger not ready yet (normal, might take a few seconds)${NC}"
fi

echo ""
echo -e "${BLUE}🎯 Observability Dashboards:${NC}"
echo -e "  📊 Seq (Structured Logs):     ${GREEN}http://localhost:5341${NC}"
echo -e "  🔍 Jaeger (Distributed Traces): ${GREEN}http://localhost:16686${NC}"
echo ""

# Step 4: Optional - Start services
read -p "Start all microservices? (y/n) " -n 1 -r
echo ""
if [[ $REPLY =~ ^[Yy]$ ]]; then
  echo -e "${BLUE}🚀 Starting microservices...${NC}"
  docker-compose up -d auth-service billing-service generator-service admin-service image-service user-service
  echo -e "${GREEN}✅ All microservices started!${NC}"
  echo ""
  echo -e "${BLUE}📍 Service Endpoints:${NC}"
  echo -e "  🔐 Auth Service:      ${GREEN}http://localhost:5001${NC}"
  echo -e "  💳 Billing Service:   ${GREEN}http://localhost:5002${NC}"
  echo -e "  🎬 Generator Service: ${GREEN}http://localhost:5003${NC}"
  echo -e "  👨‍💼 Admin Service:      ${GREEN}http://localhost:5006${NC}"
  echo -e "  🖼️  Image Service:     ${GREEN}http://localhost:5007${NC}"
  echo -e "  👥 User Service:      ${GREEN}http://localhost:5008${NC}"
  echo ""
fi

echo -e "${BLUE}📚 Useful Commands:${NC}"
echo "  View logs:        docker-compose logs -f [service-name]"
echo "  Stop services:    docker-compose down"
echo "  Remove volumes:   docker-compose down -v"
echo ""

echo -e "${GREEN}🎉 Setup complete! Start making requests to the services.${NC}"
echo ""
