# 📖 GENERATOR SERVICE API REFERENCE

**Complete API endpoint documentation with examples**

---

## Table of Contents

1. [Overview](#overview)
2. [Authentication](#authentication)
3. [Base URL](#base-url)
4. [Projects Endpoints](#projects-endpoints)
5. [Response Formats](#response-formats)
6. [Error Codes](#error-codes)
7. [Examples](#examples)

---

## 🎯 Overview

The Generator API provides endpoints for creating, managing, and downloading AI-generated websites.

**Base URL:** `http://localhost:5500/generator/api`  
**Frontend Proxy:** `http://localhost:3000/api/generator`  
**Method:** REST with JSON

---

## 🔐 Authentication

All requests require user identification:

```http
Authorization: Bearer {jwt_token}
X-User-Id: {user_id_uuid}
```

**Currently:** Using demo user ID for development
```javascript
// From app/api/generator/[...endpoint]/route.ts
headers: {
  "X-User-Id": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Production:** Replace with actual JWT from NextAuth.js

---

## 🌐 Base URL

### Development
```
Frontend:  http://localhost:3000/api/generator
Backend:   http://localhost:5500/generator/api
```

### Production (Example)
```
Frontend:  https://app.techbirdsfly.com/api/generator
Backend:   https://api.techbirdsfly.com/generator/api
```

---

## 📌 Projects Endpoints

### 1. CREATE PROJECT

**Endpoint:** `POST /api/generator/projects`

**Request:**
```http
POST /api/generator/projects HTTP/1.1
Content-Type: application/json
Authorization: Bearer {token}
X-User-Id: {user_id}

{
  "name": "My SaaS Landing Page",
  "prompt": "Create a modern SaaS landing page with hero section, features grid, pricing table, testimonials, and CTA. Use purple and indigo colors. Target audience: startup founders."
}
```

**Parameters:**
| Name | Type | Required | Description | Constraints |
|------|------|----------|-------------|-------------|
| `name` | string | Yes | Project display name | 3-100 characters |
| `prompt` | string | Yes | AI generation prompt | 20-2000 characters |

**Response (201 Created):**
```json
{
  "projectId": "123e4567-e89b-12d3-a456-426614174000",
  "name": "My SaaS Landing Page",
  "prompt": "Create a modern SaaS landing page...",
  "status": "pending",
  "progress": null,
  "previewUrl": null,
  "htmlContent": null,
  "artifacts": [],
  "createdAt": "2025-11-25T10:30:45.000Z",
  "updatedAt": "2025-11-25T10:30:45.000Z",
  "errorMessage": null
}
```

**Status Codes:**
- `201 Created` - Project created successfully
- `400 Bad Request` - Invalid input
- `401 Unauthorized` - Missing/invalid token
- `500 Internal Server Error` - Server error

**Example (JavaScript):**
```typescript
import { useGeneratorStore } from "@/lib/store/generatorStore";

const project = await useGeneratorStore.getState().createProject(
  "My SaaS Landing Page",
  "Create a modern SaaS landing page with hero section..."
);
```

---

### 2. LIST PROJECTS

**Endpoint:** `GET /api/generator/projects`

**Request:**
```http
GET /api/generator/projects HTTP/1.1
Authorization: Bearer {token}
X-User-Id: {user_id}
```

**Query Parameters:**
| Name | Type | Optional | Description |
|------|------|----------|-------------|
| `limit` | integer | Yes | Max results (default: 50) |
| `offset` | integer | Yes | Pagination offset (default: 0) |
| `status` | string | Yes | Filter by status: pending, processing, completed, failed |
| `sort` | string | Yes | Sort by: created (default), updated, status |

**Response (200 OK):**
```json
[
  {
    "projectId": "123e4567-e89b-12d3-a456-426614174000",
    "name": "My SaaS Landing Page",
    "prompt": "Create a modern SaaS landing page...",
    "status": "completed",
    "progress": 100,
    "previewUrl": "https://preview.techbirdsfly.com/123e4567",
    "htmlContent": "<html>...</html>",
    "artifacts": [
      {
        "artifactType": "html",
        "downloadUrl": "https://download.techbirdsfly.com/123e4567/html.zip",
        "previewUrl": null,
        "generatedAt": "2025-11-25T10:35:45.000Z"
      }
    ],
    "createdAt": "2025-11-25T10:30:45.000Z",
    "updatedAt": "2025-11-25T10:35:45.000Z",
    "errorMessage": null
  }
]
```

**Example (JavaScript):**
```typescript
const { listProjects, projects } = useGeneratorStore();

await listProjects();
console.log(projects); // Array of projects
```

---

### 3. GET SINGLE PROJECT

**Endpoint:** `GET /api/generator/projects/{projectId}`

**Request:**
```http
GET /api/generator/projects/123e4567-e89b-12d3-a456-426614174000 HTTP/1.1
Authorization: Bearer {token}
X-User-Id: {user_id}
```

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `projectId` | UUID | Project identifier (in URL path) |

**Response (200 OK):**
```json
{
  "projectId": "123e4567-e89b-12d3-a456-426614174000",
  "name": "My SaaS Landing Page",
  "prompt": "Create a modern SaaS landing page...",
  "status": "processing",
  "progress": 65,
  "previewUrl": null,
  "htmlContent": null,
  "artifacts": [],
  "createdAt": "2025-11-25T10:30:45.000Z",
  "updatedAt": "2025-11-25T10:32:15.000Z",
  "errorMessage": null
}
```

**Status Codes:**
- `200 OK` - Project found
- `404 Not Found` - Project doesn't exist
- `401 Unauthorized` - Not owner

**Example (JavaScript):**
```typescript
const { getProject } = useGeneratorStore();

const project = await getProject("123e4567-e89b-12d3-a456-426614174000");
console.log(project.status, project.progress);
```

---

### 4. UPDATE PROJECT

**Endpoint:** `PUT /api/generator/projects/{projectId}`

**Request:**
```http
PUT /api/generator/projects/123e4567-e89b-12d3-a456-426614174000 HTTP/1.1
Content-Type: application/json
Authorization: Bearer {token}
X-User-Id: {user_id}

{
  "name": "Updated Project Name"
}
```

**Parameters:**
| Name | Type | Optional | Description |
|------|------|----------|-------------|
| `name` | string | Yes | New project name |
| `prompt` | string | Yes | New generation prompt |

**Response (200 OK):**
```json
{
  "projectId": "123e4567-e89b-12d3-a456-426614174000",
  "name": "Updated Project Name",
  "prompt": "...",
  "status": "processing",
  "progress": 75,
  "createdAt": "2025-11-25T10:30:45.000Z",
  "updatedAt": "2025-11-25T10:32:45.000Z"
}
```

---

### 5. DELETE PROJECT

**Endpoint:** `DELETE /api/generator/projects/{projectId}`

**Request:**
```http
DELETE /api/generator/projects/123e4567-e89b-12d3-a456-426614174000 HTTP/1.1
Authorization: Bearer {token}
X-User-Id: {user_id}
```

**Response (204 No Content):**
```
[Empty response body]
```

**Status Codes:**
- `204 No Content` - Successfully deleted
- `404 Not Found` - Project doesn't exist
- `401 Unauthorized` - Not owner

**Example (JavaScript):**
```typescript
const { deleteProject } = useGeneratorStore();

await deleteProject("123e4567-e89b-12d3-a456-426614174000");
console.log("Deleted");
```

---

### 6. DOWNLOAD ARTIFACT

**Endpoint:** `GET /api/generator/projects/{projectId}/download`

**Request:**
```http
GET /api/generator/projects/123e4567-e89b-12d3-a456-426614174000/download?type=html HTTP/1.1
Authorization: Bearer {token}
X-User-Id: {user_id}
```

**Query Parameters:**
| Name | Type | Required | Options | Description |
|------|------|----------|---------|-------------|
| `type` | string | Yes | html, react, nextjs | Export format |

**Response (200 OK):**
```
[Binary ZIP file content]
Content-Type: application/zip
Content-Disposition: attachment; filename="techbirdsfly-123e4567-html.zip"
Content-Length: 245891
```

**ZIP Contents:**
```
techbirdsfly-html/
├── index.html
├── styles.css
├── script.js
├── images/
│   ├── hero-bg.jpg
│   └── feature-icon.png
└── README.md
```

**Status Codes:**
- `200 OK` - Download ready
- `404 Not Found` - Project or artifact not found
- `400 Bad Request` - Invalid type parameter
- `409 Conflict` - Project not completed

**Example (JavaScript):**
```typescript
const { downloadProject } = useGeneratorStore();

// Automatically downloads ZIP file
await downloadProject("123e4567-e89b-12d3-a456-426614174000", "html");
```

---

### 7. REGENERATE SECTION

**Endpoint:** `POST /api/generator/projects/{projectId}/regenerate`

**Request:**
```http
POST /api/generator/projects/123e4567-e89b-12d3-a456-426614174000/regenerate HTTP/1.1
Content-Type: application/json
Authorization: Bearer {token}
X-User-Id: {user_id}

{
  "sectionId": "section-hero-1"
}
```

**Parameters:**
| Name | Type | Required | Description |
|------|------|----------|-------------|
| `sectionId` | string | Yes | Section identifier |

**Response (200 OK):**
```json
{
  "projectId": "123e4567-e89b-12d3-a456-426614174000",
  "name": "My SaaS Landing Page",
  "prompt": "...",
  "status": "processing",
  "progress": 30,
  "artifacts": [],
  "createdAt": "2025-11-25T10:30:45.000Z",
  "updatedAt": "2025-11-25T10:33:45.000Z"
}
```

**Status Codes:**
- `200 OK` - Regeneration started
- `404 Not Found` - Project or section not found
- `409 Conflict` - Project not in editable state

**Example (JavaScript):**
```typescript
const { regenerateSection } = useGeneratorStore();

await regenerateSection(
  "123e4567-e89b-12d3-a456-426614174000",
  "section-hero-1"
);
```

---

## 📋 Response Formats

### Project Object

```typescript
interface WebsiteProject {
  projectId: string;              // UUID v4
  name: string;                   // 3-100 chars
  prompt: string;                 // 20-2000 chars
  status: "pending" | "processing" | "completed" | "failed";
  progress: number | null;        // 0-100 or null
  previewUrl: string | null;      // URL or null
  htmlContent: string | null;     // HTML or null
  artifacts: GeneratedArtifact[]; // Array
  createdAt: string;              // ISO 8601
  updatedAt: string;              // ISO 8601
  errorMessage: string | null;    // Error detail
}
```

### Artifact Object

```typescript
interface GeneratedArtifact {
  artifactType: string;           // "html", "react", "nextjs"
  downloadUrl: string;            // Download link
  previewUrl: string | null;      // Preview link
  generatedAt: string;            // ISO 8601
}
```

### Error Response

```json
{
  "error": "Project not found",
  "errorCode": "PROJECT_NOT_FOUND",
  "timestamp": "2025-11-25T10:35:45.000Z",
  "requestId": "req-123456"
}
```

---

## ❌ Error Codes

| Code | HTTP | Description | Solution |
|------|------|-------------|----------|
| `INVALID_INPUT` | 400 | Validation failed | Check request body |
| `UNAUTHORIZED` | 401 | Missing/invalid token | Add Authorization header |
| `PROJECT_NOT_FOUND` | 404 | Project doesn't exist | Check projectId |
| `ACCESS_DENIED` | 403 | Not project owner | Use correct userId |
| `INVALID_STATUS` | 409 | Can't perform action in current status | Wait for status change |
| `GENERATION_FAILED` | 500 | AI generation error | Check error message |
| `INTERNAL_ERROR` | 500 | Server error | Retry or contact support |

**Error Response Example:**
```json
{
  "error": "Project name must be 3-100 characters",
  "errorCode": "INVALID_INPUT",
  "timestamp": "2025-11-25T10:35:45.000Z",
  "requestId": "req-123456"
}
```

---

## 💡 Examples

### Example 1: Complete Creation → Generation → Download Flow

```typescript
import { useGeneratorStore } from "@/lib/store/generatorStore";

export async function generateAndDownload() {
  const store = useGeneratorStore();

  // 1. Create project
  console.log("1️⃣ Creating project...");
  const project = await store.createProject(
    "My E-commerce Site",
    "Build a modern e-commerce website with product grid, cart, and checkout. Use black and gold colors."
  );
  console.log(`Project ID: ${project.projectId}`);

  // 2. Start polling
  console.log("2️⃣ Starting generation (polling every 3 seconds)...");
  store.startPolling(project.projectId);

  // 3. Wait for completion
  console.log("3️⃣ Waiting for generation to complete...");
  // Polling happens automatically in background
  // Toast notifications will appear when done

  // 4. Download
  console.log("4️⃣ Downloading HTML version...");
  await store.downloadProject(project.projectId, "html");
  console.log("✅ Download complete!");
}
```

### Example 2: List and Filter Projects

```typescript
import { useGeneratorStore } from "@/lib/store/generatorStore";

export async function listCompletedProjects() {
  const store = useGeneratorStore();

  // Fetch all projects
  await store.listProjects();

  // Filter to completed only
  const completed = store.projects.filter(
    (p) => p.status === "completed"
  );

  console.log(`✅ ${completed.length} completed projects`);

  // Download first one
  if (completed.length > 0) {
    const first = completed[0];
    console.log(`Downloading: ${first.name}`);
    await store.downloadProject(first.projectId, "nextjs");
  }
}
```

### Example 3: Monitor Progress

```typescript
import { useGeneratorStore } from "@/lib/store/generatorStore";

export async function monitorProgress(projectId: string) {
  const store = useGeneratorStore();

  // Fetch once
  const project = await store.getProject(projectId);

  if (project.status === "processing") {
    console.log(`⏳ Progress: ${project.progress}%`);
    // Continue polling...
    store.startPolling(projectId);
  } else if (project.status === "completed") {
    console.log("✅ Generation complete!");
  } else if (project.status === "failed") {
    console.error(`❌ Error: ${project.errorMessage}`);
  }
}
```

### Example 4: Handle Errors

```typescript
import { useGeneratorStore } from "@/lib/store/generatorStore";
import toast from "react-hot-toast";

export async function createWithErrorHandling() {
  const store = useGeneratorStore();

  try {
    const project = await store.createProject(
      "New Site",
      "Create a website..."
    );
    toast.success("✅ Project created!");
  } catch (error) {
    if (error.message.includes("validation")) {
      toast.error("❌ Invalid input. Check your text.");
    } else if (error.message.includes("unauthorized")) {
      toast.error("❌ Please login first.");
    } else {
      toast.error(`❌ Error: ${error.message}`);
    }
  }
}
```

---

## 🔍 Request/Response Examples

### Full Request Example

```http
POST /api/generator/projects HTTP/1.1
Host: localhost:3000
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
X-User-Id: 550e8400-e29b-41d4-a716-446655440000

{
  "name": "TechBirdsFly Demo",
  "prompt": "Create a landing page for an AI-powered website builder called TechBirdsFly. Include hero section with call-to-action, features grid (4 features), pricing table (3 tiers), testimonials section, and footer. Use purple (#7c3aed) and indigo (#4f46e5) as primary colors. Target audience: small business owners and freelancers."
}
```

### Full Response Example

```http
HTTP/1.1 201 Created
Content-Type: application/json
Date: Mon, 25 Nov 2025 10:30:45 GMT

{
  "projectId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "name": "TechBirdsFly Demo",
  "prompt": "Create a landing page for an AI-powered website builder...",
  "status": "pending",
  "progress": null,
  "previewUrl": null,
  "htmlContent": null,
  "artifacts": [],
  "createdAt": "2025-11-25T10:30:45Z",
  "updatedAt": "2025-11-25T10:30:45Z",
  "errorMessage": null
}
```

---

## 📚 Related Documentation

- `GENERATOR_INTEGRATION.md` - Full implementation guide
- `GENERATOR_QUICK_REFERENCE.md` - Quick lookup
- `.env.local` - Configuration

---

**Last Updated:** November 25, 2025  
**API Version:** 1.0.0  
**Status:** Production Ready ✅
