"use client";

import { useState } from "react";
import { generateAIImage } from "@/lib/media-api";
import { Loader2, Wand2, AlertCircle, Copy, Check } from "lucide-react";
import toast from "react-hot-toast";

interface AIImageGeneratorProps {
  onGenerated?: (imageData: { base64: string; url: string; prompt: string }) => void;
  onError?: (error: string) => void;
  className?: string;
}

export default function AIImageGenerator({
  onGenerated,
  onError,
  className = "",
}: AIImageGeneratorProps) {
  const [prompt, setPrompt] = useState("");
  const [loading, setLoading] = useState(false);
  const [preview, setPreview] = useState<string>("");
  const [generatedData, setGeneratedData] = useState<any>(null);
  const [copied, setCopied] = useState(false);

  async function handleGenerate() {
    if (!prompt.trim()) {
      const errorMsg = "Please enter a description for the image";
      onError?.(errorMsg);
      toast.error(errorMsg);
      return;
    }

    if (prompt.trim().length < 10) {
      const errorMsg = "Description should be at least 10 characters";
      onError?.(errorMsg);
      toast.error(errorMsg);
      return;
    }

    setLoading(true);

    try {
      const data = await generateAIImage(prompt);
      const base64Image = `data:image/png;base64,${data.base64}`;
      setPreview(base64Image);
      setGeneratedData(data);
      toast.success("✅ Image generated successfully!");
      onGenerated?.({
        base64: data.base64,
        url: data.url,
        prompt,
      });
    } catch (error) {
      const errorMsg = error instanceof Error ? error.message : "Generation failed";
      setPreview("");
      setGeneratedData(null);
      onError?.(errorMsg);
      toast.error(errorMsg);
    } finally {
      setLoading(false);
    }
  }

  function copyPrompt() {
    navigator.clipboard.writeText(prompt);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  }

  return (
    <div className={`space-y-4 ${className}`}>
      {/* Prompt Input */}
      <div className="space-y-2">
        <label className="block text-sm font-medium text-white">
          Image Description
        </label>
        <textarea
          placeholder="Describe the image you want to generate... (e.g., 'A modern dashboard with purple gradient background and glass morphism design')"
          value={prompt}
          onChange={(e) => setPrompt(e.target.value)}
          disabled={loading}
          className="w-full h-24 bg-slate-900 border border-slate-700 rounded-lg p-3 text-white placeholder-slate-500 focus:border-purple-500 focus:outline-none focus:ring-1 focus:ring-purple-500 transition-colors disabled:opacity-50 disabled:cursor-not-allowed resize-none"
        />
        <div className="flex items-center justify-between">
          <span className="text-xs text-slate-400">
            {prompt.length}/200 characters
          </span>
          {prompt.length > 0 && (
            <button
              onClick={copyPrompt}
              className="flex items-center gap-1 text-xs text-slate-400 hover:text-slate-200 transition-colors"
            >
              {copied ? (
                <>
                  <Check className="w-3 h-3" />
                  Copied
                </>
              ) : (
                <>
                  <Copy className="w-3 h-3" />
                  Copy
                </>
              )}
            </button>
          )}
        </div>
      </div>

      {/* Tips */}
      <div className="bg-purple-900/20 border border-purple-700/30 rounded-lg p-3">
        <p className="text-xs text-purple-300">
          💡 <strong>Tip:</strong> Be specific with colors, style, and layout for better results.
        </p>
      </div>

      {/* Generate Button */}
      <button
        onClick={handleGenerate}
        disabled={loading || !prompt.trim()}
        className="w-full flex items-center justify-center gap-2 bg-purple-600 hover:bg-purple-700 disabled:bg-slate-700 disabled:cursor-not-allowed text-white px-4 py-3 rounded-lg font-semibold transition-colors"
      >
        {loading ? (
          <>
            <Loader2 className="w-5 h-5 animate-spin" />
            Generating...
          </>
        ) : (
          <>
            <Wand2 className="w-5 h-5" />
            Generate Image
          </>
        )}
      </button>

      {/* Preview */}
      {preview && (
        <div className="space-y-2">
          <label className="block text-sm font-medium text-white">Preview</label>
          <div className="relative bg-slate-900 rounded-lg overflow-hidden border border-slate-700">
            <img
              src={preview}
              alt="Generated"
              className="w-full h-auto max-h-64 object-contain"
            />
          </div>

          {generatedData && (
            <div className="bg-green-900/20 border border-green-700/50 rounded-lg p-3">
              <div className="flex items-start gap-2">
                <div className="w-5 h-5 rounded-full bg-green-500/30 flex items-center justify-center shrink-0 mt-0.5">
                  <div className="w-2 h-2 rounded-full bg-green-400" />
                </div>
                <div className="text-sm">
                  <p className="text-green-400 font-medium">Generated successfully!</p>
                  <p className="text-green-300/70 text-xs mt-1">
                    Ready to apply to your editor
                  </p>
                </div>
              </div>
            </div>
          )}
        </div>
      )}

      {/* Prompt Display when Generated */}
      {generatedData && (
        <div className="bg-slate-800/50 border border-slate-700 rounded-lg p-3">
          <p className="text-xs text-slate-400 mb-1">Used prompt:</p>
          <p className="text-sm text-slate-300 line-clamp-2">{generatedData.promptUsed}</p>
        </div>
      )}
    </div>
  );
}
