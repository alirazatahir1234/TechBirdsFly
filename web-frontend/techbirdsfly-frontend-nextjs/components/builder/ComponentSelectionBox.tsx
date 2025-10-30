'use client';

import React, { useState, useRef, useEffect } from 'react';
import { useBuilderStore } from '@/lib/store/builderStore';

interface CanvasTransform {
  x: number;
  y: number;
  scale: number;
  rotation: number;
}

interface ComponentSelectionBoxProps {
  componentId: string;
  transform: CanvasTransform;
  onTransformChange: (transform: CanvasTransform) => void;
}

export const ComponentSelectionBox: React.FC<ComponentSelectionBoxProps> = ({
  componentId,
  transform,
  onTransformChange,
}) => {
  const { updateComponent } = useBuilderStore();
  const [isDragging, setIsDragging] = useState<string | null>(null);
  const [dragStart, setDragStart] = useState({ x: 0, y: 0 });
  const containerRef = useRef<HTMLDivElement>(null);

  // Handle resize from corners/edges
  const handleHandleMouseDown = (handle: string, e: React.MouseEvent) => {
    e.stopPropagation();
    setIsDragging(handle);
    setDragStart({ x: e.clientX, y: e.clientY });
  };

  // Handle move (when dragging the box itself)
  const handleBoxMouseDown = (e: React.MouseEvent) => {
    if ((e.target as HTMLElement).classList.contains('selection-box-handle')) return;
    e.stopPropagation();
    setIsDragging('move');
    setDragStart({ x: e.clientX, y: e.clientY });
  };

  useEffect(() => {
    const handleMouseMove = (e: MouseEvent) => {
      if (!isDragging) return;

      const deltaX = e.clientX - dragStart.x;
      const deltaY = e.clientY - dragStart.y;

      switch (isDragging) {
        case 'move':
          onTransformChange({
            ...transform,
            x: transform.x + deltaX,
            y: transform.y + deltaY,
          });
          break;

        case 'rotate':
          // Calculate rotation based on mouse angle
          const centerX = transform.x + (containerRef.current?.offsetWidth || 0) / 2;
          const centerY = transform.y + (containerRef.current?.offsetHeight || 0) / 2;
          const angle = Math.atan2(e.clientY - centerY, e.clientX - centerX);
          const rotation = (angle * 180) / Math.PI;
          onTransformChange({
            ...transform,
            rotation: Math.round(rotation),
          });
          break;

        case 'scale-nw':
        case 'scale-ne':
        case 'scale-sw':
        case 'scale-se':
          // Simple uniform scale based on diagonal distance
          const distance = Math.sqrt(deltaX * deltaX + deltaY * deltaY);
          const newScale = Math.max(0.5, transform.scale + distance * 0.01);
          onTransformChange({
            ...transform,
            scale: parseFloat(newScale.toFixed(2)),
          });
          break;
      }

      setDragStart({ x: e.clientX, y: e.clientY });
    };

    const handleMouseUp = () => {
      if (isDragging) {
        updateComponent(componentId, {
          properties: {
            transform,
          },
        });
      }
      setIsDragging(null);
    };

    if (isDragging) {
      document.addEventListener('mousemove', handleMouseMove);
      document.addEventListener('mouseup', handleMouseUp);

      return () => {
        document.removeEventListener('mousemove', handleMouseMove);
        document.removeEventListener('mouseup', handleMouseUp);
      };
    }
  }, [isDragging, dragStart, transform, componentId, onTransformChange, updateComponent]);

  return (
    <div
      ref={containerRef}
      className="absolute pointer-events-auto selection-box"
      style={{
        transform: `translate(${transform.x}px, ${transform.y}px) scale(${transform.scale}) rotate(${transform.rotation}deg)`,
        transformOrigin: 'top left',
      }}
      onMouseDown={handleBoxMouseDown}
    >
      {/* Resize handles */}
      <div className="absolute -top-2 -left-2 w-4 h-4 bg-blue-500 rounded-full cursor-nwse-resize selection-box-handle"
        onMouseDown={(e) => handleHandleMouseDown('scale-nw', e)} />
      <div className="absolute -top-2 left-1/2 -translate-x-1/2 w-4 h-4 bg-blue-500 rounded-full cursor-ns-resize selection-box-handle"
        onMouseDown={(e) => handleHandleMouseDown('scale-n', e)} />
      <div className="absolute -top-2 -right-2 w-4 h-4 bg-blue-500 rounded-full cursor-nesw-resize selection-box-handle"
        onMouseDown={(e) => handleHandleMouseDown('scale-ne', e)} />

      <div className="absolute top-1/2 -left-2 -translate-y-1/2 w-4 h-4 bg-blue-500 rounded-full cursor-ew-resize selection-box-handle"
        onMouseDown={(e) => handleHandleMouseDown('scale-w', e)} />
      <div className="absolute top-1/2 -right-2 -translate-y-1/2 w-4 h-4 bg-blue-500 rounded-full cursor-ew-resize selection-box-handle"
        onMouseDown={(e) => handleHandleMouseDown('scale-e', e)} />

      <div className="absolute -bottom-2 -left-2 w-4 h-4 bg-blue-500 rounded-full cursor-nesw-resize selection-box-handle"
        onMouseDown={(e) => handleHandleMouseDown('scale-sw', e)} />
      <div className="absolute -bottom-2 left-1/2 -translate-x-1/2 w-4 h-4 bg-blue-500 rounded-full cursor-ns-resize selection-box-handle"
        onMouseDown={(e) => handleHandleMouseDown('scale-s', e)} />
      <div className="absolute -bottom-2 -right-2 w-4 h-4 bg-blue-500 rounded-full cursor-nwse-resize selection-box-handle"
        onMouseDown={(e) => handleHandleMouseDown('scale-se', e)} />

      {/* Rotation handle */}
      <div
        className="absolute -top-8 left-1/2 -translate-x-1/2 w-4 h-4 bg-purple-500 rounded-full cursor-grab active:cursor-grabbing selection-box-handle"
        onMouseDown={(e) => handleHandleMouseDown('rotate', e)}
        title="Rotate"
      />
    </div>
  );
};
