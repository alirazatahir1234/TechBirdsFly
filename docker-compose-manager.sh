#!/bin/bash

# ============================================================================
# TechBirdsFly - Docker Compose Management Script
# ============================================================================
# Usage:
#   ./docker-compose-manager.sh [command] [options]
#
# Commands:
#   up              Start all containers
#   down            Stop all containers
#   logs            View container logs
#   build           Build all images
#   rebuild         Rebuild all images (no cache)
#   clean           Remove all containers and volumes
#   status          Show container status
#   help            Show this help message
# ============================================================================

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
DOCKER_DIR="$SCRIPT_DIR/docker"

# Docker Compose file
COMPOSE_FILE="$DOCKER_DIR/docker-compose.debug.yml"

echo -e "${BLUE}╔════════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║     TechBirdsFly - Docker Compose Management                   ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════════════╝${NC}"
echo ""

# Show help
show_help() {
    echo -e "${GREEN}Available commands:${NC}"
    echo ""
    echo "  ${BLUE}up${NC}          Start all containers (infrastructure + services + frontend)"
    echo "  ${BLUE}down${NC}        Stop all running containers"
    echo "  ${BLUE}logs${NC}        View logs (use: logs [service-name] for specific service)"
    echo "  ${BLUE}build${NC}       Build all Docker images"
    echo "  ${BLUE}rebuild${NC}     Rebuild all images without cache"
    echo "  ${BLUE}ps${NC}          List running containers"
    echo "  ${BLUE}status${NC}      Show health status of all services"
    echo "  ${BLUE}clean${NC}       Remove all containers, images, and volumes (WARNING: destructive)"
    echo "  ${BLUE}help${NC}        Show this help message"
    echo ""
    echo -e "${YELLOW}Examples:${NC}"
    echo "  ./docker-compose-manager.sh up"
    echo "  ./docker-compose-manager.sh logs auth-service"
    echo "  ./docker-compose-manager.sh ps"
    echo "  ./docker-compose-manager.sh status"
    echo ""
}

# Start all containers
start_all() {
    echo -e "${YELLOW}→ Starting all TechBirdsFly services...${NC}"
    echo ""
    
    cd "$SCRIPT_DIR"
    
    docker-compose -f "$COMPOSE_FILE" up -d
    
    echo ""
    echo -e "${GREEN}✓ All services started!${NC}"
    echo ""
    echo -e "${BLUE}Service endpoints:${NC}"
    echo "  🔐 Auth Service:       http://localhost:5001"
    echo "  👤 User Service:       http://localhost:5008"
    echo "  💳 Billing Service:    http://localhost:5002"
    echo "  📨 Event Bus Service:  http://localhost:5020"
    echo "  ⚙️  Generator Service:  http://localhost:5003"
    echo "  🖼  Image Service:      http://localhost:5007"
    echo "  🛠  Admin Service:      http://localhost:5006"
    echo "  📤 Export Service:     http://localhost:5004"
    echo "  📁 Project Service:    http://localhost:5009"
    echo "  ⚡ Cache Service:      http://localhost:5021"
    echo "  🎬 Media Service:      http://localhost:5022"
    echo "  🚪 API Gateway:        http://localhost:9000"
    echo "  🌐 Frontend:           http://localhost:3000"
    echo ""
    echo -e "${BLUE}Observability:${NC}"
    echo "  📊 Seq Logs:           http://localhost:5341"
    echo "  🔍 Jaeger Traces:      http://localhost:16686"
    echo ""
    echo -e "${YELLOW}Tip: Run './docker-compose-manager.sh logs' to view all logs${NC}"
}

# Stop all containers
stop_all() {
    echo -e "${YELLOW}→ Stopping all TechBirdsFly services...${NC}"
    cd "$SCRIPT_DIR"
    docker-compose -f "$COMPOSE_FILE" down
    echo -e "${GREEN}✓ All services stopped!${NC}"
}

