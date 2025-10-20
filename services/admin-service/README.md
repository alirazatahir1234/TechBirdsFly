# Admin Service# Admin Service



Administrative dashboard, monitoring, and system management for TechBirdsFly.AI.Administrative dashboard, monitoring, and system management.



## Status## Responsibilities



✅ **Phase 1** - Fully scaffolded and operational- User management and administration

- System monitoring and health checks

## Overview- Template management

- Analytics and reporting

The Admin Service provides:- System configuration

- **Template Management** - Create, update, and manage website templates- Audit logging

- **Audit Logging** - Track all system actions and changes

- **System Settings** - Manage application configuration## API Endpoints

- **Health Monitoring** - System status endpoints

- **User Auditing** - Track user actions and changes```

GET    /api/admin/users                    - List all users

## ArchitectureDELETE /api/admin/users/{userId}           - Delete user

GET    /api/admin/analytics                - System analytics

### Data ModelsGET    /api/admin/health                   - System health

GET    /api/admin/templates                - List templates

#### TemplatePOST   /api/admin/templates                - Create template

- Id (Guid)PUT    /api/admin/templates/{templateId}   - Update template

- Name (string)DELETE /api/admin/templates/{templateId}   - Delete template

- Category (string) - portfolio, landing, blog, ecommerce, etc.GET    /api/admin/audit-logs               - Audit trail

- Description (string)```

- ThumbnailUrl (string)

- HtmlTemplate (string)## Database

- CssTemplate (string)

- IsActive (bool)- **Primary DB**: SQL Server / PostgreSQL

- Priority (int)- **Tables**: Templates, AuditLogs, AdminRoles, SystemSettings

- CreatedAt, UpdatedAt (DateTime)- **Cache**: Redis (analytics cache)



#### AuditLog## Dependencies

- Id (Guid)

- UserId (Guid?)- Auth Service (admin role validation)

- Action (string) - CREATE, UPDATE, DELETE, etc.- All services (metrics/health)

- ResourceType (string)

- ResourceId (string)## Status

- Details (string?)

- OldValues, NewValues (string? - JSON)🟡 **Phase 2** - Scaffolding ready, implementation pending

- IpAddress (string)

- UserAgent (string?)## Local Development

- CreatedAt (DateTime)

```bash

#### SystemSettingcd src

- Id (Guid)dotnet restore

- Key (string) - uniquedotnet run --urls http://localhost:5006

- Value (string)```

- Type (string) - string, int, bool, decimal

- Description (string?)## Environment Variables

- IsSecret (bool)

- CreatedAt, UpdatedAt (DateTime)```

ConnectionStrings__AdminDb=Data Source=admin.db

## API EndpointsJwt__Key=your-secret-key

Jwt__AdminRole=admin

### Health```



```## Related

GET  /api/admin/health

     Returns system health status- [Architecture](/docs/architecture.md)

```- [API Docs](/docs/README.md)


### Templates

```
GET  /api/admin/templates
     Returns all active templates

GET  /api/admin/templates/category/{category}
     Returns templates for a specific category

GET  /api/admin/templates/{id}
     Returns template details

POST /api/admin/templates
     Body: { name, category, description, thumbnailUrl, htmlTemplate, cssTemplate, priority }
     Creates new template

PUT  /api/admin/templates/{id}
     Body: { name, category, description, thumbnailUrl, htmlTemplate, cssTemplate, isActive, priority }
     Updates existing template

DELETE /api/admin/templates/{id}
     Deletes a template
```

### Audit Logs

```
GET  /api/admin/audit-logs?limit=100
     Returns recent audit logs

GET  /api/admin/audit-logs/user/{userId}?limit=50
     Returns audit logs for specific user
```

### Settings

```
GET  /api/admin/settings/{key}
     Returns setting value

POST /api/admin/settings
     Body: { key, value }
     Sets a system setting

DELETE /api/admin/settings/{key}
     Deletes a system setting
```

## Database

- **Type**: SQLite (development), SQL Server (production)
- **Tables**: Templates, AuditLogs, SystemSettings
- **Migrations**: EF Core Code-First
- **File**: `admin.db`

