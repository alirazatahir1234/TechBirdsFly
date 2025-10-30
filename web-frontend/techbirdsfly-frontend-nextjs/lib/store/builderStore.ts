import { create } from 'zustand';
import { devtools, persist } from 'zustand/middleware';

export interface WebsiteComponent {
  id: string;
  type: 'hero' | 'section' | 'footer' | 'navbar' | 'card' | 'cta' | 'testimonial' | 'faq';
  name: string;
  properties: Record<string, any>;
  children?: WebsiteComponent[];
}

export interface BuilderState {
  // Website data
  websiteName: string;
  websiteDescription: string;
  components: WebsiteComponent[];
  selectedWebsiteType: 'business' | 'ecommerce' | 'portfolio' | 'blog' | 'custom';

  // Editor state
  selectedComponentId: string | null;
  isPreviewMode: boolean;
  isSaving: boolean;

  // Actions
  setWebsiteName: (name: string) => void;
  setWebsiteDescription: (description: string) => void;
  setSelectedWebsiteType: (type: BuilderState['selectedWebsiteType']) => void;

  // Component management
  addComponent: (component: WebsiteComponent, parentId?: string) => void;
  removeComponent: (componentId: string) => void;
  updateComponent: (componentId: string, updates: Partial<WebsiteComponent>) => void;
  selectComponent: (componentId: string | null) => void;
  getSelectedComponent: () => WebsiteComponent | null;

  // Editor actions
  togglePreviewMode: () => void;
  setIsSaving: (saving: boolean) => void;
  resetBuilder: () => void;

  // Undo/Redo
  history: WebsiteComponent[][];
  historyIndex: number;
  addToHistory: () => void;
  undo: () => void;
  redo: () => void;
}

const initialState = {
  websiteName: 'My Website',
  websiteDescription: 'A beautiful website',
  components: [],
  selectedWebsiteType: 'business' as const,
  selectedComponentId: null,
  isPreviewMode: false,
  isSaving: false,
  history: [],
  historyIndex: -1,
};

export const useBuilderStore = create<BuilderState>()(
  devtools(
    persist(
      (set, get) => ({
        ...initialState,

        setWebsiteName: (name) => set({ websiteName: name }),
        setWebsiteDescription: (description) => set({ websiteDescription: description }),
        setSelectedWebsiteType: (type) => set({ selectedWebsiteType: type }),

        addComponent: (component, parentId) => {
          set((state) => {
            let newComponents = [...state.components];

            if (parentId) {
              // Find parent and add as child
              const addToParent = (components: WebsiteComponent[]): WebsiteComponent[] => {
                return components.map((c) => {
                  if (c.id === parentId) {
                    return {
                      ...c,
                      children: [...(c.children || []), component],
                    };
                  }
                  if (c.children) {
                    return {
                      ...c,
                      children: addToParent(c.children),
                    };
                  }
                  return c;
                });
              };
              newComponents = addToParent(newComponents);
            } else {
              newComponents.push(component);
            }

            return { components: newComponents };
          });
          get().addToHistory();
        },

        removeComponent: (componentId) => {
          set((state) => {
            const removeFromComponents = (components: WebsiteComponent[]): WebsiteComponent[] => {
              return components
                .filter((c) => c.id !== componentId)
                .map((c) => ({
                  ...c,
                  children: c.children ? removeFromComponents(c.children) : c.children,
                }));
            };

            return { components: removeFromComponents(state.components) };
          });
          get().addToHistory();
        },

        updateComponent: (componentId, updates) => {
          set((state) => {
            const updateInComponents = (components: WebsiteComponent[]): WebsiteComponent[] => {
              return components.map((c) => {
                if (c.id === componentId) {
                  return { ...c, ...updates };
                }
                if (c.children) {
                  return {
                    ...c,
                    children: updateInComponents(c.children),
                  };
                }
                return c;
              });
            };

            return { components: updateInComponents(state.components) };
          });
          get().addToHistory();
        },

        selectComponent: (componentId) => set({ selectedComponentId: componentId }),

        getSelectedComponent: () => {
          const { components, selectedComponentId } = get();
          if (!selectedComponentId) return null;

          const findComponent = (comps: WebsiteComponent[]): WebsiteComponent | null => {
            for (const comp of comps) {
              if (comp.id === selectedComponentId) {
                return comp;
              }
              if (comp.children) {
                const found = findComponent(comp.children);
                if (found) return found;
              }
            }
            return null;
          };

          return findComponent(components);
        },

        togglePreviewMode: () => set((state) => ({ isPreviewMode: !state.isPreviewMode })),
        setIsSaving: (saving) => set({ isSaving: saving }),

        resetBuilder: () => set({ ...initialState, history: [], historyIndex: -1 }),

        addToHistory: () => {
          set((state) => {
            const newHistory = state.history.slice(0, state.historyIndex + 1);
            newHistory.push(JSON.parse(JSON.stringify(state.components)));

            return {
              history: newHistory,
              historyIndex: newHistory.length - 1,
            };
          });
        },

        undo: () => {
          const state = get();
          if (state.historyIndex > 0) {
            const newIndex = state.historyIndex - 1;
            set({
              components: JSON.parse(JSON.stringify(state.history[newIndex])),
              historyIndex: newIndex,
            });
          }
        },

        redo: () => {
          const state = get();
          if (state.historyIndex < state.history.length - 1) {
            const newIndex = state.historyIndex + 1;
            set({
              components: JSON.parse(JSON.stringify(state.history[newIndex])),
              historyIndex: newIndex,
            });
          }
        },
      }),
      {
        name: 'builder-store', // localStorage key
      }
    )
  )
);
