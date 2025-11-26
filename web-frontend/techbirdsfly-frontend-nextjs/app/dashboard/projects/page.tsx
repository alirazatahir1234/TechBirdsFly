"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { listProjects, deleteProject, duplicateProject, renameProject, moveToTrash } from "@/lib/project-api";
import { useAuthStore } from "@/lib/store/authStore";
import ProjectCard from "@/components/project-card";
import { Plus, Loader2, AlertCircle, FolderOpen } from "lucide-react";
import toast from "react-hot-toast";

interface Project {
  id: string;
  name: string;
  industry: string;
  style: string;
  palette: string;
  version: number;
  updatedAt: string;
}

export default function ProjectsPage() {
  const router = useRouter();
  const { user, token } = useAuthStore();
  const [projects, setProjects] = useState<Project[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadProjects();
  }, [user]);

  const loadProjects = async () => {
    try {
      setIsLoading(true);
      setError(null);

      if (!user?.id) {
        setError("User not authenticated");
        return;
      }

      const response = await listProjects(user.id);

      // Handle both single response object and paginated response
      const projectsList = Array.isArray(response) ? response : response.projects || [];
      setProjects(projectsList);
    } catch (err) {
      console.error("Error loading projects:", err);
      setError(err instanceof Error ? err.message : "Failed to load projects");
      toast.error("Failed to load projects");
    } finally {
      setIsLoading(false);
    }
  };

  const handleDelete = async (projectId: string) => {
    try {
      await deleteProject(projectId);
      setProjects((prev) => prev.filter((p) => p.id !== projectId));
    } catch (err) {
      console.error("Error deleting project:", err);
      throw err;
    }
  };

  const handleDuplicate = async (projectId: string) => {
    try {
      if (!user?.id) {
        throw new Error("User not authenticated");
      }

      const response = await duplicateProject(projectId, user.id);
      
      // Reload projects to show the new duplicate
      await loadProjects();
    } catch (err) {
      console.error("Error duplicating project:", err);
      throw err;
    }
  };

  const handleRename = async (projectId: string, currentName: string) => {
    const newName = prompt("Enter new project name:", currentName);
    if (!newName || newName === currentName) {
      return;
    }

    try {
      await renameProject(projectId, newName);
      
      // Update the project in the local state
      setProjects((prev) =>
        prev.map((p) =>
          p.id === projectId ? { ...p, name: newName } : p
        )
      );
      
      toast.success("Project renamed successfully");
    } catch (err) {
      console.error("Error renaming project:", err);
      throw err;
    }
  };

  const handleTrash = async (projectId: string) => {
    try {
      if (!user?.id) {
        throw new Error("User not authenticated");
      }

      await moveToTrash(projectId, user.id);
      
      // Remove from list
      setProjects((prev) => prev.filter((p) => p.id !== projectId));
      toast.success("Project moved to trash");
    } catch (err) {
      console.error("Error moving project to trash:", err);
      throw err;
    }
  };

  const handleCreateNew = () => {
    router.push("/dashboard/create");
  };

  // ========================================================================
  // RENDER LOADING STATE
  // ========================================================================
  if (isLoading) {
    return (
      <div className="min-h-screen bg-linear-to-br from-gray-50 to-gray-100">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
          <div className="flex flex-col items-center justify-center py-20">
            <Loader2 className="animate-spin text-purple-600 mb-4" size={40} />
            <p className="text-gray-600">Loading projects...</p>
          </div>
        </div>
      </div>
    );
  }

  // ========================================================================
  // RENDER ERROR STATE
  // ========================================================================
  if (error) {
    return (
      <div className="min-h-screen bg-linear-to-br from-gray-50 to-gray-100">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
          <div className="bg-red-50 border border-red-200 rounded-lg p-6 flex items-start gap-4">
            <AlertCircle className="text-red-600 shrink-0 mt-0.5" size={20} />
            <div>
              <h3 className="font-semibold text-red-900">Error Loading Projects</h3>
              <p className="text-red-700 text-sm mt-1">{error}</p>
            </div>
          </div>
        </div>
      </div>
    );
  }

  // ========================================================================
  // RENDER EMPTY STATE
  // ========================================================================
  if (projects.length === 0) {
    return (
      <div className="min-h-screen bg-linear-to-br from-gray-50 to-gray-100">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
          <div className="text-center py-20">
            <FolderOpen className="mx-auto text-gray-400 mb-4" size={48} />
            <h3 className="text-xl font-semibold text-gray-900 mb-2">No projects yet</h3>
            <p className="text-gray-600 mb-6">Create your first AI-generated website to get started</p>
            <button
              onClick={handleCreateNew}
              className="inline-flex items-center gap-2 bg-purple-600 hover:bg-purple-700 text-white font-semibold py-3 px-6 rounded-lg transition-colors"
            >
              <Plus size={20} />
              Create First Project
            </button>
          </div>
        </div>
      </div>
    );
  }

  // ========================================================================
  // RENDER PROJECTS GRID
  // ========================================================================
  return (
    <div className="min-h-screen bg-linear-to-br from-gray-50 to-gray-100">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        {/* Header */}
        <div className="flex justify-between items-center mb-12">
          <div>
            <h1 className="text-4xl font-bold text-gray-900">Projects</h1>
            <p className="text-gray-600 mt-2">
              {projects.length} project{projects.length !== 1 ? "s" : ""}
            </p>
          </div>
          <button
            onClick={handleCreateNew}
            className="flex items-center gap-2 bg-purple-600 hover:bg-purple-700 text-white font-semibold py-3 px-6 rounded-lg transition-colors shadow-lg hover:shadow-xl"
          >
            <Plus size={20} />
            Create New
          </button>
        </div>

        {/* Projects Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {projects.map((project) => (
            <ProjectCard 
              key={project.id} 
              project={project} 
              onDelete={handleDelete} 
              onDuplicate={handleDuplicate} 
              onRename={handleRename}
              onTrash={handleTrash}
            />
          ))}
        </div>
      </div>
    </div>
  );
}
