"use client";

import { useState, useRef } from "react";
import { uploadImage } from "@/lib/media-api";
import { Upload, Loader2, CheckCircle, AlertCircle, Image as ImageIcon } from "lucide-react";
import toast from "react-hot-toast";

interface ImageUploadProps {
  onUploaded?: (imageData: { id: string; url: string; base64: string }) => void;
  onError?: (error: string) => void;
  className?: string;
}

export default function ImageUpload({
  onUploaded,
  onError,
  className = "",
}: ImageUploadProps) {
  const [loading, setLoading] = useState(false);
  const [preview, setPreview] = useState<string>("");
  const [fileName, setFileName] = useState<string>("");
  const [uploadedData, setUploadedData] = useState<any>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  async function handleUpload(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;

    // Validate file type
    if (!file.type.startsWith("image/")) {
      const errorMsg = "Please upload an image file (PNG, JPG, GIF, WebP)";
      onError?.(errorMsg);
      toast.error(errorMsg);
      return;
    }

    // Validate file size (max 10MB)
    const maxSize = 10 * 1024 * 1024;
    if (file.size > maxSize) {
      const errorMsg = "File size must be less than 10MB";
      onError?.(errorMsg);
      toast.error(errorMsg);
      return;
    }

    // Create preview
    const reader = new FileReader();
    reader.onload = (e) => {
      setPreview(e.target?.result as string);
    };
    reader.readAsDataURL(file);

    setFileName(file.name);
    setLoading(true);

    try {
      const data = await uploadImage(file);
      setUploadedData(data);
      toast.success("✅ Image uploaded successfully!");
      onUploaded?.(data);
    } catch (error) {
      const errorMsg = error instanceof Error ? error.message : "Upload failed";
      setPreview("");
      setFileName("");
      onError?.(errorMsg);
      toast.error(errorMsg);
    } finally {
      setLoading(false);
      // Reset input
      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
    }
  }

  return (
    <div className={`space-y-4 ${className}`}>
      {/* Upload Input */}
      <div
        onClick={() => fileInputRef.current?.click()}
        className="relative border-2 border-dashed border-slate-600 rounded-lg p-8 text-center cursor-pointer hover:border-purple-500 hover:bg-purple-500/5 transition-all"
      >
        <input
          ref={fileInputRef}
          type="file"
          accept="image/*"
          className="hidden"
          onChange={handleUpload}
          disabled={loading}
        />

        {loading ? (
          <div className="space-y-2">
            <Loader2 className="w-8 h-8 animate-spin text-purple-500 mx-auto" />
            <p className="text-sm text-slate-400">Uploading...</p>
          </div>
        ) : (
          <div className="space-y-2">
            <Upload className="w-8 h-8 text-slate-500 mx-auto" />
            <div>
              <p className="text-sm font-medium text-white">
                Click to upload or drag and drop
              </p>
              <p className="text-xs text-slate-400">PNG, JPG, GIF, WebP up to 10MB</p>
            </div>
          </div>
        )}
      </div>

      {/* Preview */}
      {preview && (
        <div className="space-y-2">
          <div className="relative bg-slate-900 rounded-lg overflow-hidden border border-slate-700">
            <img
              src={preview}
              alt={fileName}
              className="w-full h-auto max-h-64 object-contain"
            />
          </div>

          <div className="flex items-center justify-between text-sm">
            <span className="text-slate-400 line-clamp-1">📄 {fileName}</span>
            {uploadedData && (
              <div className="flex items-center gap-1 text-green-400">
                <CheckCircle className="w-4 h-4" />
                <span>Uploaded</span>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Upload Success Info */}
      {uploadedData && (
        <div className="bg-green-900/20 border border-green-700/50 rounded-lg p-3">
          <div className="flex items-start gap-2">
            <CheckCircle className="w-5 h-5 text-green-400 shrink-0 mt-0.5" />
            <div className="text-sm">
              <p className="text-green-400 font-medium">Upload successful!</p>
              <p className="text-green-300/70 text-xs mt-1">
                Image ID: <code className="bg-green-950 px-1 rounded">{uploadedData.id}</code>
              </p>
            </div>
          </div>
        </div>
      )}

      {/* Error State Example */}
      {fileName && !uploadedData && !loading && (
        <div className="bg-yellow-900/20 border border-yellow-700/50 rounded-lg p-3">
          <div className="flex items-start gap-2">
            <AlertCircle className="w-5 h-5 text-yellow-400 shrink-0 mt-0.5" />
            <div className="text-sm">
              <p className="text-yellow-400 font-medium">Waiting for upload...</p>
              <p className="text-yellow-300/70 text-xs mt-1">
                Click the upload area to start the upload process
              </p>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
