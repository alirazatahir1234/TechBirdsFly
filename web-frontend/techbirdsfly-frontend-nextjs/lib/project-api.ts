/**
 * Project API Client
 * Communicates with Project-Service via API Gateway
 * Gateway routes: http://localhost:9000/project/api/*
 */

const PROJECT_API_BASE = process.env.NEXT_PUBLIC_PROJECT_API_BASE || "http://localhost:9000/project/api";

interface CreateProjectData {
  userId: string;
  name: string;
  industry: string;
  style: string;
  palette: string;
  html: string;
}

interface SaveVersionData {
  projectId: string;
  html: string;
}

interface ProjectResponse {
  id: string;
  userId: string;
  name: string;
  industry: string;
  style: string;
  palette: string;
  html: string;
  version: number;
  createdAt: string;
  updatedAt: string;
}

interface ProjectsListResponse {
  projects: ProjectResponse[];
  total: number;
}

/**
 * Create a new project with generated HTML
 * Called after AI website generation
 */
export async function createProject(data: CreateProjectData): Promise<ProjectResponse> {
  try {
    const res = await fetch(`${PROJECT_API_BASE}/projects/create`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(data),
    });

    if (!res.ok) {
      const error = await res.json();
      throw new Error(error.message || `Failed to create project: ${res.status}`);
    }

    return await res.json();
  } catch (error) {
    console.error("Error creating project:", error);
    throw error;
  }
}

/**
 * List all projects for a user
 * Used on Projects Dashboard
 */
export async function listProjects(userId: string): Promise<ProjectsListResponse> {
  try {
    const res = await fetch(`${PROJECT_API_BASE}/projects/user/${userId}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (!res.ok) {
      const error = await res.json();
      throw new Error(error.message || `Failed to list projects: ${res.status}`);
    }

    return await res.json();
  } catch (error) {
    console.error("Error listing projects:", error);
    throw error;
  }
}

/**
 * Load a single project by ID
 * Used when opening project in editor
 */
export async function loadProject(projectId: string): Promise<ProjectResponse> {
  try {
    const res = await fetch(`${PROJECT_API_BASE}/projects/${projectId}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (!res.ok) {
      const error = await res.json();
      throw new Error(error.message || `Failed to load project: ${res.status}`);
    }

    return await res.json();
  } catch (error) {
    console.error("Error loading project:", error);
    throw error;
  }
}

/**
 * Save a new version of a project's HTML
 * Creates version history on backend
 */
export async function saveVersion(data: SaveVersionData): Promise<ProjectResponse> {
  try {
    const res = await fetch(`${PROJECT_API_BASE}/projects/${data.projectId}/save-version`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        html: data.html,
      }),
    });

    if (!res.ok) {
      const error = await res.json();
      throw new Error(error.message || `Failed to save version: ${res.status}`);
    }

    return await res.json();
  } catch (error) {
    console.error("Error saving version:", error);
    throw error;
  }
}

/**
 * Delete a project
 * Removes project and all versions from database
 */
export async function deleteProject(projectId: string): Promise<{ success: boolean; message: string }> {
  try {
    const res = await fetch(`${PROJECT_API_BASE}/projects/${projectId}`, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (!res.ok) {
      const error = await res.json();
      throw new Error(error.message || `Failed to delete project: ${res.status}`);
    }

    return await res.json();
  } catch (error) {
    console.error("Error deleting project:", error);
    throw error;
  }
}

/**
 * Duplicate an existing project
 * Creates a copy with "(Copy)" suffix and same content/version
 */
export async function duplicateProject(
  projectId: string,
  userId: string
): Promise<{ success: boolean; data: string; message: string }> {
  try {
    const res = await fetch(`${PROJECT_API_BASE}/projects/${projectId}/duplicate`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ userId }),
    });

    if (!res.ok) {
      const error = await res.json();
      throw new Error(error.message || `Failed to duplicate project: ${res.status}`);
    }

    return await res.json();
  } catch (error) {
    console.error("Error duplicating project:", error);
    throw error;
  }
}

/**
 * Get project version history
 * Lists all versions of a project
 */
export async function getProjectVersions(
  projectId: string
): Promise<{ versions: ProjectResponse[] }> {
  try {
    const res = await fetch(`${PROJECT_API_BASE}/projects/${projectId}/versions`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (!res.ok) {
      const error = await res.json();
      throw new Error(error.message || `Failed to get versions: ${res.status}`);
    }

    return await res.json();
  } catch (error) {
    console.error("Error getting project versions:", error);
    throw error;
  }
}

/**
 * Restore a specific version of a project
 */
export async function restoreVersion(projectId: string, version: number): Promise<ProjectResponse> {
  try {
    const res = await fetch(`${PROJECT_API_BASE}/projects/${projectId}/versions/${version}/restore`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (!res.ok) {
      const error = await res.json();
      throw new Error(error.message || `Failed to restore version: ${res.status}`);
    }

    return await res.json();
  } catch (error) {
    console.error("Error restoring version:", error);
    throw error;
  }
}

/**
 * Rename a project
 * Updates project name in database instantly
 */
export async function renameProject(projectId: string, name: string): Promise<{ success: boolean; message: string }> {
  try {
    const res = await fetch(`${PROJECT_API_BASE}/projects/rename`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        projectId,
        name,
      }),
    });

    if (!res.ok) {
      const error = await res.json();
      throw new Error(error.message || `Failed to rename project: ${res.status}`);
    }

    return await res.json();
  } catch (error) {
    console.error("Error renaming project:", error);
    throw error;
  }
}

