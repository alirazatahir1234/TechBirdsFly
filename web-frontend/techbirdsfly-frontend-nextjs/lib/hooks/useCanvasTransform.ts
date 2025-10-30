'use client';

import { useState, useCallback, useEffect } from 'react';

export interface CanvasTransform {
  x: number;
  y: number;
  scale: number;
  rotation: number;
}

export const useCanvasTransform = (initialTransform?: CanvasTransform) => {
  const [transform, setTransform] = useState<CanvasTransform>(
    initialTransform || {
      x: 0,
      y: 0,
      scale: 1,
      rotation: 0,
    }
  );

  const [spacePressed, setSpacePressed] = useState(false);

  // Track space key press
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.code === 'Space') {
        setSpacePressed(true);
        e.preventDefault();
      }
    };

    const handleKeyUp = (e: KeyboardEvent) => {
      if (e.code === 'Space') {
        setSpacePressed(false);
        e.preventDefault();
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    window.addEventListener('keyup', handleKeyUp);

    return () => {
      window.removeEventListener('keydown', handleKeyDown);
      window.removeEventListener('keyup', handleKeyUp);
    };
  }, []);

  const pan = useCallback((deltaX: number, deltaY: number) => {
    setTransform((prev) => ({
      ...prev,
      x: prev.x + deltaX,
      y: prev.y + deltaY,
    }));
  }, []);

  const zoom = useCallback((direction: 'in' | 'out', factor = 0.1) => {
    setTransform((prev) => ({
      ...prev,
      scale: Math.max(
        0.5,
        Math.min(3, direction === 'in' ? prev.scale + factor : prev.scale - factor)
      ),
    }));
  }, []);

  const rotate = useCallback((angle: number) => {
    setTransform((prev) => ({
      ...prev,
      rotation: (prev.rotation + angle) % 360,
    }));
  }, []);

  const reset = useCallback(() => {
    setTransform({
      x: 0,
      y: 0,
      scale: 1,
      rotation: 0,
    });
  }, []);

  const setTransformState = useCallback((newTransform: Partial<CanvasTransform>) => {
    setTransform((prev) => ({ ...prev, ...newTransform }));
  }, []);

  return {
    transform,
    pan,
    zoom,
    rotate,
    reset,
    setTransform: setTransformState,
    spacePressed,
  };
};
