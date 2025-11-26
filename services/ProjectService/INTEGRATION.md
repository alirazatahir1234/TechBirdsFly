# Project Service - Integration Guide

Complete guide to integrate Project Service with Gateway and Frontend.

## Architecture Overview

```
Client (Browser)
    ↓
Next.js Frontend (port 3000)
    ↓
YARP Gateway (port 5500)
    ├── → Auth Service (5001)
    ├── → Generator Service (5002)
    ├── → Export Service (5003)
    └── → Project Service (5004) ← NEW
    
PostgreSQL (5432)
├── auth_service
├── generator_service
├── export_service
└── project_service ← NEW
```

## Part 1: Gateway Integration

### Step 1: Add Project Service Route

Edit `gateway/yarp-gateway/appsettings.json`:

```json
{
  "ReverseProxy": {
    "Routes": {
      // Existing routes...
      "projects": {
        "ClusterId": "ProjectServiceCluster",
        "Match": {
          "Path": "/api/projects/**"
        },
        "AuthorizationPolicy": "default"
      }
    },
    "Clusters": {
      // Existing clusters...
      "ProjectServiceCluster": {
        "Destinations": {
          "ProjectService": {
            "Address": "http://localhost:5004"
          }
        }
      }
    }
  }
}
```

### Step 2: Update Docker Compose

Edit `infra/docker-compose.yml`:

```yaml
services:
  # Existing services...
  
  project-service:
    build:
      context: ../services/ProjectService
      dockerfile: Dockerfile
    container_name: project-service
    ports:
      - "5004:5004"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__ProjectServiceDatabase=Host=postgres;Port=5432;Database=project_service;Username=postgres;Password=postgres
      - Serilog__WriteTo__0__Args__serverUrl=http://seq:5341
    depends_on:
      - postgres
      - seq
    networks:
      - techbirdsfly-network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5004/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 5s

  postgres:
    image: postgres:15
    container_name: postgres
    environment:
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - techbirdsfly-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  postgres_data:

networks:
  techbirdsfly-network:
    driver: bridge
```

### Step 3: Verify Gateway Integration

```bash
# Test project endpoint through gateway
curl http://localhost:5500/api/projects/health

# Should return:
# Healthy
```

## Part 2: Frontend Integration

### Step 1: Create Project Store (Zustand)

Create `web-frontend/techbirdsfly-frontend-nextjs/stores/projectStore.ts`:

