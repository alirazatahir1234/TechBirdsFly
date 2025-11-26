import { create } from "zustand";
import { devtools, persist } from "zustand/middleware";
import { toast } from "react-hot-toast";

/**
 * ============================================================================
 * GENERATOR STORE — Zustand Store for AI Website Generator
 * ============================================================================
 *
 * Manages:
 * - Project creation and listing
 * - Status polling for generation jobs
 * - Project preview, download, regeneration
 * - Local cache for performance
 *
 * Integration Points:
 * - /app/dashboard/generator (create new project)
 * - /app/dashboard/projects (list projects)
 * - /app/dashboard/projects/[id] (view/edit/download)
 */

// ============================================================================
// TYPE DEFINITIONS
// ============================================================================

export interface GeneratedArtifact {
  artifactType: string; // "html", "react", "nextjs"
  downloadUrl: string;
  previewUrl?: string;
  generatedAt: string;
}

export interface WebsiteProject {
  projectId: string;
  name: string;
  prompt: string;
  status: "pending" | "processing" | "completed" | "failed";
  progress?: number; // 0-100
  previewUrl?: string;
  htmlContent?: string;
  artifacts: GeneratedArtifact[];
  createdAt: string;
  updatedAt: string;
  errorMessage?: string;
}

export interface GeneratorState {
  // Data
  projects: WebsiteProject[];
  currentProject: WebsiteProject | null;
  pollingJobs: Set<string>; // Track which projects are being polled

  // Loading & Error states
  isLoading: boolean;
  isCreating: boolean;
  isDownloading: boolean;
  error: string | null;

  // Actions
  createProject: (name: string, prompt: string) => Promise<WebsiteProject>;
  listProjects: () => Promise<void>;
  getProject: (id: string) => Promise<WebsiteProject>;
  startPolling: (projectId: string) => void;
  stopPolling: (projectId: string) => void;
  downloadProject: (projectId: string, artifactType: string) => Promise<void>;
  regenerateSection: (projectId: string, sectionId: string) => Promise<void>;
  deleteProject: (projectId: string) => Promise<void>;
  clearError: () => void;
  resetStore: () => void;
}

// ============================================================================
// ZUSTAND STORE
// ============================================================================

const POLLING_INTERVAL = 3000; // Poll every 3 seconds
const POLLING_TIMEOUT = 30 * 60 * 1000; // Stop polling after 30 minutes

