import { NextRequest, NextResponse } from 'next/server';
import { auth } from '@/auth';

// Protected routes that require authentication
const protectedRoutes = ['/dashboard', '/builder', '/projects', '/settings', '/profile'];

export async function middleware(request: NextRequest) {
  const pathname = request.nextUrl.pathname;

  // Check if route is protected
  const isProtectedRoute = protectedRoutes.some((route) =>
    pathname.startsWith(route)
  );

  if (!isProtectedRoute) {
    return NextResponse.next();
  }

  // Get session
  const session = await auth();

  // If no session and trying to access protected route, redirect to login
  if (!session) {
    const loginUrl = new URL('/login', request.url);
    loginUrl.searchParams.set('callbackUrl', pathname);
    return NextResponse.redirect(loginUrl);
  }

  // If authenticated, allow access
  return NextResponse.next();
}

// Configure which routes trigger the middleware
export const config = {
  matcher: [
    '/dashboard/:path*',
    '/builder/:path*',
    '/projects/:path*',
    '/settings/:path*',
    '/profile/:path*',
  ],
};
