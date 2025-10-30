'use client';

import React, { useRef, useEffect, useState, useCallback } from 'react';
import { useBuilderStore } from '@/lib/store/builderStore';
import { CanvasPreview } from './CanvasPreview';
import { CanvasToolbar } from './CanvasToolbar';
import { ComponentSelectionBox } from './ComponentSelectionBox';
import { useCanvasTransform, CanvasTransform } from '@/lib/hooks/useCanvasTransform';
import { Move, RotateCw } from 'lucide-react';

export const EditorCanvas: React.FC = () => {
  const canvasRef = useRef<HTMLDivElement>(null);
  const iframeRef = useRef<HTMLIFrameElement>(null);

  // Store hooks
  const { components, selectedComponentId, selectComponent, isPreviewMode } = useBuilderStore();

  // Canvas transform hook
  const {
    transform: canvasTransform,
    pan,
    zoom: handleZoom,
    reset: resetZoom,
    spacePressed,
  } = useCanvasTransform();

  const [isDragging, setIsDragging] = useState(false);
  const [dragStart, setDragStart] = useState({ x: 0, y: 0 });
  const [selectedComponentTransform, setSelectedComponentTransform] = useState<CanvasTransform>({
    x: 0,
    y: 0,
    scale: 1,
    rotation: 0,
  });

  // Pan canvas with mouse
  const handleCanvasMouseDown = (e: React.MouseEvent) => {
    // Only pan if middle mouse button or space+drag
    if (e.button === 1 || (e.button === 0 && spacePressed)) {
      setIsDragging(true);
      setDragStart({ x: e.clientX, y: e.clientY });
    }
  };

  const handleCanvasMouseMove = (e: React.MouseEvent) => {
    if (!isDragging) return;

    const deltaX = e.clientX - dragStart.x;
    const deltaY = e.clientY - dragStart.y;

    pan(deltaX, deltaY);

    setDragStart({ x: e.clientX, y: e.clientY });
  };

  const handleCanvasMouseUp = () => {
    setIsDragging(false);
  };

  // Handle component selection
  const handleComponentClick = (componentId: string, e: React.MouseEvent) => {
    e.stopPropagation();
    selectComponent(componentId);

    // Set initial transform for selected component
    setSelectedComponentTransform({
      x: 0,
      y: 0,
      scale: 1,
      rotation: 0,
    });
  };

  // Keyboard shortcuts
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      // Delete selected component
      if (e.key === 'Delete' && selectedComponentId) {
        // TODO: implement delete
      }

      // Zoom with Ctrl/Cmd + scroll
      if ((e.ctrlKey || e.metaKey) && e.key === '+') {
        e.preventDefault();
        handleZoom('in');
      }
      if ((e.ctrlKey || e.metaKey) && e.key === '-') {
        e.preventDefault();
        handleZoom('out');
      }

      // Reset zoom with Ctrl/Cmd + 0
      if ((e.ctrlKey || e.metaKey) && e.key === '0') {
        e.preventDefault();
        resetZoom();
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [selectedComponentId, handleZoom, resetZoom]);

  // Handle scroll zoom
  const handleCanvasWheel = (e: React.WheelEvent) => {
    if (!e.ctrlKey && !e.metaKey) return;

    e.preventDefault();
    const direction = e.deltaY > 0 ? 'out' : 'in';
    handleZoom(direction);
  };

  if (isPreviewMode) {
    return <CanvasPreview iframeRef={iframeRef} components={components} />;
  }

  return (
    <div className="flex h-full flex-col bg-slate-900">
      {/* Toolbar */}
      <CanvasToolbar
        canvasTransform={canvasTransform}
        onResetZoom={resetZoom}
        onZoomIn={() => handleZoom('in')}
        onZoomOut={() => handleZoom('out')}
      />

      {/* Canvas Container */}
      <div
        ref={canvasRef}
        className="flex-1 overflow-hidden bg-slate-800 cursor-grab active:cursor-grabbing"
        onMouseDown={handleCanvasMouseDown}
        onMouseMove={handleCanvasMouseMove}
        onMouseUp={handleCanvasMouseUp}
        onMouseLeave={handleCanvasMouseUp}
        onWheel={handleCanvasWheel}
      >
        {/* Canvas viewport */}
        <div
          className="relative w-full h-full origin-center transition-transform"
          style={{
            transform: `translate(${canvasTransform.x}px, ${canvasTransform.y}px) scale(${canvasTransform.scale}) rotate(${canvasTransform.rotation}deg)`,
          }}
        >
          {/* Grid background */}
          <div
            className="absolute inset-0 pointer-events-none"
            style={{
              backgroundImage: `
                linear-gradient(0deg, transparent 24%, rgba(255,255,255,.05) 25%, rgba(255,255,255,.05) 26%, transparent 27%, transparent 74%, rgba(255,255,255,.05) 75%, rgba(255,255,255,.05) 76%, transparent 77%, transparent),
                linear-gradient(90deg, transparent 24%, rgba(255,255,255,.05) 25%, rgba(255,255,255,.05) 26%, transparent 27%, transparent 74%, rgba(255,255,255,.05) 75%, rgba(255,255,255,.05) 76%, transparent 77%, transparent)
              `,
              backgroundSize: '50px 50px',
            }}
          />

          {/* Components container */}
          <div className="relative w-full h-full min-h-[600px]">
            {components.length === 0 ? (
              <div className="absolute inset-0 flex items-center justify-center text-slate-400">
                <div className="text-center">
                  <p className="text-lg font-medium mb-2">No components yet</p>
                  <p className="text-sm">Add a component from the toolbar to start</p>
                </div>
              </div>
            ) : (
              components.map((component) => (
                <CanvasComponent
                  key={component.id}
                  component={component}
                  isSelected={component.id === selectedComponentId}
                  onSelect={(e) => handleComponentClick(component.id, e)}
                  transform={
                    component.id === selectedComponentId
                      ? selectedComponentTransform
                      : { x: 0, y: 0, scale: 1, rotation: 0 }
                  }
                  onTransformChange={setSelectedComponentTransform}
                />
              ))
            )}
          </div>

          {/* Selection box for selected component */}
          {selectedComponentId && (
            <ComponentSelectionBox
              componentId={selectedComponentId}
              transform={selectedComponentTransform}
              onTransformChange={setSelectedComponentTransform}
            />
          )}
        </div>
      </div>
    </div>
  );
};

