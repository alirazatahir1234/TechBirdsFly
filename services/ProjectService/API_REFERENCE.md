# Project Service - API Reference

Complete API reference with curl examples and response schemas.

## Base URL

```
Local:       http://localhost:5004
Through Gateway: http://localhost:5500/api
Production:  https://api.techbirdsfly.com
```

## Authentication

All endpoints support JWT tokens (from Auth Service):

```bash
curl -H "Authorization: Bearer <JWT_TOKEN>" http://localhost:5004/api/projects
```

## Response Format

All responses follow standard HTTP conventions:

**Success (2xx)**:
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "Project Name",
  "...": "..."
}
```

**Error (4xx/5xx)**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "The request is invalid"
}
```

---

## Project Endpoints

### Create Project

Create a new project for the authenticated user.

**Request**:
```
POST /api/projects
Content-Type: application/json
```

**Request Body**:
```json
{
  "ownerId": "550e8400-e29b-41d4-a716-446655440000",
  "name": "My Website",
  "framework": "nextjs",
  "theme": "dark",
  "description": "A modern website generator"
}
```

**Parameters**:
| Name | Type | Required | Description |
|------|------|----------|-------------|
| ownerId | UUID | Yes | Owner/User ID from Auth Service |
| name | string | Yes | Project name (1-255 chars) |
| framework | enum | Yes | `nextjs`, `react`, or `html` |
| theme | string | No | Theme name (default: null) |
| description | string | No | Project description (max 500 chars) |

**Success Response (201 Created)**:
```json
{
  "project": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "ownerId": "550e8400-e29b-41d4-a716-446655440000",
    "name": "My Website",
    "description": "A modern website generator",
    "framework": "nextjs",
    "theme": "dark",
    "createdAt": "2024-01-15T10:30:00Z",
    "versionCount": 1
  },
  "initialVersion": {
    "id": "223e4567-e89b-12d3-a456-426614174000",
    "projectId": "123e4567-e89b-12d3-a456-426614174000",
    "versionNumber": 1,
    "createdAt": "2024-01-15T10:30:00Z",
    "artifactCount": 0
  }
}
```

**Example (curl)**:
```bash
curl -X POST http://localhost:5004/api/projects \
  -H "Content-Type: application/json" \
  -d '{
    "ownerId": "550e8400-e29b-41d4-a716-446655440000",
    "name": "My Website",
    "framework": "nextjs",
    "theme": "dark",
    "description": "A modern website"
  }'
```

**Example (TypeScript/Fetch)**:
```typescript
const response = await fetch('/api/projects', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    ownerId: '550e8400-e29b-41d4-a716-446655440000',
    name: 'My Website',
    framework: 'nextjs',
    theme: 'dark',
    description: 'A modern website'
  })
});
const data = await response.json();
console.log(data.project.id); // Project ID
```

---

### Get Project

Retrieve a specific project by ID.

**Request**:
```
GET /api/projects/{projectId}
```

**Path Parameters**:
| Name | Type | Required | Description |
|------|------|----------|-------------|
| projectId | UUID | Yes | Project ID to retrieve |

**Success Response (200 OK)**:
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "ownerId": "550e8400-e29b-41d4-a716-446655440000",
  "name": "My Website",
  "description": "A modern website generator",
  "framework": "nextjs",
  "theme": "dark",
  "createdAt": "2024-01-15T10:30:00Z",
  "versionCount": 5
}
```

**Error Responses**:
- **404 Not Found**: Project does not exist

**Example (curl)**:
```bash
curl http://localhost:5004/api/projects/123e4567-e89b-12d3-a456-426614174000
```

**Example (TypeScript)**:
```typescript
const projectId = '123e4567-e89b-12d3-a456-426614174000';
const response = await fetch(`/api/projects/${projectId}`);
const project = await response.json();
console.log(project.name); // Project Name
```

---

### List User Projects

Get all projects belonging to a user.

**Request**:
```
GET /api/projects/user/{ownerId}
```

**Path Parameters**:
| Name | Type | Required | Description |
|------|------|----------|-------------|
| ownerId | UUID | Yes | User/Owner ID |

**Success Response (200 OK)**:
```json
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "ownerId": "550e8400-e29b-41d4-a716-446655440000",
    "name": "My Website",
    "description": "A modern website",
    "framework": "nextjs",
    "theme": "dark",
    "createdAt": "2024-01-15T10:30:00Z",
    "versionCount": 3
  },
  {
    "id": "323e4567-e89b-12d3-a456-426614174000",
    "ownerId": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Blog",
    "description": "Tech blog",
    "framework": "react",
    "theme": "light",
    "createdAt": "2024-01-10T14:22:00Z",
    "versionCount": 1
  }
]
```

**Example (curl)**:
```bash
curl http://localhost:5004/api/projects/user/550e8400-e29b-41d4-a716-446655440000
```

**Example (TypeScript)**:
```typescript
const ownerId = '550e8400-e29b-41d4-a716-446655440000';
const response = await fetch(`/api/projects/user/${ownerId}`);
const projects = await response.json();
projects.forEach(p => console.log(p.name));
```

---

### Rename Project

Update project name.

**Request**:
```
PUT /api/projects/{projectId}/rename
Content-Type: application/json
```

**Path Parameters**:
| Name | Type | Required |
|------|------|----------|
| projectId | UUID | Yes |

**Request Body**:
```json
{
  "newName": "Updated Project Name"
}
```

**Success Response (200 OK)**:
```
(Empty response body)
```

**Error Responses**:
- **404 Not Found**: Project does not exist
- **400 Bad Request**: Invalid project name

**Example (curl)**:
```bash
curl -X PUT http://localhost:5004/api/projects/123e4567-e89b-12d3-a456-426614174000/rename \
  -H "Content-Type: application/json" \
  -d '{"newName":"New Name"}'
