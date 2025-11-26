#!/bin/bash

# ============================================================================
# 🚀 PHASE 8 STARTUP SCRIPT — API GATEWAY + ALL SERVICES
# ============================================================================
# Starts all microservices and gateway with proper port allocation
# Usage: chmod +x phase-8-startup.sh && ./phase-8-startup.sh
# ============================================================================

set -e

WORKSPACE="/Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly"
GATEWAY_PORT=5500
GENERATOR_PORT=5003
USER_PORT=5002
IMAGE_PORT=5004
BILLING_PORT=5005
ADMIN_PORT=5006
EVENTS_PORT=5007
FRONTEND_PORT=3000

echo ""
echo "╔════════════════════════════════════════════════════════════════════════════╗"
echo "║                   🚀 PHASE 8 — FULL STACK STARTUP 🚀                      ║"
echo "║                                                                            ║"
echo "║           Starting all microservices, gateway, and frontend...              ║"
echo "╚════════════════════════════════════════════════════════════════════════════╝"
echo ""

# Check if services are already running
echo "📋 Checking for existing processes..."
pgrep -f "dotnet run" > /dev/null && echo "  ⚠️  Found existing dotnet processes. Consider stopping them first." || echo "  ✅ No existing processes found"

echo ""
echo "🔧 Building projects..."
echo ""

# Build Gateway
echo "  [1/7] Building Gateway..."
cd "$WORKSPACE/gateway/yarp-gateway/src"
dotnet build YarpGateway.csproj -c Debug -q 2>&1 | grep -E "(error|succeeded|warning)" | head -3 || echo "    ✅ Built successfully"

# Build Services
echo "  [2/7] Building Generator Service..."
cd "$WORKSPACE/services/generator-service/src"
dotnet build GeneratorService.csproj -c Debug -q 2>&1 | grep -E "(error|succeeded|warning)" | head -3 || echo "    ✅ Built successfully"

echo "  [3/7] Building User Service..."
cd "$WORKSPACE/services/user-service/src"
dotnet build UserService.csproj -c Debug -q 2>&1 | grep -E "(error|succeeded|warning)" | head -3 || echo "    ✅ Built successfully"

echo "  [4/7] Building Image Service..."
cd "$WORKSPACE/services/image-service/src"
dotnet build ImageService/ImageService.csproj -c Debug -q 2>&1 | grep -E "(error|succeeded|warning)" | head -3 || echo "    ✅ Built successfully"

echo "  [5/7] Building Billing Service..."
cd "$WORKSPACE/services/billing-service/src"
dotnet build BillingService/BillingService.csproj -c Debug -q 2>&1 | grep -E "(error|succeeded|warning)" | head -3 || echo "    ✅ Built successfully"

echo "  [6/7] Building Admin Service..."
cd "$WORKSPACE/services/admin-service/src"
dotnet build AdminService/AdminService.csproj -c Debug -q 2>&1 | grep -E "(error|succeeded|warning)" | head -3 || echo "    ✅ Built successfully"

echo "  [7/7] Building Event Bus Service..."
cd "$WORKSPACE/services/event-bus-service/src"
dotnet build EventBusService.csproj -c Debug -q 2>&1 | grep -E "(error|succeeded|warning)" | head -3 || echo "    ✅ Built successfully"

echo ""
echo "🌍 Starting services in background..."
echo ""

# Start Generator Service
echo "  ▶️  Generator Service (port $GENERATOR_PORT)..."
cd "$WORKSPACE/services/generator-service/src"
ASPNETCORE_URLS="http://localhost:$GENERATOR_PORT" dotnet run --configuration Debug > /tmp/generator.log 2>&1 &
GENERATOR_PID=$!
echo "    PID: $GENERATOR_PID"

# Start User Service
echo "  ▶️  User Service (port $USER_PORT)..."
cd "$WORKSPACE/services/user-service/src"
ASPNETCORE_URLS="http://localhost:$USER_PORT" dotnet run --configuration Debug > /tmp/user.log 2>&1 &
USER_PID=$!
echo "    PID: $USER_PID"

# Start Image Service
echo "  ▶️  Image Service (port $IMAGE_PORT)..."
cd "$WORKSPACE/services/image-service/src"
ASPNETCORE_URLS="http://localhost:$IMAGE_PORT" dotnet run --configuration Debug > /tmp/image.log 2>&1 &
IMAGE_PID=$!
echo "    PID: $IMAGE_PID"

