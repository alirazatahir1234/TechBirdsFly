import { NextRequest, NextResponse } from 'next/server';

/**
 * ⚠️ IMPORTANT: JWT Authentication Architecture
 * 
 * This app uses JWT tokens stored in localStorage (client-side).
 * Middleware CANNOT access localStorage, so we CANNOT validate tokens here.
 * 
 * Authentication is handled 100% client-side by Zustand authStore:
 * - User logs in → Backend returns JWT tokens
 * - Tokens stored in localStorage
 * - Dashboard checks useAuthStore() hook
 * - If not authenticated → redirects to /login (client-side)
 * 
 * Therefore, middleware should NOT block any routes.
 * Client-side components handle auth validation.
 */

export function middleware(request: NextRequest) {
  // Allow all routes to load
  // Client-side Zustand store will handle authentication checks
  return NextResponse.next();
}

// Don't run middleware on any routes
// All auth is client-side via Zustand
export const config = {
  matcher: [],
};