```

---

### Update Project Settings

Update project description, framework, or theme.

**Request**:
```
PUT /api/projects/{projectId}/settings
Content-Type: application/json
```

**Path Parameters**:
| Name | Type | Required |
|------|------|----------|
| projectId | UUID | Yes |

**Request Body** (all optional):
```json
{
  "description": "Updated description",
  "framework": "react",
  "theme": "dark"
}
```

**Parameters**:
| Name | Type | Description |
|------|------|-------------|
| description | string | New description (max 500 chars) |
| framework | enum | `nextjs`, `react`, or `html` |
| theme | string | Theme name |

**Success Response (200 OK)**:
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "ownerId": "550e8400-e29b-41d4-a716-446655440000",
  "name": "My Website",
  "description": "Updated description",
  "framework": "react",
  "theme": "dark",
  "createdAt": "2024-01-15T10:30:00Z",
  "versionCount": 5
}
```

**Example (curl)**:
```bash
curl -X PUT http://localhost:5004/api/projects/123e4567-e89b-12d3-a456-426614174000/settings \
  -H "Content-Type: application/json" \
  -d '{
    "framework": "react",
    "theme": "dark"
  }'
```

---

### Delete Project

Delete a project and all associated versions/artifacts.

**Request**:
```
DELETE /api/projects/{projectId}
```

**Path Parameters**:
| Name | Type | Required |
|------|------|----------|
| projectId | UUID | Yes |

**Success Response (200 OK)**:
```
(Empty response body)
```

**Error Responses**:
- **404 Not Found**: Project does not exist

**Example (curl)**:
```bash
curl -X DELETE http://localhost:5004/api/projects/123e4567-e89b-12d3-a456-426614174000
```

**⚠️ WARNING**: This permanently deletes the project and all versions!

---

## Version Endpoints

### Create Version

Create a new version of a project (auto-incremented version number).

**Request**:
```
POST /api/projects/{projectId}/versions
```

**Path Parameters**:
| Name | Type | Required |
|------|------|----------|
| projectId | UUID | Yes |

**Success Response (201 Created)**:
```json
{
  "id": "323e4567-e89b-12d3-a456-426614174000",
  "projectId": "123e4567-e89b-12d3-a456-426614174000",
  "versionNumber": 2,
  "createdAt": "2024-01-15T11:45:00Z",
  "artifactCount": 0
}
```

**Error Responses**:
- **404 Not Found**: Project does not exist
- **400 Bad Request**: Invalid project

**Example (curl)**:
```bash
curl -X POST http://localhost:5004/api/projects/123e4567-e89b-12d3-a456-426614174000/versions
```

**Example (TypeScript)**:
```typescript
const response = await fetch('/api/projects/{projectId}/versions', {
  method: 'POST'
});
const version = await response.json();
console.log(version.versionNumber); // 2, 3, 4, etc.
```

---

### List Project Versions

Get all versions of a project.

**Request**:
```
GET /api/projects/{projectId}/versions
```

**Path Parameters**:
| Name | Type | Required |
|------|------|----------|
| projectId | UUID | Yes |

**Success Response (200 OK)**:
```json
[
  {
    "id": "423e4567-e89b-12d3-a456-426614174000",
    "projectId": "123e4567-e89b-12d3-a456-426614174000",
    "versionNumber": 3,
    "createdAt": "2024-01-15T12:00:00Z",
    "artifactCount": 2
  },
  {
    "id": "323e4567-e89b-12d3-a456-426614174000",
    "projectId": "123e4567-e89b-12d3-a456-426614174000",
    "versionNumber": 2,
    "createdAt": "2024-01-15T11:45:00Z",
    "artifactCount": 1
  },
  {
    "id": "223e4567-e89b-12d3-a456-426614174000",
    "projectId": "123e4567-e89b-12d3-a456-426614174000",
    "versionNumber": 1,
    "createdAt": "2024-01-15T10:30:00Z",
    "artifactCount": 0
  }
]
```

