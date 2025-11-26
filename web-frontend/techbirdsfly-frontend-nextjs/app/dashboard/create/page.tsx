"use client";

import { useState } from "react";
import { generateWebsite } from "@/lib/api";
import { createProject } from "@/lib/project-api";
import { useAuthStore } from "@/lib/store/authStore";
import HtmlRenderer from "@/components/html-renderer";
import SectionCard from "@/components/section-card";
import { Loader, AlertCircle, CheckCircle } from "lucide-react";
import toast from "react-hot-toast";
import { CreateFormState, GenerateResponse } from "@/lib/types";

const INDUSTRIES = ["SaaS", "Tech Startup", "E-Commerce", "Portfolio", "Agency"];
const STYLES = ["Modern", "Minimal", "Bold", "Professional", "Creative"];
const PALETTES = ["Purple", "Blue", "Orange", "Green", "Pink"];

export default function CreatePage() {
  const { user } = useAuthStore();
  const [formState, setFormState] = useState<CreateFormState>({
    projectName: "",
    description: "",
    industry: "SaaS",
    features: [],
    colorScheme: "Purple",
    includeContactForm: true,
  });

  const [result, setResult] = useState<GenerateResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  async function handleGenerate() {
    // Validation
    if (!formState.projectName.trim()) {
      setError("Project name is required");
      return;
    }
    if (!formState.description.trim()) {
      setError("Description is required");
      return;
    }

    setError(null);
    setSuccess(false);
    setLoading(true);

    try {
      const response = await generateWebsite({
        projectName: formState.projectName,
        description: formState.description,
        industry: formState.industry,
        features: formState.features,
        colorScheme: formState.colorScheme,
        includeContactForm: formState.includeContactForm,
      });

      if (response.success && response.data) {
        setResult(response.data);
        setSuccess(true);

        // Auto-save project after generation
        if (user?.id && response.data.sections) {
          try {
            // Construct HTML from sections
            const htmlContent = response.data.sections
              .map((section: any) => section.html || "")
              .join("\n");

            await createProject({
              userId: user.id,
              name: formState.projectName,
              industry: formState.industry,
              style: formState.colorScheme,
              palette: formState.colorScheme,
              html: htmlContent,
            });

            toast.success("Website generated and saved!");
          } catch (saveError) {
            console.error("Error saving project:", saveError);
            toast.error("Website generated but failed to save project");
          }
        }
      } else {
        setError(response.message || "Failed to generate website");
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "An error occurred");
    } finally {
      setLoading(false);
    }
  }

  function handleFeatureToggle(feature: string) {
    setFormState((prev) => ({
      ...prev,
      features: prev.features.includes(feature)
        ? prev.features.filter((f) => f !== feature)
        : [...prev.features, feature],
    }));
  }

  return (
    <div className="space-y-8">
      {/* Header */}
      <div>
        <h1 className="text-4xl font-bold text-gray-900 dark:text-white">
          Create Website
        </h1>
        <p className="text-gray-600 dark:text-gray-400 mt-2">
          Generate a professional website using AI
        </p>
      </div>

      {/* Form Section */}
      <div className="grid grid-cols-3 gap-8">
        {/* Left: Form */}
        <div className="col-span-1 space-y-6">
          <div>
            <label className="block text-sm font-semibold text-gray-900 dark:text-white mb-2">
              Project Name *
            </label>
            <input
              type="text"
              value={formState.projectName}
              onChange={(e) =>
                setFormState({ ...formState, projectName: e.target.value })
              }
              placeholder="e.g., My SaaS Product"
              className="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-neutral-700 dark:bg-neutral-800 text-gray-900 dark:text-white focus:ring-2 focus:ring-purple-600 outline-none"
            />
          </div>

          <div>
            <label className="block text-sm font-semibold text-gray-900 dark:text-white mb-2">
              Description *
            </label>
            <textarea
              value={formState.description}
              onChange={(e) =>
                setFormState({ ...formState, description: e.target.value })
              }
              placeholder="Describe your website..."
              rows={4}
              className="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-neutral-700 dark:bg-neutral-800 text-gray-900 dark:text-white focus:ring-2 focus:ring-purple-600 outline-none resize-none"
            />
          </div>

          <div>
            <label className="block text-sm font-semibold text-gray-900 dark:text-white mb-2">
              Industry
            </label>
            <select
              value={formState.industry}
              onChange={(e) =>
                setFormState({ ...formState, industry: e.target.value })
              }
              className="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-neutral-700 dark:bg-neutral-800 text-gray-900 dark:text-white focus:ring-2 focus:ring-purple-600 outline-none"
            >
              {INDUSTRIES.map((ind) => (
                <option key={ind} value={ind}>
                  {ind}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-semibold text-gray-900 dark:text-white mb-2">
              Color Scheme
            </label>
            <div className="flex gap-2">
              {PALETTES.map((palette) => (
                <button
                  key={palette}
                  onClick={() =>
                    setFormState({
                      ...formState,
                      colorScheme: palette,
                    })
                  }
                  className={`px-4 py-2 rounded-lg font-medium text-sm transition-all ${
                    formState.colorScheme === palette
                      ? "ring-2 ring-purple-600 bg-purple-100 dark:bg-purple-900/30"
                      : "border border-gray-300 dark:border-neutral-700 hover:bg-gray-100 dark:hover:bg-neutral-800"
                  }`}
                >
                  {palette}
                </button>
              ))}
            </div>
          </div>

          <div>
            <label className="flex items-center gap-3 cursor-pointer">
              <input
                type="checkbox"
                checked={formState.includeContactForm}
                onChange={(e) =>
                  setFormState({
                    ...formState,
                    includeContactForm: e.target.checked,
                  })
                }
                className="w-4 h-4 rounded"
              />
              <span className="text-sm font-medium text-gray-900 dark:text-white">
                Include Contact Form
              </span>
            </label>
          </div>

          {/* Generate Button */}
          <button
            onClick={handleGenerate}
            disabled={loading}
            className="w-full bg-purple-600 hover:bg-purple-700 disabled:bg-gray-400 text-white font-semibold py-3 px-6 rounded-lg transition-all flex items-center justify-center gap-2"
          >
            {loading && <Loader size={18} className="animate-spin" />}
            {loading ? "Generating..." : "Generate Website"}
          </button>

          {/* Error Alert */}
          {error && (
            <div className="bg-red-50 dark:bg-red-900/20 border border-red-300 dark:border-red-800 rounded-lg p-4 flex items-start gap-3">
              <AlertCircle size={18} className="text-red-600 dark:text-red-400 shrink-0 mt-0.5" />
              <div>
                <p className="font-medium text-red-900 dark:text-red-200">{error}</p>
              </div>
            </div>
          )}

          {/* Success Alert */}
          {success && (
            <div className="bg-green-50 dark:bg-green-900/20 border border-green-300 dark:border-green-800 rounded-lg p-4 flex items-start gap-3">
              <CheckCircle size={18} className="text-green-600 dark:text-green-400 shrink-0 mt-0.5" />
              <div>
                <p className="font-medium text-green-900 dark:text-green-200">
                  Website generated successfully!
                </p>
              </div>
            </div>
          )}
        </div>

        {/* Right: Preview */}
        {result && (
          <div className="col-span-2">
            <div className="sticky top-10">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                Live Preview
              </h2>
              <HtmlRenderer html={result.htmlContent} className="max-h-96 overflow-y-auto" />
              <p className="text-xs text-gray-500 dark:text-gray-400 mt-3">
                Generated at: {new Date(result.generatedAt).toLocaleString()}
              </p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