## Configuration

```json
{
  "ConnectionStrings": {
    "AdminDb": "Data Source=admin.db"
  },
  "Jwt": {
    "Key": "your-secret-key",
    "Issuer": "TechBirdsFly"
  }
}
```

## Local Development

### Prerequisites
- .NET 8.0 SDK
- SQLite (included with .NET)

### Setup
```bash
cd src/AdminService

dotnet restore
dotnet build
dotnet run --urls http://localhost:5006
```

### Access
- Swagger UI: http://localhost:5006/swagger
- API: http://localhost:5006/api/admin/

### Test
Use `AdminService.http` in VS Code REST Client extension:
```http
GET http://localhost:5006/api/admin/health
```

## Dependencies

### NuGet Packages
- Microsoft.EntityFrameworkCore.Sqlite 9.0.10
- Microsoft.EntityFrameworkCore.Design 9.0.10
- Microsoft.AspNetCore.Authentication.JwtBearer 8.0.8
- System.IdentityModel.Tokens.Jwt 8.14.0

### Services
- Auth Service (JWT validation)
- All services (audit logging)

## Key Features

### 1. Template Management
- Create and manage website templates
- Categorize templates (portfolio, landing, blog, etc.)
- Control template visibility with IsActive flag
- Priority ordering for display

### 2. Audit Logging
- Automatic logging of all operations
- Track user actions with UserId
- Store before/after values for changes
- Query audit trail by user or resource type

### 3. System Settings
- Store application configuration
- Support typed values (string, int, bool, decimal)
- Mark sensitive settings as secrets
- Update without code deployment

### 4. Health Checks
- Simple health status endpoint
- Returns current timestamp
- Ready for expansion with service health metrics

## Phase 2 Implementation

- [ ] User management endpoints (list, delete, ban users)
- [ ] Analytics dashboard data
- [ ] Service health integration
- [ ] Stripe webhook management
- [ ] Email notification templates
- [ ] Role-based access control (RBAC)
- [ ] Admin authentication & authorization
- [ ] Bulk template import/export
- [ ] Analytics and reporting
- [ ] Redis caching for templates

## Testing

### Example Usage Flow
```bash
# 1. Check health
curl http://localhost:5006/api/admin/health

# 2. Create template
curl -X POST http://localhost:5006/api/admin/templates \
  -H "Content-Type: application/json" \
  -d '{
    "name":"Portfolio Template",
    "category":"portfolio",
    "description":"Professional portfolio",
    "thumbnailUrl":"https://example.com/thumb.jpg",
    "htmlTemplate":"<div></div>",
    "cssTemplate":".portfolio { }",
    "priority":1
  }'

# 3. Get active templates
curl http://localhost:5006/api/admin/templates

# 4. Set system setting
curl -X POST http://localhost:5006/api/admin/settings \
  -H "Content-Type: application/json" \
  -d '{"key":"max_projects_per_user","value":"100"}'

# 5. Get audit logs
curl http://localhost:5006/api/admin/audit-logs
```

## Deployment

### Docker Build
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY src/*.csproj ./
RUN dotnet restore
COPY src/ ./
RUN dotnet publish -c Release -o publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5006
ENTRYPOINT ["dotnet", "AdminService.dll"]
```

### Environment Variables (Production)
```bash
ConnectionStrings__AdminDb=<sql-server-connection>
Jwt__Key=<your-jwt-key>
Jwt__Issuer=TechBirdsFly
ASPNETCORE_ENVIRONMENT=Production
```

## Build Status

✅ Builds successfully  
✅ Database migrations created  
✅ Service runs on port 5006  
✅ Swagger documentation available  
✅ Ready for testing

## Related Documentation

- [Main README](/README.md)
- [Architecture](/docs/architecture.md)
- [Quick Start](/QUICK_START.md)
- [Services Overview](/SERVICES_UPDATE.md)

---

**Framework**: .NET 8.0  
**Port**: 5006 (local)  
**Status**: ✅ Operational
