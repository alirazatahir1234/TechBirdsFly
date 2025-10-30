'use client';

import { useSession, signIn, signOut } from 'next-auth/react';
import { useEffect } from 'react';
import { useAuthStore } from '@/lib/store/authStore';

/**
 * Hook to use NextAuth session with Zustand store integration
 * Automatically syncs NextAuth session to Zustand authStore
 */
export function useNextAuthSession() {
  const { data: session, status } = useSession();
  const { setUser, setToken, logout } = useAuthStore();

  useEffect(() => {
    if (status === 'authenticated' && session?.user) {
      // Sync NextAuth session to Zustand store
      setUser({
        id: session.user.id || '',
        email: session.user.email || '',
        fullName: session.user.name || '',
        avatar: session.user.image || undefined,
        role: 'user',
        createdAt: new Date().toISOString(),
      });

      // Set token and refresh token from session
      if ((session as any).accessToken) {
        setToken((session as any).accessToken, (session as any).refreshToken);
      }
    } else if (status === 'unauthenticated') {
      logout();
    }
  }, [status, session, setUser, setToken, logout]);

  return {
    session,
    isLoading: status === 'loading',
    isAuthenticated: status === 'authenticated',
    signIn: (email: string, password: string) =>
      signIn('credentials', { email, password, redirect: false }),
    signInWithGoogle: () => signIn('google'),
    signInWithFacebook: () => signIn('facebook'),
    signOut: () => signOut({ redirect: false }),
  };
}

export { useSession, signIn, signOut };
