'use client';

import { ReactNode } from 'react';

/**
 * SessionProvider wrapper
 * 
 * ⚠️ NOTE: This app uses JWT authentication (from C# backend),
 * NOT NextAuth. We don't need NextAuth's SessionProvider.
 * 
 * This is just a pass-through wrapper to maintain compatibility.
 * Actual auth is handled by Zustand store (authStore.ts)
 * which stores JWT tokens in localStorage.
 */
export function SessionProvider({ children }: { children: ReactNode }) {
  return <>{children}</>;
}
