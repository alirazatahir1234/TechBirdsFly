'use client';

import { ReactNode } from 'react';

interface AuthLayoutProps {
  children: ReactNode;
}

/**
 * Layout for authentication pages (login, register, forgot-password)
 * No navigation header, centered layout
 */
export default function AuthLayout({ children }: AuthLayoutProps) {
  return (
    <div className="min-h-screen flex flex-col">
      {children}
    </div>
  );
}
