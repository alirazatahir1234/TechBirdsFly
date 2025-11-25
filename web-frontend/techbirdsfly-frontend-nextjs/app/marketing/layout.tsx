'use client';

import { ReactNode } from 'react';
import Navigation from '@/components/Navigation';

interface MarketingLayoutProps {
  children: ReactNode;
}

/**
 * Layout for marketing/public pages
 * Includes navigation at the top
 */
export default function MarketingLayout({ children }: MarketingLayoutProps) {
  return (
    <>
      <Navigation />
      {children}
    </>
  );
}
