@echo off
REM Auth Service - Comprehensive Test Execution Script (Windows)
REM Purpose: Run all Auth Service tests with detailed reporting
REM Usage: run-all-tests.bat [option]
REM Options: all, unit, integration, api, performance, coverage, ci

setlocal enabledelayedexpansion

REM Configuration
set AUTH_SERVICE_PATH=services\auth-service\src
set TEST_RESULTS_DIR=TestResults
set COVERAGE_DIR=CoverageReports

REM Parse command line argument
set TEST_OPTION=%1
if "%TEST_OPTION%"=="" set TEST_OPTION=all

REM Navigate to auth service
cd /d "%AUTH_SERVICE_PATH%" || (
    echo.
    echo [ERROR] Could not navigate to %AUTH_SERVICE_PATH%
    exit /b 1
)

REM Create results directories
if not exist "%TEST_RESULTS_DIR%" mkdir "%TEST_RESULTS_DIR%"
if not exist "%COVERAGE_DIR%" mkdir "%COVERAGE_DIR%"

echo.
echo ================================================================
echo Auth Service - Comprehensive Test Suite
echo ================================================================
echo.

if "%TEST_OPTION%"=="unit" (
    echo Running Unit Tests Only...
    echo.
    dotnet test Tests\UnitTests\AuthServiceUnitTests.cs ^
        --logger "trx;LogFileName=%TEST_RESULTS_DIR%\unit-tests.trx" ^
        --verbosity normal
    echo.
    echo [SUCCESS] Unit tests completed
    goto end

) else if "%TEST_OPTION%"=="integration" (
    echo Running Integration Tests Only...
    echo.
    dotnet test Tests\IntegrationTests\ ^
        --logger "trx;LogFileName=%TEST_RESULTS_DIR%\integration-tests.trx" ^
        --verbosity normal ^
        --filter "AuthControllerIntegrationTests or AuthApiEndpointTests"
    echo.
    echo [SUCCESS] Integration tests completed
    goto end

) else if "%TEST_OPTION%"=="api" (
    echo Running API Endpoint Tests...
    echo.
    dotnet test Tests\IntegrationTests\AuthApiEndpointTests.cs ^
        --logger "trx;LogFileName=%TEST_RESULTS_DIR%\api-tests.trx" ^
        --verbosity normal
    echo.
    echo [SUCCESS] API endpoint tests completed
    goto end

) else if "%TEST_OPTION%"=="performance" (
    echo Running Performance Tests...
    echo These tests measure throughput, latency, and scalability...
    echo.
    dotnet test Tests\IntegrationTests\AuthServicePerformanceTests.cs ^
        --logger "trx;LogFileName=%TEST_RESULTS_DIR%\performance-tests.trx" ^
        --verbosity detailed
    echo.
    echo [SUCCESS] Performance tests completed
    goto end

) else if "%TEST_OPTION%"=="coverage" (
    echo Running All Tests with Code Coverage...
    echo Generating LCOV and OpenCover coverage reports...
    echo.
    dotnet test ^
        /p:CollectCoverage=true ^
        /p:CoverageFormat=lcov ^
        /p:CoverageFileName=%COVERAGE_DIR%\coverage.lcov ^
        /p:CoverageFormat=opencover ^
        /p:Exclude="[*Tests*]*" ^
        --logger "trx;LogFileName=%TEST_RESULTS_DIR%\all-tests-coverage.trx" ^
        --verbosity normal
    echo.
    echo [SUCCESS] Coverage report generated
    echo   LCOV: %COVERAGE_DIR%\coverage.lcov
    echo   OpenCover: %COVERAGE_DIR%\coverage.opencover.xml
    goto end

) else if "%TEST_OPTION%"=="ci" (
    echo Running Tests for CI/CD Pipeline...
    echo Running all tests with detailed logging and coverage...
    echo.
    
    echo Building solution (Release)...
    dotnet build --configuration Release
    if errorlevel 1 (
        echo [ERROR] Build failed
        exit /b 1
    )
    
    echo Running test suite...
    dotnet test ^
        --configuration Release ^
        --no-build ^
        /p:CollectCoverage=true ^
        /p:CoverageFormat=lcov ^
        /p:CoverageFileName=%COVERAGE_DIR%\coverage.lcov ^
        --logger "trx;LogFileName=%TEST_RESULTS_DIR%\ci-tests.trx" ^
        --logger "console;verbosity=minimal"
    
    if errorlevel 1 (
        echo [ERROR] Tests failed in CI pipeline
        exit /b 1
    )
    
    echo.
    echo [SUCCESS] CI pipeline test suite completed successfully
    goto end

) else if "%TEST_OPTION%"=="help" (
    goto help

) else if "%TEST_OPTION%"=="--help" (
    goto help

) else if "%TEST_OPTION%"=="-h" (
    goto help

) else if "%TEST_OPTION%"=="all" (
    echo Running Complete Test Suite (All 40+ Tests)...
    echo This will run unit, integration, API, and performance tests...
    echo.
    
    REM Clean previous results
    echo Cleaning previous test results...
    del /q "%TEST_RESULTS_DIR%\*.trx" 2>nul
    
    REM Unit Tests
    echo.
    echo [1/4] Running Unit Tests (15 tests)...
    dotnet test Tests\UnitTests\AuthServiceUnitTests.cs ^
        --logger "trx;LogFileName=%TEST_RESULTS_DIR%\unit-tests.trx" ^
        --verbosity minimal
    
    REM Integration Tests
    echo.
    echo [2/4] Running Integration Tests (20 tests)...
    dotnet test Tests\IntegrationTests\AuthControllerIntegrationTests.cs ^
        --logger "trx;LogFileName=%TEST_RESULTS_DIR%\integration-controller-tests.trx" ^
        --verbosity minimal
    
    REM API Tests
    echo.
    echo [3/4] Running API Endpoint Tests (15 tests)...
    dotnet test Tests\IntegrationTests\AuthApiEndpointTests.cs ^
        --logger "trx;LogFileName=%TEST_RESULTS_DIR%\api-endpoint-tests.trx" ^
        --verbosity minimal
    
    REM Performance Tests
    echo.
    echo [4/4] Running Performance Tests (10 tests)...
    dotnet test Tests\IntegrationTests\AuthServicePerformanceTests.cs ^
        --logger "trx;LogFileName=%TEST_RESULTS_DIR%\performance-tests.trx" ^
        --verbosity minimal
    
    echo.
    echo [SUCCESS] All test categories completed
    goto end

) else (
    echo [ERROR] Unknown option: %TEST_OPTION%
    echo Use 'run-all-tests.bat help' for usage information
    exit /b 1
)

:help
echo Auth Service Test Runner
echo.
echo Usage: run-all-tests.bat [option]
echo.
echo Options:
echo   all           Run all tests (default)
echo   unit          Run unit tests only (15 tests)
echo   integration   Run integration tests only (20 tests)
echo   api           Run API endpoint tests only (15 tests)
echo   performance   Run performance tests only (10 tests)
echo   coverage      Run all tests with code coverage reports
echo   ci            Run tests for CI/CD pipeline
echo   help          Show this help message
echo.
echo Examples:
echo   run-all-tests.bat                ^REM Run all tests
echo   run-all-tests.bat unit           ^REM Run unit tests
echo   run-all-tests.bat performance    ^REM Run performance tests
echo   run-all-tests.bat coverage       ^REM Run with coverage report
echo.
goto end

:end
echo.
echo ================================================================
echo Test Execution Summary
echo ================================================================
echo Test Results Location: .\%TEST_RESULTS_DIR%\
echo Coverage Location: .\%COVERAGE_DIR%\
echo.
echo [SUCCESS] Test execution completed!
echo Review the logs and reports in the test results directory
echo.

cd /d ..\..\..
exit /b 0
