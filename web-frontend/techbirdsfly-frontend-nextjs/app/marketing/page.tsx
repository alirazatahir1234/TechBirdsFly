'use client';

import Link from 'next/link';
import { Zap, Code, Palette, FileText, Image, Rocket, Check, Star } from 'lucide-react';

export default function HomePage() {
  return (
    <div className="min-h-screen bg-white">
      {/* ===== HERO SECTION ===== */}
      <section className="py-20 px-4 md:py-28 bg-linear-to-br from-purple-50 via-white to-blue-50">
        <div className="max-w-6xl mx-auto">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-12 items-center">
            {/* Left: Text */}
            <div>
              <h1 className="text-5xl md:text-6xl font-bold text-gray-900 mb-6 leading-tight">
                Create Stunning Websites in Minutes — Powered by AI
              </h1>
              <p className="text-xl text-gray-600 mb-8 leading-relaxed max-w-xl">
                TechBirdsFly instantly generates modern, responsive, and production-ready websites using advanced AI.
                Customize, export, and deploy — all inside one platform.
              </p>
              
              {/* CTAs */}
              <div className="flex flex-col md:flex-row gap-4 mb-10">
                <Link href="/auth/register" className="px-8 py-4 bg-purple-600 text-white rounded-lg font-semibold hover:bg-purple-700 transition-all text-center">
                  Generate Your Website
                </Link>
                <button className="px-8 py-4 border-2 border-purple-600 text-purple-600 rounded-lg font-semibold hover:bg-purple-50 transition-all">
                  Watch Demo
                </button>
              </div>

              {/* Trust Badges */}
              <div className="flex flex-wrap gap-4">
                <div className="flex items-center gap-2 text-gray-700">
                  <Check size={20} className="text-green-600" />
                  <span>AI-Powered</span>
                </div>
                <div className="flex items-center gap-2 text-gray-700">
                  <Check size={20} className="text-green-600" />
                  <span>Production-Ready</span>
                </div>
                <div className="flex items-center gap-2 text-gray-700">
                  <Check size={20} className="text-green-600" />
                  <span>No Coding Needed</span>
                </div>
                <div className="flex items-center gap-2 text-gray-700">
                  <Check size={20} className="text-green-600" />
                  <span>Export to React/Next.js</span>
                </div>
              </div>
            </div>

            {/* Right: Hero Visual */}
            <div className="bg-linear-to-br from-purple-100 to-blue-100 rounded-2xl h-96 flex items-center justify-center">
              <div className="text-center">
                <Code size={80} className="text-purple-600 mx-auto mb-4" />
                <p className="text-gray-600 font-medium">Dashboard Preview</p>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* ===== FEATURES SECTION ===== */}
      <section className="py-24 px-4 bg-white">
        <div className="max-w-6xl mx-auto">
          <div className="text-center mb-16">
            <h2 className="text-4xl md:text-5xl font-bold text-gray-900 mb-4">
              Build Websites 10× Faster With AI Automation
            </h2>
            <p className="text-xl text-gray-600 max-w-3xl mx-auto">
              Whether you're a developer, designer, or entrepreneur — TechBirdsFly helps you build production-quality websites instantly.
            </p>
          </div>

          {/* Feature Grid */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {[
              {
                icon: Zap,
                title: 'AI Website Generator',
                description: 'Instantly create complete web pages, UI components, and responsive layouts with one prompt.'
              },
              {
                icon: Code,
                title: 'Export to React / Next.js',
                description: 'Generate real, production-ready code compatible with Next.js, Tailwind, and modern frameworks.'
              },
              {
                icon: Palette,
                title: 'Full Theme Customizer',
                description: 'Update colors, typography, spacing, and layout globally with one click.'
              },
              {
                icon: FileText,
                title: 'Smart Content Generator',
                description: 'AI writes professional copy, SEO tags, button labels, and images automatically.'
              },
              {
                icon: Image,
                title: 'Image & Asset Generator',
                description: 'Generate logos, banners, favicons, and illustrations using AI.'
              },
              {
                icon: Rocket,
                title: '1-Click Deployment',
                description: 'Export your website or deploy directly to Vercel, Netlify, or custom hosting.'
              },
            ].map((feature, i) => {
              const Icon = feature.icon;
              return (
                <div key={i} className="p-8 border border-gray-200 rounded-xl hover:shadow-lg transition-all hover:border-purple-200">
                  <Icon size={40} className="text-purple-600 mb-4" />
                  <h3 className="text-xl font-bold text-gray-900 mb-3">
                    {feature.title}
                  </h3>
                  <p className="text-gray-600 leading-relaxed">
                    {feature.description}
                  </p>
                </div>
              );
            })}
          </div>
        </div>
      </section>

      {/* ===== HOW IT WORKS SECTION ===== */}
      <section className="py-24 px-4 bg-gray-50">
        <div className="max-w-6xl mx-auto">
          <div className="text-center mb-16">
            <h2 className="text-4xl md:text-5xl font-bold text-gray-900 mb-4">
              How TechBirdsFly Works
            </h2>
          </div>

          {/* Step Cards */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {[
              {
                step: '1',
                title: 'Describe Your Website',
                description: 'Enter your brand name, industry, and the type of website you want. AI understands your requirements instantly.'
              },
              {
                step: '2',
                title: 'AI Generates Everything',
                description: 'TechBirdsFly creates the full landing page, sections, UI components, images, and text.'
              },
              {
                step: '3',
                title: 'Customize & Export',
                description: 'Edit anything → preview → export to clean React/Next.js code → deploy.'
              },
            ].map((item, i) => (
              <div key={i} className="bg-white p-8 rounded-xl border border-gray-200 text-center">
                <div className="inline-flex items-center justify-center w-12 h-12 bg-purple-600 text-white rounded-full font-bold text-xl mb-6">
                  {item.step}
                </div>
                <h3 className="text-2xl font-bold text-gray-900 mb-4">
                  {item.title}
                </h3>
                <p className="text-gray-600 leading-relaxed">
                  {item.description}
                </p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* ===== AI DEMO SECTION ===== */}
      <section className="py-24 px-4 bg-white">
        <div className="max-w-6xl mx-auto text-center">
          <h2 className="text-4xl md:text-5xl font-bold text-gray-900 mb-6">
            See TechBirdsFly in Action
          </h2>
          
          {/* Demo Mockup */}
          <div className="bg-linear-to-br from-purple-50 to-blue-50 rounded-2xl h-96 md:h-[500px] flex items-center justify-center border border-gray-200">
            <div className="text-center">
              <Zap size={80} className="text-purple-600 mx-auto mb-4" />
              <p className="text-gray-600 font-medium text-lg mb-2">Builder Interface Preview</p>
              <p className="text-gray-500">Generated with AI in under 15 seconds</p>
            </div>
          </div>
        </div>
      </section>

      {/* ===== PRICING SECTION ===== */}
      <section className="py-24 px-4 bg-gray-50">
        <div className="max-w-6xl mx-auto">
          <div className="text-center mb-16">
            <h2 className="text-4xl md:text-5xl font-bold text-gray-900 mb-4">
              Simple, Transparent Pricing
            </h2>
          </div>

          {/* Pricing Cards */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {[
              {
                name: 'Starter',
                price: '0',
                badge: 'FREE',
                features: [
                  'AI website creator (basic)',
                  '5 projects',
                  'Export HTML & CSS',
                  'Community support'
                ],
                highlighted: false
              },
              {
                name: 'Pro',
                price: '19',
                badge: 'POPULAR',
                features: [
                  'Full AI website builder',
                  'Unlimited projects',
                  'Export React & Next.js',
                  'AI Image Generator',
                  'Custom themes',
                  'Priority support'
                ],
                highlighted: true
              },
              {
                name: 'Enterprise',
                price: 'Custom',
                badge: 'CUSTOM',
                features: [
                  'Everything in Pro',
                  'API access',
                  'Team collaboration',
                  'White-label solutions',
                  'Dedicated support'
                ],
                highlighted: false
              },
            ].map((plan, i) => (
              <div key={i} className={`rounded-xl p-8 ${plan.highlighted ? 'bg-white border-2 border-purple-600 shadow-lg' : 'bg-white border border-gray-200'}`}>
                <div className="mb-4">
                  <span className="text-xs font-bold text-purple-600 bg-purple-50 px-3 py-1 rounded-full">
                    {plan.badge}
                  </span>
                </div>
                <h3 className="text-2xl font-bold text-gray-900 mb-2">
                  {plan.name}
                </h3>
                <div className="mb-6">
                  <span className="text-5xl font-bold text-gray-900">${plan.price}</span>
                  {plan.price !== 'Custom' && <span className="text-gray-600 ml-2">/month</span>}
                </div>
                
                <Link href="/auth/register" className={`block w-full py-3 rounded-lg font-semibold mb-8 text-center transition-all ${plan.highlighted ? 'bg-purple-600 text-white hover:bg-purple-700' : 'border-2 border-gray-200 text-gray-900 hover:bg-gray-50'}`}>
                  Get Started
                </Link>

                <ul className="space-y-4">
                  {plan.features.map((feature, j) => (
                    <li key={j} className="flex items-center gap-3 text-gray-700">
                      <Check size={20} className="text-green-600 shrink-0" />
                      <span>{feature}</span>
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* ===== TESTIMONIALS SECTION ===== */}
      <section className="py-24 px-4 bg-white">
        <div className="max-w-6xl mx-auto">
          <div className="text-center mb-16">
            <h2 className="text-4xl md:text-5xl font-bold text-gray-900 mb-4">
              Loved by Developers, Designers & Founders
            </h2>
          </div>

          {/* Testimonial Cards */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            {[
              {
                quote: 'TechBirdsFly reduced our development time by 70%. We shipped our client website in 2 days instead of 2 weeks.',
                author: 'CEO, Infinity Technologies',
                rating: 5
              },
              {
                quote: 'The AI builder generates clean Next.js code. Huge time-saver for our development team. Highly recommended!',
                author: 'Senior Developer, UAE',
                rating: 5
              },
              {
                quote: 'As a founder with zero coding experience, I was able to create a professional website in minutes. Game-changer!',
                author: 'Founder, TechStartup Inc',
                rating: 5
              },
              {
                quote: 'The export quality is production-ready. No manual code cleanup needed. Saves hours every project.',
                author: 'Senior Designer, Creative Agency',
                rating: 5
              },
            ].map((testimonial, i) => (
              <div key={i} className="bg-gray-50 p-8 rounded-xl border border-gray-200">
                <div className="flex gap-1 mb-4">
                  {[...Array(testimonial.rating)].map((_, j) => (
                    <Star key={j} size={18} className="fill-yellow-400 text-yellow-400" />
                  ))}
                </div>
                <p className="text-lg text-gray-700 mb-6 italic">
                  "{testimonial.quote}"
                </p>
                <p className="font-semibold text-gray-900">
                  {testimonial.author}
                </p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* ===== FINAL CTA SECTION ===== */}
      <section className="py-24 px-4 bg-linear-to-r from-purple-600 to-blue-600 text-white">
        <div className="max-w-4xl mx-auto text-center">
          <h2 className="text-4xl md:text-5xl font-bold mb-6">
            Ready to Build Your Website in Minutes?
          </h2>
          <p className="text-xl text-purple-100 mb-10 leading-relaxed">
            Start generating stunning websites with AI — fast, simple, powerful. Join thousands of creators and developers using TechBirdsFly today.
          </p>
          
          <div className="flex flex-col md:flex-row gap-4 justify-center">
            <Link href="/auth/register" className="px-8 py-4 bg-white text-purple-600 rounded-lg font-semibold hover:bg-purple-50 transition-all">
              Start Generating Website
            </Link>
            <button className="px-8 py-4 border-2 border-white text-white rounded-lg font-semibold hover:bg-white hover:bg-opacity-10 transition-all">
              Try Demo
            </button>
          </div>
        </div>
      </section>
    </div>
  );
}