```typescript
import { create } from 'zustand';
import { API_BASE_URL } from '@/config/api';

export interface Project {
  id: string;
  name: string;
  description?: string;
  framework: 'nextjs' | 'react' | 'html';
  theme?: string;
  createdAt: string;
  versionCount: number;
}

export interface ProjectVersion {
  id: string;
  projectId: string;
  versionNumber: number;
  createdAt: string;
  artifactCount: number;
}

interface ProjectStore {
  projects: Project[];
  selectedProject: Project | null;
  versions: ProjectVersion[];
  isLoading: boolean;
  error: string | null;
  
  // Actions
  createProject: (req: CreateProjectRequest) => Promise<Project>;
  getProject: (id: string) => Promise<Project>;
  getProjects: (ownerId: string) => Promise<Project[]>;
  updateProject: (id: string, req: UpdateProjectSettingsRequest) => Promise<Project>;
  renameProject: (id: string, newName: string) => Promise<void>;
  deleteProject: (id: string) => Promise<void>;
  
  getVersions: (projectId: string) => Promise<ProjectVersion[]>;
  createVersion: (projectId: string) => Promise<ProjectVersion>;
  
  linkArtifact: (versionId: string, artifactId: string, type: string) => Promise<void>;
  
  setSelectedProject: (project: Project | null) => void;
  clearError: () => void;
}

interface CreateProjectRequest {
  ownerId: string;
  name: string;
  framework: 'nextjs' | 'react' | 'html';
  theme?: string;
  description?: string;
}

interface UpdateProjectSettingsRequest {
  description?: string;
  framework?: 'nextjs' | 'react' | 'html';
  theme?: string;
}

export const useProjectStore = create<ProjectStore>((set, get) => ({
  projects: [],
  selectedProject: null,
  versions: [],
  isLoading: false,
  error: null,
  
  createProject: async (req: CreateProjectRequest) => {
    set({ isLoading: true, error: null });
    try {
      const response = await fetch(`${API_BASE_URL}/projects`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(req),
      });
      
      if (!response.ok) throw new Error('Failed to create project');
      const data = await response.json();
      
      set((state) => ({
        projects: [...state.projects, data.project],
        selectedProject: data.project,
        isLoading: false,
      }));
      
      return data.project;
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },
  
  getProject: async (id: string) => {
    set({ isLoading: true, error: null });
    try {
      const response = await fetch(`${API_BASE_URL}/projects/${id}`);
      if (!response.ok) throw new Error('Project not found');
      const project = await response.json();
      
      set({ selectedProject: project, isLoading: false });
      return project;
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },
  
  getProjects: async (ownerId: string) => {
    set({ isLoading: true, error: null });
    try {
      const response = await fetch(`${API_BASE_URL}/projects/user/${ownerId}`);
      if (!response.ok) throw new Error('Failed to fetch projects');
      const projects = await response.json();
      
      set({ projects, isLoading: false });
      return projects;
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },
  
  updateProject: async (id: string, req: UpdateProjectSettingsRequest) => {
    set({ isLoading: true, error: null });
    try {
      const response = await fetch(`${API_BASE_URL}/projects/${id}/settings`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(req),
      });
      
      if (!response.ok) throw new Error('Failed to update project');
      const project = await response.json();
      
      set((state) => ({
        projects: state.projects.map(p => p.id === id ? project : p),
        selectedProject: project,
        isLoading: false,
      }));
      
      return project;
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },
  
  renameProject: async (id: string, newName: string) => {
    set({ isLoading: true, error: null });
    try {
      const response = await fetch(`${API_BASE_URL}/projects/${id}/rename`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ newName }),
      });
      
      if (!response.ok) throw new Error('Failed to rename project');
      
      set((state) => ({
        projects: state.projects.map(p =>
          p.id === id ? { ...p, name: newName } : p
        ),
        selectedProject: state.selectedProject
          ? { ...state.selectedProject, name: newName }
          : null,
        isLoading: false,
      }));
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },
  
  deleteProject: async (id: string) => {
    set({ isLoading: true, error: null });
    try {
      const response = await fetch(`${API_BASE_URL}/projects/${id}`, {
        method: 'DELETE',
      });
      
      if (!response.ok) throw new Error('Failed to delete project');
      
      set((state) => ({
        projects: state.projects.filter(p => p.id !== id),
        selectedProject: state.selectedProject?.id === id ? null : state.selectedProject,
        isLoading: false,
      }));
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },
  
  getVersions: async (projectId: string) => {
    set({ isLoading: true, error: null });
    try {
      const response = await fetch(`${API_BASE_URL}/projects/${projectId}/versions`);
      if (!response.ok) throw new Error('Failed to fetch versions');
      const versions = await response.json();
      
      set({ versions, isLoading: false });
      return versions;
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },
  
  createVersion: async (projectId: string) => {
    set({ isLoading: true, error: null });
    try {
      const response = await fetch(`${API_BASE_URL}/projects/${projectId}/versions`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
      });
      
      if (!response.ok) throw new Error('Failed to create version');
      const version = await response.json();
      
      set((state) => ({
        versions: [...state.versions, version],
        isLoading: false,
      }));
      
      return version;
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },
  
  linkArtifact: async (versionId: string, artifactId: string, type: string) => {
    set({ isLoading: true, error: null });
    try {
      const response = await fetch(`${API_BASE_URL}/projects/versions/link-artifact`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ versionId, artifactId, type }),
      });
      
      if (!response.ok) throw new Error('Failed to link artifact');
      
      set({ isLoading: false });
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },
  
  setSelectedProject: (project: Project | null) => {
    set({ selectedProject: project });
  },
  
  clearError: () => {
    set({ error: null });
  },
}));
```

### Step 2: Create Projects Page

Create `web-frontend/techbirdsfly-frontend-nextjs/app/dashboard/projects/page.tsx`:

```typescript
'use client';

import { useEffect, useState } from 'react';
import { useProjectStore } from '@/stores/projectStore';
import { useAuthStore } from '@/stores/authStore';
import { Button } from '@/components/ui/button';

export default function ProjectsPage() {
  const { user } = useAuthStore();
  const { projects, getProjects, isLoading, error } = useProjectStore();
  const [showCreateForm, setShowCreateForm] = useState(false);

  useEffect(() => {
    if (user) {
      getProjects(user.id);
    }
  }, [user]);

  return (
    <div className="container mx-auto py-8">
      <div className="flex justify-between items-center mb-8">
        <h1 className="text-3xl font-bold">My Projects</h1>
        <Button onClick={() => setShowCreateForm(true)}>
          Create Project
        </Button>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {isLoading && <div>Loading...</div>}

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {projects.map((project) => (
          <div key={project.id} className="card bg-base-100 shadow-xl">
            <div className="card-body">
              <h2 className="card-title">{project.name}</h2>
              <p>{project.description}</p>
              <div className="badge">{project.framework}</div>
              <div className="card-actions justify-end">
                <Button>Edit</Button>
                <Button>View</Button>
              </div>
            </div>
          </div>
        ))}
      </div>

      {showCreateForm && (
        // Create project modal form
        <ProjectCreateModal onClose={() => setShowCreateForm(false)} />
      )}
    </div>
  );
}
```

### Step 3: Add API Configuration

Update `web-frontend/techbirdsfly-frontend-nextjs/config/api.ts`:

