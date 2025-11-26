'use client';

import React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/components/ui/button';
import Link from 'next/link';
import { Mail, Lock, User, Chrome, Facebook, Loader2 } from 'lucide-react';
import { FormInput } from '@/components/forms/FormInput';
import { FormCheckbox } from '@/components/forms/FormCheckbox';
import { registerSchema } from '@/lib/schemas/auth';
import { useAuthStore } from '@/lib/store/authStore';
import { useRouter } from 'next/navigation';
import { AppLogoIcon, AppLogoText } from '@/components/AppLogo';
import { generateGoogleAuthUrl, validateOAuthState, getOAuthRedirectPath } from '@/lib/oauth';
import toast from 'react-hot-toast';

export default function RegisterPage() {
  const router = useRouter();
  const { register, registerWithGoogle, isLoading } = useAuthStore();
  const [apiError, setApiError] = React.useState('');
  const [oauthLoading, setOauthLoading] = React.useState(false);

  const {
    control,
    handleSubmit,
    formState: { isSubmitting },
  } = useForm({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      firstName: '',
      lastName: '',
      email: '',
      password: '',
      confirmPassword: '',
      agreeToTerms: false,
    },
  });

  // Check for OAuth callback
  React.useEffect(() => {
    if (typeof window !== 'undefined') {
      const params = new URLSearchParams(window.location.search);
      const code = params.get('code');
      const state = params.get('state');

      if (code && state) {
        handleOAuthCallback(code, state);
        // Clean up URL
        window.history.replaceState({}, document.title, '/auth/register');
      }
    }
  }, []);

  const handleOAuthCallback = async (code: string, state: string) => {
    setOauthLoading(true);
    try {
      // Validate state for CSRF protection
      if (!validateOAuthState(state)) {
        throw new Error('Invalid OAuth state - security validation failed');
      }

      // Call the store's registerWithGoogle action
      await registerWithGoogle(code);
      toast.success('Account created successfully!');

      // Get redirect path and navigate
      const redirectPath = getOAuthRedirectPath();
      setTimeout(() => {
        router.push(redirectPath);
      }, 100);
    } catch (error) {
      setApiError(error instanceof Error ? error.message : 'OAuth signup failed');
      toast.error('OAuth signup failed');
    } finally {
      setOauthLoading(false);
    }
  };

  const onSubmit = async (data: any) => {
    setApiError('');
    try {
      await register(data.email, data.firstName, data.lastName, data.password);
      toast.success('Account created successfully!');
      router.push('/dashboard');
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Registration failed';
      setApiError(message);
      toast.error(message);
    }
  };

  const handleGoogleSignup = async () => {
    try {
      setOauthLoading(true);
      const authUrl = await generateGoogleAuthUrl('/dashboard');
      window.location.href = authUrl;
    } catch (error) {
      setApiError('Failed to start Google signup');
      toast.error('Failed to start Google signup');
      setOauthLoading(false);
    }
  };

  const handleFacebookSignup = () => {
    console.log('Facebook signup - implement OAuth flow');
  };

  const isLoading_ = isSubmitting || isLoading;

  return (
    <div className="min-h-screen flex">
      {/* Left Side - Register Form */}
      <div className="w-full lg:w-1/2 flex items-center justify-center p-6 md:p-12 bg-gray-50">
        <div className="w-full max-w-md">
          {/* Logo */}
          <div className="mb-8 flex flex-col items-center justify-center gap-1">
            <AppLogoIcon size="lg" />
            <AppLogoText size="md" />
          </div>

          {/* Header */}
          <div className="mb-8">
            <h1 className="text-4xl font-bold text-gray-900 mb-2">Create Account</h1>
            <p className="text-gray-600">Join thousands of creators building amazing websites</p>
          </div>

          {/* Error Message */}
          {apiError && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
              {apiError}
            </div>
          )}

          {/* Register Form */}
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
            {/* First Name & Last Name */}
            <div className="grid grid-cols-2 gap-4">
              <FormInput
                control={control}
                name="firstName"
                label="First Name"
                placeholder="Enter your first name"
                type="text"
                icon={<User className="w-5 h-5" />}
                required
              />
              <FormInput
                control={control}
                name="lastName"
                label="Last Name"
                placeholder="Enter your last name"
                type="text"
                icon={<User className="w-5 h-5" />}
                required
              />
            </div>
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
              helperText="Min 8 chars, 1 uppercase, 1 lowercase, 1 number, 1 special char"
            />
            {/* Confirm Password */}
            <FormInput
              control={control}
              name="confirmPassword"
              label="Confirm Password"
              placeholder="Re-enter your password"
              type="password"
              icon={<Lock className="w-5 h-5" />}
              required
            />
            {/* Terms & Conditions */}
            <FormCheckbox
              control={control}
              name="agreeToTerms"
              label={
                <span>
                  I agree to the{' '}
                  <Link href="/(marketing)/terms" className="text-purple-600 hover:text-purple-700 font-medium">
                    Terms and Conditions
                  </Link>
                </span>
              }
            />

            {/* Sign Up Button */}
            <Button
              type="submit"
              disabled={isLoading_}
              className="w-full py-3 bg-purple-600 hover:bg-purple-700 text-white font-semibold rounded-lg transition-all"
            >
              {isLoading_ ? 'Creating Account...' : 'Create Account'}
            </Button>
          </form>

          {/* Divider */}
          <div className="my-8 flex items-center">
            <div className="flex-1 border-t border-gray-300"></div>
            <span className="px-4 text-gray-500 text-sm">Or sign up with</span>
            <div className="flex-1 border-t border-gray-300"></div>
          </div>

          {/* Social Signup Buttons */}
          <div className="space-y-3">
            <button
              onClick={handleGoogleSignup}
              disabled={oauthLoading}
              type="button"
              className="w-full py-3 px-4 border border-gray-300 rounded-lg font-medium text-gray-700 hover:bg-gray-50 transition-all flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {oauthLoading ? (
                <Loader2 className="w-5 h-5 animate-spin" />
              ) : (
                <Chrome className="w-5 h-5 text-blue-500" />
              )}
              {oauthLoading ? 'Connecting...' : 'Sign up with Google'}
            </button>

            <button
              onClick={handleFacebookSignup}
              type="button"
              className="w-full py-3 px-4 border border-gray-300 rounded-lg font-medium text-gray-700 hover:bg-gray-50 transition-all flex items-center justify-center gap-2"
            >
              <Facebook className="w-5 h-5 text-blue-600" />
              Sign up with Facebook
            </button>
          </div>

          {/* Login Link */}
          <p className="text-center text-gray-600">
            Already have an account?{" "}
            <Link href="/auth/login" className="text-purple-600 hover:text-purple-700 font-semibold">
              Log in
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
        <div className="relative z-10 text-center text-white max-w-md">
          <h2 className="text-5xl font-bold mb-6 leading-tight">
            Join thousands creating amazing websites
          </h2>
          <p className="text-2xl font-semibold mb-12">Start Your Journey</p>

          {/* Placeholder for illustration */}
          <div className="relative h-80 flex items-center justify-center">
            <div className="text-center text-purple-200 opacity-50">
              <div className="text-6xl mb-4">🚀</div>
              <p className="text-sm">Illustration placeholder</p>
              <p className="text-xs mt-2">(Add your image here)</p>
            </div>
          </div>

          {/* Decorative line */}
          <div className="w-1 h-16 bg-white/30 mx-auto mt-8"></div>
        </div>
      </div>
    </div>
  );
}
