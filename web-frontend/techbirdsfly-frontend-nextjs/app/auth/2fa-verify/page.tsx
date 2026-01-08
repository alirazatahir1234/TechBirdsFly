'use client';

import React, { Suspense } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/components/ui/button';
import { Shield, Loader2, AlertCircle, ChevronDown } from 'lucide-react';
import { FormInput } from '@/components/forms/FormInput';
import {
  twoFactorVerificationSchema,
  TwoFactorVerificationData,
  twoFactorBackupCodeSchema,
  TwoFactorBackupCodeData,
} from '@/lib/schemas/auth';
import { useRouter, useSearchParams } from 'next/navigation';
import { AppLogoIcon, AppLogoText } from '@/components/AppLogo';
import toast from 'react-hot-toast';
import { check2FAAttempts, record2FAAttempt, clear2FAAttempts } from '@/lib/2fa';

const API_BASE = process.env.NEXT_PUBLIC_API_BASE || 'http://localhost:5500/api';

function TwoFactorVerifyContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [isLoading, setIsLoading] = React.useState(false);
  const [apiError, setApiError] = React.useState('');
  const [useBackupCode, setUseBackupCode] = React.useState(false);
  const email = searchParams.get('email') || '';

  const totpForm = useForm({
    resolver: zodResolver(twoFactorVerificationSchema),
    defaultValues: { code: '' },
  });

  const backupForm = useForm({
    resolver: zodResolver(twoFactorBackupCodeSchema),
    defaultValues: { code: '' },
  });

  if (!email) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center p-6">
        <div className="text-center">
          <AlertCircle className="w-12 h-12 text-red-600 mx-auto mb-4" />
          <p className="text-gray-600">Email not found. Please try logging in again.</p>
        </div>
      </div>
    );
  }

  const onSubmitTOTP = async (data: TwoFactorVerificationData) => {
    setApiError('');
    const { canVerify, attemptsRemaining } = check2FAAttempts();

    if (!canVerify) {
      setApiError('Too many attempts. Please try again later.');
      toast.error('Too many failed 2FA attempts. Please try again later.');
      return;
    }

    setIsLoading(true);
    try {
      const response = await fetch(`${API_BASE}/auth/2fa/verify`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          email: email,
          code: data.code,
        }),
      });

      const result = await response.json();

      if (!response.ok) {
        record2FAAttempt();
        const message = result.message || result.error || '2FA verification failed';
        setApiError(`${message}. ${attemptsRemaining - 1} attempts remaining.`);
        toast.error(message);
        return;
      }

      clear2FAAttempts();
      toast.success('2FA verification successful!');

      // Store 2FA token and redirect to dashboard
      if (result.token) {
        localStorage.setItem('temp_2fa_token', result.token);
      }

      setTimeout(() => {
        router.push('/dashboard');
      }, 1000);
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Verification failed';
      setApiError(message);
      toast.error(message);
    } finally {
      setIsLoading(false);
    }
  };

  const onSubmitBackupCode = async (data: TwoFactorBackupCodeData) => {
    setApiError('');
    setIsLoading(true);

    try {
      const response = await fetch(`${API_BASE}/auth/2fa/backup-code`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          email: email,
          code: data.code,
        }),
      });

      const result = await response.json();

      if (!response.ok) {
        const message = result.message || result.error || 'Backup code verification failed';
        setApiError(message);
        toast.error(message);
        return;
      }

      clear2FAAttempts();
      toast.success('Backup code verified!');

      // Store 2FA token and redirect to dashboard
      if (result.token) {
        localStorage.setItem('temp_2fa_token', result.token);
      }

      setTimeout(() => {
        router.push('/dashboard');
      }, 1000);
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Verification failed';
      setApiError(message);
      toast.error(message);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center p-6">
      <div className="w-full max-w-md">
        {/* Logo */}
        <div className="mb-8 flex flex-col items-center justify-center gap-1">
          <AppLogoIcon size="lg" />
          <AppLogoText size="md" />
        </div>

        {/* Header */}
        <div className="mb-8 text-center">
          <div className="w-12 h-12 bg-purple-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <Shield className="w-6 h-6 text-purple-600" />
          </div>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Two-Factor Authentication</h1>
          <p className="text-gray-600">Enter your 6-digit authentication code</p>
        </div>

        {/* Error Message */}
        {apiError && (
          <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm flex gap-3">
            <AlertCircle className="w-5 h-5 shrink-0 mt-0.5" />
            <div>{apiError}</div>
          </div>
        )}

        {/* TOTP Form */}
        {!useBackupCode ? (
          <form onSubmit={totpForm.handleSubmit(onSubmitTOTP)} className="space-y-6">
            {/* Code Input */}
            <FormInput
              control={totpForm.control}
              name="code"
              label="Authentication Code"
              placeholder="000000"
              type="text"
              required
              helperText="Enter the 6-digit code from your authenticator app"
            />

            {/* Submit Button */}
            <Button
              type="submit"
              disabled={isLoading}
              className="w-full py-3 bg-purple-600 hover:bg-purple-700 text-white font-semibold rounded-lg transition-all"
            >
              {isLoading ? (
                <>
                  <Loader2 className="w-4 h-4 animate-spin mr-2" />
                  Verifying...
                </>
              ) : (
                'Verify 2FA Code'
              )}
            </Button>

            {/* Backup Code Option */}
            <button
              type="button"
              onClick={() => setUseBackupCode(true)}
              className="w-full py-3 px-4 border border-gray-300 rounded-lg font-medium text-gray-700 hover:bg-gray-50 transition-all flex items-center justify-center gap-2"
            >
              <ChevronDown className="w-4 h-4" />
              Use Backup Code Instead
            </button>
          </form>
        ) : (
          /* Backup Code Form */
          <form onSubmit={backupForm.handleSubmit(onSubmitBackupCode)} className="space-y-6">
            {/* Backup Code Input */}
            <FormInput
              control={backupForm.control}
              name="code"
              label="Backup Code"
              placeholder="XXXX-XXXX-XXXX"
              type="text"
              required
              helperText="Enter one of your backup codes"
            />

            {/* Submit Button */}
            <Button
              type="submit"
              disabled={isLoading}
              className="w-full py-3 bg-purple-600 hover:bg-purple-700 text-white font-semibold rounded-lg transition-all"
            >
              {isLoading ? (
                <>
                  <Loader2 className="w-4 h-4 animate-spin mr-2" />
                  Verifying...
                </>
              ) : (
                'Verify Backup Code'
              )}
            </Button>

            {/* Back to TOTP Option */}
            <button
              type="button"
              onClick={() => {
                setUseBackupCode(false);
                setApiError('');
              }}
              className="w-full py-3 px-4 border border-gray-300 rounded-lg font-medium text-gray-700 hover:bg-gray-50 transition-all flex items-center justify-center gap-2"
            >
              <ChevronDown className="w-4 h-4" />
              Use Authenticator App Instead
            </button>
          </form>
        )}

        {/* Help Section */}
        <div className="mt-8 p-4 bg-blue-50 rounded-lg">
          <h3 className="font-semibold text-blue-900 text-sm mb-2">Troubleshooting:</h3>
          <ul className="text-xs text-blue-800 space-y-1">
            <li>• Make sure your device time is synchronized</li>
            <li>• If you lost access, use a backup code</li>
            <li>• If codes don't work, contact support</li>
          </ul>
        </div>
      </div>
    </div>
  );
}

export default function TwoFactorVerifyPage() {
  return (
    <Suspense fallback={<div className="flex items-center justify-center min-h-screen">Loading...</div>}>
      <TwoFactorVerifyContent />
    </Suspense>
  );
}
