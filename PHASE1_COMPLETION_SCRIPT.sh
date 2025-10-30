#!/bin/zsh
# Phase 1 Implementation Completion Script
# Completes Admin, Image, and User services with observability setup

set -e

SERVICES_DIR="/Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly/services"

# Color codes for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}=== Phase 1 Implementation: Completing Remaining Services ===${NC}\n"

# Function to setup a service
setup_service() {
  local service_name=$1
  local service_path=$2
  
  echo -e "${BLUE}Processing $service_name...${NC}"
  
  # Create Middleware directory if it doesn't exist
  mkdir -p "$service_path/Middleware"
  
  # Create CorrelationIdMiddleware.cs
  cat > "$service_path/Middleware/CorrelationIdMiddleware.cs" << 'EOF'
using Serilog.Context;

using Serilog.Context;

namespace NAMESPACE.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private const string CorrelationIdProperty = "CorrelationId";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        context.Response.Headers[CorrelationIdHeader] = correlationId;
        using (LogContext.PushProperty(CorrelationIdProperty, correlationId))
        {
            await _next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
        {
            return correlationId.ToString();
        }
        var newCorrelationId = Guid.NewGuid().ToString("N");
        context.Items[CorrelationIdProperty] = newCorrelationId;
        return newCorrelationId;
    }
}
EOF

  # Create GlobalExceptionMiddleware.cs
  cat > "$service_path/Middleware/GlobalExceptionMiddleware.cs" << 'EOF'
using System.Net;
using System.Text.Json;
using Serilog;

namespace NAMESPACE.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            error = new
            {
                message = GetErrorMessage(exception),
                type = exception.GetType().Name,
                timestamp = DateTime.UtcNow
            }
        };
        context.Response.StatusCode = GetStatusCode(exception);
        return context.Response.WriteAsJsonAsync(response);
    }

    private static int GetStatusCode(Exception exception) =>
        exception switch
        {
            ArgumentNullException => (int)HttpStatusCode.BadRequest,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            _ => (int)HttpStatusCode.InternalServerError
        };

    private static string GetErrorMessage(Exception exception) =>
        exception switch
        {
            ArgumentNullException argNull => $"Required argument is null: {argNull.ParamName}",
            ArgumentException arg => $"Invalid argument: {arg.Message}",
            InvalidOperationException => "Operation is not valid in current context",
            UnauthorizedAccessException => "Access denied",
            KeyNotFoundException => "Resource not found",
            _ => "An internal server error occurred"
        };
}
EOF

  echo -e "${GREEN}✓ $service_name middleware created${NC}"
}

# Process Admin Service
echo -e "${BLUE}1. Setting up Admin Service...${NC}"
setup_service "AdminService" "$SERVICES_DIR/admin-service/src/AdminService"

# Process Image Service
echo -e "\n${BLUE}2. Installing packages for Image Service...${NC}"
cd "$SERVICES_DIR/image-service/src/ImageService" && \
  dotnet add package Serilog Serilog.AspNetCore Serilog.Sinks.Seq Serilog.Enrichers.Context OpenTelemetry OpenTelemetry.Exporter.Jaeger OpenTelemetry.Instrumentation.AspNetCore OpenTelemetry.Instrumentation.Http OpenTelemetry.Extensions.Hosting OpenTelemetry.Instrumentation.Runtime 2>&1 | grep -E "succeeded" | tail -1
echo -e "${GREEN}✓ Image Service packages installed${NC}"

# Process User Service
echo -e "\n${BLUE}3. Installing packages for User Service...${NC}"
cd "$SERVICES_DIR/user-service/src/UserService" && \
  dotnet add package Serilog Serilog.AspNetCore Serilog.Sinks.Seq Serilog.Enrichers.Context OpenTelemetry OpenTelemetry.Exporter.Jaeger OpenTelemetry.Instrumentation.AspNetCore OpenTelemetry.Instrumentation.Http OpenTelemetry.Extensions.Hosting OpenTelemetry.Instrumentation.Runtime 2>&1 | grep -E "succeeded" | tail -1
echo -e "${GREEN}✓ User Service packages installed${NC}"

echo -e "\n${GREEN}=== Packages Installation Complete ===${NC}"
echo -e "${BLUE}Next steps:${NC}"
echo "1. Update Program.cs files for Admin, Image, User services (copy from Generator template)"
echo "2. Run: dotnet build for each service"
echo "3. Update docker-compose.yml with Seq and Jaeger"
echo "4. Update appsettings.json files"
echo "5. Run end-to-end tests"
