import { create } from 'zustand';
import { devtools, persist } from 'zustand/middleware';

export interface User {
  id: string;
  email: string;
  fullName: string;
  avatar?: string;
  role: 'user' | 'admin';
  createdAt: string;
}

export interface AuthState {
  // User data
  user: User | null;
  token: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;

  // Loading states
  isLoading: boolean;
  error: string | null;

  // Actions
  setUser: (user: User | null) => void;
  setToken: (token: string, refreshToken: string) => void;
  setIsLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;

  // Auth actions
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  register: (email: string, fullName: string, password: string) => Promise<void>;
  updateUser: (updates: Partial<User>) => void;
  clearError: () => void;
}

export const useAuthStore = create<AuthState>()(
  devtools(
    persist(
      (set, get) => ({
        user: null,
        token: null,
        refreshToken: null,
        isAuthenticated: false,
        isLoading: false,
        error: null,

        setUser: (user) =>
          set({
            user,
            isAuthenticated: !!user,
          }),

        setToken: (token, refreshToken) => {
          localStorage.setItem('token', token);
          localStorage.setItem('refreshToken', refreshToken);
          set({ token, refreshToken, isAuthenticated: true });
        },

        setIsLoading: (loading) => set({ isLoading: loading }),
        setError: (error) => set({ error }),

        login: async (email: string, password: string) => {
          set({ isLoading: true, error: null });
          try {
            const response = await fetch('http://localhost:5000/api/auth/login', {
              method: 'POST',
              headers: { 'Content-Type': 'application/json' },
              body: JSON.stringify({ email, password }),
            });

            if (!response.ok) {
              const data = await response.json();
              throw new Error(data.message || 'Login failed');
            }

            const data = await response.json();
            const { user, accessToken, refreshToken } = data;

            get().setUser(user);
            get().setToken(accessToken, refreshToken || '');
            set({ isLoading: false });
          } catch (err) {
            const error = err instanceof Error ? err.message : 'An error occurred';
            set({ error, isLoading: false });
            throw err;
          }
        },

        register: async (email: string, fullName: string, password: string) => {
          set({ isLoading: true, error: null });
          try {
            const response = await fetch('http://localhost:5000/api/auth/register', {
              method: 'POST',
              headers: { 'Content-Type': 'application/json' },
              body: JSON.stringify({ email, fullName, password }),
            });

            if (!response.ok) {
              const data = await response.json();
              throw new Error(data.message || 'Registration failed');
            }

            const data = await response.json();
            const { user, accessToken, refreshToken } = data;

            get().setUser(user);
            get().setToken(accessToken, refreshToken || '');
            set({ isLoading: false });
          } catch (err) {
            const error = err instanceof Error ? err.message : 'An error occurred';
            set({ error, isLoading: false });
            throw err;
          }
        },

        logout: () => {
          localStorage.removeItem('token');
          localStorage.removeItem('refreshToken');
          set({
            user: null,
            token: null,
            refreshToken: null,
            isAuthenticated: false,
            error: null,
          });
        },

        updateUser: (updates) =>
          set((state) => ({
            user: state.user ? { ...state.user, ...updates } : null,
          })),

        clearError: () => set({ error: null }),
      }),
      {
        name: 'auth-store', // localStorage key
        partialize: (state) => ({
          token: state.token,
          refreshToken: state.refreshToken,
          user: state.user,
        }),
      }
    )
  )
);
