"use client";

import { useRouter } from "next/navigation";
import { Trash2, ExternalLink, Calendar, Tag, Copy, Edit2, Image as ImageIcon } from "lucide-react";
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
}

export default function ProjectCard({ project, onDelete, onDuplicate, onRename }: ProjectCardProps) {
  const router = useRouter();
  const [isDeleting, setIsDeleting] = useState(false);
  const [isDuplicating, setIsDuplicating] = useState(false);
  const [isRenaming, setIsRenaming] = useState(false);
  const [thumbnailLoading, setThumbnailLoading] = useState(true);

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

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
    });
  };

  return (
    <div className="bg-white border border-gray-200 rounded-lg overflow-hidden hover:shadow-lg transition-shadow duration-200">
      {/* THUMBNAIL IMAGE - NEW */}
      {project.thumbnailUrl ? (
        <div className="relative w-full h-40 bg-gray-100 overflow-hidden group cursor-pointer" onClick={handleOpen}>
          <img
            src={project.thumbnailUrl}
            alt={project.name}
            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
            onLoad={() => setThumbnailLoading(false)}
          />
          {/* HOVER OVERLAY */}
          <div className="absolute inset-0 bg-black bg-opacity-0 group-hover:bg-opacity-20 transition-all duration-200 flex items-center justify-center">
            <ExternalLink className="text-white opacity-0 group-hover:opacity-100 transition-opacity" size={24} />
          </div>
        </div>
      ) : (
        <div className="w-full h-40 bg-gradient-to-b from-gray-50 to-gray-100 flex flex-col items-center justify-center text-sm text-gray-400">
          <ImageIcon size={32} className="mb-2 opacity-40" />
          <span>Generating preview...</span>
        </div>
      )}

      {/* CONTENT SECTION */}
      <div className="p-4">
        {/* HEADER */}
        <div className="mb-3">
          <h3 className="text-base font-semibold text-gray-900 line-clamp-2">{project.name}</h3>
          <p className="text-xs text-gray-500 mt-1">
            v{project.version} • {formatDate(project.updatedAt)}
          </p>
        </div>

        {/* METADATA */}
        <div className="space-y-1.5 mb-4">
          <div className="flex items-center gap-2 text-xs text-gray-600">
            <Tag size={14} className="text-purple-600 flex-shrink-0" />
            <span className="capitalize line-clamp-1">{project.industry}</span>
            <span className="text-gray-300">•</span>
            <span className="capitalize line-clamp-1">{project.style}</span>
          </div>
          <div className="flex items-center gap-2 text-xs text-gray-600">
            <Calendar size={14} className="text-blue-600 flex-shrink-0" />
            <span className="line-clamp-1">Palette: {project.palette}</span>
          </div>
        </div>

        {/* ACTIONS */}
        <div className="flex gap-2 pt-3 border-t border-gray-100">
          <button
            onClick={handleOpen}
            className="flex-1 flex items-center justify-center gap-1.5 bg-purple-600 hover:bg-purple-700 text-white font-medium py-2 px-3 rounded-md transition-colors text-xs"
          >
            <ExternalLink size={14} />
            Open
          </button>
          <button
            onClick={handleDuplicate}
            disabled={isDuplicating || !onDuplicate}
            className="px-3 py-2 text-blue-600 hover:bg-blue-50 border border-blue-200 rounded-md transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            title="Duplicate project"
          >
            <Copy size={16} />
          </button>
          <button
            onClick={handleRename}
            disabled={isRenaming || !onRename}
            className="px-3 py-2 text-gray-600 hover:bg-gray-50 border border-gray-200 rounded-md transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            title="Rename project"
          >
            <Edit2 size={16} />
          </button>
          <button
            onClick={handleDelete}
            disabled={isDeleting}
            className="px-3 py-2 text-red-600 hover:bg-red-50 border border-red-200 rounded-md transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            title="Delete project"
          >
            <Trash2 size={16} />
          </button>
        </div>
      </div>
    </div>
  );
}