interface CanvasComponentProps {
  component: any;
  isSelected: boolean;
  onSelect: (e: React.MouseEvent) => void;
  transform: CanvasTransform;
  onTransformChange: (transform: CanvasTransform) => void;
}

const CanvasComponent: React.FC<CanvasComponentProps> = ({
  component,
  isSelected,
  onSelect,
  transform,
  onTransformChange,
}) => {
  const baseClasses = {
    hero: 'w-full h-64 bg-gradient-to-r from-blue-600 to-purple-600 rounded-lg flex items-center justify-center text-white',
    section:
      'w-full h-48 bg-slate-700 rounded-lg p-4 flex items-center justify-center text-white border-2 border-slate-600',
    footer: 'w-full h-24 bg-slate-950 rounded-lg flex items-center justify-center text-white',
    navbar:
      'w-full h-16 bg-slate-800 rounded-lg flex items-center justify-between px-4 text-white border-b-2 border-slate-700',
    card: 'w-64 h-80 bg-white rounded-lg shadow-lg p-4 flex flex-col justify-between',
    cta: 'w-full h-32 bg-gradient-to-r from-green-500 to-emerald-500 rounded-lg flex items-center justify-center text-white font-bold',
    testimonial: 'w-full h-40 bg-slate-700 rounded-lg p-4 text-white italic border-l-4 border-blue-500',
    faq: 'w-full h-auto bg-slate-700 rounded-lg p-4 text-white',
  };

  return (
    <div
      className={`
        absolute p-2 rounded-lg cursor-pointer transition-all
        ${isSelected ? 'ring-2 ring-blue-500 ring-offset-2 ring-offset-slate-800' : 'hover:ring-1 hover:ring-slate-600'}
        ${baseClasses[component.type as keyof typeof baseClasses] || baseClasses.section}
      `}
      style={{
        transform: `translate(${transform.x}px, ${transform.y}px) scale(${transform.scale}) rotate(${transform.rotation}deg)`,
        transformOrigin: 'top left',
      }}
      onClick={onSelect}
      draggable
      onDragStart={(e) => {
        e.stopPropagation();
        e.dataTransfer?.setData('componentId', component.id);
      }}
    >
      <div className="flex items-center justify-center h-full">
        <span className="text-sm font-medium opacity-75">{component.name}</span>
      </div>

      {/* Component handles */}
      {isSelected && (
        <>
          <div className="absolute -top-2 -left-2 w-4 h-4 bg-blue-500 rounded-full cursor-nwse-resize" />
          <div className="absolute -top-2 -right-2 w-4 h-4 bg-blue-500 rounded-full cursor-nesw-resize" />
          <div className="absolute -bottom-2 -left-2 w-4 h-4 bg-blue-500 rounded-full cursor-nesw-resize" />
          <div className="absolute -bottom-2 -right-2 w-4 h-4 bg-blue-500 rounded-full cursor-nwse-resize" />
        </>
      )}
    </div>
  );
};
