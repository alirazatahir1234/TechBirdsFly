'use client';

import { useState } from 'react';
import { updateTheme } from '@/lib/project-api';
import { Palette } from 'lucide-react';

interface ThemeModalProps {
  projectId: string;
  userId: string;
  primaryColor?: string;
  secondaryColor?: string;
  accentColor?: string;
  backgroundColor?: string;
  textColor?: string;
  fontFamily?: string;
  fontSizeBase?: string;
  borderRadius?: string;
  onClose: () => void;
  onSuccess: () => void;
}

const FONT_FAMILIES = ['Poppins', 'Inter', 'Georgia', 'Arial', 'Courier', 'Times New Roman', 'Verdana'];

export default function ThemeModal({
  projectId,
  userId,
  primaryColor = '#0066CC',
  secondaryColor = '#66BB6A',
  accentColor = '#FF6B6B',
  backgroundColor = '#FFFFFF',
  textColor = '#333333',
  fontFamily = 'Poppins',
  fontSizeBase = '16',
  borderRadius = '8',
  onClose,
  onSuccess,
}: ThemeModalProps) {
  const [formData, setFormData] = useState({
    primaryColor,
    secondaryColor,
    accentColor,
    backgroundColor,
    textColor,
    fontFamily,
    fontSizeBase,
    borderRadius,
  });

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleColorChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSelectChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleRangeChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      await updateTheme(projectId, userId, formData);
      onSuccess();
      onClose();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to update theme settings';
      setError(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg shadow-lg max-w-3xl w-full mx-4 p-6">
        <div className="flex justify-between items-center mb-4">
          <div className="flex items-center gap-2">
            <Palette className="text-purple-600" size={24} />
            <h2 className="text-2xl font-bold text-gray-900">Theme Settings</h2>
          </div>
          <button
            onClick={onClose}
            className="text-gray-500 hover:text-gray-700 text-2xl"
          >
            ×
          </button>
        </div>

        {error && (
          <div className="mb-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-6 max-h-96 overflow-y-auto">
          {/* Colors Section */}
          <div>
            <h3 className="text-lg font-semibold text-gray-800 mb-4">Colors</h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {[
                { key: 'primaryColor', label: 'Primary Color', desc: 'Buttons, headers, main accents' },
                { key: 'secondaryColor', label: 'Secondary Color', desc: 'Highlights, secondary elements' },
                { key: 'accentColor', label: 'Accent Color', desc: 'Warnings, CTAs, danger states' },
                { key: 'backgroundColor', label: 'Background Color', desc: 'Main page background' },
                { key: 'textColor', label: 'Text Color', desc: 'Default text color' },
              ].map(({ key, label, desc }) => (
                <div key={key}>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    {label}
                  </label>
                  <p className="text-xs text-gray-500 mb-2">{desc}</p>
                  <div className="flex gap-2 items-center">
                    <input
                      type="color"
                      name={key}
                      value={formData[key as keyof typeof formData]}
                      onChange={handleColorChange}
                      className="w-12 h-10 border border-gray-300 rounded-lg cursor-pointer"
                    />
                    <input
                      type="text"
                      name={key}
                      value={formData[key as keyof typeof formData]}
                      onChange={handleColorChange}
                      placeholder="#000000"
                      className="flex-1 px-3 py-2 border border-gray-300 rounded-lg text-sm font-mono focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                    />
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Typography Section */}
          <div className="border-t pt-4">
            <h3 className="text-lg font-semibold text-gray-800 mb-4">Typography</h3>

            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Font Family
              </label>
              <select
                name="fontFamily"
                value={formData.fontFamily}
                onChange={handleSelectChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
              >
                {FONT_FAMILIES.map((font) => (
                  <option key={font} value={font}>
                    {font}
                  </option>
                ))}
              </select>
            </div>

            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Font Size: <span className="text-purple-600">{formData.fontSizeBase}px</span>
              </label>
              <input
                type="range"
                name="fontSizeBase"
                value={formData.fontSizeBase}
                onChange={handleRangeChange}
                min="14"
                max="24"
                className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer"
              />
              <p className="text-xs text-gray-500 mt-1">Range: 14px - 24px</p>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Border Radius: <span className="text-purple-600">{formData.borderRadius}px</span>
              </label>
              <input
                type="range"
                name="borderRadius"
                value={formData.borderRadius}
                onChange={handleRangeChange}
                min="0"
                max="24"
                className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer"
              />
              <p className="text-xs text-gray-500 mt-1">Range: 0px - 24px (for buttons, inputs)</p>
            </div>
          </div>

          {/* Live Preview */}
          <div className="border-t pt-4">
            <h3 className="text-lg font-semibold text-gray-800 mb-3">Preview</h3>
            <div className="p-4 rounded-lg border border-gray-200 bg-gray-50">
              <div className="flex gap-3 flex-wrap">
                <button
                  type="button"
                  disabled
                  style={{ backgroundColor: formData.primaryColor }}
                  className="px-4 py-2 text-white rounded disabled:opacity-50 font-medium"
                >
                  Primary
                </button>
                <button
                  type="button"
                  disabled
                  style={{ backgroundColor: formData.secondaryColor }}
                  className="px-4 py-2 text-white rounded disabled:opacity-50 font-medium"
                >
                  Secondary
                </button>
                <button
                  type="button"
                  disabled
                  style={{ backgroundColor: formData.accentColor }}
                  className="px-4 py-2 text-white rounded disabled:opacity-50 font-medium"
                >
                  Accent
                </button>
              </div>
              <div
                className="mt-4 p-4 rounded-lg text-center"
                style={{
                  backgroundColor: formData.backgroundColor,
                  color: formData.textColor,
                  borderRadius: `${formData.borderRadius}px`,
                  fontFamily: formData.fontFamily,
                  fontSize: `${formData.fontSizeBase}px`,
                  border: `2px solid ${formData.primaryColor}`,
                }}
              >
                Sample text with your theme
              </div>
            </div>
          </div>
        </form>

        <div className="flex justify-end gap-3 mt-6 border-t pt-4">
          <button
            onClick={onClose}
            disabled={loading}
            className="px-4 py-2 text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50"
          >
            Cancel
          </button>
          <button
            onClick={handleSubmit}
            disabled={loading}
            className="px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 disabled:opacity-50 font-medium"
          >
            {loading ? 'Saving...' : 'Save Theme Settings'}
          </button>
        </div>
      </div>
    </div>
  );
}