**Example (curl)**:
```bash
curl http://localhost:5004/api/projects/123e4567-e89b-12d3-a456-426614174000/versions
```

---

## Artifact Endpoints

### Link Artifact

Link an artifact from GeneratorService to a project version.

**Request**:
```
POST /api/projects/versions/link-artifact
Content-Type: application/json
```

**Request Body**:
```json
{
  "versionId": "323e4567-e89b-12d3-a456-426614174000",
  "artifactId": "523e4567-e89b-12d3-a456-426614174000",
  "type": "generated_page"
}
```

**Parameters**:
| Name | Type | Required | Description |
|------|------|----------|-------------|
| versionId | UUID | Yes | Project version ID |
| artifactId | UUID | Yes | GeneratorService artifact ID |
| type | string | Yes | Artifact type (e.g., "generated_page") |

**Success Response (200 OK)**:
```
(Empty response body)
```

**Error Responses**:
- **400 Bad Request**: Invalid version or artifact
- **404 Not Found**: Version does not exist

**Example (curl)**:
```bash
curl -X POST http://localhost:5004/api/projects/versions/link-artifact \
  -H "Content-Type: application/json" \
  -d '{
    "versionId": "323e4567-e89b-12d3-a456-426614174000",
    "artifactId": "523e4567-e89b-12d3-a456-426614174000",
    "type": "generated_page"
  }'
```

---

## Health & Status

### Health Check

Check service health status.

**Request**:
```
GET /health
```

**Success Response (200 OK)**:
```
Healthy
```

**Example (curl)**:
```bash
curl http://localhost:5004/health
```

---

## Common Status Codes

| Code | Meaning | Example |
|------|---------|---------|
| **200** | OK | GET successful, DELETE successful |
| **201** | Created | POST successful |
| **400** | Bad Request | Invalid input data |
| **404** | Not Found | Project/version doesn't exist |
| **409** | Conflict | Duplicate name (if added) |
| **500** | Server Error | Database error, unexpected exception |

---

## Error Examples

### 404 Not Found
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Project not found"
}
```

### 400 Bad Request
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Invalid request body"
}
```

---

## Rate Limiting

Currently: **No rate limiting** (development)

For production, consider:
- Per-user rate limit: 100 requests/minute
- Per-IP rate limit: 1000 requests/minute
- Per-endpoint customization

---

## Pagination (Future)

Coming soon: Add `page` and `pageSize` to list endpoints.

```bash
# Future: List with pagination
curl "http://localhost:5004/api/projects/user/{ownerId}?page=1&pageSize=10"
```

---

## Filtering & Sorting (Future)

Coming soon:

```bash
# Future: Filter by framework
curl "http://localhost:5004/api/projects/user/{ownerId}?framework=nextjs"

# Future: Sort by date
curl "http://localhost:5004/api/projects/user/{ownerId}?sort=createdAt&order=desc"
```

---

## Testing Tools

### Swagger UI
```
http://localhost:5004/swagger
```
Interactive API documentation and testing.

### Postman
1. Import all endpoints to Postman
2. Set collection variables: `projectId`, `versionId`
3. Run requests in sequence
4. Save responses for testing

### cURL
See examples above for each endpoint.

### TypeScript/Fetch
```typescript
// Helper for API calls
async function apiCall(method: string, path: string, body?: any) {
  const response = await fetch(`/api${path}`, {
    method,
    headers: { 'Content-Type': 'application/json' },
    body: body ? JSON.stringify(body) : undefined
  });
  if (!response.ok) throw new Error(`API error: ${response.status}`);
  return response.json();
}

// Usage
const project = await apiCall('POST', '/projects', {
  ownerId: '...',
  name: 'Test',
  framework: 'nextjs'
});
```

---

## Integration Checklist

- [ ] Service running on port 5004
- [ ] Health check returns 200
- [ ] Can create project via POST /api/projects
- [ ] Can retrieve project via GET /api/projects/{id}
- [ ] Can list projects via GET /api/projects/user/{ownerId}
- [ ] Can create version via POST /api/projects/{id}/versions
- [ ] Can link artifact via POST /api/projects/versions/link-artifact
- [ ] All endpoints return proper status codes
- [ ] Swagger UI accessible at /swagger
- [ ] Database auto-migrations run successfully

---

## Support

For more information:
- **README.md** - Architecture and design
- **QUICK_START.md** - Local setup
- **INTEGRATION.md** - Gateway & Frontend integration
- **API Docs** - Swagger at http://localhost:5004/swagger

---

**Last Updated**: January 2024
**API Version**: 1.0
**Status**: Production Ready ✅
