'use client';

import React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/components/ui/button';
import Link from 'next/link';
import { Mail, Loader2, ChevronLeft } from 'lucide-react';
import { FormInput } from '@/components/forms/FormInput';
import { emailVerificationSchema, EmailVerificationData } from '@/lib/schemas/auth';
import { useRouter, useSearchParams } from 'next/navigation';
import { AppLogoIcon, AppLogoText } from '@/components/AppLogo';
import toast from 'react-hot-toast';
import { 
  canResendVerificationCode, 
  setResendCooldown, 
  formatTimeRemaining,
  checkVerificationAttempts,
  recordVerificationAttempt,
} from '@/lib/email-verification';

const API_BASE = process.env.NEXT_PUBLIC_API_BASE || 'http://localhost:5500/api';

export default function VerifyEmailPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [isLoading, setIsLoading] = React.useState(false);
  const [apiError, setApiError] = React.useState('');
  const [resendCountdown, setResendCountdown] = React.useState(0);
  const [verificationEmail, setVerificationEmail] = React.useState('');

  const email = searchParams.get('email') || '';
  const type = searchParams.get('type') || 'signup'; // 'signup' or 'email-change'

  const {
    control,
    handleSubmit,
    formState: { isSubmitting },
  } = useForm({
    resolver: zodResolver(emailVerificationSchema),
    defaultValues: {
      code: '',
    },
  });

  // Load verification email on mount
  React.useEffect(() => {
    if (email) {
      setVerificationEmail(email);
    } else if (typeof window !== 'undefined') {
      // Try to get from session storage
      const stored = sessionStorage.getItem('verification_email');
      if (stored) {
        setVerificationEmail(stored);
      }
    }
  }, [email]);

  // Handle resend cooldown
  React.useEffect(() => {
    const { canResend, remainingSeconds } = canResendVerificationCode();
    if (!canResend) {
      setResendCountdown(remainingSeconds);
      const timer = setInterval(() => {
        setResendCountdown((prev) => {
          if (prev <= 1) {
            clearInterval(timer);
            return 0;
          }
          return prev - 1;
        });
      }, 1000);
      return () => clearInterval(timer);
    }
  }, []);

  const onSubmit = async (data: EmailVerificationData) => {
    setApiError('');
    const { canVerify, attemptsRemaining } = checkVerificationAttempts();

    if (!canVerify) {
      setApiError(`Too many attempts. Please try again later.`);
      toast.error('Too many failed attempts. Please try again later.');
      return;
    }

    setIsLoading(true);
    try {
      const response = await fetch(`${API_BASE}/auth/verify-email`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          email: verificationEmail,
          code: data.code,
          type: type,
        }),
      });

      const result = await response.json();

      if (!response.ok) {
        recordVerificationAttempt();
        const message = result.message || result.error || 'Verification failed';
        setApiError(message);
        toast.error(message);
        return;
      }

      // Success
      toast.success('Email verified successfully!');
      
      if (type === 'signup') {
        // Redirect to complete signup or dashboard
        setTimeout(() => {
          router.push('/dashboard');
        }, 1000);
      } else {
        // Redirect back to settings
        setTimeout(() => {
          router.push('/dashboard/settings');
        }, 1000);
      }
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Verification failed';
      setApiError(message);
      toast.error(message);
    } finally {
      setIsLoading(false);
    }
  };

  const handleResend = async () => {
    const { canResend } = canResendVerificationCode();
    if (!canResend) {
      toast.error('Please wait before requesting a new code');
      return;
    }

    setIsLoading(true);
    try {
      const response = await fetch(`${API_BASE}/auth/verify-email/resend`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          email: verificationEmail,
          type: type,
        }),
      });

      if (!response.ok) {
        const result = await response.json();
        throw new Error(result.message || 'Failed to resend code');
      }

      setResendCountdown(300); // 5 minutes
      setResendCooldown();
      toast.success('Verification code sent to your email');
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Failed to resend code';
      setApiError(message);
      toast.error(message);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center p-6">
      <div className="w-full max-w-md">
        {/* Back Button */}
        <Link
          href="/auth/login"
          className="inline-flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-8 font-medium"
        >
          <ChevronLeft className="w-4 h-4" />
          Back to Login
        </Link>

        {/* Logo */}
        <div className="mb-8 flex flex-col items-center justify-center gap-1">
          <AppLogoIcon size="lg" />
          <AppLogoText size="md" />
        </div>

        {/* Header */}
        <div className="mb-8 text-center">
          <div className="w-12 h-12 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <Mail className="w-6 h-6 text-blue-600" />
          </div>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Verify Email</h1>
          <p className="text-gray-600">
            We've sent a verification code to <strong>{verificationEmail}</strong>
          </p>
        </div>

        {/* Error Message */}
        {apiError && (
          <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
            {apiError}
          </div>
        )}

        {/* Verification Form */}
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
          {/* Verification Code Input */}
          <FormInput
            control={control}
            name="code"
            label="Verification Code"
            placeholder="Enter 6-digit code"
            type="text"
            required
            helperText="Check your email for the verification code"
          />

          {/* Submit Button */}
          <Button
            type="submit"
            disabled={isLoading || isSubmitting}
            className="w-full py-3 bg-purple-600 hover:bg-purple-700 text-white font-semibold rounded-lg transition-all"
          >
            {isLoading || isSubmitting ? (
              <>
                <Loader2 className="w-4 h-4 animate-spin mr-2" />
                Verifying...
              </>
            ) : (
              'Verify Email'
            )}
          </Button>
        </form>

        {/* Resend Section */}
        <div className="mt-8 text-center border-t border-gray-200 pt-6">
          <p className="text-gray-600 mb-4">Didn't receive the code?</p>
          <Button
            onClick={handleResend}
            disabled={resendCountdown > 0 || isLoading}
            variant="outline"
            className="w-full py-3 border-gray-300 text-gray-700 hover:bg-gray-50"
          >
            {resendCountdown > 0
              ? `Resend in ${formatTimeRemaining(resendCountdown)}`
              : 'Resend Code'}
          </Button>
        </div>

        {/* FAQ Section */}
        <div className="mt-8 p-4 bg-blue-50 rounded-lg">
          <h3 className="font-semibold text-blue-900 mb-2">Verification Tips:</h3>
          <ul className="text-sm text-blue-800 space-y-1">
            <li>• Check your spam/junk folder if you don't see the email</li>
            <li>• Code expires in 15 minutes</li>
            <li>• You have 3 attempts to enter the correct code</li>
          </ul>
        </div>
      </div>
    </div>
  );
}
