import { useEffect, useState } from 'react';
import { useAuthStore } from '@/lib/store/authStore';

/**
 * ✅ CRITICAL HOOK: Properly detects when Zustand has finished hydrating from localStorage
 * 
 * WHY THIS MATTERS:
 * - localStorage is empty on first render (async operation)
 * - If you check isAuthenticated before localStorage loads, it will be false
 * - This causes false redirects and hydration mismatches
 * - Solution: Wait for onFinishHydration callback before rendering protected content
 * 
 * FLOW:
 * 1. Component mounts → isHydrated = false
 * 2. useAuthStore loads from localStorage asynchronously
 * 3. onFinishHydration callback fires → isHydrated = true
 * 4. ProtectedRoute can now trust the authentication state
 * 
 * @returns {boolean} true when Zustand has rehydrated from localStorage, false during hydration
 */
export function useAuthHydration() {
  const [isHydrated, setIsHydrated] = useState(false);
  
  useEffect(() => {
    // Use Zustand's built-in hydration detection callback
    // This fires AFTER localStorage has been read and merged into state
    const unsubscribe = useAuthStore.persist.onFinishHydration(() => {
      console.log('✅ [useAuthHydration] Zustand hydration complete - localStorage loaded');
      setIsHydrated(true);
    });

    // Fallback: also check if already hydrated synchronously
    // (in case hydration completed before effect runs)
    const state = useAuthStore.getState();
    if ((state as any)._hasHydrated) {
      console.log('✅ [useAuthHydration] Already hydrated on mount');
      setIsHydrated(true);
    }

    // Cleanup function to unsubscribe from hydration listener
    return () => {
      unsubscribe?.();
    };
  }, []);

  return isHydrated;
}

/**
 * ✅ Check if user is actually authenticated
 * 
 * Returns true ONLY when ALL of these are true:
 * 1. ✔ Hydration is complete (localStorage loaded)
 * 2. ✔ isAuthenticated flag is true in store
 * 3. ✔ token exists (not null/empty)
 * 4. ✔ user object exists (not null)
 * 
 * USAGE:
 * - In ProtectedRoute: if (!isAuthenticated) redirect to login
 * - In Navigation: if (isAuthenticated) show logout button
 * - In Components: if (isAuthenticated) show premium features
 * 
 * @returns {boolean} true if user has valid token and user object, false otherwise
 */
export function useIsAuthenticated() {
  const { isAuthenticated, token, user } = useAuthStore();
  const isHydrated = useAuthHydration();

  // All four conditions must be true for authentication
  const isFullyAuthenticated = isHydrated && isAuthenticated && !!token && !!user;
  
  if (!isHydrated) {
    console.log('⏳ [useIsAuthenticated] Still hydrating...');
  } else if (!isFullyAuthenticated) {
    console.log('❌ [useIsAuthenticated] Not authenticated:', {
      isHydrated,
      isAuthenticated,
      hasToken: !!token,
      hasUser: !!user,
    });
  } else {
    console.log('✅ [useIsAuthenticated] User authenticated:', user?.email);
  }

  return isFullyAuthenticated;
}
