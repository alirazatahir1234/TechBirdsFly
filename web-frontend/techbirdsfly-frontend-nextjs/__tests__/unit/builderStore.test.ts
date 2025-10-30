import { describe, it, expect, beforeEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useBuilderStore } from '@/lib/store/builderStore';
import { mockHeroComponent, mockSectionComponent } from '../utils/mock-data';

describe('builderStore', () => {
  beforeEach(() => {
    // Reset store before each test
    const { resetBuilder } = useBuilderStore.getState();
    resetBuilder();
  });

  describe('Component Management', () => {
    it('should add a component', () => {
      const { result } = renderHook(() => useBuilderStore());

      act(() => {
        result.current.addComponent(mockHeroComponent);
      });

      expect(result.current.components).toHaveLength(1);
      expect(result.current.components[0]).toEqual(mockHeroComponent);
    });

    it('should remove a component', () => {
      const { result } = renderHook(() => useBuilderStore());

      act(() => {
        result.current.addComponent(mockHeroComponent);
        result.current.addComponent(mockSectionComponent);
      });

      expect(result.current.components).toHaveLength(2);

      act(() => {
        result.current.removeComponent(mockHeroComponent.id);
      });

      expect(result.current.components).toHaveLength(1);
      expect(result.current.components[0].id).toBe(mockSectionComponent.id);
    });

    it('should update a component', () => {
      const { result } = renderHook(() => useBuilderStore());

      act(() => {
        result.current.addComponent(mockHeroComponent);
      });

      act(() => {
        result.current.updateComponent(mockHeroComponent.id, {
          name: 'Updated Hero',
          properties: {
            title: 'New Title',
          },
        });
      });

      const updatedComponent = result.current.components[0];
      expect(updatedComponent.name).toBe('Updated Hero');
      expect(updatedComponent.properties.title).toBe('New Title');
    });

    it('should select a component', () => {
      const { result } = renderHook(() => useBuilderStore());

      act(() => {
        result.current.addComponent(mockHeroComponent);
        result.current.selectComponent(mockHeroComponent.id);
      });

      expect(result.current.selectedComponentId).toBe(mockHeroComponent.id);
    });

    it('should get selected component', () => {
      const { result } = renderHook(() => useBuilderStore());

      act(() => {
        result.current.addComponent(mockHeroComponent);
        result.current.selectComponent(mockHeroComponent.id);
      });

      const selected = result.current.getSelectedComponent();
      expect(selected).toEqual(mockHeroComponent);
    });

    it('should return null when no component is selected', () => {
      const { result } = renderHook(() => useBuilderStore());

      const selected = result.current.getSelectedComponent();
      expect(selected).toBeNull();
    });
  });

  describe('Website Metadata', () => {
    it('should set website name', () => {
      const { result } = renderHook(() => useBuilderStore());

      act(() => {
        result.current.setWebsiteName('My Awesome Website');
      });

      expect(result.current.websiteName).toBe('My Awesome Website');
    });

    it('should set website description', () => {
      const { result } = renderHook(() => useBuilderStore());

      act(() => {
        result.current.setWebsiteDescription('A description');
      });

      expect(result.current.websiteDescription).toBe('A description');
    });

    it('should set website type', () => {
      const { result } = renderHook(() => useBuilderStore());

      act(() => {
        result.current.setSelectedWebsiteType('ecommerce');
      });

      expect(result.current.selectedWebsiteType).toBe('ecommerce');
    });
  });

  describe('Editor State', () => {
    it('should toggle preview mode', () => {
      const { result } = renderHook(() => useBuilderStore());

      expect(result.current.isPreviewMode).toBe(false);

      act(() => {
        result.current.togglePreviewMode();
      });

      expect(result.current.isPreviewMode).toBe(true);

      act(() => {
        result.current.togglePreviewMode();
      });

      expect(result.current.isPreviewMode).toBe(false);
    });

    it('should set saving state', () => {
      const { result } = renderHook(() => useBuilderStore());

      expect(result.current.isSaving).toBe(false);

      act(() => {
        result.current.setIsSaving(true);
      });

      expect(result.current.isSaving).toBe(true);
    });
  });

  describe('History (Undo/Redo)', () => {
    it('should add to history when component is added', () => {
      const { result } = renderHook(() => useBuilderStore());

      const initialHistoryLength = result.current.history.length;

      act(() => {
        result.current.addComponent(mockHeroComponent);
      });

      expect(result.current.history.length).toBeGreaterThan(initialHistoryLength);
    });

    it.skip('should undo component addition', () => {
      // TODO: Implement undo functionality in store
      const { result } = renderHook(() => useBuilderStore());

      act(() => {
        result.current.addComponent(mockHeroComponent);
      });

      expect(result.current.components).toHaveLength(1);

      act(() => {
        result.current.undo();
      });

      expect(result.current.components).toHaveLength(0);
    });

    it.skip('should redo component addition', () => {
      // TODO: Implement redo functionality in store
      const { result } = renderHook(() => useBuilderStore());

      act(() => {
        result.current.addComponent(mockHeroComponent);
        result.current.undo();
      });

      expect(result.current.components).toHaveLength(0);

      act(() => {
        result.current.redo();
      });

      expect(result.current.components).toHaveLength(1);
    });
  });

  describe('Reset', () => {
    it('should reset builder to initial state', () => {
      const { result } = renderHook(() => useBuilderStore());

      act(() => {
        result.current.setWebsiteName('Test Site');
        result.current.addComponent(mockHeroComponent);
        result.current.selectComponent(mockHeroComponent.id);
      });

      expect(result.current.websiteName).toBe('Test Site');
      expect(result.current.components).toHaveLength(1);

      act(() => {
        result.current.resetBuilder();
      });

      expect(result.current.websiteName).toBe('My Website');
      expect(result.current.components).toHaveLength(0);
      expect(result.current.selectedComponentId).toBeNull();
    });
  });
});
