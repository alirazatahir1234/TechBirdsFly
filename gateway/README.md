# YARP API Gateway

Centralized reverse proxy and API gateway routing all traffic to microservices.

## Responsibilities

- Request routing to services
- JWT validation and token refresh
- CORS handling
- Rate limiting
- Request/response transformation
- Load balancing
- Service discovery
- Health checks

## Route Configuration

```
/api/auth/**        → Auth Service (5001)
/api/users/**       → User Service (5002)
/api/projects/**    → Generator Service (5003)
/api/images/**      → Image Service (5004)
/api/billing/**     → Billing Service (5005)
/api/admin/**       → Admin Service (5006)
/swagger/**         → Service Swagger UIs
/health/**          → Health check endpoints
```

## Middleware Stack

1. **Authentication** - JWT bearer token validation
2. **CORS** - Cross-origin resource sharing
3. **Rate Limiting** - Request throttling per user
4. **Request Logging** - HTTP request/response logging
5. **Error Handling** - Standardized error responses
6. **Response Transformation** - Envelope wrapping
7. **Caching** - Response caching for GET requests

## Local Development

```bash
cd src
dotnet restore
dotnet run --urls http://localhost:5000
```

## Configuration

```json
{
  "Yarp": {
    "Routes": {
      "authApi": {
        "ClusterId": "auth",
        "Match": { "Path": "/api/auth/{**catch-all}" }
      },
      "generatorApi": {
        "ClusterId": "generator",
        "Match": { "Path": "/api/projects/{**catch-all}" }
      }
    },
    "Clusters": {
      "auth": {
        "Destinations": {
          "destination1": { "Address": "http://localhost:5001" }
        }
      },
      "generator": {
        "Destinations": {
          "destination1": { "Address": "http://localhost:5003" }
        }
      }
    }
  }
}
```

## Status

✅ **COMPLETE** - Production ready with JWT validation, rate limiting, and CORS

## Implementation

The YARP Gateway has been fully implemented with:
- ✅ JWT Bearer authentication
- ✅ Multi-tier rate limiting (per-user, per-IP, global)
- ✅ CORS configuration for frontend origins
- ✅ Active health monitoring of all downstream services
- ✅ Request/response logging with Serilog
- ✅ Swagger/OpenAPI documentation

**Build Status**: ✅ SUCCESS (0 errors, 0 warnings)

## Quick Start

```bash
# Navigate to gateway
cd yarp-gateway/src

# Run gateway
dotnet run --urls http://localhost:5000

# Verify
curl http://localhost:5000/health
open http://localhost:5000/swagger
```

## Documentation

- **Quick Start**: [QUICK_START.md](QUICK_START.md)
- **Implementation Details**: [GATEWAY_IMPLEMENTATION_COMPLETE.md](GATEWAY_IMPLEMENTATION_COMPLETE.md)
- **Gateway Documentation**: [yarp-gateway/README.md](yarp-gateway/README.md)
- **Test Examples**: [yarp-gateway/src/YarpGateway.http](yarp-gateway/src/YarpGateway.http)

## Related

- [Services Overview](/services/README.md)
- [Architecture](/docs/architecture.md)
- [Phase 3.2 Completion](/PHASE3_2_COMPLETION.md)
