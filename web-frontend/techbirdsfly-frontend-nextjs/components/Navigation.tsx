'use client';

import React from 'react';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Palette } from 'lucide-react';

export default function Navigation() {
  return (
    <nav className="bg-white shadow-md sticky top-0 z-50">
      <div className="max-w-6xl mx-auto px-4 md:px-8 py-4">
        <div className="flex items-center justify-between">
          {/* Logo */}
          <Link href="/" className="flex items-center gap-2 font-bold text-2xl text-indigo-600">
            <div className="w-10 h-10 bg-indigo-600 rounded-lg flex items-center justify-center">
              <Palette className="text-white w-6 h-6" />
            </div>
            <span>TechBirdsFly.AI</span>
          </Link>

          {/* Nav Links */}
          <div className="hidden md:flex items-center gap-8">
            <Link href="#features" className="text-gray-700 hover:text-indigo-600 font-medium">
              Features
            </Link>
            <Link href="#pricing" className="text-gray-700 hover:text-indigo-600 font-medium">
              Pricing
            </Link>
            <Link href="/docs" className="text-gray-700 hover:text-indigo-600 font-medium">
              Docs
            </Link>
          </div>

          {/* Auth Buttons */}
          <div className="flex items-center gap-4">
            <Link href="/login">
              <Button variant="ghost" className="text-gray-700 hover:bg-gray-100">
                Login
              </Button>
            </Link>
            <Link href="/register">
              <Button className="px-6 py-2 bg-purple-600 text-white hover:bg-purple-700 rounded-lg font-medium">
                Sign Up
              </Button>
            </Link>
          </div>
        </div>
      </div>
    </nav>
  );
}
