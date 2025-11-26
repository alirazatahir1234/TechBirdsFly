"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useGeneratorStore } from "@/lib/store/generatorStore";
import { Loader2, Sparkles, ArrowRight } from "lucide-react";

export default function GeneratorPage() {
  const router = useRouter();
  const [projectName, setProjectName] = useState("");
  const [prompt, setPrompt] = useState("");
  const [nameError, setNameError] = useState("");
  const [promptError, setPromptError] = useState("");

  const { createProject, isCreating } = useGeneratorStore();

  // ========================================================================
  // VALIDATION
  // ========================================================================
  const validateForm = (): boolean => {
    let isValid = true;

    // Validate name
    if (!projectName.trim()) {
      setNameError("Project name is required");
      isValid = false;
    } else if (projectName.trim().length < 3) {
      setNameError("Project name must be at least 3 characters");
      isValid = false;
    } else {
      setNameError("");
    }

    // Validate prompt
    if (!prompt.trim()) {
      setPromptError("Please describe your website");
      isValid = false;
    } else if (prompt.trim().length < 20) {
      setPromptError("Prompt must be at least 20 characters");
      isValid = false;
    } else {
      setPromptError("");
    }

    return isValid;
  };

  // ========================================================================
  // HANDLE GENERATION
  // ========================================================================
  const handleGenerate = async () => {
    if (!validateForm()) {
      return;
    }

    try {
      const project = await createProject(projectName, prompt);

      // Redirect to project details page
      setTimeout(() => {
        router.push(`/dashboard/projects/${project.projectId}`);
      }, 1000);
    } catch (err) {
      console.error("Generation error:", err);
    }
  };

  const isFormValid = projectName.trim().length >= 3 && prompt.trim().length >= 20;

  return (
    <div className="min-h-screen bg-linear-to-br from-slate-900 to-slate-800 py-16 px-4">
      <div className="max-w-2xl mx-auto">
        {/* HEADER */}
        <div className="text-center mb-12">
          <div className="flex items-center justify-center gap-3 mb-4">
            <div className="p-3 bg-purple-600/20 rounded-lg">
              <Sparkles className="w-8 h-8 text-purple-400" />
            </div>
            <h1 className="text-4xl font-bold text-white">Website Generator</h1>
          </div>
          <p className="text-slate-400 text-lg">
            Describe your dream website and let AI build it in seconds
          </p>
        </div>

        {/* FORM CARD */}
        <div className="bg-slate-800 rounded-xl p-8 space-y-6 border border-slate-700">
          {/* PROJECT NAME INPUT */}
          <div className="space-y-3">
            <label className="block text-sm font-semibold text-white">
              Project Name
            </label>
            <input
              type="text"
              value={projectName}
              onChange={(e) => {
                setProjectName(e.target.value);
                if (nameError) setNameError("");
              }}
              placeholder="E.g., My SaaS Landing Page"
              maxLength={100}
              className={`w-full px-4 py-3 rounded-lg bg-slate-700 border-2 transition-colors
                ${
                  nameError
                    ? "border-red-500 focus:border-red-600"
                    : "border-slate-600 focus:border-purple-500"
                }
                text-white placeholder-slate-400 focus:outline-none`}
            />
            {nameError && (
              <p className="text-sm text-red-400">{nameError}</p>
            )}
            <p className="text-xs text-slate-400">
              {projectName.length}/100 characters
            </p>
          </div>

          {/* PROMPT TEXTAREA */}
          <div className="space-y-3">
            <label className="block text-sm font-semibold text-white">
              Website Description
            </label>
            <textarea
              value={prompt}
              onChange={(e) => {
                setPrompt(e.target.value);
                if (promptError) setPromptError("");
              }}
              placeholder="Describe your website... E.g., A modern SaaS landing page for a productivity app with hero section, features, pricing table, testimonials, and CTA"
              maxLength={2000}
              rows={8}
              className={`w-full px-4 py-3 rounded-lg bg-slate-700 border-2 transition-colors resize-none
                ${
                  promptError
                    ? "border-red-500 focus:border-red-600"
                    : "border-slate-600 focus:border-purple-500"
                }
                text-white placeholder-slate-400 focus:outline-none`}
            />
            {promptError && (
              <p className="text-sm text-red-400">{promptError}</p>
            )}
            <p className="text-xs text-slate-400">
              {prompt.length}/2000 characters
              {prompt.length >= 20 && (
                <span className="ml-2 text-green-400">✓ Good length</span>
              )}
            </p>
          </div>

          {/* HELP TEXT */}
          <div className="bg-slate-700/50 rounded-lg p-4 border border-slate-600">
            <p className="text-sm text-slate-300">
              <span className="font-semibold">💡 Tips for better results:</span>
            </p>
            <ul className="text-sm text-slate-400 mt-2 space-y-1 ml-4">
              <li>• Be specific about your industry and target audience</li>
              <li>• Describe the sections you want (hero, features, pricing, etc)</li>
              <li>• Mention design preferences (modern, minimal, bold, etc)</li>
              <li>• Include any specific colors or branding guidelines</li>
            </ul>
          </div>

          {/* GENERATE BUTTON */}
          <button
            onClick={handleGenerate}
            disabled={!isFormValid || isCreating}
            className={`w-full py-4 rounded-lg font-semibold text-white flex items-center justify-center gap-2 transition-all
              ${
                isFormValid && !isCreating
                  ? "bg-purple-600 hover:bg-purple-700 cursor-pointer"
                  : "bg-slate-600 cursor-not-allowed opacity-50"
              }`}
          >
            {isCreating ? (
              <>
                <Loader2 className="w-5 h-5 animate-spin" />
                <span>Creating project...</span>
              </>
            ) : (
              <>
                <Sparkles className="w-5 h-5" />
                <span>Generate Website</span>
                <ArrowRight className="w-5 h-5" />
              </>
            )}
          </button>

          {/* INFO */}
          <p className="text-xs text-slate-500 text-center">
            Generation typically takes 30-60 seconds. You'll be redirected to
            your project dashboard where you can monitor progress.
          </p>
        </div>

        {/* FEATURES */}
        <div className="grid grid-cols-3 gap-4 mt-12">
          <div className="text-center">
            <div className="text-3xl font-bold text-purple-400">⚡</div>
            <p className="text-sm text-slate-400 mt-2">Fast Generation</p>
          </div>
          <div className="text-center">
            <div className="text-3xl font-bold text-purple-400">🎨</div>
            <p className="text-sm text-slate-400 mt-2">Professional Design</p>
          </div>
          <div className="text-center">
            <div className="text-3xl font-bold text-purple-400">📥</div>
            <p className="text-sm text-slate-400 mt-2">Download Code</p>
          </div>
        </div>
      </div>
    </div>
  );
}
