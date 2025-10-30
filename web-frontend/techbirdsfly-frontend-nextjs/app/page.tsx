'use client';

import React from 'react';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import Link from 'next/link';
import { Code2, Zap, Palette, ChevronRight } from 'lucide-react';
import Navigation from '@/components/Navigation';

export default function Home() {
  return (
    <div className="min-h-screen bg-white">
      {/* Navigation */}
      <Navigation />

      {/* Hero Section */}
      <section className="bg-gradient-to-r from-indigo-600 via-purple-600 to-pink-600 text-white py-20 px-4 md:px-8">
        <div className="max-w-6xl mx-auto">
          <div className="flex flex-col md:flex-row items-center justify-between gap-12 py-12">
            {/* Left Content */}
            <div className="flex-1 md:max-w-md">
              <h1 className="text-5xl md:text-6xl font-bold mb-6 leading-tight">
                Create Stunning Websites with AI
              </h1>
              <p className="text-xl text-indigo-100 mb-8">
                TechBirdsFly generates beautiful, responsive websites powered by artificial intelligence
              </p>

              {/* Dropdown and CTA */}
              <div className="flex flex-col sm:flex-row gap-4 mb-8">
                <select className="px-4 py-3 rounded-lg bg-white/20 text-white border border-white/30 backdrop-blur-sm focus:outline-none focus:ring-2 focus:ring-white font-medium">
                  <option value="business">Business Website</option>
                  <option value="ecommerce">E-Commerce Store</option>
                  <option value="portfolio">Portfolio</option>
                  <option value="blog">Blog</option>
                </select>

                <Button
                  className="px-8 py-3 bg-gradient-to-r from-yellow-400 to-orange-500 text-gray-900 font-bold rounded-lg hover:shadow-lg transition-all text-lg"
                  onClick={() => alert('Generate Website')}
                >
                  ✨ Generate Website
                </Button>
              </div>

              {/* Call to Action Buttons */}
              <div className="flex gap-4">
                <Button
                  variant="outline"
                  className="px-6 py-2 bg-transparent border-2 border-white text-white hover:bg-white/10"
                >
                  View Demo
                </Button>
                <Button
                  variant="outline"
                  className="px-6 py-2 bg-transparent border-2 border-white text-white hover:bg-white/10"
                >
                  Watch Tutorial
                </Button>
              </div>
            </div>

            {/* Right Visual */}
            <div className="flex-1 hidden md:block">
              <div className="relative w-full h-80 bg-gradient-to-br from-white/10 to-white/5 rounded-2xl border border-white/20 backdrop-blur-sm flex items-center justify-center">
                <div className="text-center">
                  <Palette className="w-24 h-24 mx-auto mb-4 text-indigo-200 opacity-50" />
                  <p className="text-white/60 text-lg">Website Preview</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="py-20 px-4 md:px-8">
        <div className="max-w-6xl mx-auto">
          {/* Section Header */}
          <div className="text-center mb-16">
            <h2 className="text-4xl md:text-5xl font-bold text-gray-900 mb-4">
              Powerful AI-Driven Features
            </h2>
            <p className="text-xl text-gray-600">
              Everything you need to create, customize, and deploy professional websites
            </p>
          </div>

          {/* Feature Cards */}
          <div className="grid md:grid-cols-3 gap-8">
            {/* Feature 1: AI Website Builder */}
            <Card className="p-8 hover:shadow-xl transition-shadow border-0 bg-gradient-to-br from-blue-50 to-indigo-50">
              <div className="mb-6">
                <div className="w-16 h-16 bg-blue-600 rounded-xl flex items-center justify-center mb-4">
                  <Zap className="w-8 h-8 text-white" />
                </div>
              </div>
              <h3 className="text-2xl font-bold text-gray-900 mb-3">AI Website Builder</h3>
              <p className="text-gray-600 mb-6">
                Describe your vision and watch our AI create a complete website with modern design, responsive layout, and optimized code.
              </p>
              <ul className="space-y-3">
                <li className="flex items-center gap-3">
                  <span className="text-green-500 font-bold">✓</span>
                  <span className="text-gray-700">Natural language prompts</span>
                </li>
                <li className="flex items-center gap-3">
                  <span className="text-green-500 font-bold">✓</span>
                  <span className="text-gray-700">Smart component selection</span>
                </li>
                <li className="flex items-center gap-3">
                  <span className="text-green-500 font-bold">✓</span>
                  <span className="text-gray-700">SEO optimized output</span>
                </li>
              </ul>
            </Card>

            {/* Feature 2: Smart Templates */}
            <Card className="p-8 hover:shadow-xl transition-shadow border-0 bg-gradient-to-br from-purple-50 to-pink-50">
              <div className="mb-6">
                <div className="w-16 h-16 bg-purple-600 rounded-xl flex items-center justify-center mb-4">
                  <Palette className="w-8 h-8 text-white" />
                </div>
              </div>
              <h3 className="text-2xl font-bold text-gray-900 mb-3">Smart Templates</h3>
              <p className="text-gray-600 mb-6">
                Choose from hundreds of AI-generated templates or let our system recommend the perfect design for your industry.
              </p>
              <ul className="space-y-3">
                <li className="flex items-center gap-3">
                  <span className="text-green-500 font-bold">✓</span>
                  <span className="text-gray-700">Industry-specific designs</span>
                </li>
                <li className="flex items-center gap-3">
                  <span className="text-green-500 font-bold">✓</span>
                  <span className="text-gray-700">Customizable components</span>
                </li>
                <li className="flex items-center gap-3">
                  <span className="text-green-500 font-bold">✓</span>
                  <span className="text-gray-700">Mobile-first approach</span>
                </li>
              </ul>
            </Card>

            {/* Feature 3: Export to Code */}
            <Card className="p-8 hover:shadow-xl transition-shadow border-0 bg-gradient-to-br from-green-50 to-emerald-50">
              <div className="mb-6">
                <div className="w-16 h-16 bg-green-600 rounded-xl flex items-center justify-center mb-4">
                  <Code2 className="w-8 h-8 text-white" />
                </div>
              </div>
              <h3 className="text-2xl font-bold text-gray-900 mb-3">Export to Code</h3>
              <p className="text-gray-600 mb-6">
                Get clean, production-ready code that you can host anywhere. Full ownership of your website's source code.
              </p>
              <ul className="space-y-3">
                <li className="flex items-center gap-3">
                  <span className="text-green-500 font-bold">✓</span>
                  <span className="text-gray-700">HTML, CSS, JavaScript</span>
                </li>
                <li className="flex items-center gap-3">
                  <span className="text-green-500 font-bold">✓</span>
                  <span className="text-gray-700">Framework integration</span>
                </li>
                <li className="flex items-center gap-3">
                  <span className="text-green-500 font-bold">✓</span>
                  <span className="text-gray-700">Hosting flexibility</span>
                </li>
              </ul>
            </Card>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="bg-gradient-to-r from-indigo-600 to-purple-600 text-white py-16 px-4 md:px-8">
        <div className="max-w-4xl mx-auto text-center">
          <h2 className="text-4xl font-bold mb-6">Ready to Build Your Website?</h2>
          <p className="text-xl text-indigo-100 mb-8">
            Join thousands of users creating beautiful websites with TechBirdsFly
          </p>
          <Link href="/register">
            <Button className="px-8 py-3 bg-white text-purple-600 font-bold rounded-lg hover:bg-gray-100 text-lg">
              Get Started Free <ChevronRight className="ml-2 w-5 h-5" />
            </Button>
          </Link>
        </div>
      </section>

      {/* Footer */}
      <footer className="bg-gray-900 text-gray-300 py-12 px-4 md:px-8">
        <div className="max-w-6xl mx-auto">
          <div className="grid md:grid-cols-4 gap-8 mb-8">
            <div>
              <h4 className="text-white font-bold mb-4">Product</h4>
              <ul className="space-y-2">
                <li><a href="#" className="hover:text-white">Features</a></li>
                <li><a href="#" className="hover:text-white">Pricing</a></li>
                <li><a href="#" className="hover:text-white">Templates</a></li>
              </ul>
            </div>
            <div>
              <h4 className="text-white font-bold mb-4">Company</h4>
              <ul className="space-y-2">
                <li><a href="#" className="hover:text-white">About</a></li>
                <li><a href="#" className="hover:text-white">Blog</a></li>
                <li><a href="#" className="hover:text-white">Contact</a></li>
              </ul>
            </div>
            <div>
              <h4 className="text-white font-bold mb-4">Legal</h4>
              <ul className="space-y-2">
                <li><a href="#" className="hover:text-white">Privacy</a></li>
                <li><a href="#" className="hover:text-white">Terms</a></li>
              </ul>
            </div>
            <div>
              <h4 className="text-white font-bold mb-4">Social</h4>
              <ul className="space-y-2">
                <li><a href="#" className="hover:text-white">Twitter</a></li>
                <li><a href="#" className="hover:text-white">GitHub</a></li>
                <li><a href="#" className="hover:text-white">Discord</a></li>
              </ul>
            </div>
          </div>
          <div className="border-t border-gray-700 pt-8 text-center">
            <p>&copy; 2025 TechBirdsFly. All rights reserved.</p>
          </div>
        </div>
      </footer>
    </div>
  );
}
