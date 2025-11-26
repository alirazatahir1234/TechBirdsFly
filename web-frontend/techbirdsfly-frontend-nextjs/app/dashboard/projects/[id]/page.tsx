"use client";

import { useEffect, useState } from "react";
import { useGeneratorStore } from "@/lib/store/generatorStore";
import Link from "next/link";
import {
  Loader2,
  Download,
  RefreshCw,
  ArrowLeft,
  CheckCircle,
  AlertCircle,
  Clock,
  Code,
  Eye,
} from "lucide-react";

interface ProjectDetailsParams {
  params: {
    id: string;
  };
}

export default function ProjectDetailsPage({ params }: ProjectDetailsParams) {
  const { id } = params;
  const { currentProject, getProject, downloadProject, isLoading, isDownloading } =
    useGeneratorStore();
  const [downloadingType, setDownloadingType] = useState<string | null>(null);

  // ========================================================================
  // FETCH PROJECT ON MOUNT & SETUP POLLING
  // ========================================================================
  useEffect(() => {
    // Initial fetch
    getProject(id);

    // Setup polling for status updates
    const pollInterval = setInterval(() => {
      getProject(id);
    }, 3000);

    return () => clearInterval(pollInterval);
  }, [id]);

  // ========================================================================
  // HANDLE DOWNLOAD
  // ========================================================================
  const handleDownload = async (type: string) => {
    try {
      setDownloadingType(type);
      await downloadProject(id, type);
    } catch (err) {
      console.error("Download error:", err);
    } finally {
      setDownloadingType(null);
    }
  };

  // ========================================================================
  // RENDER LOADING STATE
  // ========================================================================
  if (isLoading && !currentProject) {
    return (
      <div className="space-y-6">
        <Link href="/dashboard/projects" className="flex items-center gap-2 text-slate-400 hover:text-white">
          <ArrowLeft className="w-5 h-5" />
          Back to Projects
        </Link>
        <div className="flex items-center justify-center py-24">
          <div className="text-center">
            <Loader2 className="w-12 h-12 animate-spin text-purple-400 mx-auto mb-4" />
            <p className="text-slate-400">Loading project...</p>
          </div>
        </div>
      </div>
    );
  }

  if (!currentProject) {
    return (
      <div className="space-y-6">
        <Link href="/dashboard/projects" className="flex items-center gap-2 text-slate-400 hover:text-white">
          <ArrowLeft className="w-5 h-5" />
          Back to Projects
        </Link>
        <div className="bg-red-900/20 border border-red-700 rounded-lg p-6">
          <div className="flex items-center gap-3">
            <AlertCircle className="w-6 h-6 text-red-400" />
            <div>
              <h3 className="text-white font-semibold">Project not found</h3>
              <p className="text-red-300 text-sm mt-1">
                The project you're looking for doesn't exist or has been deleted.
              </p>
            </div>
          </div>
        </div>
      </div>
    );
  }

  const isComplete = currentProject.status === "completed";
  const isFailed = currentProject.status === "failed";
  const isProcessing = currentProject.status === "processing";

  return (
    <div className="space-y-6">
      {/* HEADER */}
      <div className="flex items-center justify-between">
        <Link href="/dashboard/projects" className="flex items-center gap-2 text-slate-400 hover:text-white transition-colors">
          <ArrowLeft className="w-5 h-5" />
          Back to Projects
        </Link>
        <button
          onClick={() => getProject(id)}
          className="flex items-center gap-2 px-4 py-2 rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-300 transition-colors"
        >
          <RefreshCw className={`w-4 h-4 ${isLoading ? "animate-spin" : ""}`} />
          Refresh
        </button>
      </div>

      {/* PROJECT HEADER */}
      <div className="bg-slate-800 border border-slate-700 rounded-lg p-6">
        <div className="flex items-start justify-between">
          <div>
            <h1 className="text-3xl font-bold text-white mb-2">{currentProject.name}</h1>
            <p className="text-slate-400 text-sm mb-4">{currentProject.prompt}</p>

            {/* STATUS BADGE */}
            <div className="flex items-center gap-2">
              {isProcessing && (
                <div className="inline-flex items-center gap-2 px-3 py-1 bg-yellow-100 text-yellow-800 rounded-full">
                  <Clock className="w-4 h-4 animate-spin" />
                  <span className="text-sm font-medium">Processing</span>
                </div>
              )}
              {isComplete && (
                <div className="inline-flex items-center gap-2 px-3 py-1 bg-green-100 text-green-800 rounded-full">
                  <CheckCircle className="w-4 h-4" />
                  <span className="text-sm font-medium">Complete</span>
                </div>
              )}
              {isFailed && (
                <div className="inline-flex items-center gap-2 px-3 py-1 bg-red-100 text-red-800 rounded-full">
                  <AlertCircle className="w-4 h-4" />
                  <span className="text-sm font-medium">Failed</span>
                </div>
              )}
            </div>
          </div>

          {/* METADATA */}
          <div className="text-right">
            <p className="text-slate-500 text-sm">
              Created: {new Date(currentProject.createdAt).toLocaleDateString()}
            </p>
          </div>
        </div>

        {/* ERROR MESSAGE */}
        {isFailed && currentProject.errorMessage && (
          <div className="mt-4 p-3 bg-red-900/20 border border-red-700 rounded text-red-300 text-sm">
            {currentProject.errorMessage}
          </div>
        )}

        {/* PROGRESS BAR */}
        {isProcessing && currentProject.progress !== undefined && (
          <div className="mt-6">
            <div className="flex justify-between mb-2">
              <span className="text-sm text-slate-300">Generation Progress</span>
              <span className="text-sm font-semibold text-purple-400">
                {currentProject.progress}%
              </span>
            </div>
            <div className="w-full bg-slate-700 rounded-full h-3 overflow-hidden">
              <div
                className="bg-linear-to-r from-purple-600 to-purple-400 h-full transition-all duration-300"
                style={{ width: `${currentProject.progress}%` }}
              />
            </div>
            <p className="text-xs text-slate-400 mt-2">
              This usually takes 30-60 seconds. Page auto-refreshes every 3 seconds.
            </p>
          </div>
        )}
      </div>

      {/* TWO COLUMN LAYOUT */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* LEFT: PREVIEW (2 COLUMNS) */}
        <div className="lg:col-span-2">
          <div className="bg-slate-800 border border-slate-700 rounded-lg overflow-hidden">
            {isComplete && currentProject.previewUrl ? (
              <>
                <div className="bg-slate-900 px-4 py-3 border-b border-slate-700 flex items-center gap-2">
                  <Eye className="w-4 h-4 text-slate-400" />
                  <span className="text-sm text-slate-400">Live Preview</span>
                </div>
                <iframe
                  src={currentProject.previewUrl}
                  className="w-full h-[600px] bg-white"
                  title="Preview"
                  sandbox="allow-scripts allow-same-origin"
                />
              </>
            ) : (
              <div className="h-[600px] flex items-center justify-center bg-slate-900">
                <div className="text-center">
                  {isProcessing && (
                    <>
                      <Loader2 className="w-12 h-12 animate-spin text-purple-400 mx-auto mb-4" />
                      <p className="text-slate-400">Generating website...</p>
                    </>
                  )}
                  {isFailed && (
                    <>
                      <AlertCircle className="w-12 h-12 text-red-400 mx-auto mb-4" />
                      <p className="text-slate-400">Generation failed</p>
                    </>
                  )}
                  {currentProject.status === "pending" && (
                    <>
                      <Clock className="w-12 h-12 text-yellow-400 mx-auto mb-4" />
                      <p className="text-slate-400">Waiting to start...</p>
                    </>
                  )}
                </div>
              </div>
            )}
          </div>
        </div>

        {/* RIGHT: ACTIONS (1 COLUMN) */}
        <div className="space-y-4">
          {/* DOWNLOAD OPTIONS */}
          {isComplete && currentProject.artifacts && currentProject.artifacts.length > 0 && (
            <div className="bg-slate-800 border border-slate-700 rounded-lg p-6 space-y-3">
              <h3 className="font-semibold text-white flex items-center gap-2">
                <Download className="w-5 h-5" />
                Download Code
              </h3>
              <p className="text-sm text-slate-400">
                Choose your preferred framework
              </p>

              {currentProject.artifacts.map((artifact) => (
                <button
                  key={artifact.artifactType}
                  onClick={() => handleDownload(artifact.artifactType)}
                  disabled={downloadingType !== null}
                  className="w-full px-4 py-3 rounded-lg bg-purple-600 hover:bg-purple-700 disabled:opacity-50 text-white font-semibold transition-colors flex items-center justify-center gap-2"
                >
                  {downloadingType === artifact.artifactType ? (
                    <>
                      <Loader2 className="w-4 h-4 animate-spin" />
                      Downloading...
                    </>
                  ) : (
                    <>
                      <Code className="w-4 h-4" />
                      {artifact.artifactType.toUpperCase()}
                    </>
                  )}
                </button>
              ))}
            </div>
          )}

          {/* PROJECT INFO */}
          <div className="bg-slate-800 border border-slate-700 rounded-lg p-6">
            <h3 className="font-semibold text-white mb-4">Project Info</h3>
            <div className="space-y-3 text-sm">
              <div>
                <p className="text-slate-400">Project ID</p>
                <p className="text-slate-300 font-mono text-xs break-all">
                  {currentProject.projectId}
                </p>
              </div>
              <div>
                <p className="text-slate-400">Created</p>
                <p className="text-slate-300">
                  {new Date(currentProject.createdAt).toLocaleString()}
                </p>
              </div>
              <div>
                <p className="text-slate-400">Last Updated</p>
                <p className="text-slate-300">
                  {new Date(currentProject.updatedAt).toLocaleString()}
                </p>
              </div>
            </div>
          </div>

          {/* HELP */}
          <div className="bg-slate-800/50 border border-slate-700 rounded-lg p-4">
            <p className="text-xs text-slate-400">
              💡 <span className="font-semibold">Tip:</span> Generation status updates
              automatically. You can close this page and come back later.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
