'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import toast from 'react-hot-toast';
import {
  Save,
  Download,
  RotateCcw,
  Plus,
  Trash2,
  Eye,
  Code,
  Palette,
} from 'lucide-react';

// Type Definitions
interface Section {
  id: string;
  title: string;
  type: 'hero' | 'features' | 'pricing' | 'testimonials' | 'cta' | 'footer';
  content: string;
  backgroundColor?: string;
  textColor?: string;
  image?: string;
}

interface GlobalStyles {
  primaryColor: string;
  secondaryColor: string;
  fontFamily: 'inter' | 'playfair' | 'roboto';
  fontSize: 'small' | 'medium' | 'large';
  spacing: 'compact' | 'normal' | 'spacious';
}

interface GeneratedWebsite {
  prompt: string;
  style: string;
  industry: string;
  palette: string;
}

interface EditorState {
  sections: Section[];
  globalStyles: GlobalStyles;
  selectedSectionId: string | null;
  previewMode: 'desktop' | 'tablet' | 'mobile';
  isSaving: boolean;
  isDirty: boolean;
}

// Mock section templates (without ID, added during creation)
const sectionTemplates = {
  hero: {
    title: 'Hero Section',
    type: 'hero' as const,
    content: 'Welcome to Your Amazing Website. Start creating something incredible today.',
    backgroundColor: '#ffffff',
    textColor: '#000000',
  },
  features: {
    title: 'Features',
    type: 'features' as const,
    content: 'Feature 1\nFeature 2\nFeature 3',
    backgroundColor: '#f9fafb',
    textColor: '#111827',
  },
  pricing: {
    title: 'Pricing Plans',
    type: 'pricing' as const,
    content: 'Starter - $29/mo\nProfessional - $99/mo\nEnterprise - Custom',
    backgroundColor: '#ffffff',
    textColor: '#000000',
  },
  testimonials: {
    title: 'Testimonials',
    type: 'testimonials' as const,
    content: '"This is amazing!" - Customer 1\n"Love it!" - Customer 2',
    backgroundColor: '#f3f4f6',
    textColor: '#111827',
  },
  cta: {
    title: 'Call to Action',
    type: 'cta' as const,
    content: 'Ready to get started? Join thousands of happy users.',
    backgroundColor: '#7c3aed',
    textColor: '#ffffff',
  },
  footer: {
    title: 'Footer',
    type: 'footer' as const,
    content: '© 2025 Your Company. All rights reserved.',
    backgroundColor: '#1f2937',
    textColor: '#ffffff',
  },
};

