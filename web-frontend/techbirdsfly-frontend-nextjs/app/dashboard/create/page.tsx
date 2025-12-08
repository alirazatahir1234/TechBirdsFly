"use client";

import { useState } from "react";
import { generateWebsite } from "@/lib/api";
import { createProject } from "@/lib/project-api";
import { useAuthStore } from "@/lib/store/authStore";
import HtmlRenderer from "@/components/html-renderer";
import SectionCard from "@/components/section-card";
import { Loader, AlertCircle, CheckCircle, ArrowRight, Sparkles, Code2, Palette, Settings } from "lucide-react";
import toast from "react-hot-toast";
import { CreateFormState, GenerateResponse } from "@/lib/types";

const INDUSTRIES = ["SaaS", "Tech Startup", "E-Commerce", "Portfolio", "Agency"];
const STYLES = ["Modern", "Minimal", "Bold", "Professional", "Creative"];
const PALETTES = [
  { name: "Purple", bg: "from-purple-400 to-purple-600" },
  { name: "Blue", bg: "from-blue-400 to-blue-600" },
  { name: "Orange", bg: "from-orange-400 to-orange-600" },
  { name: "Green", bg: "from-green-400 to-green-600" },
  { name: "Pink", bg: "from-pink-400 to-pink-600" },
];
const FEATURES = [
  { id: "hero", label: "Hero Section", icon: "✨" },
  { id: "pricing", label: "Pricing Table", icon: "💰" },
  { id: "testimonials", label: "Testimonials", icon: "⭐" },
  { id: "faq", label: "FAQ Section", icon: "❓" },
];

const STEP_LABELS = [
  { num: 1, title: "Project Details", icon: Code2 },
  { num: 2, title: "Style & Branding", icon: Palette },
  { num: 3, title: "Features", icon: Settings },
  { num: 4, title: "Review & Generate", icon: Sparkles },
];

