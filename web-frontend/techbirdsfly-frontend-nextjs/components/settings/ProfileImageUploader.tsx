"use client";

import { useState, useRef } from "react";
import toast from "react-hot-toast";
import { Button } from "@/components/ui/button";
import { Cloud } from "lucide-react";
import Image from "next/image";

interface ProfileImageUploaderProps {
  user?: any;
}

export default function ProfileImageUploader({ user }: ProfileImageUploaderProps) {
  const [preview, setPreview] = useState<string | null>(user?.profileImageUrl || null);
  const [isUploading, setIsUploading] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (event) => {
        setPreview(event.target?.result as string);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleUpload = async () => {
    if (!preview) return;
    
    // Upload handled by parent page component
  };

  return (
    <div className="space-y-4">
      {/* Preview Image */}
      <div className="flex justify-center">
        {preview ? (
          <Image
            src={preview}
            alt="Profile"
            width={100}
            height={100}
            className="w-24 h-24 rounded-full object-cover border-4 border-purple-200"
          />
        ) : (
          <div className="w-24 h-24 rounded-full bg-gray-100 border-4 border-gray-200 flex items-center justify-center">
            <Cloud className="w-8 h-8 text-gray-400" />
          </div>
        )}
      </div>

      {/* Upload Area */}
      <div
        className="border-2 border-dashed border-gray-300 rounded-lg p-6 text-center cursor-pointer hover:border-purple-400 hover:bg-purple-50 transition"
        onClick={() => fileInputRef.current?.click()}
      >
        <Cloud className="w-8 h-8 mx-auto text-gray-400 mb-2" />
        <p className="text-sm text-gray-600 font-medium">Click to upload or drag and drop</p>
        <p className="text-xs text-gray-500">SVG, PNG, JPG or GIF (max. 800×400px)</p>
        <input
          ref={fileInputRef}
          type="file"
          accept="image/*"
          onChange={handleFileSelect}
          className="hidden"
        />
      </div>

      {/* Upload Button */}
      <Button
        disabled={!preview}
        className="w-full bg-purple-600 hover:bg-purple-700 text-white"
      >
        Photo Ready
      </Button>
    </div>
  );
}
