'use client';

import { useEffect, ReactNode } from 'react';
import { useRouter } from 'next/navigation';
import { useAuthHydration, useIsAuthenticated } from '@/lib/hooks/useAuthHydration';

interface ProtectedRouteProps {
  children: ReactNode;
  fallback?: ReactNode;
}

/**
 * ✅ Wrapper component for protecting routes from unauthenticated access
 * 
 * CRITICAL BEHAVIOR:
 * 1. ⏳ While hydrating → shows loading spinner (prevents flash/redirect flicker)
 * 2. ✔ After hydration + authenticated → renders children
 * 3. ❌ After hydration + NOT authenticated → redirects to /login with router.replace()
 * 
 * ⚠️ IMPORTANT NOTES:
 * - Uses router.replace() NOT router.push() (prevents back button issues)
 * - Hydration detection uses Zustand's onFinishHydration callback
 * - No race conditions - waits for localStorage to be loaded
 * - useEffect ensures redirect only happens in browser, not during SSR
 * 
 * Usage:
 * <ProtectedRoute>
 *   <YourProtectedComponent />
 * </ProtectedRoute>
 */
export function ProtectedRoute({ children, fallback }: ProtectedRouteProps) {
  const router = useRouter();
  const isHydrated = useAuthHydration();
  const isAuthenticated = useIsAuthenticated();

  useEffect(() => {
    // Only redirect AFTER hydration is complete and we know user is NOT authenticated
    if (isHydrated && !isAuthenticated) {
      console.log('🔐 [ProtectedRoute] Not authenticated - redirecting to /auth/login');
      router.replace('/auth/login');
    }
  }, [isAuthenticated, isHydrated, router]);

  // ⏳ Show loading state while hydrating
  // This prevents:
  // - Flash of redirects before hydration is complete
  // - Hydration mismatch warnings
  // - Race conditions between localStorage load and initial render
  if (!isHydrated) {
    return (
      fallback || (
        <div className="flex items-center justify-center h-screen bg-gray-50">
          <div className="text-center">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-purple-600 mx-auto mb-4"></div>
            <p className="text-gray-600 font-medium">Checking authentication...</p>
          </div>
        </div>
      )
    );
  }

  // If still not authenticated after hydration, return null
  // The useEffect above will handle the redirect
  if (!isAuthenticated) {
    return null;
  }

  // ✅ User is authenticated - render children
  return <>{children}</>;
}
