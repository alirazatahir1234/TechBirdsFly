'use client';

import React, { useState } from 'react';
import { Sparkles, ArrowRight, Loader } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import toast from 'react-hot-toast';
import { useRouter } from 'next/navigation';

/**
 * Create Website Page (AI Generation Flow)
 * 
 * Steps:
 * 1. AI Prompt - Describe what website they want
 * 2. Choose Style - Modern, Minimal, Bold, Creative
 * 3. Choose Industry - Tech, E-commerce, Blog, Portfolio, Agency
 * 4. Color Palette - Select color scheme
 * 5. Generate - Create the website
 * 
 * Flow inspired by Base44, Durable AI, Mixo
 */

type GenerationStep = 'prompt' | 'style' | 'industry' | 'palette' | 'generating';

interface GenerationState {
  prompt: string;
  style: string;
  industry: string;
  palette: string;
}

const STYLES = [
  { id: 'modern', label: 'Modern', description: 'Clean & contemporary', color: 'from-blue-500 to-cyan-500' },
  { id: 'minimal', label: 'Minimal', description: 'Simple & elegant', color: 'from-gray-500 to-slate-500' },
  { id: 'bold', label: 'Bold', description: 'Vibrant & eye-catching', color: 'from-orange-500 to-red-500' },
  { id: 'creative', label: 'Creative', description: 'Artistic & unique', color: 'from-purple-500 to-pink-500' },
];

const INDUSTRIES = [
  { id: 'tech', label: 'Tech Startup', icon: '🚀' },
  { id: 'ecommerce', label: 'E-commerce', icon: '🛒' },
  { id: 'blog', label: 'Blog/Magazine', icon: '📝' },
  { id: 'portfolio', label: 'Portfolio', icon: '🎨' },
  { id: 'agency', label: 'Agency', icon: '🏢' },
  { id: 'saas', label: 'SaaS', icon: '💻' },
];

const PALETTES = [
  { id: 'vibrant', label: 'Vibrant', colors: ['#FF6B6B', '#4ECDC4', '#FFE66D'] },
  { id: 'calm', label: 'Calm', colors: ['#A8D8D8', '#7FB3D5', '#C9ADA7'] },
  { id: 'dark', label: 'Dark & Bold', colors: ['#1A1A1A', '#FF0000', '#FFFFFF'] },
  { id: 'sunset', label: 'Sunset', colors: ['#FF6B35', '#F7931E', '#FDB833'] },
  { id: 'ocean', label: 'Ocean', colors: ['#006994', '#0099CC', '#06B9D6'] },
  { id: 'forest', label: 'Forest', colors: ['#2D5016', '#61892F', '#A4C639'] },
];

