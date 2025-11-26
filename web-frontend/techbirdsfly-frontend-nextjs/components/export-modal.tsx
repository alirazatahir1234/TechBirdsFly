'use client';

import { useState } from 'react';
import { Download, X, AlertCircle, CheckCircle, Loader } from 'lucide-react';

interface ExportModalProps {
  projectId: string;
  userId: string;
  projectName: string;
  onClose: () => void;
  onSuccess: () => void;
}

type ExportFormat = 'html' | 'react' | 'nextjs' | 'zip';

export function ExportModal({
  projectId,
  userId,
  projectName,
  onClose,
  onSuccess,
}: ExportModalProps) {
  const [selectedFormat, setSelectedFormat] = useState<ExportFormat>('html');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const formats: { id: ExportFormat; label: string; description: string; icon: string }[] = [
    {
      id: 'html',
      label: 'HTML',
      description: 'Static HTML with embedded theme CSS and SEO tags',
      icon: '📄',
    },
    {
      id: 'react',
      label: 'React',
      description: 'React JSX components with Tailwind CSS',
      icon: '⚛️',
    },
    {
      id: 'nextjs',
      label: 'Next.js',
      description: 'Full Next.js project with TypeScript',
      icon: '▲',
    },
    {
      id: 'zip',
      label: 'ZIP Archive',
      description: 'All assets and HTML bundled as ZIP file',
      icon: '📦',
    },
  ];

  const handleExport = async () => {
    setLoading(true);
    setError(null);

    try {
      const response = await fetch(
        `${process.env.NEXT_PUBLIC_API_BASE}/projects/${projectId}/export`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            userId,
            format: selectedFormat,
          }),
        }
      );

      if (!response.ok) {
        const data = await response.json();
        throw new Error(data.message || `Export failed: ${response.statusText}`);
      }

      const data = await response.json();

      if (data.data && data.data.downloadUrl) {
        // Trigger download
        const downloadUrl = `${process.env.NEXT_PUBLIC_API_BASE}${data.data.downloadUrl}`;
        const link = document.createElement('a');
        link.href = downloadUrl;
        link.download = data.data.fileName || `${projectName}-export.${selectedFormat}`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        setSuccess(true);
        setTimeout(() => {
          onSuccess();
          onClose();
        }, 2000);
      }
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Unknown error occurred';
      setError(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full mx-4 max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="sticky top-0 bg-white border-b border-gray-200 p-6 flex items-center justify-between">
          <div className="flex items-center gap-3">
            <Download size={24} className="text-blue-600" />
            <h2 className="text-2xl font-bold text-gray-900">Export Project</h2>
          </div>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 transition"
            disabled={loading}
          >
            <X size={24} />
          </button>
        </div>

        {/* Content */}
        <div className="p-6">
          {/* Success State */}
          {success && (
            <div className="mb-6 p-4 bg-green-50 border border-green-200 rounded-lg flex items-start gap-3">
              <CheckCircle size={20} className="text-green-600 mt-0.5" />
              <div>
                <h3 className="font-semibold text-green-900">Export Successful!</h3>
                <p className="text-green-800 text-sm mt-1">
                  Your project has been exported as {selectedFormat.toUpperCase()} and is being downloaded.
                </p>
              </div>
            </div>
          )}

          {/* Error State */}
          {error && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg flex items-start gap-3">
              <AlertCircle size={20} className="text-red-600 mt-0.5" />
              <div>
                <h3 className="font-semibold text-red-900">Export Failed</h3>
                <p className="text-red-800 text-sm mt-1">{error}</p>
              </div>
            </div>
          )}

          {/* Project Info */}
          <div className="mb-6 p-4 bg-blue-50 rounded-lg">
            <p className="text-sm text-gray-600">
              <strong>Project:</strong> {projectName}
            </p>
            <p className="text-sm text-gray-600 mt-1">
              <strong>Format:</strong> {formats.find((f) => f.id === selectedFormat)?.label}
            </p>
          </div>

          {/* Format Selection */}
          <div className="mb-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Select Export Format</h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {formats.map((format) => (
                <button
                  key={format.id}
                  onClick={() => setSelectedFormat(format.id)}
                  disabled={loading}
                  className={`p-4 rounded-lg border-2 transition text-left ${
                    selectedFormat === format.id
                      ? 'border-blue-600 bg-blue-50'
                      : 'border-gray-200 bg-white hover:border-gray-300'
                  } disabled:opacity-50 disabled:cursor-not-allowed`}
                >
                  <div className="flex items-start gap-3">
                    <span className="text-2xl">{format.icon}</span>
                    <div>
                      <p className="font-semibold text-gray-900">{format.label}</p>
                      <p className="text-sm text-gray-600 mt-1">{format.description}</p>
                    </div>
                  </div>
                </button>
              ))}
            </div>
          </div>

          {/* Features */}
          <div className="mb-6 p-4 bg-gray-50 rounded-lg">
            <h4 className="font-semibold text-gray-900 mb-3">Export Includes:</h4>
            <ul className="space-y-2 text-sm text-gray-700">
              <li className="flex items-center gap-2">
                <span className="text-blue-600">✓</span>
                Your project theme (colors, fonts, spacing)
              </li>
              <li className="flex items-center gap-2">
                <span className="text-blue-600">✓</span>
                SEO meta tags and Open Graph data
              </li>
              <li className="flex items-center gap-2">
                <span className="text-blue-600">✓</span>
                Project thumbnail and media assets
              </li>
              <li className="flex items-center gap-2">
                <span className="text-blue-600">✓</span>
                Production-ready HTML/CSS
              </li>
            </ul>
          </div>
        </div>

        {/* Footer */}
        <div className="sticky bottom-0 bg-gray-50 border-t border-gray-200 p-6 flex gap-3">
          <button
            onClick={onClose}
            disabled={loading}
            className="flex-1 px-4 py-2 text-gray-700 bg-gray-200 rounded-lg hover:bg-gray-300 transition disabled:opacity-50 disabled:cursor-not-allowed font-medium"
          >
            Cancel
          </button>
          <button
            onClick={handleExport}
            disabled={loading || success}
            className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition disabled:opacity-50 disabled:cursor-not-allowed font-medium flex items-center justify-center gap-2"
          >
            {loading && <Loader size={18} className="animate-spin" />}
            {loading ? 'Exporting...' : 'Export Project'}
          </button>
        </div>
      </div>
    </div>
  );
}
