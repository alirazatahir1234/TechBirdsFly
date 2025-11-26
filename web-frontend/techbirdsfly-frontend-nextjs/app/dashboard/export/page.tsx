"use client";

import { exportAsHtml, exportAsZip } from "@/lib/api";
import { Download, FileJson, FileCode, Package } from "lucide-react";
import { useState } from "react";

export default function ExportPage() {
  const [isExporting, setIsExporting] = useState(false);

  async function handleExportHTML() {
    setIsExporting(true);
    try {
      // Simulate getting HTML from previous generation
      const sampleHtml = `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>My Website</title>
</head>
<body>
    <h1>Welcome to Your Website</h1>
    <p>This is your generated website.</p>
</body>
</html>`;
      exportAsHtml(sampleHtml, "website.html");
    } finally {
      setIsExporting(false);
    }
  }

  return (
    <div className="space-y-8">
      {/* Header */}
      <div>
        <h1 className="text-4xl font-bold text-gray-900 dark:text-white">
          Export Website
        </h1>
        <p className="text-gray-600 dark:text-gray-400 mt-2">
          Download your generated website in various formats
        </p>
      </div>

      {/* Export Options */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* HTML Export */}
        <div className="bg-white dark:bg-neutral-900 rounded-lg border border-gray-200 dark:border-neutral-800 p-6 hover:shadow-lg transition-shadow">
          <div className="flex items-start justify-between mb-4">
            <div className="flex items-center gap-3">
              <div className="p-3 bg-orange-100 dark:bg-orange-900/20 rounded-lg">
                <FileCode size={24} className="text-orange-600 dark:text-orange-400" />
              </div>
              <div>
                <h3 className="font-semibold text-gray-900 dark:text-white">
                  Export as HTML
                </h3>
                <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                  Single HTML file
                </p>
              </div>
            </div>
          </div>

          <p className="text-sm text-gray-600 dark:text-gray-400 mb-4">
            Download your website as a standalone HTML file. Perfect for quick hosting or local development.
          </p>

          <button
            onClick={handleExportHTML}
            disabled={isExporting}
            className="w-full bg-orange-600 hover:bg-orange-700 disabled:bg-gray-400 text-white font-medium py-2 px-4 rounded-lg transition-all flex items-center justify-center gap-2"
          >
            <Download size={18} />
            {isExporting ? "Exporting..." : "Download HTML"}
          </button>
        </div>

        {/* React Export */}
        <div className="bg-white dark:bg-neutral-900 rounded-lg border border-gray-200 dark:border-neutral-800 p-6 hover:shadow-lg transition-shadow opacity-50">
          <div className="flex items-start justify-between mb-4">
            <div className="flex items-center gap-3">
              <div className="p-3 bg-blue-100 dark:bg-blue-900/20 rounded-lg">
                <FileJson size={24} className="text-blue-600 dark:text-blue-400" />
              </div>
              <div>
                <h3 className="font-semibold text-gray-900 dark:text-white">
                  Export as React
                </h3>
                <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                  React component (Coming Soon)
                </p>
              </div>
            </div>
          </div>

          <p className="text-sm text-gray-600 dark:text-gray-400 mb-4">
            Export as a reusable React component with props for customization.
          </p>

          <button
            disabled
            className="w-full bg-gray-400 text-white font-medium py-2 px-4 rounded-lg transition-all flex items-center justify-center gap-2 cursor-not-allowed"
          >
            <Download size={18} />
            Coming Soon
          </button>
        </div>

        {/* Next.js Export */}
        <div className="bg-white dark:bg-neutral-900 rounded-lg border border-gray-200 dark:border-neutral-800 p-6 hover:shadow-lg transition-shadow opacity-50">
          <div className="flex items-start justify-between mb-4">
            <div className="flex items-center gap-3">
              <div className="p-3 bg-black dark:bg-white/10 rounded-lg">
                <Package size={24} className="text-black dark:text-white" />
              </div>
              <div>
                <h3 className="font-semibold text-gray-900 dark:text-white">
                  Export as Next.js
                </h3>
                <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                  Full Next.js project (Coming Soon)
                </p>
              </div>
            </div>
          </div>

          <p className="text-sm text-gray-600 dark:text-gray-400 mb-4">
            Export as a complete Next.js project ready to deploy.
          </p>

          <button
            disabled
            className="w-full bg-gray-400 text-white font-medium py-2 px-4 rounded-lg transition-all flex items-center justify-center gap-2 cursor-not-allowed"
          >
            <Download size={18} />
            Coming Soon
          </button>
        </div>

        {/* GitHub Export */}
        <div className="bg-white dark:bg-neutral-900 rounded-lg border border-gray-200 dark:border-neutral-800 p-6 hover:shadow-lg transition-shadow opacity-50">
          <div className="flex items-start justify-between mb-4">
            <div className="flex items-center gap-3">
              <div className="p-3 bg-purple-100 dark:bg-purple-900/20 rounded-lg">
                <Package size={24} className="text-purple-600 dark:text-purple-400" />
              </div>
              <div>
                <h3 className="font-semibold text-gray-900 dark:text-white">
                  Push to GitHub
                </h3>
                <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                  Push directly to repo (Coming Soon)
                </p>
              </div>
            </div>
          </div>

          <p className="text-sm text-gray-600 dark:text-gray-400 mb-4">
            Push your generated website directly to a GitHub repository.
          </p>

          <button
            disabled
            className="w-full bg-gray-400 text-white font-medium py-2 px-4 rounded-lg transition-all flex items-center justify-center gap-2 cursor-not-allowed"
          >
            <Download size={18} />
            Coming Soon
          </button>
        </div>
      </div>

      {/* Info Box */}
      <div className="bg-blue-50 dark:bg-blue-900/20 border border-blue-300 dark:border-blue-800 rounded-lg p-6">
        <h3 className="font-semibold text-blue-900 dark:text-blue-200 mb-2">
          💡 Pro Tip
        </h3>
        <p className="text-sm text-blue-800 dark:text-blue-300">
          Your website is generated with responsive design and modern CSS. All exports include Tailwind CSS classes for easy customization.
        </p>
      </div>
    </div>
  );
}
