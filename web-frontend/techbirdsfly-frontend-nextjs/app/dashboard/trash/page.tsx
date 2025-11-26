"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { listTrash, restoreProject, permanentDelete } from "@/lib/project-api";
import { useAuthStore } from "@/lib/store/authStore";
import { Loader2, AlertCircle, Trash2, RotateCcw, Zap } from "lucide-react";
import toast from "react-hot-toast";

interface TrashItem {
  id: string;
  name: string;
  industry: string;
  style: string;
  palette: string;
  version: number;
  updatedAt: string;
}

export default function TrashPage() {
  const router = useRouter();
  const { user } = useAuthStore();
  const [items, setItems] = useState<TrashItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadTrash();
  }, [user]);

  const loadTrash = async () => {
    try {
      setIsLoading(true);
      setError(null);

      if (!user?.id) {
        setError("User not authenticated");
        return;
      }

      const response = await listTrash(user.id);
      const trashList = Array.isArray(response) ? response : (response as any).projects || [];
      setItems(trashList);
    } catch (err) {
      console.error("Error loading trash:", err);
      setError(err instanceof Error ? err.message : "Failed to load trash");
      toast.error("Failed to load trash");
    } finally {
      setIsLoading(false);
    }
  };

  const handleRestore = async (itemId: string) => {
    try {
      if (!user?.id) {
        throw new Error("User not authenticated");
      }

      await restoreProject(itemId, user.id);
      setItems((prev) => prev.filter((p) => p.id !== itemId));
      toast.success("Project restored successfully");
    } catch (err) {
      console.error("Error restoring project:", err);
      toast.error("Failed to restore project");
    }
  };

  const handleDeleteForever = async (itemId: string, itemName: string) => {
    if (!window.confirm(`Permanently delete "${itemName}"? This cannot be undone.`)) {
      return;
    }

    try {
      if (!user?.id) {
        throw new Error("User not authenticated");
      }

      await permanentDelete(itemId, user.id);
      setItems((prev) => prev.filter((p) => p.id !== itemId));
      toast.success("Project permanently deleted");
    } catch (err) {
      console.error("Error permanently deleting project:", err);
      toast.error("Failed to permanently delete project");
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
    });
  };

  // ========================================================================
  // RENDER LOADING STATE
  // ========================================================================
  if (isLoading) {
    return (
      <div className="min-h-screen bg-linear-to-br from-gray-50 to-gray-100">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
          <div className="flex flex-col items-center justify-center py-20">
            <Loader2 className="animate-spin text-red-600 mb-4" size={40} />
            <p className="text-gray-600">Loading trash...</p>
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
              <h3 className="font-semibold text-red-900">Error Loading Trash</h3>
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
  if (items.length === 0) {
    return (
      <div className="min-h-screen bg-linear-to-br from-gray-50 to-gray-100">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
          <div className="text-center py-20">
            <Trash2 className="mx-auto text-gray-400 mb-4" size={48} />
            <h3 className="text-xl font-semibold text-gray-900 mb-2">Trash is empty</h3>
            <p className="text-gray-600">Deleted projects will appear here</p>
          </div>
        </div>
      </div>
    );
  }

  // ========================================================================
  // RENDER TRASH ITEMS
  // ========================================================================
  return (
    <div className="min-h-screen bg-linear-to-br from-gray-50 to-gray-100">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        {/* Header */}
        <div className="mb-12">
          <div className="flex items-center gap-2 mb-2">
            <Trash2 size={24} className="text-red-600" />
            <h1 className="text-4xl font-bold text-gray-900">Trash</h1>
          </div>
          <p className="text-gray-600">
            {items.length} item{items.length !== 1 ? "s" : ""} in trash
          </p>
        </div>

        {/* Trash Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {items.map((item) => (
            <div key={item.id} className="bg-white border border-gray-200 rounded-lg p-6 hover:shadow-lg transition-shadow duration-200">
              {/* Header */}
              <div className="mb-4">
                <h3 className="text-lg font-semibold text-gray-900 line-clamp-2">{item.name}</h3>
                <p className="text-sm text-gray-500 mt-1">
                  v{item.version} • {formatDate(item.updatedAt)}
                </p>
              </div>

              {/* Metadata */}
              <div className="space-y-2 mb-4">
                <div className="flex items-center gap-2 text-sm text-gray-600">
                  <span className="capitalize">{item.industry}</span>
                  <span className="text-gray-300">•</span>
                  <span className="capitalize">{item.style}</span>
                </div>
                <div className="flex items-center gap-2 text-sm text-gray-600">
                  <span>Palette: {item.palette}</span>
                </div>
              </div>

              {/* Actions */}
              <div className="flex gap-3 pt-4 border-t border-gray-100">
                <button
                  onClick={() => handleRestore(item.id)}
                  className="flex-1 flex items-center justify-center gap-2 bg-green-600 hover:bg-green-700 text-white font-medium py-2 px-4 rounded-md transition-colors"
                >
                  <RotateCcw size={16} />
                  Restore
                </button>
                <button
                  onClick={() => handleDeleteForever(item.id, item.name)}
                  className="flex-1 flex items-center justify-center gap-2 bg-red-700 hover:bg-red-800 text-white font-medium py-2 px-4 rounded-md transition-colors"
                >
                  <Zap size={16} />
                  Delete
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
