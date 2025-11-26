"use client";

import { useRouter } from "next/navigation";
import { Trash2, ExternalLink, Calendar, Tag, Copy, Edit2 } from "lucide-react";
import { useState } from "react";
import toast from "react-hot-toast";

interface ProjectCardProps {
  project: {
    id: string;
    name: string;
    industry: string;
    style: string;
    palette: string;
    version: number;
    updatedAt: string;
    thumbnailUrl?: string;
  };
  onDelete: (projectId: string) => Promise<void>;
  onDuplicate?: (projectId: string) => Promise<void>;
  onRename?: (projectId: string, currentName: string) => Promise<void>;
  onTrash?: (projectId: string) => Promise<void>;
}

export default function ProjectCard({ project, onDelete, onDuplicate, onRename, onTrash }: ProjectCardProps) {
  const router = useRouter();
  const [isDeleting, setIsDeleting] = useState(false);
  const [isDuplicating, setIsDuplicating] = useState(false);
  const [isRenaming, setIsRenaming] = useState(false);
  const [isTrashingDeleting, setIsTrashingDeleting] = useState(false);

  const handleOpen = () => {
    router.push(`/dashboard/editor?project=${project.id}`);
  };

  const handleDelete = async () => {
    if (!window.confirm(`Delete project "${project.name}"? This cannot be undone.`)) {
      return;
    }

    try {
      setIsDeleting(true);
      await onDelete(project.id);
      toast.success("Project deleted successfully");
    } catch (error) {
      console.error("Error deleting project:", error);
      toast.error("Failed to delete project");
    } finally {
      setIsDeleting(false);
    }
  };

  const handleDuplicate = async () => {
    if (!onDuplicate) return;

    try {
      setIsDuplicating(true);
      await onDuplicate(project.id);
      toast.success("Project duplicated successfully");
    } catch (error) {
      console.error("Error duplicating project:", error);
      toast.error("Failed to duplicate project");
    } finally {
      setIsDuplicating(false);
    }
  };

  const handleRename = async () => {
    if (!onRename) return;

    try {
      setIsRenaming(true);
      await onRename(project.id, project.name);
    } catch (error) {
      console.error("Error renaming project:", error);
      toast.error("Failed to rename project");
    } finally {
      setIsRenaming(false);
    }
  };

  const handleMoveToTrash = async () => {
    if (!onTrash) return;

    if (!window.confirm(`Move "${project.name}" to trash?`)) {
      return;
    }

    try {
      setIsTrashingDeleting(true);
      await onTrash(project.id);
      toast.success("Project moved to trash");
    } catch (error) {
      console.error("Error moving project to trash:", error);
      toast.error("Failed to move project to trash");
    } finally {
      setIsTrashingDeleting(false);
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
    });
  };

  return (
    <div className="bg-white border border-gray-200 rounded-lg overflow-hidden hover:shadow-lg transition-shadow duration-200">
      {/* Thumbnail */}
      {project.thumbnailUrl ? (
        <div className="w-full h-40 bg-gray-100 overflow-hidden">
          <img
            src={project.thumbnailUrl}
            alt={project.name}
            className="w-full h-full object-cover hover:scale-105 transition-transform duration-200 cursor-pointer"
            onClick={handleOpen}
            title="Click to open project"
          />
        </div>
      ) : (
        <div className="w-full h-40 bg-gray-100 flex items-center justify-center">
          <div className="text-center">
            <div className="text-gray-400 text-sm font-medium">Generating preview...</div>
          </div>
        </div>
      )}

      {/* Content */}
      <div className="p-6">
        {/* Header */}
        <div className="mb-4">
          <h3 className="text-lg font-semibold text-gray-900 line-clamp-2">{project.name}</h3>
          <p className="text-sm text-gray-500 mt-1">
            v{project.version} • {formatDate(project.updatedAt)}
          </p>
        </div>

        {/* Metadata */}
        <div className="space-y-2 mb-4">
          <div className="flex items-center gap-2 text-sm text-gray-600">
            <Tag size={16} className="text-purple-600" />
            <span className="capitalize">{project.industry}</span>
            <span className="text-gray-300">•</span>
            <span className="capitalize">{project.style}</span>
          </div>
          <div className="flex items-center gap-2 text-sm text-gray-600">
            <Calendar size={16} className="text-blue-600" />
            <span>Palette: {project.palette}</span>
          </div>
        </div>

        {/* Actions */}
        <div className="flex gap-3 pt-4 border-t border-gray-100">
          <button
            onClick={handleOpen}
            className="flex-1 flex items-center justify-center gap-2 bg-purple-600 hover:bg-purple-700 text-white font-medium py-2 px-4 rounded-md transition-colors"
          >
            <ExternalLink size={16} />
            Open
          </button>
          <button
            onClick={handleDuplicate}
            disabled={isDuplicating || !onDuplicate}
            className="px-4 py-2 text-blue-600 hover:bg-blue-50 border border-blue-200 rounded-md transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            title="Duplicate project"
          >
            <Copy size={18} />
          </button>
          <button
            onClick={handleRename}
            disabled={isRenaming || !onRename}
            className="px-4 py-2 text-gray-600 hover:bg-gray-50 border border-gray-200 rounded-md transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            title="Rename project"
          >
            <Edit2 size={18} />
          </button>
          <button
            onClick={handleMoveToTrash}
            disabled={isTrashingDeleting || !onTrash}
            className="px-4 py-2 text-red-600 hover:bg-red-50 border border-red-200 rounded-md transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            title="Move to trash"
          >
            <Trash2 size={18} />
          </button>
        </div>
      </div>
    </div>
  );
}
