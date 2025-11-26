"use client";

import { useState } from "react";
import ImageUpload from "./image-upload";
import AIImageGenerator from "./ai-image-generator";
import { Upload, Wand2, X, CheckCircle } from "lucide-react";

interface ImageReplaceModalProps {
  isOpen?: boolean;
  onClose?: () => void;
  onReplace: (imageData: {
    type: "upload" | "ai-generated";
    base64: string;
    url?: string;
    prompt?: string;
  }) => void;
  className?: string;
}

export default function ImageReplaceModal({
  isOpen = true,
  onClose,
  onReplace,
  className = "",
}: ImageReplaceModalProps) {
  const [tab, setTab] = useState<"upload" | "ai">("upload");
  const [selected, setSelected] = useState<any>(null);
  const [loading, setLoading] = useState(false);

  function handleUpload(imageData: any) {
    setSelected({
      type: "upload" as const,
      ...imageData,
    });
  }

  function handleGenerate(imageData: any) {
    setSelected({
      type: "ai-generated" as const,
      ...imageData,
    });
  }

  async function handleApply() {
    if (!selected) return;
    
    setLoading(true);
    try {
      onReplace(selected);
      // Reset after successful apply
      setTimeout(() => {
        setSelected(null);
        setTab("upload");
        setLoading(false);
      }, 500);
    } catch (error) {
      setLoading(false);
    }
  }

  function handleReset() {
    setSelected(null);
    setTab("upload");
  }

  if (!isOpen) return null;

  return (
    <div className={`space-y-4 ${className}`}>
      {/* Header */}
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold text-white">Replace Image</h3>
        {onClose && (
          <button
            onClick={onClose}
            className="p-1 hover:bg-slate-700 rounded-lg transition-colors"
          >
            <X className="w-5 h-5 text-slate-400" />
          </button>
        )}
      </div>

      {/* Tab Navigation */}
      <div className="flex gap-2 border-b border-slate-700">
        <button
          onClick={() => {
            setTab("upload");
            setSelected(null);
          }}
          className={`flex items-center gap-2 px-4 py-2 border-b-2 transition-colors ${
            tab === "upload"
              ? "border-purple-500 text-purple-400"
              : "border-transparent text-slate-400 hover:text-white"
          }`}
        >
          <Upload className="w-4 h-4" />
          <span className="text-sm font-medium">Upload</span>
        </button>

        <button
          onClick={() => {
            setTab("ai");
            setSelected(null);
          }}
          className={`flex items-center gap-2 px-4 py-2 border-b-2 transition-colors ${
            tab === "ai"
              ? "border-purple-500 text-purple-400"
              : "border-transparent text-slate-400 hover:text-white"
          }`}
        >
          <Wand2 className="w-4 h-4" />
          <span className="text-sm font-medium">AI Generate</span>
        </button>
      </div>

      {/* Tab Content */}
      <div className="min-h-[400px]">
        {tab === "upload" && (
          <ImageUpload
            onUploaded={handleUpload}
            className="pt-2"
          />
        )}

        {tab === "ai" && (
          <AIImageGenerator
            onGenerated={handleGenerate}
            className="pt-2"
          />
        )}
      </div>

      {/* Action Buttons */}
      {selected && (
        <div className="space-y-3 bg-slate-800/50 border border-slate-700 rounded-lg p-4">
          <div className="flex items-center gap-2 text-green-400 text-sm mb-2">
            <CheckCircle className="w-5 h-5" />
            <span>Image ready to apply</span>
          </div>

          <div className="flex gap-3">
            <button
              onClick={handleReset}
              className="flex-1 px-4 py-2 bg-slate-700 hover:bg-slate-600 text-white rounded-lg font-medium transition-colors"
            >
              Choose Different
            </button>

            <button
              onClick={handleApply}
              disabled={loading}
              className="flex-1 px-4 py-2 bg-purple-600 hover:bg-purple-700 disabled:bg-slate-700 disabled:cursor-not-allowed text-white rounded-lg font-medium transition-colors"
            >
              {loading ? "Applying..." : "Apply to Editor"}
            </button>
          </div>
        </div>
      )}

      {!selected && tab === "upload" && (
        <div className="text-center py-4 text-slate-400 text-sm">
          👆 Upload an image to get started
        </div>
      )}

      {!selected && tab === "ai" && (
        <div className="text-center py-4 text-slate-400 text-sm">
          ✨ Enter a description to generate an image
        </div>
      )}
    </div>
  );
}