# View logs
view_logs() {
    local service=$1
    cd "$SCRIPT_DIR"
    
    if [ -z "$service" ]; then
        echo -e "${YELLOW}→ Showing logs for all services...${NC}"
        echo -e "${YELLOW}Tip: Press Ctrl+C to exit${NC}"
        docker-compose -f "$COMPOSE_FILE" logs -f
    else
        echo -e "${YELLOW}→ Showing logs for $service...${NC}"
        docker-compose -f "$COMPOSE_FILE" logs -f "$service"
    fi
}

# Build images
build_images() {
    echo -e "${YELLOW}→ Building Docker images...${NC}"
    cd "$SCRIPT_DIR"
    docker-compose -f "$COMPOSE_FILE" build
    echo -e "${GREEN}✓ Images built successfully!${NC}"
}

# Rebuild images
rebuild_images() {
    echo -e "${YELLOW}→ Rebuilding Docker images (no cache)...${NC}"
    cd "$SCRIPT_DIR"
    docker-compose -f "$COMPOSE_FILE" build --no-cache
    echo -e "${GREEN}✓ Images rebuilt successfully!${NC}"
}

# Show container list
show_ps() {
    echo -e "${YELLOW}→ Running containers:${NC}"
    echo ""
    cd "$SCRIPT_DIR"
    docker-compose -f "$COMPOSE_FILE" ps
}

# Show service health status
show_status() {
    echo -e "${YELLOW}→ Service Health Status:${NC}"
    echo ""
    cd "$SCRIPT_DIR"
    
    services=(
        "postgres"
        "kafka"
        "redis"
        "mongodb"
        "seq"
        "jaeger"
        "auth-service"
        "user-service"
        "billing-service"
        "event-bus-service"
        "generator-service"
        "admin-service"
        "image-service"
        "export-service"
        "project-service"
        "cache-service"
        "media-service"
        "api-gateway"
        "frontend"
    )
    
    for service in "${services[@]}"; do
        container=$(docker-compose -f "$COMPOSE_FILE" ps -q "$service" 2>/dev/null || echo "")
        
        if [ -z "$container" ]; then
            echo -e "  ${RED}✗${NC} $service (not running)"
        else
            status=$(docker inspect --format='{{.State.Status}}' "$container" 2>/dev/null || echo "unknown")
            health=$(docker inspect --format='{{.State.Health.Status}}' "$container" 2>/dev/null || echo "no health check")
            
            if [ "$status" = "running" ]; then
                if [ "$health" = "healthy" ]; then
                    echo -e "  ${GREEN}✓${NC} $service (healthy)"
                elif [ "$health" = "unhealthy" ]; then
                    echo -e "  ${RED}✗${NC} $service (unhealthy)"
                else
                    echo -e "  ${YELLOW}⋯${NC} $service (starting...)"
                fi
            else
                echo -e "  ${RED}✗${NC} $service ($status)"
            fi
        fi
    done
    echo ""
}

# Clean everything
clean_all() {
    echo -e "${RED}WARNING: This will remove all containers, images, and volumes!${NC}"
    echo -n "Are you sure? (y/n) "
    read -r response
    
    if [ "$response" != "y" ] && [ "$response" != "Y" ]; then
        echo -e "${YELLOW}Cancelled.${NC}"
        return
    fi
    
    echo -e "${YELLOW}→ Removing all services...${NC}"
    cd "$SCRIPT_DIR"
    docker-compose -f "$COMPOSE_FILE" down -v
    
    echo -e "${YELLOW}→ Removing images...${NC}"
    docker-compose -f "$COMPOSE_FILE" down --rmi all
    
    echo -e "${GREEN}✓ Cleanup complete!${NC}"
}

# Main command handler
case "${1:-help}" in
    up)
        start_all
        ;;
    down)
        stop_all
        ;;
    logs)
        view_logs "$2"
        ;;
    build)
        build_images
        ;;
    rebuild)
        rebuild_images
        ;;
    ps)
        show_ps
        ;;
    status)
        show_status
        ;;
    clean)
        clean_all
        ;;
    help|*)
        show_help
        ;;
esac
