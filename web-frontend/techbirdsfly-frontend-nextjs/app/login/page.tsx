'use client';

import React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/components/ui/button';
import Link from 'next/link';
import { Mail, Lock, Chrome, Facebook } from 'lucide-react';
import { FormInput } from '@/components/forms/FormInput';
import { FormCheckbox } from '@/components/forms/FormCheckbox';
import { loginSchema, LoginFormData } from '@/lib/schemas/auth';
import { useAuthStore } from '@/lib/store/authStore';
import { useRouter } from 'next/navigation';

export default function LoginPage() {
  const router = useRouter();
  const { login, isLoading } = useAuthStore();
  const [apiError, setApiError] = React.useState('');

  const {
    control,
    handleSubmit,
    formState: { isSubmitting },
  } = useForm({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: '',
      password: '',
      rememberMe: false,
    },
  });

  const onSubmit = async (data: LoginFormData) => {
    setApiError('');
    try {
      await login(data.email, data.password);
      router.push('/dashboard');
    } catch (error) {
      setApiError(error instanceof Error ? error.message : 'Login failed');
    }
  };

  const handleGoogleLogin = () => {
    console.log('Google login - implement OAuth flow');
  };

  const handleFacebookLogin = () => {
    console.log('Facebook login - implement OAuth flow');
  };

  const isLoading_ = isSubmitting || isLoading;

  return (
    <div className="min-h-screen flex">
      {/* Left Side - Login Form */}
      <div className="w-full lg:w-1/2 flex items-center justify-center p-6 md:p-12 bg-gray-50">
        <div className="w-full max-w-md">
          {/* Header */}
          <div className="mb-8">
            <h1 className="text-4xl font-bold text-gray-900 mb-2">Login</h1>
            <p className="text-gray-600">How do I get started lorem ipsum dolor at?</p>
          </div>

          {/* Error Message */}
          {apiError && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
              {apiError}
            </div>
          )}

          {/* Login Form */}
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
            {/* Email */}
            <FormInput
              control={control}
              name="email"
              label="Email"
              placeholder="Enter your email"
              type="email"
              icon={<Mail className="w-5 h-5" />}
              required
            />

            {/* Password */}
            <FormInput
              control={control}
              name="password"
              label="Password"
              placeholder="Enter your password"
              type="password"
              icon={<Lock className="w-5 h-5" />}
              required
            />

            {/* Remember Me */}
            <FormCheckbox
              control={control}
              name="rememberMe"
              label="Remember me"
            />

            {/* Forgot Password */}
            <div className="text-right">
              <Link href="/forgot-password" className="text-purple-600 hover:text-purple-700 font-medium text-sm">
                Forgot password?
              </Link>
            </div>

            {/* Sign In Button */}
            <Button
              type="submit"
              disabled={isLoading_}
              className="w-full py-3 bg-purple-600 hover:bg-purple-700 text-white font-semibold rounded-lg transition-all"
            >
              {isLoading_ ? 'Signing in...' : 'Sign in'}
            </Button>
          </form>

          {/* Divider */}
          <div className="my-8 flex items-center">
            <div className="flex-1 border-t border-gray-300"></div>
            <span className="px-4 text-gray-500 text-sm">Or continue with</span>
            <div className="flex-1 border-t border-gray-300"></div>
          </div>

          {/* Social Login Buttons */}
          <div className="space-y-3">
            <button
              onClick={handleGoogleLogin}
              type="button"
              className="w-full py-3 px-4 border border-gray-300 rounded-lg font-medium text-gray-700 hover:bg-gray-50 transition-all flex items-center justify-center gap-2"
            >
              <Chrome className="w-5 h-5 text-blue-500" />
              Sign in with Google
            </button>

            <button
              onClick={handleFacebookLogin}
              type="button"
              className="w-full py-3 px-4 border border-gray-300 rounded-lg font-medium text-gray-700 hover:bg-gray-50 transition-all flex items-center justify-center gap-2"
            >
              <Facebook className="w-5 h-5 text-blue-600" />
              Sign in with Facebook
            </button>
          </div>

          {/* Sign Up Link */}
          <p className="text-center text-gray-600 mt-8">
            Don't have an account?{' '}
            <Link href="/register" className="text-purple-600 hover:text-purple-700 font-semibold">
              Sign up
            </Link>
          </p>
        </div>
      </div>

      {/* Right Side - Hero Section (Hidden on mobile) */}
      <div className="hidden lg:flex lg:w-1/2 bg-linear-to-br from-purple-600 via-purple-700 to-purple-900 relative items-center justify-center p-12 overflow-hidden">
        {/* Decorative circles */}
        <div className="absolute top-10 right-10 w-32 h-32 bg-purple-500/30 rounded-full blur-3xl"></div>
        <div className="absolute bottom-10 left-10 w-40 h-40 bg-purple-400/20 rounded-full blur-3xl"></div>

        {/* Content */}
        <div className="relative z-10 text-center text-white max-w-md space-y-6">
          {/* About TechBirdsFly */}
          <div>
            <h2 className="text-5xl font-bold mb-4 leading-tight">
              Welcome to TechBirdsFly.AI
            </h2>
            <p className="text-lg text-purple-100 mb-6">
              Create stunning, professional websites in minutes using artificial intelligence
            </p>
          </div>

          {/* Features List */}
          <div className="space-y-4 my-8">
            <div className="flex items-start gap-3">
              <div className="w-6 h-6 rounded-full bg-purple-300 flex items-center justify-center flex-shrink-0 mt-0.5">
                <span className="text-purple-900 text-sm font-bold">✓</span>
              </div>
              <div className="text-left">
                <h3 className="font-semibold text-white mb-1">AI-Powered Design</h3>
                <p className="text-sm text-purple-100">Generate beautiful, responsive websites with natural language</p>
              </div>
            </div>

            <div className="flex items-start gap-3">
              <div className="w-6 h-6 rounded-full bg-purple-300 flex items-center justify-center flex-shrink-0 mt-0.5">
                <span className="text-purple-900 text-sm font-bold">✓</span>
              </div>
              <div className="text-left">
                <h3 className="font-semibold text-white mb-1">Smart Templates</h3>
                <p className="text-sm text-purple-100">Choose from hundreds of industry-specific templates or customize your own</p>
              </div>
            </div>

            <div className="flex items-start gap-3">
              <div className="w-6 h-6 rounded-full bg-purple-300 flex items-center justify-center flex-shrink-0 mt-0.5">
                <span className="text-purple-900 text-sm font-bold">✓</span>
              </div>
              <div className="text-left">
                <h3 className="font-semibold text-white mb-1">Export & Deploy</h3>
                <p className="text-sm text-purple-100">Get clean, production-ready code and deploy anywhere</p>
              </div>
            </div>

            <div className="flex items-start gap-3">
              <div className="w-6 h-6 rounded-full bg-purple-300 flex items-center justify-center flex-shrink-0 mt-0.5">
                <span className="text-purple-900 text-sm font-bold">✓</span>
              </div>
              <div className="text-left">
                <h3 className="font-semibold text-white mb-1">No Coding Required</h3>
                <p className="text-sm text-purple-100">Build professional websites without any technical knowledge</p>
              </div>
            </div>
          </div>

          {/* Headline */}
          <div className="pt-4">
            <p className="text-xl font-semibold text-purple-200 italic">
              "Transform your ideas into amazing websites in seconds"
            </p>
          </div>

          {/* Decorative line */}
          <div className="w-1 h-12 bg-white/30 mx-auto"></div>
        </div>
      </div>
    </div>
  );
}
