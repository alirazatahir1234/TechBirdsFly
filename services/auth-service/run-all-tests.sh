#!/bin/bash

# Auth Service - Comprehensive Test Execution Script
# Purpose: Run all Auth Service tests with detailed reporting
# Usage: ./run-all-tests.sh [option]
# Options: all, unit, integration, api, performance, coverage, ci

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
AUTH_SERVICE_PATH="services/auth-service/src"
TEST_RESULTS_DIR="TestResults"
COVERAGE_DIR="CoverageReports"

# Functions
print_header() {
    echo -e "\n${BLUE}════════════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}\n"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

print_info() {
    echo -e "${YELLOW}ℹ️  $1${NC}"
}

# Navigate to auth service
cd "$AUTH_SERVICE_PATH" || {
    print_error "Could not navigate to $AUTH_SERVICE_PATH"
    exit 1
}

# Create results directory
mkdir -p "$TEST_RESULTS_DIR"
mkdir -p "$COVERAGE_DIR"

# Parse command line argument
TEST_OPTION="${1:-all}"

case "$TEST_OPTION" in
    "unit")
        print_header "Running Unit Tests Only"
        dotnet test Tests/UnitTests/AuthServiceUnitTests.cs \
            --logger "trx;LogFileName=$TEST_RESULTS_DIR/unit-tests.trx" \
            --verbosity normal
        print_success "Unit tests completed"
        ;;

    "integration")
        print_header "Running Integration Tests Only"
        dotnet test Tests/IntegrationTests/ \
            --logger "trx;LogFileName=$TEST_RESULTS_DIR/integration-tests.trx" \
            --verbosity normal \
            --filter "AuthControllerIntegrationTests|AuthApiEndpointTests"
        print_success "Integration tests completed"
        ;;

    "api")
        print_header "Running API Endpoint Tests"
        dotnet test Tests/IntegrationTests/AuthApiEndpointTests.cs \
            --logger "trx;LogFileName=$TEST_RESULTS_DIR/api-tests.trx" \
            --verbosity normal
        print_success "API endpoint tests completed"
        ;;

    "performance")
        print_header "Running Performance Tests"
        print_info "These tests measure throughput, latency, and scalability..."
        dotnet test Tests/IntegrationTests/AuthServicePerformanceTests.cs \
            --logger "trx;LogFileName=$TEST_RESULTS_DIR/performance-tests.trx" \
            --verbosity detailed
        print_success "Performance tests completed"
        ;;

    "coverage")
        print_header "Running All Tests with Code Coverage"
        print_info "Generating LCOV and OpenCover coverage reports..."
        
        dotnet test \
            /p:CollectCoverage=true \
            /p:CoverageFormat=lcov \
            /p:CoverageFileName="$COVERAGE_DIR/coverage.lcov" \
            /p:CoverageFormat=opencover \
            /p:Exclude="[*Tests*]*" \
            --logger "trx;LogFileName=$TEST_RESULTS_DIR/all-tests-coverage.trx" \
            --verbosity normal
        
        print_success "Coverage report generated:"
        print_info "  LCOV: $COVERAGE_DIR/coverage.lcov"
        print_info "  OpenCover: $COVERAGE_DIR/coverage.opencover.xml"
        ;;

    "ci")
        print_header "Running Tests for CI/CD Pipeline"
        print_info "Running all tests with detailed logging and coverage..."
        
        # Build first
        print_info "Building solution..."
        dotnet build --configuration Release || {
            print_error "Build failed"
            exit 1
        }
        
        # Run all tests with coverage
        print_info "Running test suite..."
        dotnet test \
            --configuration Release \
            --no-build \
            /p:CollectCoverage=true \
            /p:CoverageFormat=lcov \
            /p:CoverageFileName="$COVERAGE_DIR/coverage.lcov" \
            --logger "trx;LogFileName=$TEST_RESULTS_DIR/ci-tests.trx" \
            --logger "console;verbosity=minimal" \
            || {
            print_error "Tests failed in CI pipeline"
            exit 1
        }
        
        print_success "CI pipeline test suite completed successfully"
        ;;

    "all")
        print_header "Running Complete Test Suite (All 40+ Tests)"
        print_info "This will run unit, integration, API, and performance tests..."
        
        # Clean previous results
        print_info "Cleaning previous test results..."
        rm -f "$TEST_RESULTS_DIR"/*.trx
        
        # Unit Tests
        print_info "\n1️⃣  Running Unit Tests (15 tests)..."
        dotnet test Tests/UnitTests/AuthServiceUnitTests.cs \
            --logger "trx;LogFileName=$TEST_RESULTS_DIR/unit-tests.trx" \
            --verbosity minimal || print_error "Unit tests failed"
        
        # Integration Tests
        print_info "\n2️⃣  Running Integration Tests (20 tests)..."
        dotnet test Tests/IntegrationTests/AuthControllerIntegrationTests.cs \
            --logger "trx;LogFileName=$TEST_RESULTS_DIR/integration-controller-tests.trx" \
            --verbosity minimal || print_error "Integration controller tests failed"
        
        # API Tests
        print_info "\n3️⃣  Running API Endpoint Tests (15 tests)..."
        dotnet test Tests/IntegrationTests/AuthApiEndpointTests.cs \
            --logger "trx;LogFileName=$TEST_RESULTS_DIR/api-endpoint-tests.trx" \
            --verbosity minimal || print_error "API endpoint tests failed"
        
        # Performance Tests
        print_info "\n4️⃣  Running Performance Tests (10 tests)..."
        dotnet test Tests/IntegrationTests/AuthServicePerformanceTests.cs \
            --logger "trx;LogFileName=$TEST_RESULTS_DIR/performance-tests.trx" \
            --verbosity minimal || print_error "Performance tests failed"
        
        print_success "All test categories completed"
        ;;

    "help"|"--help"|"-h")
        echo "Auth Service Test Runner"
        echo ""
        echo "Usage: ./run-all-tests.sh [option]"
        echo ""
        echo "Options:"
        echo "  all           Run all tests (default)"
        echo "  unit          Run unit tests only (15 tests)"
        echo "  integration   Run integration tests only (20 tests)"
        echo "  api           Run API endpoint tests only (15 tests)"
        echo "  performance   Run performance tests only (10 tests)"
        echo "  coverage      Run all tests with code coverage reports"
        echo "  ci            Run tests for CI/CD pipeline"
        echo "  help          Show this help message"
        echo ""
        echo "Examples:"
        echo "  ./run-all-tests.sh                # Run all tests"
        echo "  ./run-all-tests.sh unit           # Run unit tests"
        echo "  ./run-all-tests.sh performance    # Run performance tests"
        echo "  ./run-all-tests.sh coverage       # Run with coverage report"
        exit 0
        ;;

    *)
        print_error "Unknown option: $TEST_OPTION"
        print_info "Use './run-all-tests.sh help' for usage information"
        exit 1
        ;;
esac

# Print summary
print_header "Test Execution Summary"
echo -e "${GREEN}Test Results Location:${NC} ./$TEST_RESULTS_DIR/"
echo -e "${GREEN}Coverage Location:${NC} ./$COVERAGE_DIR/"

# Count test files
TOTAL_TESTS=$(grep -r "public async Task\|public void" Tests/ 2>/dev/null | grep "Test" | wc -l || echo "N/A")
echo -e "${GREEN}Total Test Methods:${NC} $TOTAL_TESTS"

# Final message
print_success "Test execution completed!"
print_info "Review the logs and reports in the test results directory"

exit 0