/**
 * Move project to trash (soft delete)
 */
export async function moveToTrash(projectId: string, userId: string): Promise<{ success: boolean; message: string }> {
  try {
    const res = await fetch(`${PROJECT_API_BASE}/projects/trash/${projectId}`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ userId }),
    });

    if (!res.ok) {
      const error = await res.json();
      throw new Error(error.message || `Failed to move project to trash: ${res.status}`);
    }

    return await res.json();
  } catch (error) {
    console.error("Error moving project to trash:", error);
    throw error;
  }
}

/**
 * Restore project from trash
 */
export async function restoreProject(projectId: string, userId: string): Promise<{ success: boolean; message: string }> {
  try {
    const res = await fetch(`${PROJECT_API_BASE}/projects/restore/${projectId}`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ userId }),
    });

    if (!res.ok) {
      const error = await res.json();
      throw new Error(error.message || `Failed to restore project: ${res.status}`);
    }

    return await res.json();
  } catch (error) {
    console.error("Error restoring project:", error);
    throw error;
  }
}

/**
 * Permanently delete project from database
 */
export async function permanentDelete(projectId: string, userId: string): Promise<{ success: boolean; message: string }> {
  try {
    const res = await fetch(`${PROJECT_API_BASE}/projects/permanent/${projectId}`, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ userId }),
    });

    if (!res.ok) {
      const error = await res.json();
      throw new Error(error.message || `Failed to permanently delete project: ${res.status}`);
    }

    return await res.json();
  } catch (error) {
    console.error("Error permanently deleting project:", error);
    throw error;
  }
}

/**
 * List all trashed projects for user
 */
export async function listTrash(userId: string): Promise<ProjectsListResponse> {
  try {
    const res = await fetch(`${PROJECT_API_BASE}/projects/trash/user/${userId}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (!res.ok) {
      const error = await res.json();
      throw new Error(error.message || `Failed to list trash: ${res.status}`);
    }

    return await res.json();
  } catch (error) {
    console.error("Error listing trash:", error);
    throw error;
  }
}

/**
 * Update project SEO and OG meta tags
 */
export async function updateSeo(
  projectId: string,
  userId: string,
  seoData: {
    seoTitle?: string;
    seoDescription?: string;
    seoKeywords?: string;
    ogTitle?: string;
    ogDescription?: string;
    ogImageUrl?: string;
  }
): Promise<{ success: boolean; message: string }> {
  try {
    const res = await fetch(`${PROJECT_API_BASE}/projects/${projectId}/seo`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        userId,
        ...seoData,
      }),
    });

    if (!res.ok) {
      const error = await res.json();
      throw new Error(error.message || `Failed to update SEO settings: ${res.status}`);
    }

    return await res.json();
  } catch (error) {
    console.error("Error updating SEO settings:", error);
    throw error;
  }
}

/**
 * Update project theme (colors & fonts)
 */
export async function updateTheme(
  projectId: string,
  userId: string,
  themeData: {
    primaryColor?: string;
    secondaryColor?: string;
    accentColor?: string;
    backgroundColor?: string;
    textColor?: string;
    fontFamily?: string;
    fontSizeBase?: string;
    borderRadius?: string;
  }
): Promise<{ success: boolean; message: string }> {
  try {
    const res = await fetch(`${PROJECT_API_BASE}/projects/${projectId}/theme`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        userId,
        ...themeData,
      }),
    });

    if (!res.ok) {
      const error = await res.json();
      throw new Error(error.message || `Failed to update theme settings: ${res.status}`);
    }

    return await res.json();
  } catch (error) {
    console.error("Error updating theme settings:", error);
    throw error;
  }
}

/**
 * Export project in specified format with theme CSS and SEO tags
 * Supports: html, react, nextjs, zip
 */
export async function exportProject(
  projectId: string,
  userId: string,
  format: 'html' | 'react' | 'nextjs' | 'zip' = 'html'
): Promise<{ success: boolean; data: { downloadUrl: string; fileName: string; fileSize: number }; message: string }> {
  try {
    const res = await fetch(`${PROJECT_API_BASE}/projects/${projectId}/export`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        userId,
        format,
      }),
    });

    if (!res.ok) {
      const error = await res.json();
      throw new Error(error.message || `Failed to export project: ${res.status}`);
    }

    return await res.json();
  } catch (error) {
    console.error("Error exporting project:", error);
    throw error;
  }
}
