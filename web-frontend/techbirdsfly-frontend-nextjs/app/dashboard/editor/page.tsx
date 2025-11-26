"use client";

import { useSearchParams } from "next/navigation";
import { useState, useEffect, useRef } from "react";
import { loadProject, saveVersion, renameProject } from "@/lib/project-api";
import HtmlRenderer from "@/components/html-renderer";
import ImageReplaceModal from "@/components/image-replace-modal";
import SeoModal from "@/components/seo-modal";
import ThemeModal from "@/components/theme-modal";
import { Copy, Download, Image as ImageIcon, Save, Loader2, Settings, Palette, FileDown } from "lucide-react";
import toast from "react-hot-toast";
import { ExportModal } from "@/components/export-modal";

export default function EditorPage() {
  const params = useSearchParams();
  const htmlParam = params.get("html");
  const projectParam = params.get("project");
  const projectName = params.get("name") || "Untitled Project";

  const [html, setHtml] = useState<string>(htmlParam || "");
  const [projectVersion, setProjectVersion] = useState<number>(1);
  const [projectTitle, setProjectTitle] = useState<string>(projectName);
  const [selectedImage, setSelectedImage] = useState<string>("");
  const [showImageModal, setShowImageModal] = useState(false);
  const [showSeoModal, setShowSeoModal] = useState(false);
  const [showThemeModal, setShowThemeModal] = useState(false);
  const [showExportModal, setShowExportModal] = useState(false);
  const [imageSources, setImageSources] = useState<string[]>([]);
  const [currentImageIndex, setCurrentImageIndex] = useState(0);
  const [isSaving, setIsSaving] = useState(false);
  const [isRenamingSaving, setIsRenamingSaving] = useState(false);
  const [isLoadingProject, setIsLoadingProject] = useState(!!projectParam);
  const [projectData, setProjectData] = useState<any>(null);
  const iframeRef = useRef<HTMLIFrameElement>(null);

  // Load project if projectParam exists
  useEffect(() => {
    if (!projectParam) return;

    const loadProjectData = async () => {
      try {
        setIsLoadingProject(true);
        const project = await loadProject(projectParam);
        setHtml(project.html);
        setProjectVersion(project.version);
        setProjectData(project);
      } catch (error) {
        console.error("Error loading project:", error);
        toast.error("Failed to load project");
      } finally {
        setIsLoadingProject(false);
      }
    };

    loadProjectData();
  }, [projectParam]);

  async function handleRenameProject(newName: string) {
    if (!projectParam || !newName || newName === projectTitle) {
      return;
    }

    try {
      setIsRenamingSaving(true);
      await renameProject(projectParam, newName);
      setProjectTitle(newName);
      toast.success("✅ Project renamed!");
    } catch (error) {
      console.error("Error renaming project:", error);
      toast.error("Failed to rename project");
      // Revert the input
      setProjectTitle(projectTitle);
    } finally {
      setIsRenamingSaving(false);
    }
  }

  // Extract all image sources from HTML
  useEffect(() => {
    if (!html) return;

    const imgRegex = /<img[^>]+src=["']([^"']+)["'][^>]*>/g;
    const sources: string[] = [];
    let match;

    while ((match = imgRegex.exec(html)) !== null) {
      sources.push(match[1]);
    }

    setImageSources(sources);
  }, [html]);

  // Setup iframe and add click listeners to images
  useEffect(() => {
    if (!iframeRef.current || !html) return;

    try {
      const doc = iframeRef.current.contentDocument || iframeRef.current.contentWindow?.document;
      if (!doc) return;

      doc.open();
      doc.write(html);
      doc.close();

      // Add click handlers to images
      const images = doc.querySelectorAll("img");
      images.forEach((img, index) => {
        img.style.cursor = "pointer";
        img.style.border = "2px solid transparent";
        img.style.transition = "border 0.2s";

        img.addEventListener("click", () => {
          setSelectedImage(img.src);
          setCurrentImageIndex(index);
          setShowImageModal(true);
        });

        img.addEventListener("mouseover", () => {
          img.style.border = "2px solid #a855f7";
          img.style.borderRadius = "4px";
        });

        img.addEventListener("mouseout", () => {
          img.style.border = "2px solid transparent";
        });
      });
    } catch (error) {
      console.error("Failed to setup iframe:", error);
    }
  }, [html]);

  function handleReplaceImage(imageData: {
    type: "upload" | "ai-generated";
    base64: string;
    url?: string;
    prompt?: string;
  }) {
    if (!selectedImage || !html) {
      toast.error("No image selected");
      return;
    }

    try {
      let newHtml = html;

      // Replace the selected image src
      if (imageData.type === "upload" && imageData.url) {
        // For uploaded images, use the URL from the media service
        newHtml = html.replace(selectedImage, imageData.url);
      } else if (imageData.type === "ai-generated") {
        // For AI-generated images, use base64
        const base64Src = `data:image/png;base64,${imageData.base64}`;
        newHtml = html.replace(selectedImage, base64Src);
      }

      setHtml(newHtml);
      setShowImageModal(false);
      setSelectedImage("");

      const typeLabel = imageData.type === "upload" ? "📤 Uploaded" : "✨ AI-Generated";
      toast.success(`${typeLabel} image applied!`);
    } catch (error) {
      console.error("Failed to replace image:", error);
      toast.error("Failed to replace image");
    }
  }

  function handleCopyHtml() {
    if (html) {
      navigator.clipboard.writeText(html);
      toast.success("✅ HTML copied to clipboard!");
    }
  }

  function handleDownload() {
    if (html) {
      const element = document.createElement("a");
      element.setAttribute(
        "href",
        "data:text/html;charset=utf-8," + encodeURIComponent(html)
      );
      element.setAttribute("download", `${projectName}.html`);
      element.style.display = "none";
      document.body.appendChild(element);
      element.click();
      document.body.removeChild(element);
      toast.success("✅ HTML file downloaded!");
    }
  }

  async function handleSaveVersion() {
    if (!projectParam || !html) {
      toast.error("No project to save");
      return;
    }

    try {
      setIsSaving(true);
      const updated = await saveVersion({
        projectId: projectParam,
        html,
      });
      setProjectVersion(updated.version);
      toast.success(`✅ Saved as version ${updated.version}!`);
    } catch (error) {
      console.error("Error saving version:", error);
      toast.error("Failed to save version");
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          {projectParam ? (
            <div className="space-y-2">
              <input
                type="text"
                value={projectTitle}
                onChange={(e) => setProjectTitle(e.target.value)}
                onBlur={() => handleRenameProject(projectTitle)}
                disabled={isRenamingSaving}
                className="text-4xl font-bold text-white bg-transparent border-2 border-transparent hover:border-purple-500 focus:border-purple-600 rounded px-2 py-1 transition-colors disabled:opacity-50 outline-none"
                placeholder="Project name"
              />
              <p className="text-slate-400">
                v{projectVersion}
              </p>
            </div>
          ) : (
            <>
              <h1 className="text-4xl font-bold text-white">
                Editor
              </h1>
              <p className="text-slate-400 mt-2">
                {projectName}
              </p>
            </>
          )}
        </div>

        <div className="flex gap-3">
          {projectParam && (
            <>
              <button
                onClick={handleSaveVersion}
                disabled={isSaving}
                className="flex items-center gap-2 bg-purple-600 hover:bg-purple-700 disabled:opacity-50 disabled:cursor-not-allowed text-white px-4 py-2 rounded-lg transition-all font-medium"
              >
                {isSaving ? (
                  <Loader2 size={18} className="animate-spin" />
                ) : (
                  <Save size={18} />
                )}
                {isSaving ? "Saving..." : "Save Version"}
              </button>
              <button
                onClick={() => setShowSeoModal(true)}
                className="flex items-center gap-2 bg-orange-600 hover:bg-orange-700 text-white px-4 py-2 rounded-lg transition-all font-medium"
              >
                <Settings size={18} />
                SEO Settings
              </button>
              <button
                onClick={() => setShowThemeModal(true)}
                className="flex items-center gap-2 bg-violet-600 hover:bg-violet-700 text-white px-4 py-2 rounded-lg transition-all font-medium"
              >
                <Palette size={18} />
                Theme Settings
              </button>
              <button
                onClick={() => setShowExportModal(true)}
                className="flex items-center gap-2 bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-lg transition-all font-medium"
              >
                <FileDown size={18} />
                Export Project
              </button>
            </>
          )}
          <button
            onClick={handleCopyHtml}
            className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg transition-all font-medium"
          >
            <Copy size={18} />
            Copy HTML
          </button>
          <button
            onClick={handleDownload}
            className="flex items-center gap-2 bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded-lg transition-all font-medium"
          >
            <Download size={18} />
            Download
          </button>
        </div>
      </div>

      {isLoadingProject && (
        <div className="bg-blue-900/20 border border-blue-700/50 rounded-lg p-6 flex items-center gap-2">
          <Loader2 className="animate-spin text-blue-400" size={20} />
          <p className="text-blue-400">Loading project...</p>
        </div>
      )}

      {!html && !isLoadingProject && (
        <div className="bg-yellow-900/20 border border-yellow-700/50 rounded-lg p-6">
          <p className="text-yellow-400">
            No project loaded. Go back to Create to generate a website.
          </p>
        </div>
      )}

      {html && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Preview Area (2/3) */}
          <div className="lg:col-span-2 space-y-3">
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-semibold text-white">Preview</h2>
              {imageSources.length > 0 && (
                <span className="text-xs bg-purple-500/20 text-purple-300 px-3 py-1 rounded-full">
                  💡 Click images to edit ({imageSources.length})
                </span>
              )}
            </div>

            <div className="bg-slate-800 border border-slate-700 rounded-lg overflow-hidden shadow-lg">
              <iframe
                ref={iframeRef}
                className="w-full h-[600px] bg-white"
                title="Website Preview"
                sandbox={{ allow: ["same-origin", "scripts"] } as any}
              />
            </div>
          </div>

          {/* Image Editor Panel (1/3) */}
          <div className="lg:col-span-1">
            <div className="bg-slate-800 border border-slate-700 rounded-lg p-4 sticky top-6 max-h-[700px] overflow-y-auto">
              <div className="flex items-center gap-2 mb-4">
                <ImageIcon className="w-5 h-5 text-purple-400" />
                <h3 className="text-lg font-semibold text-white">Image Editor</h3>
              </div>

              {imageSources.length === 0 ? (
                <div className="text-center py-8">
                  <p className="text-slate-400 text-sm">
                    No images found in this design.
                  </p>
                </div>
              ) : (
                <ImageReplaceModal
                  isOpen={showImageModal}
                  onClose={() => setShowImageModal(false)}
                  onReplace={handleReplaceImage}
                />
              )}

              {/* Image List */}
              {imageSources.length > 0 && (
                <div className="mt-6 space-y-2">
                  <p className="text-xs text-slate-400 font-medium">Images in page:</p>
                  <div className="space-y-1 max-h-40 overflow-y-auto">
                    {imageSources.map((src, idx) => (
                      <button
                        key={idx}
                        onClick={() => {
                          setSelectedImage(src);
                          setCurrentImageIndex(idx);
                          setShowImageModal(true);
                        }}
                        className={`w-full text-left px-3 py-2 rounded-lg text-xs transition-colors ${
                          selectedImage === src
                            ? "bg-purple-600 text-white"
                            : "bg-slate-700 text-slate-300 hover:bg-slate-600"
                        }`}
                      >
                        <span className="truncate block">
                          Image {idx + 1}
                        </span>
                        <span className="text-xs opacity-70 truncate block">
                          {src.substring(0, 40)}...
                        </span>
                      </button>
                    ))}
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      )}

      {showImageModal && (
        <ImageReplaceModal
          isOpen={showImageModal}
          onClose={() => setShowImageModal(false)}
          onReplace={(imageData: any) => {
            const newUrl = imageData.url || imageData.base64;
            const newHtml = html.replace(selectedImage, newUrl);
            setHtml(newHtml);
            toast.success("✅ Image updated!");
          }}
        />
      )}

      {showSeoModal && projectParam && (
        <SeoModal
          projectId={projectParam}
          userId={params.get("userId") || projectData?.userId || ""}
          seoTitle={projectData?.seoTitle}
          seoDescription={projectData?.seoDescription}
          seoKeywords={projectData?.seoKeywords}
          ogTitle={projectData?.ogTitle}
          ogDescription={projectData?.ogDescription}
          ogImageUrl={projectData?.ogImageUrl}
          onClose={() => setShowSeoModal(false)}
          onSuccess={() => {
            toast.success("✅ SEO settings updated!");
          }}
        />
      )}

      {showThemeModal && projectParam && (
        <ThemeModal
          projectId={projectParam}
          userId={params.get("userId") || projectData?.userId || ""}
          primaryColor={projectData?.primaryColor}
          secondaryColor={projectData?.secondaryColor}
          accentColor={projectData?.accentColor}
          backgroundColor={projectData?.backgroundColor}
          textColor={projectData?.textColor}
          fontFamily={projectData?.fontFamily}
          fontSizeBase={projectData?.fontSizeBase}
          borderRadius={projectData?.borderRadius}
          onClose={() => setShowThemeModal(false)}
          onSuccess={() => {
            toast.success("✅ Theme settings updated!");
          }}
        />
      )}

      {showExportModal && projectParam && (
        <ExportModal
          projectId={projectParam}
          userId={params.get("userId") || projectData?.userId || ""}
          projectName={projectTitle}
          onClose={() => setShowExportModal(false)}
          onSuccess={() => {
            toast.success("✅ Project exported successfully!");
          }}
        />
      )}
    </div>
  );
}
