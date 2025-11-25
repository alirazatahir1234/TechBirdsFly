import { create } from 'zustand';
import { devtools, persist } from 'zustand/middleware';

// ============================================================================
// API Base URL Configuration
// ============================================================================
// Uses NEXT_PUBLIC_API_BASE from environment, defaults to gateway on port 5500
const API_BASE = process.env.NEXT_PUBLIC_API_BASE || 'http://localhost:5500/api';

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
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

  // Hydration flag
  _hasHydrated?: boolean;

  // Actions
  setUser: (user: User | null) => void;
  setToken: (token: string, refreshToken: string) => void;
  setIsLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;

  // Auth actions
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  register: (email: string, firstName: string, lastName: string, password: string) => Promise<void>;
  forgotPassword: (email: string) => Promise<{ resetToken: string }>;
  resetPassword: (email: string, resetToken: string, newPassword: string) => Promise<void>;
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
            const response = await fetch(`${API_BASE}/auth/login`, {
              method: 'POST',
              headers: { 'Content-Type': 'application/json' },
              body: JSON.stringify({ email, password }),
            });

            // ✅ Read JSON only ONCE
            const data = await response.json();
            
            console.log('API_BASE =', API_BASE);
            console.log('Response status =', response.status);
            console.log('Response data =', data);

            if (!response.ok) {
              throw new Error(data.message || data.error || 'Login failed');
            }

            const { user, accessToken, refreshToken } = data;

            // ✅ If backend doesn't return user, create a minimal user object from token
            if (user) {
              get().setUser(user);
            } else if (accessToken) {
              // Decode JWT to extract user info
              try {
                const base64Url = accessToken.split('.')[1];
                const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
                const jsonPayload = decodeURIComponent(atob(base64).split('').map((c) => {
                  return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
                }).join(''));
                
                const tokenData = JSON.parse(jsonPayload);
                console.log('Decoded token:', tokenData);
                
                // Map C# claim types to simple names
                // C# uses full URIs like: http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier
                const claimNameId = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
                const claimEmail = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress';
                const claimGivenName = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname';
                const claimSurname = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname';
                
                // Create user object from token claims (with C# URI fallbacks)
                const userFromToken: User = {
                  id: tokenData[claimNameId] || tokenData.sub || tokenData.id || 'unknown',
                  email: tokenData[claimEmail] || tokenData.email || 'unknown',
                  firstName: tokenData[claimGivenName] || tokenData.firstName || 'User',
                  lastName: tokenData[claimSurname] || tokenData.lastName || '',
                  role: tokenData.role || 'user',
                  createdAt: tokenData.createdAt || new Date().toISOString(),
                };
                
                console.log('User from token:', userFromToken);
                get().setUser(userFromToken);
              } catch (decodeErr) {
                console.warn('Could not decode token, setting minimal user', decodeErr);
                get().setUser({
                  id: 'unknown',
                  email: 'unknown',
                  firstName: 'User',
                  lastName: '',
                  role: 'user',
                  createdAt: new Date().toISOString(),
                });
              }
            }

            get().setToken(accessToken, refreshToken || '');
            set({ isLoading: false });
          } catch (err) {
            const error = err instanceof Error ? err.message : 'An error occurred';
            console.error('Login error:', error);
            set({ error, isLoading: false });
            throw err;
          }
        },

        register: async (email: string, firstName: string, lastName: string, password: string) => {
          set({ isLoading: true, error: null });
          try {
            const response = await fetch(`${API_BASE}/auth/register`, {
              method: 'POST',
              headers: { 'Content-Type': 'application/json' },
              body: JSON.stringify({ email, firstName, lastName, password, confirmPassword: password }),
            });

            // ✅ Read JSON only ONCE
            const data = await response.json();
            
            console.log('API_BASE =', API_BASE);
            console.log('Response status =', response.status);
            console.log('Response data =', data);

            if (!response.ok) {
              throw new Error(data.message || data.error || 'Registration failed');
            }

            const { user, accessToken, refreshToken } = data;

            // ✅ If backend doesn't return user, create a minimal user object from token
            if (user) {
              get().setUser(user);
            } else if (accessToken) {
              // Decode JWT to extract user info
              try {
                const base64Url = accessToken.split('.')[1];
                const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
                const jsonPayload = decodeURIComponent(atob(base64).split('').map((c) => {
                  return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
                }).join(''));
                
                const tokenData = JSON.parse(jsonPayload);
                console.log('Decoded token:', tokenData);
                
                // Map C# claim types to simple names
                // C# uses full URIs like: http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier
                const claimNameId = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
                const claimEmail = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress';
                const claimGivenName = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname';
                const claimSurname = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname';
                
                // Create user object from token claims (with C# URI fallbacks)
                const userFromToken: User = {
                  id: tokenData[claimNameId] || tokenData.sub || tokenData.id || 'unknown',
                  email: tokenData[claimEmail] || tokenData.email || 'unknown',
                  firstName: tokenData[claimGivenName] || tokenData.firstName || 'User',
                  lastName: tokenData[claimSurname] || tokenData.lastName || '',
                  role: tokenData.role || 'user',
                  createdAt: tokenData.createdAt || new Date().toISOString(),
                };
                
                console.log('User from token:', userFromToken);
                get().setUser(userFromToken);
              } catch (decodeErr) {
                console.warn('Could not decode token, setting minimal user', decodeErr);
                get().setUser({
                  id: 'unknown',
                  email: 'unknown',
                  firstName: 'User',
                  lastName: '',
                  role: 'user',
                  createdAt: new Date().toISOString(),
                });
              }
            }

            get().setToken(accessToken, refreshToken || '');
            set({ isLoading: false });
          } catch (err) {
            const error = err instanceof Error ? err.message : 'An error occurred';
            console.error('Registration error:', error);
            set({ error, isLoading: false });
            throw err;
          }
        },

        forgotPassword: async (email: string) => {
          set({ isLoading: true, error: null });
          try {
            const response = await fetch(`${API_BASE}/auth/forgot-password`, {
              method: 'POST',
              headers: { 'Content-Type': 'application/json' },
              body: JSON.stringify({ email }),
            });

            if (!response.ok) {
              const data = await response.json();
              throw new Error(data.message || 'Failed to send reset email');
            }

            const data = await response.json();
            set({ isLoading: false });
            return { resetToken: data.resetToken };
          } catch (err) {
            const error = err instanceof Error ? err.message : 'An error occurred';
            set({ error, isLoading: false });
            throw err;
          }
        },

        resetPassword: async (email: string, resetToken: string, newPassword: string) => {
          set({ isLoading: true, error: null });
          try {
            const response = await fetch(`${API_BASE}/auth/reset-password`, {
              method: 'POST',
              headers: { 'Content-Type': 'application/json' },
              body: JSON.stringify({ email, resetToken, newPassword }),
            });

            if (!response.ok) {
              const data = await response.json();
              throw new Error(data.message || 'Failed to reset password');
            }

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
        onRehydrateStorage: () => (state) => {
          // After localStorage is loaded, ensure isAuthenticated matches token status
          if (state) {
            state._hasHydrated = true;
            // Set isAuthenticated based on token existence
            state.isAuthenticated = !!state.token && !!state.user;
            console.log('🔄 Rehydrated from localStorage - isAuthenticated:', state.isAuthenticated, 'token exists:', !!state.token);
          }
        },
      }
    )
  )
);