export const useGeneratorStore = create<GeneratorState>()(
  devtools(
    persist(
      (set, get) => ({
        projects: [],
        currentProject: null,
        pollingJobs: new Set(),
        isLoading: false,
        isCreating: false,
        isDownloading: false,
        error: null,

        // ====================================================================
        // CREATE PROJECT
        // ====================================================================
        createProject: async (name: string, prompt: string) => {
          set({ isCreating: true, error: null });
          try {
            const response = await fetch("/api/generator/projects", {
              method: "POST",
              headers: { "Content-Type": "application/json" },
              body: JSON.stringify({ name, prompt }),
            });

            if (!response.ok) {
              const errorData = await response.json();
              throw new Error(
                errorData.message || "Failed to create project"
              );
            }

            const project: WebsiteProject = await response.json();

            set((state) => ({
              projects: [project, ...state.projects],
              currentProject: project,
              isCreating: false,
            }));

            // Auto-start polling
            get().startPolling(project.projectId);

            toast.success("🚀 Project created! Starting generation...");
            return project;
          } catch (err) {
            const errorMsg =
              err instanceof Error ? err.message : "Unknown error";
            set({ error: errorMsg, isCreating: false });
            toast.error(errorMsg);
            throw err;
          }
        },

        // ====================================================================
        // LIST PROJECTS
        // ====================================================================
        listProjects: async () => {
          set({ isLoading: true, error: null });
          try {
            const response = await fetch("/api/generator/projects", {
              method: "GET",
            });

            if (!response.ok) {
              console.warn(
                `[listProjects] API returned status ${response.status}`
              );
              // Check if backend is running
              if (response.status === 404) {
                throw new Error(
                  "Backend service unavailable. Is the .NET Generator Service running on port 5500?"
                );
              } else if (response.status >= 500) {
                throw new Error(
                  "Backend service error. Check if the .NET Generator Service is running."
                );
              }
              throw new Error(`Failed to fetch projects (${response.status})`);
            }

            const data = await response.json();
            const projectsList = Array.isArray(data)
              ? data
              : data.projects || [];

            set({ projects: projectsList, isLoading: false });
          } catch (err) {
            const errorMsg =
              err instanceof Error ? err.message : "Unknown error";
            set({ error: errorMsg, isLoading: false, projects: [] });
            console.error("List projects error:", errorMsg);
            // Don't throw - allow UI to show empty state with error message
          }
        },

        // ====================================================================
        // GET SINGLE PROJECT
        // ====================================================================
        getProject: async (id: string) => {
          set({ isLoading: true, error: null });
          try {
            const response = await fetch(`/api/generator/projects/${id}`, {
              method: "GET",
            });

            if (!response.ok) {
              throw new Error("Failed to fetch project");
            }

            const project: WebsiteProject = await response.json();

            set((state) => ({
              currentProject: project,
              projects: state.projects.map((p) =>
                p.projectId === id ? project : p
              ),
              isLoading: false,
            }));

            return project;
          } catch (err) {
            const errorMsg =
              err instanceof Error ? err.message : "Unknown error";
            set({ error: errorMsg, isLoading: false });
            throw err;
          }
        },

        // ====================================================================
        // POLLING: Start polling project status
        // ====================================================================
        startPolling: (projectId: string) => {
          const state = get();

          // Don't start multiple pollers for same project
          if (state.pollingJobs.has(projectId)) {
            return;
          }

          set((s) => ({
            pollingJobs: new Set([...s.pollingJobs, projectId]),
          }));

          const startTime = Date.now();
          const pollInterval = setInterval(async () => {
            const elapsed = Date.now() - startTime;

            // Stop polling after timeout
            if (elapsed > POLLING_TIMEOUT) {
              clearInterval(pollInterval);
              get().stopPolling(projectId);
              toast.error(
                `⏱️ Polling timeout for project ${projectId}. Check status manually.`
              );
              return;
            }

            try {
              const project = await get().getProject(projectId);

              // Stop polling when completed or failed
              if (
                project.status === "completed" ||
                project.status === "failed"
              ) {
                clearInterval(pollInterval);
                get().stopPolling(projectId);

                if (project.status === "completed") {
                  toast.success("✅ Website generation complete!");
                } else {
                  toast.error(
                    `❌ Generation failed: ${project.errorMessage || "Unknown error"}`
                  );
                }
              }
            } catch (err) {
              console.error(`Polling error for ${projectId}:`, err);
              // Continue polling on transient errors
            }
          }, POLLING_INTERVAL);
        },

        // ====================================================================
        // POLLING: Stop polling project status
        // ====================================================================
        stopPolling: (projectId: string) => {
          set((state) => {
            const newPollingJobs = new Set(state.pollingJobs);
            newPollingJobs.delete(projectId);
            return { pollingJobs: newPollingJobs };
          });
        },

        // ====================================================================
        // DOWNLOAD PROJECT ARTIFACT
        // ====================================================================
        downloadProject: async (
          projectId: string,
          artifactType: string
        ) => {
          set({ isDownloading: true, error: null });
          try {
            const response = await fetch(
              `/api/generator/projects/${projectId}/download?type=${artifactType}`,
              { method: "GET" }
            );

            if (!response.ok) {
              throw new Error("Failed to download project");
            }

            const blob = await response.blob();
            const url = URL.createObjectURL(blob);
            const link = document.createElement("a");
            link.href = url;
            link.download = `techbirdsfly-${projectId}-${artifactType}.zip`;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            URL.revokeObjectURL(url);

            toast.success(`✅ Downloaded ${artifactType} version`);
            set({ isDownloading: false });
          } catch (err) {
            const errorMsg =
              err instanceof Error ? err.message : "Unknown error";
            set({ error: errorMsg, isDownloading: false });
            toast.error(errorMsg);
            throw err;
          }
        },

        // ====================================================================
        // REGENERATE SECTION
        // ====================================================================
        regenerateSection: async (projectId: string, sectionId: string) => {
          set({ isLoading: true, error: null });
          try {
            const response = await fetch(
              `/api/generator/projects/${projectId}/regenerate`,
              {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ sectionId }),
              }
            );

            if (!response.ok) {
              throw new Error("Failed to regenerate section");
            }

            const project: WebsiteProject = await response.json();

            set((state) => ({
              currentProject: project,
              projects: state.projects.map((p) =>
                p.projectId === projectId ? project : p
              ),
              isLoading: false,
            }));

            toast.success("🔄 Section regenerated!");
          } catch (err) {
            const errorMsg =
              err instanceof Error ? err.message : "Unknown error";
            set({ error: errorMsg, isLoading: false });
            toast.error(errorMsg);
            throw err;
          }
        },

        // ====================================================================
        // DELETE PROJECT
        // ====================================================================
        deleteProject: async (projectId: string) => {
          set({ isLoading: true, error: null });
          try {
            const response = await fetch(
              `/api/generator/projects/${projectId}`,
              { method: "DELETE" }
            );

            if (!response.ok) {
              throw new Error("Failed to delete project");
            }

            set((state) => ({
              projects: state.projects.filter((p) => p.projectId !== projectId),
              currentProject:
                state.currentProject?.projectId === projectId
                  ? null
                  : state.currentProject,
              isLoading: false,
            }));

            get().stopPolling(projectId);
            toast.success("🗑️ Project deleted");
          } catch (err) {
            const errorMsg =
              err instanceof Error ? err.message : "Unknown error";
            set({ error: errorMsg, isLoading: false });
            toast.error(errorMsg);
            throw err;
          }
        },

        // ====================================================================
        // UTILITIES
        // ====================================================================
        clearError: () => set({ error: null }),

        resetStore: () => {
          set({
            projects: [],
            currentProject: null,
            pollingJobs: new Set(),
            isLoading: false,
            isCreating: false,
            isDownloading: false,
            error: null,
          });
        },
      }),
      {
        name: "generator-store",
        partialize: (state) => ({
          projects: state.projects,
          currentProject: state.currentProject,
        }),
        version: 1,
      }
    )
  )
);