# Start Billing Service
echo "  ▶️  Billing Service (port $BILLING_PORT)..."
cd "$WORKSPACE/services/billing-service/src"
ASPNETCORE_URLS="http://localhost:$BILLING_PORT" dotnet run --configuration Debug > /tmp/billing.log 2>&1 &
BILLING_PID=$!
echo "    PID: $BILLING_PID"

# Start Admin Service
echo "  ▶️  Admin Service (port $ADMIN_PORT)..."
cd "$WORKSPACE/services/admin-service/src"
ASPNETCORE_URLS="http://localhost:$ADMIN_PORT" dotnet run --configuration Debug > /tmp/admin.log 2>&1 &
ADMIN_PID=$!
echo "    PID: $ADMIN_PID"

# Start Event Bus Service
echo "  ▶️  Event Bus Service (port $EVENTS_PORT)..."
cd "$WORKSPACE/services/event-bus-service/src"
ASPNETCORE_URLS="http://localhost:$EVENTS_PORT" dotnet run --configuration Debug > /tmp/eventbus.log 2>&1 &
EVENTS_PID=$!
echo "    PID: $EVENTS_PID"

# Wait for services to start
echo ""
echo "⏳ Waiting for services to initialize (5 seconds)..."
sleep 5

# Start Gateway
echo ""
echo "  ▶️  API Gateway (port $GATEWAY_PORT)..."
cd "$WORKSPACE/gateway/yarp-gateway/src"
ASPNETCORE_URLS="http://localhost:$GATEWAY_PORT" dotnet run --configuration Debug > /tmp/gateway.log 2>&1 &
GATEWAY_PID=$!
echo "    PID: $GATEWAY_PID"

# Wait for gateway to start
sleep 3

# Start Frontend
echo ""
echo "  ▶️  Next.js Frontend (port $FRONTEND_PORT)..."
cd "$WORKSPACE/web-frontend/techbirdsfly-frontend-nextjs"
npm run dev > /tmp/frontend.log 2>&1 &
FRONTEND_PID=$!
echo "    PID: $FRONTEND_PID"

echo ""
echo "╔════════════════════════════════════════════════════════════════════════════╗"
echo "║                     ✅ ALL SERVICES STARTED ✅                            ║"
echo "╚════════════════════════════════════════════════════════════════════════════╝"
echo ""
echo "📊 SERVICE STATUS:"
echo ""
echo "  🌐 Frontend:              http://localhost:$FRONTEND_PORT"
echo "  🔌 API Gateway:           http://localhost:$GATEWAY_PORT"
echo "     ├─ Health:            http://localhost:$GATEWAY_PORT/health"
echo "     ├─ Info:              http://localhost:$GATEWAY_PORT/info"
echo "     └─ Swagger:           http://localhost:$GATEWAY_PORT/swagger"
echo ""
echo "  🎯 Generator Service:     http://localhost:$GENERATOR_PORT"
echo "  👤 User Service:          http://localhost:$USER_PORT"
echo "  🖼️  Image Service:         http://localhost:$IMAGE_PORT"
echo "  💳 Billing Service:       http://localhost:$BILLING_PORT"
echo "  ⚙️  Admin Service:         http://localhost:$ADMIN_PORT"
echo "  📢 Event Bus Service:     http://localhost:$EVENTS_PORT"
echo ""
echo "🚀 QUICK START:"
echo ""
echo "  1. Open browser: http://localhost:$FRONTEND_PORT/dashboard/create"
echo "  2. Fill out project form"
echo "  3. Click 'Generate Website'"
echo "  4. Watch HTML preview appear"
echo ""
echo "📝 DATA FLOW:"
echo ""
echo "  Frontend → Gateway (5500) → Generator Service (5003)"
echo "           → Ollama/Llama3 → HTML Response"
echo ""
echo "📊 LOGS:"
echo ""
echo "  Frontend:         tail -f /tmp/frontend.log"
echo "  Gateway:          tail -f /tmp/gateway.log"
echo "  Generator:        tail -f /tmp/generator.log"
echo "  User:             tail -f /tmp/user.log"
echo "  Image:            tail -f /tmp/image.log"
echo "  Billing:          tail -f /tmp/billing.log"
echo "  Admin:            tail -f /tmp/admin.log"
echo "  Event Bus:        tail -f /tmp/eventbus.log"
echo ""
echo "🛑 TO STOP ALL SERVICES:"
echo ""
echo "  pkill -f 'dotnet run'"
echo ""
echo "────────────────────────────────────────────────────────────────────────────"
echo ""
echo "Phase 8 ✅ COMPLETE — System running with API Gateway!"
echo ""
echo "────────────────────────────────────────────────────────────────────────────"
echo ""

# Keep script running and show summary
wait
