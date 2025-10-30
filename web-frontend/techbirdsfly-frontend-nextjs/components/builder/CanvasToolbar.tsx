'use client';

import React from 'react';
import { ZoomIn, ZoomOut, Maximize2, Eye, EyeOff } from 'lucide-react';
import { useBuilderStore } from '@/lib/store/builderStore';

interface CanvasToolbarProps {
  canvasTransform: {
    x: number;
    y: number;
    scale: number;
    rotation: number;
  };
  onResetZoom: () => void;
  onZoomIn: () => void;
  onZoomOut: () => void;
}

export const CanvasToolbar: React.FC<CanvasToolbarProps> = ({
  canvasTransform,
  onResetZoom,
  onZoomIn,
  onZoomOut,
}) => {
  const { togglePreviewMode, isPreviewMode } = useBuilderStore();

  return (
    <div className="flex items-center justify-between px-4 py-3 bg-slate-950 border-b border-slate-700">
      {/* Left: Zoom controls */}
      <div className="flex items-center gap-2">
        <button
          onClick={onZoomOut}
          className="p-2 hover:bg-slate-800 rounded text-slate-300 hover:text-white transition"
          title="Zoom out (Ctrl -)"
        >
          <ZoomOut size={20} />
        </button>

        <div className="px-3 py-1 bg-slate-800 rounded text-sm text-slate-300 min-w-[80px] text-center">
          {Math.round(canvasTransform.scale * 100)}%
        </div>

        <button
          onClick={onZoomIn}
          className="p-2 hover:bg-slate-800 rounded text-slate-300 hover:text-white transition"
          title="Zoom in (Ctrl +)"
        >
          <ZoomIn size={20} />
        </button>

        <div className="w-px h-6 bg-slate-700 mx-2" />

        <button
          onClick={onResetZoom}
          className="p-2 hover:bg-slate-800 rounded text-slate-300 hover:text-white transition"
          title="Reset zoom (Ctrl 0)"
        >
          <Maximize2 size={20} />
        </button>
      </div>

      {/* Center: Info */}
      <div className="text-sm text-slate-400 flex gap-6">
        <span>
          Position: ({Math.round(canvasTransform.x)}, {Math.round(canvasTransform.y)})
        </span>
        <span>Rotation: {canvasTransform.rotation}°</span>
      </div>

      {/* Right: Preview toggle */}
      <button
        onClick={togglePreviewMode}
        className={`p-2 rounded transition flex items-center gap-2 ${
          isPreviewMode
            ? 'bg-blue-600 text-white hover:bg-blue-700'
            : 'hover:bg-slate-800 text-slate-300 hover:text-white'
        }`}
        title="Toggle preview mode"
      >
        {isPreviewMode ? <Eye size={20} /> : <EyeOff size={20} />}
        <span className="text-sm">{isPreviewMode ? 'Preview' : 'Edit'}</span>
      </button>
    </div>
  );
};