export default function CreateWebsitePage() {
  const router = useRouter();
  const [step, setStep] = useState<GenerationStep>('prompt');
  const [state, setState] = useState<GenerationState>({
    prompt: '',
    style: '',
    industry: '',
    palette: '',
  });

  const canProceedToStyle = state.prompt.trim().length >= 10;
  const canProceedToIndustry = canProceedToStyle && state.style;
  const canProceedToPalette = canProceedToIndustry && state.industry;
  const canGenerate = canProceedToPalette && state.palette;

  const handleGenerate = async () => {
    if (!canGenerate) {
      toast.error('Please complete all steps');
      return;
    }

    setStep('generating');
    toast.loading('Creating your website with AI...', { id: 'generating' });

    try {
      // Simulate API call to GeneratorService
      // In production: POST /api/generate with state
      await new Promise(resolve => setTimeout(resolve, 2000));

      toast.success('Website created! Redirecting to editor...', { id: 'generating' });
      
      // Store generation state in sessionStorage for editor page
      sessionStorage.setItem('generatedWebsite', JSON.stringify(state));
      
      // Redirect to editor
      setTimeout(() => {
        router.push('/dashboard/editor');
      }, 1000);
    } catch (error) {
      console.error('Generation error:', error);
      toast.error('Failed to generate website', { id: 'generating' });
      setStep('palette');
    }
  };

  return (
    <div className="min-h-screen bg-linear-to-br from-gray-50 via-purple-50 to-gray-100">
      {/* Header */}
      <div className="px-6 py-8 bg-white border-b border-gray-200">
        <div className="max-w-4xl mx-auto">
          <div className="flex items-center gap-3 mb-2">
            <Sparkles className="w-8 h-8 text-purple-600" />
            <h1 className="text-3xl font-bold text-gray-900">Create Website with AI</h1>
          </div>
          <p className="text-gray-600">Describe your vision and let AI build your website in minutes</p>
        </div>
      </div>

      {/* Main Content */}
      <div className="max-w-4xl mx-auto px-6 py-12">
        {/* Progress Bar */}
        <div className="mb-12">
          <div className="flex items-center justify-between mb-4">
            <div className="flex items-center gap-2">
              <span className={`text-sm font-semibold ${step === 'prompt' || step === 'generating' ? 'text-purple-600' : 'text-gray-400'}`}>
                Describe
              </span>
              <ArrowRight className="w-4 h-4 text-gray-300" />
              <span className={`text-sm font-semibold ${step === 'style' || step === 'generating' ? 'text-purple-600' : 'text-gray-400'}`}>
                Style
              </span>
              <ArrowRight className="w-4 h-4 text-gray-300" />
              <span className={`text-sm font-semibold ${step === 'industry' || step === 'generating' ? 'text-purple-600' : 'text-gray-400'}`}>
                Industry
              </span>
              <ArrowRight className="w-4 h-4 text-gray-300" />
              <span className={`text-sm font-semibold ${step === 'palette' || step === 'generating' ? 'text-purple-600' : 'text-gray-400'}`}>
                Colors
              </span>
              <ArrowRight className="w-4 h-4 text-gray-300" />
              <span className={`text-sm font-semibold ${step === 'generating' ? 'text-purple-600' : 'text-gray-400'}`}>
                Generate
              </span>
            </div>
          </div>
          <div className="w-full bg-gray-200 rounded-full h-2">
            <div 
              className="bg-linear-to-r from-purple-600 to-indigo-600 h-2 rounded-full transition-all duration-300"
              style={{
                width: step === 'prompt' ? '20%' : step === 'style' ? '40%' : step === 'industry' ? '60%' : step === 'palette' ? '80%' : '100%'
              }}
            />
          </div>
        </div>

        {/* Step 1: AI Prompt */}
        {step === 'prompt' && (
          <div className="bg-white rounded-2xl border border-gray-200 p-8 shadow-sm">
            <h2 className="text-2xl font-bold text-gray-900 mb-2">Describe Your Website</h2>
            <p className="text-gray-600 mb-6">What kind of website do you want to create? Be as detailed as possible.</p>
            
            <textarea
              value={state.prompt}
              onChange={(e) => setState({ ...state, prompt: e.target.value })}
              placeholder="e.g., 'A modern SaaS landing page for a project management tool. Include hero section, features, pricing, and testimonials. Target audience is small teams and startups.'"
              className="w-full h-40 p-4 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent resize-none"
            />
            
            <div className="mt-6 flex justify-between items-center">
              <p className="text-sm text-gray-500">{state.prompt.length} characters</p>
              <Button
                onClick={() => setStep('style')}
                disabled={!canProceedToStyle}
                className="bg-linear-to-r from-purple-600 to-indigo-600 hover:from-purple-700 hover:to-indigo-700 text-white"
              >
                Continue to Style <ArrowRight className="w-4 h-4 ml-2" />
              </Button>
            </div>
          </div>
        )}

        {/* Step 2: Style Selection */}
        {step === 'style' && (
          <div className="bg-white rounded-2xl border border-gray-200 p-8 shadow-sm">
            <h2 className="text-2xl font-bold text-gray-900 mb-2">Choose Your Style</h2>
            <p className="text-gray-600 mb-6">Pick a design style that matches your brand</p>
            
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
              {STYLES.map((style) => (
                <button
                  key={style.id}
                  onClick={() => setState({ ...state, style: style.id })}
                  className={`p-6 rounded-lg border-2 transition-all ${
                    state.style === style.id
                      ? 'border-purple-600 bg-purple-50'
                      : 'border-gray-200 bg-gray-50 hover:border-purple-300'
                  }`}
                >
                  <div className={`w-full h-12 bg-linear-to-r ${style.color} rounded-lg mb-3`} />
                  <h3 className="font-semibold text-gray-900">{style.label}</h3>
                  <p className="text-sm text-gray-600 mt-1">{style.description}</p>
                </button>
              ))}
            </div>

            <div className="flex justify-between gap-4">
              <Button
                onClick={() => setStep('prompt')}
                variant="outline"
                className="border-gray-300"
              >
                ← Back
              </Button>
              <Button
                onClick={() => setStep('industry')}
                disabled={!canProceedToIndustry}
                className="bg-linear-to-r from-purple-600 to-indigo-600 hover:from-purple-700 hover:to-indigo-700 text-white"
              >
                Continue to Industry <ArrowRight className="w-4 h-4 ml-2" />
              </Button>
            </div>
          </div>
        )}

        {/* Step 3: Industry Selection */}
        {step === 'industry' && (
          <div className="bg-white rounded-2xl border border-gray-200 p-8 shadow-sm">
            <h2 className="text-2xl font-bold text-gray-900 mb-2">Select Your Industry</h2>
            <p className="text-gray-600 mb-6">What industry does your website belong to?</p>
            
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
              {INDUSTRIES.map((industry) => (
                <button
                  key={industry.id}
                  onClick={() => setState({ ...state, industry: industry.id })}
                  className={`p-6 rounded-lg border-2 transition-all text-left ${
                    state.industry === industry.id
                      ? 'border-purple-600 bg-purple-50'
                      : 'border-gray-200 bg-gray-50 hover:border-purple-300'
                  }`}
                >
                  <span className="text-3xl mb-2">{industry.icon}</span>
                  <h3 className="font-semibold text-gray-900">{industry.label}</h3>
                </button>
              ))}
            </div>

            <div className="flex justify-between gap-4">
              <Button
                onClick={() => setStep('style')}
                variant="outline"
                className="border-gray-300"
              >
                ← Back
              </Button>
              <Button
                onClick={() => setStep('palette')}
                disabled={!canProceedToPalette}
                className="bg-linear-to-r from-purple-600 to-indigo-600 hover:from-purple-700 hover:to-indigo-700 text-white"
              >
                Continue to Colors <ArrowRight className="w-4 h-4 ml-2" />
              </Button>
            </div>
          </div>
        )}

        {/* Step 4: Color Palette Selection */}
        {step === 'palette' && (
          <div className="bg-white rounded-2xl border border-gray-200 p-8 shadow-sm">
            <h2 className="text-2xl font-bold text-gray-900 mb-2">Choose Color Palette</h2>
            <p className="text-gray-600 mb-6">Select a color scheme for your website</p>
            
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
              {PALETTES.map((palette) => (
                <button
                  key={palette.id}
                  onClick={() => setState({ ...state, palette: palette.id })}
                  className={`p-6 rounded-lg border-2 transition-all ${
                    state.palette === palette.id
                      ? 'border-purple-600 bg-purple-50'
                      : 'border-gray-200 bg-gray-50 hover:border-purple-300'
                  }`}
                >
                  <div className="flex gap-2 mb-3">
                    {palette.colors.map((color, idx) => (
                      <div
                        key={idx}
                        className="flex-1 h-12 rounded-lg"
                        style={{ backgroundColor: color }}
                      />
                    ))}
                  </div>
                  <h3 className="font-semibold text-gray-900">{palette.label}</h3>
                </button>
              ))}
            </div>

            <div className="flex justify-between gap-4">
              <Button
                onClick={() => setStep('industry')}
                variant="outline"
                className="border-gray-300"
              >
                ← Back
              </Button>
              <Button
                onClick={handleGenerate}
                disabled={!canGenerate || step === 'generating'}
                className="bg-linear-to-r from-purple-600 to-indigo-600 hover:from-purple-700 hover:to-indigo-700 text-white"
              >
                {step === 'generating' ? (
                  <>
                    <Loader className="w-4 h-4 mr-2 animate-spin" />
                    Creating Website...
                  </>
                ) : (
                  <>
                    Create My Website <Sparkles className="w-4 h-4 ml-2" />
                  </>
                )}
              </Button>
            </div>
          </div>
        )}

        {/* Generation Complete */}
        {step === 'generating' && (
          <div className="bg-white rounded-2xl border border-gray-200 p-12 shadow-sm text-center">
            <Loader className="w-12 h-12 animate-spin text-purple-600 mx-auto mb-4" />
            <h2 className="text-2xl font-bold text-gray-900 mb-2">Creating Your Website</h2>
            <p className="text-gray-600">AI is generating your website based on your preferences...</p>
            
            <div className="mt-8 space-y-2 max-w-md mx-auto text-left text-sm">
              <p className="flex items-center gap-2">
                <span className="text-purple-600">✓</span> Analyzing requirements
              </p>
              <p className="flex items-center gap-2">
                <span className="text-purple-600">✓</span> Generating layout
              </p>
              <p className="flex items-center gap-2">
                <span className="text-gray-400">⟳</span> Creating content
              </p>
              <p className="flex items-center gap-2">
                <span className="text-gray-400">⟳</span> Applying design
              </p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