```typescript
export const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5500/api';

export const API_ENDPOINTS = {
  // Auth
  auth: {
    register: `${API_BASE_URL}/auth/register`,
    login: `${API_BASE_URL}/auth/login`,
    logout: `${API_BASE_URL}/auth/logout`,
  },
  
  // Generator
  generator: {
    generate: `${API_BASE_URL}/generator/generate`,
    getArtifacts: `${API_BASE_URL}/generator/artifacts`,
  },
  
  // Projects
  projects: {
    list: (ownerId: string) => `${API_BASE_URL}/projects/user/${ownerId}`,
    get: (id: string) => `${API_BASE_URL}/projects/${id}`,
    create: `${API_BASE_URL}/projects`,
    update: (id: string) => `${API_BASE_URL}/projects/${id}/settings`,
    rename: (id: string) => `${API_BASE_URL}/projects/${id}/rename`,
    delete: (id: string) => `${API_BASE_URL}/projects/${id}`,
    
    versions: {
      list: (projectId: string) => `${API_BASE_URL}/projects/${projectId}/versions`,
      create: (projectId: string) => `${API_BASE_URL}/projects/${projectId}/versions`,
    },
    
    artifacts: {
      link: `${API_BASE_URL}/projects/versions/link-artifact`,
    },
  },
};
```

### Step 4: Environment Variables

Add to `.env.local`:

```env
NEXT_PUBLIC_API_BASE_URL=http://localhost:5500/api
```

For production:
```env
NEXT_PUBLIC_API_BASE_URL=https://api.techbirdsfly.com
```

## Part 3: Testing Integration

### Test 1: Gateway Routing

```bash
# Health check through gateway
curl http://localhost:5500/api/projects/health

# Create project through gateway
curl -X POST http://localhost:5500/api/projects \
  -H "Content-Type: application/json" \
  -d '{
    "ownerId": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Test Project",
    "framework": "nextjs"
  }'
```

### Test 2: Frontend Integration

```bash
# Start frontend
cd web-frontend/techbirdsfly-frontend-nextjs
npm run dev

# Navigate to http://localhost:3000/dashboard/projects
# Should see project list and create button
```

### Test 3: End-to-End Flow

1. **Create Project**
   - Click "Create Project" button
   - Fill form (Name, Framework, Theme)
   - Submit

2. **View Project**
   - Project appears in list
   - Click to view details
   - Show versions and artifacts

3. **Create Version**
   - Click "Create Version"
   - System increments version number
   - Shows empty artifacts

4. **Link Artifact**
   - Generate artifact in Generator Service
   - Link artifact to version
   - Artifact appears in version

## Part 4: Docker Compose Integration

### Start All Services

```bash
# From root directory
docker-compose -f infra/docker-compose.yml up -d

# Verify all services
docker-compose -f infra/docker-compose.yml ps

# Expected:
# - postgres (5432) - Ready
# - seq (5341) - Ready
# - auth-service (5001) - Ready
# - generator-service (5002) - Ready
# - export-service (5003) - Ready
# - project-service (5004) - Ready ← NEW
# - gateway (5500) - Ready
# - frontend (3000) - Ready
```

### Health Checks

```bash
# Gateway
curl http://localhost:5500/health

# Project Service (direct)
curl http://localhost:5004/health

# Project Service (through gateway)
curl http://localhost:5500/api/projects/health
```

## Troubleshooting

### ❌ Gateway returns 404 for project endpoints

**Problem**: Route not configured in YARP

**Solution**: Verify `appsettings.json` has projects route

```json
"Routes": {
  "projects": {
    "ClusterId": "ProjectServiceCluster",
    "Match": { "Path": "/api/projects/**" }
  }
}
```

### ❌ Frontend requests fail CORS

**Problem**: CORS policy prevents cross-origin requests

**Solution**: Gateway acts as proxy, no CORS needed if all go through gateway

```bash
# Should go to gateway, not direct service
const API_BASE_URL = 'http://localhost:5500/api'; ✅
const API_BASE_URL = 'http://localhost:5004/api'; ❌
```

### ❌ Project Service can't reach database

**Problem**: Connection string wrong in Docker

**Solution**: Use service name as hostname

```yaml
environment:
  ConnectionStrings__ProjectServiceDatabase=Host=postgres;... # Not localhost
```

### ❌ Zustand store shows loading forever

**Problem**: API not responding

**Solution**: Check service is running
```bash
curl http://localhost:5004/health
curl http://localhost:5500/api/projects/health
```

## Next Steps

1. **Add Pagination**
   - Update `GetUserProjectsQuery` with Page/PageSize
   - Paginate results in store

2. **Add Search/Filter**
   - Filter by framework
   - Search by name
   - Sort by date

3. **Add Project Templates**
   - Clone existing project
   - Save as template
   - List templates

4. **Add Collaboration**
   - Share projects
   - Permission levels
   - Activity log

## Summary

✅ **Gateway**: Project Service route configured
✅ **Frontend**: Zustand store with all CRUD operations
✅ **Docker**: Service integrated in docker-compose
✅ **API**: All endpoints accessible through gateway
✅ **Integration**: E2E flow from frontend to database

You can now create, read, update, and delete projects from the frontend!

---

For detailed Project Service documentation, see `README.md`
For quick start, see `QUICK_START.md`