export default function CreatePage() {
  const { user } = useAuthStore();
  const [currentStep, setCurrentStep] = useState(1);
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
  const [showAdvanced, setShowAdvanced] = useState(false);

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
        setCurrentStep(4);

        // Auto-save project after generation
        if (user?.id && response.data.sections) {
          try {
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

  const canProceedStep1 = formState.projectName.trim() && formState.description.trim();
  const canProceedStep2 = true;
  const canProceedStep3 = true;

  return (
    <div className="space-y-8 pb-12">
      {/* Hero Header */}
      <div className="relative overflow-hidden rounded-2xl bg-gradient-to-r from-purple-600 via-purple-500 to-pink-500 p-12 text-white shadow-lg">
        <div className="relative z-10">
          <div className="flex items-center gap-3 mb-4">
            <Sparkles size={32} className="animate-pulse" />
            <h1 className="text-5xl font-bold">Create Your Website</h1>
          </div>
          <p className="text-xl text-purple-100 max-w-2xl">
            Generate a professional, stunning website powered by AI in just a few simple steps
          </p>
        </div>
        <div className="absolute -right-20 -top-20 w-40 h-40 bg-white/10 rounded-full blur-3xl" />
        <div className="absolute -left-20 -bottom-20 w-40 h-40 bg-white/10 rounded-full blur-3xl" />
      </div>

      {/* Progress Steps */}
      <div>
        <div className="grid grid-cols-4 gap-4 mb-8">
          {STEP_LABELS.map((step, idx) => {
            const StepIcon = step.icon;
            const isActive = currentStep === step.num;
            const isCompleted = currentStep > step.num;

            return (
              <div key={step.num} className="flex items-center">
                <div className="flex flex-col items-center flex-1">
                  <div
                    className={`w-14 h-14 rounded-full flex items-center justify-center font-bold text-lg transition-all duration-300 ${
                      isActive
                        ? "bg-purple-600 text-white scale-110 shadow-lg"
                        : isCompleted
                        ? "bg-green-500 text-white"
                        : "bg-gray-200 dark:bg-gray-700 text-gray-600 dark:text-gray-400"
                    }`}
                  >
                    {isCompleted ? (
                      <CheckCircle size={24} />
                    ) : (
                      <StepIcon size={24} />
                    )}
                  </div>
                  <p className="text-xs font-semibold mt-2 text-center text-gray-700 dark:text-gray-300">
                    {step.title}
                  </p>
                </div>
                {idx < STEP_LABELS.length - 1 && (
                  <div
                    className={`w-8 h-1 mx-2 rounded transition-all ${
                      currentStep > step.num
                        ? "bg-green-500"
                        : currentStep === step.num
                        ? "bg-purple-600"
                        : "bg-gray-200 dark:bg-gray-700"
                    }`}
                  />
                )}
              </div>
            );
          })}
        </div>
      </div>

      {/* Main Content */}
      <div className="grid grid-cols-3 gap-8">
        {/* Left: Form Content */}
        <div className="col-span-1 space-y-6">
          {/* Step 1: Project Details */}
          {currentStep === 1 && (
            <div className="space-y-6 animate-fadeIn">
              <div>
                <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-1">
                  Project Details
                </h2>
                <p className="text-sm text-gray-600 dark:text-gray-400 mb-6">
                  Tell us about your project to get started
                </p>
              </div>

              <div className="bg-white dark:bg-neutral-800 rounded-xl p-6 border border-gray-200 dark:border-neutral-700 shadow-sm">
                <label className="block text-sm font-semibold text-gray-900 dark:text-white mb-3">
                  Project Name <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={formState.projectName}
                  onChange={(e) =>
                    setFormState({ ...formState, projectName: e.target.value })
                  }
                  placeholder="e.g., My Awesome SaaS"
                  className="w-full px-4 py-3 rounded-lg border border-gray-300 dark:border-neutral-600 dark:bg-neutral-700 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:ring-2 focus:ring-purple-500 focus:border-transparent outline-none transition-all"
                />
              </div>

              <div className="bg-white dark:bg-neutral-800 rounded-xl p-6 border border-gray-200 dark:border-neutral-700 shadow-sm">
                <label className="block text-sm font-semibold text-gray-900 dark:text-white mb-3">
                  Description <span className="text-red-500">*</span>
                </label>
                <textarea
                  value={formState.description}
                  onChange={(e) =>
                    setFormState({ ...formState, description: e.target.value })
                  }
                  placeholder="Describe what your website does, who it's for, and what makes it special..."
                  rows={5}
                  className="w-full px-4 py-3 rounded-lg border border-gray-300 dark:border-neutral-600 dark:bg-neutral-700 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:ring-2 focus:ring-purple-500 focus:border-transparent outline-none transition-all resize-none"
                />
              </div>

              <button
                onClick={() => currentStep < 4 && setCurrentStep(2)}
                disabled={!canProceedStep1}
                className="w-full bg-purple-600 hover:bg-purple-700 disabled:bg-gray-300 dark:disabled:bg-gray-700 text-white font-semibold py-3 px-6 rounded-lg transition-all flex items-center justify-center gap-2 group"
              >
                Next Step
                <ArrowRight size={18} className="group-hover:translate-x-1 transition-transform" />
              </button>
            </div>
          )}

          {/* Step 2: Style & Branding */}
          {currentStep === 2 && (
            <div className="space-y-6 animate-fadeIn">
              <div>
                <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-1">
                  Style & Branding
                </h2>
                <p className="text-sm text-gray-600 dark:text-gray-400 mb-6">
                  Choose your design style and color palette
                </p>
              </div>

              <div className="bg-white dark:bg-neutral-800 rounded-xl p-6 border border-gray-200 dark:border-neutral-700 shadow-sm">
                <label className="block text-sm font-semibold text-gray-900 dark:text-white mb-4">
                  Industry
                </label>
                <select
                  value={formState.industry}
                  onChange={(e) =>
                    setFormState({ ...formState, industry: e.target.value })
                  }
                  className="w-full px-4 py-3 rounded-lg border border-gray-300 dark:border-neutral-600 dark:bg-neutral-700 text-gray-900 dark:text-white focus:ring-2 focus:ring-purple-500 focus:border-transparent outline-none transition-all"
                >
                  {INDUSTRIES.map((ind) => (
                    <option key={ind} value={ind}>
                      {ind}
                    </option>
                  ))}
                </select>
              </div>

              <div className="bg-white dark:bg-neutral-800 rounded-xl p-6 border border-gray-200 dark:border-neutral-700 shadow-sm">
                <label className="block text-sm font-semibold text-gray-900 dark:text-white mb-4">
                  Color Palette
                </label>
                <div className="grid grid-cols-2 gap-3">
                  {PALETTES.map((palette) => (
                    <button
                      key={palette.name}
                      onClick={() =>
                        setFormState({
                          ...formState,
                          colorScheme: palette.name,
                        })
                      }
                      className={`relative overflow-hidden rounded-lg p-4 transition-all duration-300 transform hover:scale-105 ${
                        formState.colorScheme === palette.name
                          ? "ring-2 ring-purple-600 shadow-lg scale-105"
                          : "hover:shadow-md"
                      }`}
                    >
                      <div
                        className={`w-full h-12 rounded-lg bg-gradient-to-r ${palette.bg} mb-2`}
                      />
                      <p className="text-sm font-medium text-gray-900 dark:text-white">
                        {palette.name}
                      </p>
                    </button>
                  ))}
                </div>
              </div>

              <div className="flex gap-3">
                <button
                  onClick={() => setCurrentStep(1)}
                  className="flex-1 bg-gray-200 hover:bg-gray-300 dark:bg-neutral-700 dark:hover:bg-neutral-600 text-gray-900 dark:text-white font-semibold py-3 px-6 rounded-lg transition-all"
                >
                  Back
                </button>
                <button
                  onClick={() => canProceedStep2 && setCurrentStep(3)}
                  className="flex-1 bg-purple-600 hover:bg-purple-700 text-white font-semibold py-3 px-6 rounded-lg transition-all flex items-center justify-center gap-2 group"
                >
                  Next Step
                  <ArrowRight size={18} className="group-hover:translate-x-1 transition-transform" />
                </button>
              </div>
            </div>
          )}

          {/* Step 3: Features */}
          {currentStep === 3 && (
            <div className="space-y-6 animate-fadeIn">
              <div>
                <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-1">
                  Features & Options
                </h2>
                <p className="text-sm text-gray-600 dark:text-gray-400 mb-6">
                  Select the sections you'd like to include
                </p>
              </div>

              <div className="bg-white dark:bg-neutral-800 rounded-xl p-6 border border-gray-200 dark:border-neutral-700 shadow-sm space-y-3">
                {FEATURES.map((feature) => (
                  <label
                    key={feature.id}
                    className="flex items-center p-3 rounded-lg border border-gray-200 dark:border-neutral-700 hover:bg-gray-50 dark:hover:bg-neutral-700/50 cursor-pointer transition-all"
                  >
                    <input
                      type="checkbox"
                      checked={formState.features.includes(feature.id)}
                      onChange={() => handleFeatureToggle(feature.id)}
                      className="w-5 h-5 rounded border-gray-300 text-purple-600 focus:ring-2 focus:ring-purple-500"
                    />
                    <span className="ml-3 text-lg">{feature.icon}</span>
                    <span className="ml-3 font-medium text-gray-900 dark:text-white">
                      {feature.label}
                    </span>
                  </label>
                ))}
              </div>

              <div className="bg-white dark:bg-neutral-800 rounded-xl p-6 border border-gray-200 dark:border-neutral-700 shadow-sm">
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
                    className="w-5 h-5 rounded"
                  />
                  <div>
                    <p className="font-medium text-gray-900 dark:text-white">
                      Include Contact Form
                    </p>
                    <p className="text-xs text-gray-500 dark:text-gray-400">
                      Add a professional contact form to your website
                    </p>
                  </div>
                </label>
              </div>

              <div className="flex gap-3">
                <button
                  onClick={() => setCurrentStep(2)}
                  className="flex-1 bg-gray-200 hover:bg-gray-300 dark:bg-neutral-700 dark:hover:bg-neutral-600 text-gray-900 dark:text-white font-semibold py-3 px-6 rounded-lg transition-all"
                >
                  Back
                </button>
                <button
                  onClick={() => canProceedStep3 && setCurrentStep(4)}
                  className="flex-1 bg-purple-600 hover:bg-purple-700 text-white font-semibold py-3 px-6 rounded-lg transition-all flex items-center justify-center gap-2 group"
                >
                  Review
                  <ArrowRight size={18} className="group-hover:translate-x-1 transition-transform" />
                </button>
              </div>
            </div>
          )}

          {/* Step 4: Review & Generate */}
          {currentStep === 4 && (
            <div className="space-y-6 animate-fadeIn">
              <div>
                <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-1">
                  {result ? "Website Generated!" : "Review & Generate"}
                </h2>
                <p className="text-sm text-gray-600 dark:text-gray-400 mb-6">
                  {result ? "Your website is ready to use" : "Preview your settings before generating"}
                </p>
              </div>

              {!result && (
                <>
                  <div className="bg-gradient-to-br from-purple-50 to-pink-50 dark:from-purple-900/20 dark:to-pink-900/20 rounded-xl p-6 border border-purple-200 dark:border-purple-800 space-y-4">
                    <div className="flex items-center justify-between pb-4 border-b border-purple-200 dark:border-purple-800">
                      <span className="text-sm font-medium text-gray-600 dark:text-gray-400">Project Name</span>
                      <span className="font-semibold text-gray-900 dark:text-white">{formState.projectName}</span>
                    </div>
                    <div className="flex items-center justify-between pb-4 border-b border-purple-200 dark:border-purple-800">
                      <span className="text-sm font-medium text-gray-600 dark:text-gray-400">Industry</span>
                      <span className="font-semibold text-gray-900 dark:text-white">{formState.industry}</span>
                    </div>
                    <div className="flex items-center justify-between pb-4 border-b border-purple-200 dark:border-purple-800">
                      <span className="text-sm font-medium text-gray-600 dark:text-gray-400">Color Palette</span>
                      <div className="flex items-center gap-2">
                        <div className={`w-4 h-4 rounded-full bg-gradient-to-r ${PALETTES.find(p => p.name === formState.colorScheme)?.bg}`} />
                        <span className="font-semibold text-gray-900 dark:text-white">{formState.colorScheme}</span>
                      </div>
                    </div>
                    <div className="flex items-start justify-between">
                      <span className="text-sm font-medium text-gray-600 dark:text-gray-400">Features</span>
                      <div className="text-right">
                        <p className="font-semibold text-gray-900 dark:text-white">{formState.features.length + 1} sections</p>
                        <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                          {formState.includeContactForm && "Incl. contact form"}
                        </p>
                      </div>
                    </div>
                  </div>

                  {error && (
                    <div className="bg-red-50 dark:bg-red-900/20 border border-red-300 dark:border-red-800 rounded-lg p-4 flex items-start gap-3">
                      <AlertCircle size={18} className="text-red-600 dark:text-red-400 shrink-0 mt-0.5" />
                      <p className="font-medium text-red-900 dark:text-red-200">{error}</p>
                    </div>
                  )}

                  <div className="flex gap-3">
                    <button
                      onClick={() => setCurrentStep(3)}
                      className="flex-1 bg-gray-200 hover:bg-gray-300 dark:bg-neutral-700 dark:hover:bg-neutral-600 text-gray-900 dark:text-white font-semibold py-3 px-6 rounded-lg transition-all"
                    >
                      Back
                    </button>
                    <button
                      onClick={handleGenerate}
                      disabled={loading}
                      className="flex-1 bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700 disabled:from-gray-400 disabled:to-gray-400 text-white font-semibold py-3 px-6 rounded-lg transition-all flex items-center justify-center gap-2 group"
                    >
                      {loading && <Loader size={18} className="animate-spin" />}
                      {loading ? "Creating..." : "Generate Website"}
                      {!loading && <Sparkles size={18} className="group-hover:scale-110 transition-transform" />}
                    </button>
                  </div>
                </>
              )}

              {success && (
                <>
                  <div className="bg-green-50 dark:bg-green-900/20 border border-green-300 dark:border-green-800 rounded-lg p-6 flex items-start gap-4">
                    <CheckCircle size={24} className="text-green-600 dark:text-green-400 shrink-0 mt-0.5" />
                    <div>
                      <p className="font-bold text-green-900 dark:text-green-200 text-lg">Success!</p>
                      <p className="text-green-800 dark:text-green-300 text-sm mt-1">
                        Your website has been generated and saved successfully.
                      </p>
                    </div>
                  </div>
                  <button
                    onClick={() => window.location.href = "/dashboard/projects"}
                    className="w-full bg-green-600 hover:bg-green-700 text-white font-semibold py-3 px-6 rounded-lg transition-all"
                  >
                    View Your Project
                  </button>
                </>
              )}
            </div>
          )}
        </div>

        {/* Right: Live Preview */}
        {result && (
          <div className="col-span-2">
            <div className="sticky top-10 space-y-4">
              <div className="bg-white dark:bg-neutral-800 rounded-xl border border-gray-200 dark:border-neutral-700 shadow-lg overflow-hidden">
                <div className="bg-gradient-to-r from-purple-600 to-pink-600 px-6 py-4">
                  <h2 className="text-lg font-semibold text-white flex items-center gap-2">
                    <Code2 size={20} />
                    Live Preview
                  </h2>
                </div>
                <div className="p-6 max-h-[600px] overflow-y-auto">
                  <HtmlRenderer html={result.htmlContent} />
                </div>
                <div className="bg-gray-50 dark:bg-neutral-900 px-6 py-3 border-t border-gray-200 dark:border-neutral-700">
                  <p className="text-xs text-gray-500 dark:text-gray-400">
                    Generated: {new Date(result.generatedAt).toLocaleString()}
                  </p>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Mobile: Show preview only on step 4 */}
        {!result && currentStep < 4 && (
          <div className="col-span-2 hidden lg:block">
            <div className="sticky top-10 bg-white dark:bg-neutral-800 rounded-xl border border-gray-200 dark:border-neutral-700 shadow-lg p-12 flex items-center justify-center h-96">
              <div className="text-center">
                <div className="w-24 h-24 mx-auto mb-4 rounded-full bg-gradient-to-br from-purple-100 to-pink-100 dark:from-purple-900/20 dark:to-pink-900/20 flex items-center justify-center">
                  <Sparkles size={48} className="text-purple-600" />
                </div>
                <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-2">
                  Preview Coming
                </h3>
                <p className="text-gray-600 dark:text-gray-400">
                  Complete all steps and your website preview will appear here
                </p>
              </div>
            </div>
          </div>
        )}
      </div>

      <style>{`
        @keyframes fadeIn {
          from {
            opacity: 0;
            transform: translateY(10px);
          }
          to {
            opacity: 1;
            transform: translateY(0);
          }
        }
        .animate-fadeIn {
          animation: fadeIn 0.3s ease-out;
        }
      `}</style>
    </div>
  );
}
