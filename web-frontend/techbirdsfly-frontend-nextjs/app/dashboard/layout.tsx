'use client';

import { ReactNode } from 'react';
import { ProtectedRoute } from '@/components/auth/ProtectedRoute';
import DashboardLayout from '@/components/layout/DashboardLayout';

interface DashboardLayoutProps {
  children: ReactNode;
}

/**
 * Layout for all dashboard routes /(dashboard)/*
 * 
 * ✅ Wraps ALL child routes with ProtectedRoute
 * ✅ Includes sidebar, header, and authentication protection
 * ✅ All child routes are automatically protected without per-page boilerplate
 * 
 * Flow:
 * 1. ProtectedRoute checks if user is hydrated + authenticated
 * 2. If not authenticated after hydration → redirects to /login
 * 3. If authenticated → shows DashboardLayout with sidebar + children
 */
export default function DashboardGroupLayout({ children }: DashboardLayoutProps) {
  return (
    <ProtectedRoute>
      <DashboardLayout title="Dashboard">
        {children}
      </DashboardLayout>
    </ProtectedRoute>
  );
}
