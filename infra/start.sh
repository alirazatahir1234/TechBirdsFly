#!/bin/bash
# Quick start script for TechBirdsFly infrastructure

set -e

echo "🚀 TechBirdsFly Infrastructure Quick Start"
echo "=========================================="
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check prerequisites
echo "📋 Checking prerequisites..."

if ! command -v docker &> /dev/null; then
    echo -e "${RED}❌ Docker is not installed${NC}"
    exit 1
fi

if ! command -v docker-compose &> /dev/null; then
    echo -e "${RED}❌ Docker Compose is not installed${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Docker found:${NC} $(docker --version)"
echo -e "${GREEN}✓ Docker Compose found:${NC} $(docker-compose --version)"
echo ""

# Check if .env file exists
if [ ! -f .env ]; then
    echo -e "${YELLOW}⚠️  .env file not found. Creating from .env.example...${NC}"
    cp .env.example .env
    echo -e "${GREEN}✓ Created .env${NC}"
fi
echo ""

# Stop existing containers if running
if [ "$(docker-compose ps -q)" ]; then
    echo "⏹️  Stopping existing containers..."
    docker-compose down
fi
echo ""

# Create necessary directories
echo "📁 Creating directories..."
mkdir -p postgres kafka
chmod +x kafka/init-topics.sh 2>/dev/null || true
echo ""

# Start infrastructure
echo "🐳 Starting infrastructure services..."
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up -d

echo -e "${GREEN}✓ Starting services...${NC}"
echo ""

# Wait for services to be ready
echo "⏳ Waiting for services to be healthy..."
echo ""

# Function to check if service is healthy
check_service() {
    local service=$1
    local max_attempts=30
    local attempt=0
    
    while [ $attempt -lt $max_attempts ]; do
        status=$(docker-compose ps $service | grep -E "(healthy|running)" || true)
        if [ ! -z "$status" ]; then
            echo -e "${GREEN}✓${NC} $service is ready"
            return 0
        fi
        attempt=$((attempt + 1))
        sleep 1
    done
    
    echo -e "${YELLOW}⚠️${NC} $service took too long to be ready"
    return 0
}

check_service "postgres"
check_service "redis"
check_service "zookeeper"
check_service "kafka"
check_service "schema-registry"
check_service "seq"
check_service "jaeger"

echo ""
echo "📊 Creating Kafka topics..."
docker-compose exec -T kafka bash -c '
kafka-broker-api-versions.sh --bootstrap-server localhost:9092 > /dev/null 2>&1 || sleep 5

# Create topics
kafka-topics.sh --create --bootstrap-server localhost:9092 --topic user-events --partitions 3 --replication-factor 1 --if-not-exists 2>/dev/null || true
kafka-topics.sh --create --bootstrap-server localhost:9092 --topic user-registered --partitions 3 --replication-factor 1 --if-not-exists 2>/dev/null || true
kafka-topics.sh --create --bootstrap-server localhost:9092 --topic subscription-events --partitions 3 --replication-factor 1 --if-not-exists 2>/dev/null || true
kafka-topics.sh --create --bootstrap-server localhost:9092 --topic website-events --partitions 3 --replication-factor 1 --if-not-exists 2>/dev/null || true
kafka-topics.sh --create --bootstrap-server localhost:9092 --topic billing-events --partitions 3 --replication-factor 1 --if-not-exists 2>/dev/null || true

echo "Created Kafka topics"
'

echo ""
echo -e "${GREEN}════════════════════════════════════════${NC}"
echo -e "${GREEN}✅ Infrastructure is ready!${NC}"
echo -e "${GREEN}════════════════════════════════════════${NC}"
echo ""
echo "📍 Service Endpoints:"
echo ""
echo "  Database:"
echo -e "    ${YELLOW}PostgreSQL${NC}:           localhost:5432 (postgres/Alisheikh@123)"
echo ""
echo "  Event Streaming:"
echo -e "    ${YELLOW}Kafka${NC}:                localhost:9092"
echo -e "    ${YELLOW}Schema Registry${NC}:      http://localhost:8081"
echo -e "    ${YELLOW}Zookeeper${NC}:            localhost:2181"
echo ""
echo "  Caching:"
echo -e "    ${YELLOW}Redis${NC}:                localhost:6379"
echo ""
echo "  Message Queue:"
echo -e "    ${YELLOW}RabbitMQ Management${NC}:  http://localhost:15672 (guest/guest)"
echo ""
echo "  Observability:"
echo -e "    ${YELLOW}Seq${NC}:                  http://localhost:80"
echo -e "    ${YELLOW}Jaeger UI${NC}:            http://localhost:16686"
echo ""
echo "🔗 Quick Commands:"
echo ""
echo "  # View logs"
echo "    docker-compose logs -f"
echo ""
echo "  # Connect to PostgreSQL"
echo "    docker-compose exec postgres psql -U postgres"
echo ""
echo "  # List Kafka topics"
echo "    docker-compose exec kafka kafka-topics.sh --list --bootstrap-server localhost:9092"
echo ""
echo "  # Stop services"
echo "    docker-compose down"
echo ""
echo "📚 Documentation:"
echo "    See DOCKER_SETUP.md for comprehensive guide"
echo ""
