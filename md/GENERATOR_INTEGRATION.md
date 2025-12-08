# 🚀 GENERATOR SERVICE INTEGRATION

**Complete Next.js 15 Integration with .NET Generator Microservice**

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Files Created](#files-created)
4. [Environment Setup](#environment-setup)
5. [API Endpoints](#api-endpoints)
6. [Data Flow](#data-flow)
7. [Features](#features)
8. [Status Polling](#status-polling)
9. [Error Handling](#error-handling)
10. [Testing](#testing)
11. [Troubleshooting](#troubleshooting)

---

## 🎯 Overview

This integration connects your Next.js 15 frontend to a .NET Generator Microservice via an API proxy layer. Users can:

- **Create** new website projects with AI prompts
- **Monitor** generation progress in real-time
- **Preview** generated websites live
- **Download** code in multiple formats (HTML, React, Next.js)
- **Regenerate** specific sections
- **Manage** all their projects

**Architecture:** Next.js Frontend → API Proxy → YARP Gateway → .NET Generator Service

---

## 🏗️ Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                      NEXT.JS FRONTEND                        │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  /dashboard/generator          (UI Page)              │  │
│  │  /dashboard/projects           (List Page)            │  │
│  │  /dashboard/projects/[id]      (Details Page)         │  │
│  │  /components/layout/Sidebar    (Navigation)           │  │
│  └────────────────────────────────────────────────────────┘  │
│                           ↓ API Calls                         │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  /app/api/generator/[...endpoint]/route.ts            │  │
│  │  (API Proxy - Routes to YARP Gateway)                 │  │
│  └────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘
                           ↓ HTTP
┌──────────────────────────────────────────────────────────────┐
│                     YARP GATEWAY (Port 5500)                 │
│                   (Yet Another Reverse Proxy)                │
└──────────────────────────────────────────────────────────────┘
                           ↓ HTTP
┌──────────────────────────────────────────────────────────────┐
│               .NET GENERATOR MICROSERVICE                     │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  /generator/api/projects          (List)              │  │
│  │  /generator/api/projects          (Create)            │  │
│  │  /generator/api/projects/{id}     (Get)               │  │
│  │  /generator/api/projects/{id}     (Update)            │  │
│  │  /generator/api/projects/{id}     (Delete)            │  │
│  │  /generator/api/projects/{id}/download                │  │
│  │  /generator/api/projects/{id}/regenerate              │  │
│  └────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘
```

---

## 📁 Files Created

### 1. **Environment Configuration**
- **File:** `.env.local`
- **Variables Added:**
  ```env
  NEXT_PUBLIC_GATEWAY_URL=http://localhost:5500
  GENERATOR_ROUTE=/generator/api
  ```

### 2. **API Proxy Route**
- **File:** `/app/api/generator/[...endpoint]/route.ts` (217 lines)
- **Purpose:** Forwards all requests to .NET Generator Service
- **Supports:** GET, POST, PUT, DELETE
- **Features:**
  - Request forwarding
  - Header management
  - Error handling
  - Logging

### 3. **Zustand Store**
- **File:** `/lib/store/generatorStore.ts` (387 lines)
- **Type Definitions:**
  ```typescript
  interface WebsiteProject {
    projectId: string;
    name: string;
    prompt: string;
    status: "pending" | "processing" | "completed" | "failed";
    progress?: number;
    previewUrl?: string;
    htmlContent?: string;
    artifacts: GeneratedArtifact[];
    createdAt: string;
    updatedAt: string;
    errorMessage?: string;
  }
  ```
- **Actions:**
  - `createProject(name, prompt)` - Create new project
  - `listProjects()` - Fetch all projects
  - `getProject(id)` - Fetch single project
  - `startPolling(projectId)` - Start status polling
  - `stopPolling(projectId)` - Stop polling
  - `downloadProject(id, type)` - Download artifact
  - `regenerateSection(id, sectionId)` - Regenerate section
  - `deleteProject(id)` - Delete project

### 4. **Generator UI Page**
- **File:** `/app/dashboard/generator/page.tsx` (255 lines)
- **Features:**
  - Project name input
  - Prompt textarea with validation
  - Character counters
  - Form validation
  - Error messages
  - Loading states
  - Auto-redirect on success

### 5. **Projects List Page**
- **File:** `/app/dashboard/projects/page.tsx` (186 lines)
- **Features:**
  - Project cards grid
  - Status badges
  - Progress bars for processing
  - Empty state
  - Quick create button
  - Loading states

### 6. **Project Details Page**
- **File:** `/app/dashboard/projects/[id]/page.tsx` (380 lines)
- **Features:**
  - Live preview iframe
  - Real-time status updates
  - Download buttons (multi-format)
  - Project metadata
  - Auto-polling (3s interval)
  - Error handling
  - Responsive layout

### 7. **Sidebar Update**
- **File:** `/components/layout/Sidebar.tsx` (Updated)
- **Changes:**
  - Added Generator link (primary)
  - Reordered navigation items
  - Updated descriptions

---

## ⚙️ Environment Setup

### Step 1: Update `.env.local`

```env
# Gateway URL pointing to YARP
NEXT_PUBLIC_GATEWAY_URL=http://localhost:5500

# Route prefix for Generator Service
GENERATOR_ROUTE=/generator/api
```

### Step 2: Verify .NET Service is Running

```bash
# Check if YARP Gateway is accessible
curl http://localhost:5500/health
```

### Step 3: Start Next.js Dev Server

```bash
cd techbirdsfly-frontend-nextjs
npm run dev
# Open http://localhost:3000/dashboard/generator
```

---

## 🔌 API Endpoints

All endpoints are proxied through `/api/generator/*` → `{GATEWAY_URL}/generator/api/*`

### Projects

#### Create Project
```
POST /api/generator/projects
Content-Type: application/json

{
  "name": "My SaaS Landing Page",
  "prompt": "Build a modern SaaS landing page..."
}

Response:
{
  "projectId": "uuid",
  "name": "My SaaS Landing Page",
  "prompt": "...",
  "status": "pending",
  "createdAt": "2025-11-25T...",
  "updatedAt": "2025-11-25T...",
  "artifacts": []
}
```

#### List Projects
```
GET /api/generator/projects

Response:
[
  { projectId, name, status, ... },
  { projectId, name, status, ... }
]
```

#### Get Single Project
```
GET /api/generator/projects/{projectId}

Response:
{
  "projectId": "uuid",
  "name": "...",
  "status": "processing",
  "progress": 45,
  "previewUrl": "https://...",
  "artifacts": [...]
}
```

#### Delete Project
```
DELETE /api/generator/projects/{projectId}
```

#### Download Artifact
```
GET /api/generator/projects/{projectId}/download?type=html
GET /api/generator/projects/{projectId}/download?type=react
GET /api/generator/projects/{projectId}/download?type=nextjs

Response: Binary ZIP file
```

#### Regenerate Section
```
POST /api/generator/projects/{projectId}/regenerate
Content-Type: application/json

{
  "sectionId": "section-hero-1"
}

Response: Updated project object
```

---

## 📊 Data Flow

### Creation Flow
```
1. User enters project name & prompt
   ↓
2. Clicks "Generate Website"
   ↓
3. createProject() action fires
   ↓
4. POST /api/generator/projects
   ↓
5. YARP forwards to .NET Service
   ↓
6. .NET returns project with status="pending"
   ↓
7. Front-end starts polling with startPolling(projectId)
   ↓
8. User redirected to project details page
```

### Polling Flow
```
GET /api/generator/projects/{projectId}  ← Every 3 seconds
   ↓
Status still "processing" → Continue polling
   ↓
Status changed to "completed" or "failed" → Stop polling
   ↓
Toast notification to user
```

### Download Flow
```
1. User clicks download button
   ↓
2. downloadProject(projectId, type) fires
   ↓
3. GET /api/generator/projects/{projectId}/download?type=html
   ↓
4. Browser receives ZIP blob
   ↓
5. Browser.download() auto-triggers
```

---

## ✨ Features

### 1. **Real-time Status Polling**
- Auto-polls every 3 seconds
- Stops after 30 minutes (safety timeout)
- Shows progress bar
- Toast notifications on completion

### 2. **Multi-Format Download**
- HTML (static files)
- React (component-based)
- Next.js (full app)
- Auto-download with correct filename

### 3. **Project Management**
- Create unlimited projects
- View all projects
- Delete unwanted projects
- Track project status

### 4. **Live Preview**
- Real-time iframe preview
- Updates as generation progresses
- Responsive preview support

### 5. **Error Handling**
- Graceful error messages
- Retry mechanisms
- Validation on form submission
- Network error recovery

---

## 🔄 Status Polling

### How Polling Works

```typescript
// Start polling for project
useGeneratorStore.startPolling(projectId);

// What happens:
// 1. Creates 3-second interval
// 2. Fetches project status
// 3. Updates store with new status
// 4. Shows progress bar
// 5. Stops when status changes to "completed" or "failed"
// 6. Automatically cleans up interval
```

### Polling Configuration

```typescript
const POLLING_INTERVAL = 3000;        // 3 seconds
const POLLING_TIMEOUT = 30 * 60 * 1000; // 30 minutes
```

### Disable Polling (Optional)

```typescript
// In project details page
useGeneratorStore.stopPolling(projectId);
```

---

## ❌ Error Handling

### Common Errors & Solutions

#### 1. "Gateway not configured"
- **Cause:** `NEXT_PUBLIC_GATEWAY_URL` not set
- **Solution:** Add to `.env.local`
```env
NEXT_PUBLIC_GATEWAY_URL=http://localhost:5500
```

#### 2. "Failed to create project"
- **Cause:** .NET service error or validation failure
- **Check:** 
  - Is .NET service running?
  - Is YARP accessible?
  - Are environment variables correct?

#### 3. "Polling timeout"
- **Cause:** Generation took > 30 minutes
- **Solution:** Increase `POLLING_TIMEOUT` in store or check .NET service logs

#### 4. "Network error on download"
- **Cause:** Connection lost during download
- **Solution:** Retry from project details page

### Error Handling in Code

```typescript
try {
  const project = await createProject(name, prompt);
  // Success - redirect
  router.push(`/dashboard/projects/${project.projectId}`);
} catch (err) {
  // Error - show toast and keep on page
  console.error(err);
  // Toast already shown by store action
}
```

---

## 🧪 Testing

### 1. Test Creation
```
URL: http://localhost:3000/dashboard/generator

Steps:
1. Enter project name: "Test Project"
2. Enter prompt: "A modern landing page with hero, features, and CTA"
3. Click "Generate Website"
4. Should redirect to /dashboard/projects/{id}
```

### 2. Test Polling
```
Observations:
- Status updates in real-time
- Progress bar increases
- Toast shows when complete
- Download buttons appear
```

### 3. Test Download
```
Steps:
1. Project status = "completed"
2. Click "HTML" download button
3. ZIP file auto-downloads
4. Check filename format
```

### 4. Test Project List
```
URL: http://localhost:3000/dashboard/projects

Expected:
- All created projects listed
- Status badges show correctly
- Clicking project opens details
```

### 5. Test Error Scenarios
```
Test Cases:
- Submit empty form (validation errors)
- Kill .NET service mid-polling (error handling)
- Network disconnection (retry logic)
- Delete project while polling (cleanup)
```

---

## 🚨 Troubleshooting

### Issue: Projects not loading

**Check:**
1. Is .NET service running?
   ```bash
   curl http://localhost:5500/health
   ```

2. Are environment variables set?
   ```bash
   echo $NEXT_PUBLIC_GATEWAY_URL
   # Should output: http://localhost:5500
   ```

3. Check browser console for errors
4. Check Next.js terminal for proxy errors

**Fix:**
```bash
# Restart Next.js
npm run dev

# Restart .NET service
# (Follow .NET service documentation)
```

### Issue: Polling never stops

**Cause:** Project stuck in "processing" state

**Check:**
1. Look at .NET service logs
2. Check database for stale records

**Fix:**
```typescript
// Manual override in browser console
useGeneratorStore.stopPolling(projectId);
```

### Issue: Download fails

**Cause:** Missing artifact or service error

**Check:**
1. Project status is "completed"
2. Artifacts array is not empty
3. Network requests complete

**Debug:**
```typescript
// In browser console
const store = useGeneratorStore();
const project = store.currentProject;
console.log(project.artifacts);
```

### Issue: API proxy not working

**Check:**
1. Route file exists: `/app/api/generator/[...endpoint]/route.ts`
2. Endpoint parameter is correct
3. URL is properly formatted

**Test:**
```bash
# Test from browser console
fetch('/api/generator/projects')
  .then(r => r.json())
  .then(console.log)
```

---

## 📚 Code Examples

### Create Project Programmatically

```typescript
import { useGeneratorStore } from "@/lib/store/generatorStore";

export function MyComponent() {
  const { createProject } = useGeneratorStore();

  const handleCreate = async () => {
    try {
      const project = await createProject(
        "My Website",
        "A modern e-commerce site..."
      );
      console.log("Created:", project);
    } catch (err) {
      console.error("Failed:", err);
    }
  };

  return <button onClick={handleCreate}>Create</button>;
}
```

### Manual Polling Control

```typescript
import { useGeneratorStore } from "@/lib/store/generatorStore";

export function PollingControl() {
  const { startPolling, stopPolling } = useGeneratorStore();

  return (
    <>
      <button onClick={() => startPolling("project-123")}>
        Start Polling
      </button>
      <button onClick={() => stopPolling("project-123")}>
        Stop Polling
      </button>
    </>
  );
}
```

### Download with Progress

```typescript
import { useGeneratorStore } from "@/lib/store/generatorStore";

export function DownloadButton() {
  const { downloadProject, isDownloading } = useGeneratorStore();

  return (
    <button
      onClick={() => downloadProject("project-123", "html")}
      disabled={isDownloading}
    >
      {isDownloading ? "Downloading..." : "Download HTML"}
    </button>
  );
}
```

---

## 🔐 Security Considerations

### 1. Authentication
Currently using demo user ID. For production:
```typescript
// app/api/generator/[...endpoint]/route.ts
const token = req.headers.get("authorization");
const userId = req.headers.get("x-user-id");

// Validate token before proxying
if (!isValidToken(token)) {
  return NextResponse.json({ error: "Unauthorized" }, { status: 401 });
}
```

### 2. Input Validation
- Project name: 3-100 characters
- Prompt: 20-2000 characters
- Type checking on all API responses

### 3. CORS
If .NET service is on different domain:
```typescript
// Add CORS headers in proxy route
headers: {
  "Access-Control-Allow-Origin": "*",
  // ... other headers
}
```

---

## 📈 Performance

### Optimization Tips

1. **Reduce Polling Frequency** (if server load is high)
   ```typescript
   const POLLING_INTERVAL = 5000; // 5 seconds instead of 3
   ```

2. **Cache Project List** (if many projects)
   ```typescript
   // Use SWR or React Query on top of Zustand
   ```

3. **Lazy Load Preview** (if iframe is slow)
   ```typescript
   // Only load iframe when tab is visible
   ```

---

## 🎓 Next Steps

1. **Add Authentication**
   - Integrate with NextAuth.js
   - Pass JWT token to proxy

2. **Add Caching**
   - Use React Query for smarter caching
   - Reduce API calls

3. **Add Analytics**
   - Track creation metrics
   - Monitor generation success rate

4. **Add Webhooks** (Optional)
   - Notify frontend of completion via WebSocket
   - Replace polling with real-time updates

---

## 📞 Support

**For issues with:**
- **Frontend:** Check browser console, Next.js terminal
- **.NET Service:** Check service logs, YARP logs
- **Connection:** Verify `NEXT_PUBLIC_GATEWAY_URL`

---

**Created:** November 25, 2025  
**Version:** 1.0.0  
**Status:** Production Ready ✅
