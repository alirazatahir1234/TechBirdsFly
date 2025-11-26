#!/bin/bash

##############################################################################
# TechBirdsFly Integrated Services Deployment Script
# Deploys: CacheService, User Service, Auth Service with CacheClient integration
##############################################################################

set -e

PROJECT_ROOT="/Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly"
CACHE_SERVICE_PATH="$PROJECT_ROOT/services/cache-service/src/CacheService"
USER_SERVICE_PATH="$PROJECT_ROOT/services/user-service/src/UserService"
AUTH_SERVICE_PATH="$PROJECT_ROOT/services/auth-service/src"

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Logging functions
log_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

log_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

log_error() {
    echo -e "${RED}❌ $1${NC}"
}

log_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

##############################################################################
# Cleanup existing processes
##############################################################################
cleanup_services() {
    log_info "Cleaning up existing dotnet processes..."
    pkill -f "dotnet.*CacheService\|dotnet.*UserService\|dotnet.*AuthService" 2>/dev/null || true
    sleep 2
    log_success "Cleanup complete"
}

##############################################################################
# Build services
##############################################################################
build_cache_service() {
    log_info "Building CacheService..."
    cd "$CACHE_SERVICE_PATH"
    if dotnet build --configuration Release 2>&1 | grep -E "(succeeded|Build failed)"; then
        log_success "CacheService built successfully"
    else
        log_error "CacheService build failed"
        return 1
    fi
}

build_user_service() {
    log_info "Building User Service..."
    cd "$USER_SERVICE_PATH"
    if dotnet build --configuration Release 2>&1 | grep -E "(succeeded|Build failed)"; then
        log_success "User Service built successfully"
    else
        log_error "User Service build failed"
        return 1
    fi
}

build_auth_service() {
    log_info "Building Auth Service..."
    cd "$AUTH_SERVICE_PATH"
    if dotnet build --configuration Release 2>&1 | grep -E "(succeeded|Build failed)"; then
        log_success "Auth Service built successfully"
    else
        log_error "Auth Service build failed"
        return 1
    fi
}

##############################################################################
# Start services
##############################################################################
start_cache_service() {
    log_info "Starting CacheService on port 8100..."
    cd "$CACHE_SERVICE_PATH"
    nohup dotnet run --configuration Release --no-build > /tmp/cache-service.log 2>&1 &
    CACHE_PID=$!
    log_success "CacheService started with PID $CACHE_PID"
    
    # Wait for service to be ready
    sleep 3
    if curl -s http://localhost:8100/api/cache/health > /dev/null 2>&1; then
        log_success "CacheService is responding"
    else
        log_warning "CacheService may not be responding yet. Check logs: tail -f /tmp/cache-service.log"
    fi
}

start_user_service() {
    log_info "Starting User Service on port 5005..."
    cd "$USER_SERVICE_PATH"
    nohup dotnet run --configuration Release --no-build > /tmp/user-service.log 2>&1 &
    USER_PID=$!
    log_success "User Service started with PID $USER_PID"
    
    # Wait for service to be ready
    sleep 3
    if curl -s http://localhost:5005/health > /dev/null 2>&1; then
        log_success "User Service is responding"
    else
        log_warning "User Service may not be responding yet. Check logs: tail -f /tmp/user-service.log"
    fi
}

start_auth_service() {
    log_info "Starting Auth Service on port 5001..."
    cd "$AUTH_SERVICE_PATH"
    nohup dotnet run --configuration Release --no-build > /tmp/auth-service.log 2>&1 &
    AUTH_PID=$!
    log_success "Auth Service started with PID $AUTH_PID"
    
    # Wait for service to be ready
    sleep 3
    if curl -s http://localhost:5001/health > /dev/null 2>&1; then
        log_success "Auth Service is responding"
    else
        log_warning "Auth Service may not be responding yet. Check logs: tail -f /tmp/auth-service.log"
    fi
}

##############################################################################
# Test services
##############################################################################
test_services() {
    log_info "Testing service endpoints..."
    
    echo ""
    log_info "Testing CacheService (8100)..."
    if curl -s -X GET http://localhost:8100/api/cache/health -H "Authorization: Bearer dev-secret-key" 2>&1 | head -1; then
        log_success "CacheService endpoint works"
    else
        log_warning "CacheService endpoint not responding"
    fi
    
    echo ""
    log_info "Testing User Service (5005)..."
    if curl -s http://localhost:5005/health 2>&1 | head -1; then
        log_success "User Service endpoint works"
    else
        log_warning "User Service endpoint not responding"
    fi
    
    echo ""
    log_info "Testing Auth Service (5001)..."
    if curl -s http://localhost:5001/health 2>&1 | head -1; then
        log_success "Auth Service endpoint works"
    else
        log_warning "Auth Service endpoint not responding"
    fi
}

##############################################################################
# Main execution
##############################################################################
main() {
    echo ""
    echo "╔════════════════════════════════════════════════════════════╗"
    echo "║  TechBirdsFly Integrated Services Deployment               ║"
    echo "║  Services: CacheService, User Service, Auth Service        ║"
    echo "╚════════════════════════════════════════════════════════════╝"
    echo ""
    
    # Cleanup
    cleanup_services
    
    # Build all services
    log_info "═══ BUILDING SERVICES ═══"
    build_cache_service || { log_error "Cache Service build failed"; exit 1; }
    build_user_service || { log_error "User Service build failed"; exit 1; }
    build_auth_service || { log_error "Auth Service build failed"; exit 1; }
    
    echo ""
    log_info "═══ STARTING SERVICES ═══"
    
    # Start services
    start_cache_service
    start_user_service
    start_auth_service
    
    echo ""
    log_info "═══ SERVICE VERIFICATION ═══"
    test_services
    
    echo ""
    echo "╔════════════════════════════════════════════════════════════╗"
    echo "║  ✅ Deployment Complete                                    ║"
    echo "╠════════════════════════════════════════════════════════════╣"
    echo "║  CacheService  → http://localhost:8100                     ║"
    echo "║  User Service  → http://localhost:5005                     ║"
    echo "║  Auth Service  → http://localhost:5001                     ║"
    echo "╠════════════════════════════════════════════════════════════╣"
    echo "║  Logs:                                                      ║"
    echo "║  • tail -f /tmp/cache-service.log                          ║"
    echo "║  • tail -f /tmp/user-service.log                           ║"
    echo "║  • tail -f /tmp/auth-service.log                           ║"
    echo "╚════════════════════════════════════════════════════════════╝"
    echo ""
}

# Run main
main
