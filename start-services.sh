#!/bin/bash
# 🚀 TechBirdsFly Complete Startup Script

echo "🚀 Starting all services..."
echo ""

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Get the base directory
BASE_DIR="/Applications/My Drive/TechBirdsFly"

# Function to start a service
start_service() {
    local service_name=$1
    local port=$2
    local path=$3
    
    echo -e "${YELLOW}Starting $service_name on port $port...${NC}"
    cd "$path"
    dotnet run --urls "http://localhost:$port" &
    echo -e "${GREEN}✓ $service_name started${NC}"
    echo ""
}

# Kill any existing processes on our ports (optional - uncomment if needed)
# lsof -ti:5000 | xargs kill -9 2>/dev/null || true
# lsof -ti:5001 | xargs kill -9 2>/dev/null || true
# etc...

echo "=========================================="
echo "Services to start:"
echo "=========================================="
echo "1. YARP Gateway (already running on 5000)"
echo "2. Auth Service (5001)"
echo "3. User Service (5008)"
echo "4. Image Service (5007)"
echo "5. Generator Service (5003)"
echo "6. Admin Service (5006)"
echo "7. React Frontend (3000)"
echo ""

# Start services
start_service "Auth Service" 5001 "$BASE_DIR/services/auth-service/src"
start_service "User Service" 5008 "$BASE_DIR/services/user-service/src"
start_service "Image Service" 5007 "$BASE_DIR/services/image-service/src"
start_service "Generator Service" 5003 "$BASE_DIR/services/generator-service/src"
start_service "Admin Service" 5006 "$BASE_DIR/services/admin-service/src"

echo "=========================================="
echo -e "${GREEN}All services started!${NC}"
echo "=========================================="
echo ""
echo "Next: Start React frontend in another terminal:"
echo "cd $BASE_DIR/web-frontend/techbirdsfly-frontend && npm start"
echo ""
echo "Then visit: http://localhost:3000"
echo ""
echo "Press Ctrl+C to stop all services"

# Wait for all background jobs
wait
