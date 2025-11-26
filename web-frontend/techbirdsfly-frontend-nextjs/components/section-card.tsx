"use client";

import React from "react";
import { Copy, Trash2 } from "lucide-react";

interface SectionCardProps {
  title: string;
  type: string;
  html: string;
  onCopy?: () => void;
  onDelete?: () => void;
}

export default function SectionCard({
  title,
  type,
  html,
  onCopy,
  onDelete,
}: SectionCardProps) {
  return (
    <div className="bg-white dark:bg-neutral-900 rounded-lg border border-gray-200 dark:border-neutral-800 p-4 shadow-sm hover:shadow-md transition-shadow">
      <div className="flex items-start justify-between mb-3">
        <div>
          <h3 className="text-sm font-semibold text-gray-900 dark:text-white">
            {title}
          </h3>
          <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
            Type: {type}
          </p>
        </div>
        <div className="flex gap-2">
          {onCopy && (
            <button
              onClick={onCopy}
              className="p-2 hover:bg-gray-100 dark:hover:bg-neutral-800 rounded transition-colors"
              title="Copy HTML"
            >
              <Copy size={16} className="text-gray-600 dark:text-gray-400" />
            </button>
          )}
          {onDelete && (
            <button
              onClick={onDelete}
              className="p-2 hover:bg-red-100 dark:hover:bg-red-900/20 rounded transition-colors"
              title="Delete"
            >
              <Trash2 size={16} className="text-red-600 dark:text-red-400" />
            </button>
          )}
        </div>
      </div>

      {/* Preview */}
      <div className="bg-gray-50 dark:bg-neutral-800 rounded p-3 text-xs text-gray-600 dark:text-gray-400 font-mono overflow-x-auto max-h-24">
        {html.substring(0, 150)}...
      </div>
    </div>
  );
}