export default function EditorPage() {
  const router = useRouter();

  // State Management
  const [state, setState] = useState<EditorState>({
    sections: [
      {
        id: 'section-hero-1',
        ...sectionTemplates.hero,
      },
    ],
    globalStyles: {
      primaryColor: '#7c3aed',
      secondaryColor: '#4f46e5',
      fontFamily: 'inter',
      fontSize: 'medium',
      spacing: 'normal',
    },
    selectedSectionId: '',
    previewMode: 'desktop',
    isSaving: false,
    isDirty: false,
  });

  const [generatedWebsite, setGeneratedWebsite] = useState<GeneratedWebsite | null>(null);

  // Retrieve data from sessionStorage on mount
  useEffect(() => {
    const stored = sessionStorage.getItem('generatedWebsite');
    if (stored) {
      try {
        const website = JSON.parse(stored);
        setGeneratedWebsite(website);
      } catch (error) {
        console.error('Failed to parse generated website:', error);
      }
    }

    // Initialize first section as selected
    if (state.sections.length > 0) {
      setState((prev) => ({
        ...prev,
        selectedSectionId: state.sections[0].id,
      }));
    }
  }, []);

  // Handlers
  const handleAddSection = (type: keyof typeof sectionTemplates) => {
    const newSection: Section = {
      id: `section-${Date.now()}`,
      ...sectionTemplates[type],
    };

    setState((prev) => ({
      ...prev,
      sections: [...prev.sections, newSection],
      isDirty: true,
    }));

    toast.success(`Added ${type} section`);
  };

  const handleDeleteSection = (id: string) => {
    if (state.sections.length === 1) {
      toast.error('You must keep at least one section');
      return;
    }

    setState((prev) => ({
      ...prev,
      sections: prev.sections.filter((s) => s.id !== id),
      selectedSectionId: prev.sections[0]?.id || null,
      isDirty: true,
    }));

    toast.success('Section deleted');
  };

  const handleUpdateSection = (id: string, updates: Partial<Section>) => {
    setState((prev) => ({
      ...prev,
      sections: prev.sections.map((s) =>
        s.id === id ? { ...s, ...updates } : s
      ),
      isDirty: true,
    }));
  };

  const handleRegenerateSection = (id: string) => {
    const section = state.sections.find((s) => s.id === id);
    if (!section) return;

    // Simulate regeneration
    const loadingToast = toast.loading('Regenerating section...');
    setTimeout(() => {
      handleUpdateSection(id, {
        content: `${section.content}\n\n[Regenerated content]`,
      });
      toast.dismiss(loadingToast);
      toast.success('Section regenerated');
    }, 1500);
  };

  const handleUpdateStyles = (updates: Partial<GlobalStyles>) => {
    setState((prev) => ({
      ...prev,
      globalStyles: { ...prev.globalStyles, ...updates },
      isDirty: true,
    }));
  };

  const handleSaveProject = async () => {
    setState((prev) => ({ ...prev, isSaving: true }));

    try {
      // Simulate API call
      await new Promise((resolve) => setTimeout(resolve, 1500));

      // Store project data
      const projectData = {
        id: `project-${Date.now()}`,
        generatedWebsite,
        sections: state.sections,
        globalStyles: state.globalStyles,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      };

      sessionStorage.setItem('currentProject', JSON.stringify(projectData));

      setState((prev) => ({ ...prev, isDirty: false }));
      toast.success('Project saved successfully!');
    } catch (error) {
      toast.error('Failed to save project');
      console.error('Save error:', error);
    } finally {
      setState((prev) => ({ ...prev, isSaving: false }));
    }
  };

  const handleExport = () => {
    router.push('/dashboard/export');
  };

  const selectedSection = state.sections.find((s) => s.id === state.selectedSectionId);

  // Get responsive container width
  const getPreviewWidth = () => {
    switch (state.previewMode) {
      case 'mobile':
        return 'w-full max-w-sm';
      case 'tablet':
        return 'w-full max-w-2xl';
      case 'desktop':
        return 'w-full';
      default:
        return 'w-full';
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="border-b border-gray-200 bg-white sticky top-0 z-10">
        <div className="px-6 py-4">
          <div className="flex items-center justify-between gap-4">
            <div>
              <h1 className="text-2xl font-bold text-gray-900">Website Editor</h1>
              <p className="text-sm text-gray-600 mt-1">
                {generatedWebsite
                  ? `${generatedWebsite.style} | ${generatedWebsite.industry}`
                  : 'Edit your website'}
              </p>
            </div>

            {/* Action Buttons */}
            <div className="flex items-center gap-3">
              {state.isDirty && (
                <span className="text-xs font-medium text-orange-600 bg-orange-50 px-3 py-1 rounded-full">
                  Unsaved changes
                </span>
              )}

              <button
                onClick={handleSaveProject}
                disabled={state.isSaving}
                className="inline-flex items-center gap-2 px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 disabled:opacity-50 transition-all"
              >
                <Save size={18} />
                {state.isSaving ? 'Saving...' : 'Save'}
              </button>

              <button
                onClick={handleExport}
                className="inline-flex items-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-all"
              >
                <Download size={18} />
                Export
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* Main Editor Layout */}
      <div className="flex h-[calc(100vh-100px)]">
        {/* Left Panel: Live Preview */}
        <div className="flex-1 border-r border-gray-200 bg-gray-100 p-6 overflow-y-auto">
          <div className="flex flex-col items-center gap-4">
            {/* Preview Mode Buttons */}
            <div className="flex gap-2 bg-white p-1 rounded-lg border border-gray-200">
              {(['desktop', 'tablet', 'mobile'] as const).map((mode) => (
                <button
                  key={mode}
                  onClick={() =>
                    setState((prev) => ({ ...prev, previewMode: mode }))
                  }
                  className={`px-3 py-2 rounded text-sm font-medium capitalize transition-all ${
                    state.previewMode === mode
                      ? 'bg-purple-600 text-white'
                      : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                  }`}
                >
                  {mode === 'desktop' && <Eye size={16} className="mr-1 inline" />}
                  {mode}
                </button>
              ))}
            </div>

            {/* Live Preview Container */}
            <div className={`${getPreviewWidth()} mx-auto bg-white rounded-lg shadow-lg overflow-hidden`}>
              {state.sections.map((section) => (
                <div
                  key={section.id}
                  style={{
                    backgroundColor: section.backgroundColor,
                    color: section.textColor,
                  }}
                  className="p-8 min-h-[200px] border-b border-gray-200 hover:border-purple-300 transition-colors cursor-pointer"
                  onClick={() =>
                    setState((prev) => ({
                      ...prev,
                      selectedSectionId: section.id,
                    }))
                  }
                >
                  <div className={`max-w-4xl mx-auto ${state.selectedSectionId === section.id ? 'ring-2 ring-purple-500 rounded p-4' : ''}`}>
                    <h2 className="text-xl font-bold mb-3">{section.title}</h2>
                    <div className="whitespace-pre-wrap text-sm leading-relaxed">
                      {section.content}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Right Panel: Editor Controls */}
        <div className="w-80 bg-white border-l border-gray-200 overflow-y-auto">
          {/* Section Management */}
          <div className="border-b border-gray-200 p-4">
            <h3 className="text-sm font-semibold text-gray-900 mb-3">Sections</h3>

            <div className="space-y-2 mb-4">
              {state.sections.map((section) => (
                <div
                  key={section.id}
                  onClick={() =>
                    setState((prev) => ({
                      ...prev,
                      selectedSectionId: section.id,
                    }))
                  }
                  className={`p-3 rounded-lg border-2 cursor-pointer transition-all ${
                    state.selectedSectionId === section.id
                      ? 'border-purple-600 bg-purple-50'
                      : 'border-gray-200 bg-gray-50 hover:border-gray-300'
                  }`}
                >
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-sm font-medium text-gray-900">
                        {section.title}
                      </p>
                      <p className="text-xs text-gray-500 capitalize">{section.type}</p>
                    </div>
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        handleDeleteSection(section.id);
                      }}
                      className="p-1 text-red-600 hover:bg-red-50 rounded transition-colors"
                    >
                      <Trash2 size={16} />
                    </button>
                  </div>
                </div>
              ))}
            </div>

            {/* Add Section Dropdown */}
            <details className="border border-gray-200 rounded-lg">
              <summary className="p-3 cursor-pointer hover:bg-gray-50 flex items-center gap-2 font-medium text-sm">
                <Plus size={16} />
                Add Section
              </summary>
              <div className="p-3 space-y-2 bg-gray-50">
                {(Object.keys(sectionTemplates) as Array<keyof typeof sectionTemplates>).map(
                  (type) => (
                    <button
                      key={type}
                      onClick={() => handleAddSection(type)}
                      className="w-full text-left px-3 py-2 text-sm rounded hover:bg-gray-200 capitalize transition-colors"
                    >
                      + {type}
                    </button>
                  )
                )}
              </div>
            </details>
          </div>

          {/* Section Editor */}
          {selectedSection && (
            <div className="border-b border-gray-200 p-4">
              <div className="flex items-center justify-between mb-3">
                <h3 className="text-sm font-semibold text-gray-900">Edit Section</h3>
                <button
                  onClick={() => handleRegenerateSection(selectedSection.id)}
                  className="p-1 text-purple-600 hover:bg-purple-50 rounded transition-colors"
                  title="Regenerate with AI"
                >
                  <RotateCcw size={16} />
                </button>
              </div>

              <div className="space-y-4">
                {/* Title */}
                <div>
                  <label className="text-xs font-semibold text-gray-700">Title</label>
                  <input
                    type="text"
                    value={selectedSection.title}
                    onChange={(e) =>
                      handleUpdateSection(selectedSection.id, {
                        title: e.target.value,
                      })
                    }
                    className="w-full mt-1 px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-purple-500 outline-none"
                  />
                </div>

                {/* Content */}
                <div>
                  <label className="text-xs font-semibold text-gray-700">Content</label>
                  <textarea
                    value={selectedSection.content}
                    onChange={(e) =>
                      handleUpdateSection(selectedSection.id, {
                        content: e.target.value,
                      })
                    }
                    rows={4}
                    className="w-full mt-1 px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-purple-500 outline-none resize-none"
                  />
                </div>

                {/* Background Color */}
                <div>
                  <label className="text-xs font-semibold text-gray-700">
                    Background Color
                  </label>
                  <div className="flex gap-2 mt-1">
                    <input
                      type="color"
                      value={selectedSection.backgroundColor || '#ffffff'}
                      onChange={(e) =>
                        handleUpdateSection(selectedSection.id, {
                          backgroundColor: e.target.value,
                        })
                      }
                      className="w-10 h-10 rounded border border-gray-300 cursor-pointer"
                    />
                    <input
                      type="text"
                      value={selectedSection.backgroundColor || '#ffffff'}
                      readOnly
                      className="flex-1 px-3 py-2 border border-gray-300 rounded text-sm bg-gray-50"
                    />
                  </div>
                </div>

                {/* Text Color */}
                <div>
                  <label className="text-xs font-semibold text-gray-700">
                    Text Color
                  </label>
                  <div className="flex gap-2 mt-1">
                    <input
                      type="color"
                      value={selectedSection.textColor || '#000000'}
                      onChange={(e) =>
                        handleUpdateSection(selectedSection.id, {
                          textColor: e.target.value,
                        })
                      }
                      className="w-10 h-10 rounded border border-gray-300 cursor-pointer"
                    />
                    <input
                      type="text"
                      value={selectedSection.textColor || '#000000'}
                      readOnly
                      className="flex-1 px-3 py-2 border border-gray-300 rounded text-sm bg-gray-50"
                    />
                  </div>
                </div>
              </div>
            </div>
          )}

          {/* Global Styles */}
          <div className="p-4">
            <h3 className="text-sm font-semibold text-gray-900 mb-3 flex items-center gap-2">
              <Palette size={16} />
              Global Styles
            </h3>

            <div className="space-y-4">
              {/* Primary Color */}
              <div>
                <label className="text-xs font-semibold text-gray-700">
                  Primary Color
                </label>
                <div className="flex gap-2 mt-1">
                  <input
                    type="color"
                    value={state.globalStyles.primaryColor}
                    onChange={(e) =>
                      handleUpdateStyles({ primaryColor: e.target.value })
                    }
                    className="w-10 h-10 rounded border border-gray-300 cursor-pointer"
                  />
                  <input
                    type="text"
                    value={state.globalStyles.primaryColor}
                    readOnly
                    className="flex-1 px-3 py-2 border border-gray-300 rounded text-sm bg-gray-50"
                  />
                </div>
              </div>

              {/* Font Family */}
              <div>
                <label className="text-xs font-semibold text-gray-700">
                  Font Family
                </label>
                <select
                  value={state.globalStyles.fontFamily}
                  onChange={(e) =>
                    handleUpdateStyles({
                      fontFamily: e.target.value as any,
                    })
                  }
                  className="w-full mt-1 px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-purple-500 outline-none"
                >
                  <option value="inter">Inter</option>
                  <option value="playfair">Playfair Display</option>
                  <option value="roboto">Roboto</option>
                </select>
              </div>

              {/* Font Size */}
              <div>
                <label className="text-xs font-semibold text-gray-700">
                  Font Size
                </label>
                <select
                  value={state.globalStyles.fontSize}
                  onChange={(e) =>
                    handleUpdateStyles({
                      fontSize: e.target.value as any,
                    })
                  }
                  className="w-full mt-1 px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-purple-500 outline-none"
                >
                  <option value="small">Small</option>
                  <option value="medium">Medium</option>
                  <option value="large">Large</option>
                </select>
              </div>

              {/* Spacing */}
              <div>
                <label className="text-xs font-semibold text-gray-700">
                  Spacing
                </label>
                <select
                  value={state.globalStyles.spacing}
                  onChange={(e) =>
                    handleUpdateStyles({
                      spacing: e.target.value as any,
                    })
                  }
                  className="w-full mt-1 px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-purple-500 outline-none"
                >
                  <option value="compact">Compact</option>
                  <option value="normal">Normal</option>
                  <option value="spacious">Spacious</option>
                </select>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
