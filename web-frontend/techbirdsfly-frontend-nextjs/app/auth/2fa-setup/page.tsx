'use client';

import React from 'react';
import { Button } from '@/components/ui/button';
import { useRouter } from 'next/navigation';
import { ChevronLeft, Copy, Check, Shield } from 'lucide-react';
import Link from 'next/link';
import { AppLogoIcon, AppLogoText } from '@/components/AppLogo';
import toast from 'react-hot-toast';
import {
  generateTOTPSecret,
  generateTOTPQRCodeURL,
  generateBackupCodes,
  store2FASetupData,
  get2FASetupData,
  clear2FASetupData,
} from '@/lib/2fa';

const API_BASE = process.env.NEXT_PUBLIC_API_BASE || 'http://localhost:5500/api';

export default function TwoFactorSetupPage() {
  const router = useRouter();
  const [step, setStep] = React.useState<'scan' | 'verify' | 'complete'>('scan');
  const [isLoading, setIsLoading] = React.useState(false);
  const [copiedSecret, setCopiedSecret] = React.useState(false);
  const [copiedCode, setCopiedCode] = React.useState(false);
  const [userEmail, setUserEmail] = React.useState('');

  // Setup data
  const [setupData, setSetupData] = React.useState<{
    secret: string;
    qrCodeURL: string;
    backupCodes: string[];
    email: string;
  } | null>(null);

  // Initialize 2FA setup
  React.useEffect(() => {
    const initializeSetup = async () => {
      try {
        // Get user email from auth store or session
        const email = sessionStorage.getItem('user_email') || '';
        setUserEmail(email);

        if (!email) {
          toast.error('User email not found');
          router.push('/dashboard/settings');
          return;
        }

        // Generate TOTP secret and QR code
        const secret = generateTOTPSecret();
        const qrCodeURL = generateTOTPQRCodeURL(secret, email, 'TechBirdsFly');
        const backupCodes = generateBackupCodes(10);

        const data = { secret, qrCodeURL, backupCodes, email };
        setSetupData(data);
        store2FASetupData(data);
      } catch (error) {
        toast.error('Failed to initialize 2FA setup');
        router.push('/dashboard/settings');
      }
    };

    initializeSetup();
  }, [router]);

  const copyToClipboard = (text: string, type: 'secret' | 'code') => {
    navigator.clipboard.writeText(text);
    if (type === 'secret') {
      setCopiedSecret(true);
      setTimeout(() => setCopiedSecret(false), 2000);
      toast.success('Secret copied to clipboard');
    } else {
      setCopiedCode(true);
      setTimeout(() => setCopiedCode(false), 2000);
      toast.success('Backup code copied');
    }
  };

  const downloadBackupCodes = () => {
    if (!setupData) return;

    const content = `TechBirdsFly 2FA Backup Codes\n\n${setupData.backupCodes.join('\n')}\n\nGenerated on: ${new Date().toLocaleString()}`;
    const blob = new Blob([content], { type: 'text/plain' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `techbirdsfly-2fa-backup-codes.txt`;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
    toast.success('Backup codes downloaded');
  };

  const completeSetup = async () => {
    if (!setupData) return;

    setIsLoading(true);
    try {
      const response = await fetch(`${API_BASE}/auth/2fa/setup`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          email: setupData.email,
          secret: setupData.secret,
          backupCodes: setupData.backupCodes,
        }),
      });

      if (!response.ok) {
        const result = await response.json();
        throw new Error(result.message || '2FA setup failed');
      }

      clear2FASetupData();
      toast.success('Two-factor authentication enabled!');
      setTimeout(() => {
        router.push('/dashboard/settings?2fa=enabled');
      }, 1000);
    } catch (error) {
      const message = error instanceof Error ? error.message : '2FA setup failed';
      toast.error(message);
    } finally {
      setIsLoading(false);
    }
  };

  if (!setupData) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center p-6">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-purple-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Setting up 2FA...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center p-6">
      <div className="w-full max-w-md">
        {/* Back Button */}
        <Link
          href="/dashboard/settings"
          className="inline-flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-8 font-medium"
        >
          <ChevronLeft className="w-4 h-4" />
          Back to Settings
        </Link>

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
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Enable Two-Factor Authentication</h1>
          <p className="text-gray-600">Secure your account with an authenticator app</p>
        </div>

        {step === 'scan' && (
          <div className="space-y-6">
            {/* Step Indicator */}
            <div className="flex gap-2 justify-center">
              <div className="w-2 h-2 bg-purple-600 rounded-full"></div>
              <div className="w-2 h-2 bg-gray-300 rounded-full"></div>
              <div className="w-2 h-2 bg-gray-300 rounded-full"></div>
            </div>

            {/* QR Code Section */}
            <div className="bg-white p-6 rounded-lg border border-gray-200">
              <p className="text-sm text-gray-600 mb-4 font-medium">Step 1: Scan QR Code</p>
              <p className="text-xs text-gray-500 mb-4">
                Use an authenticator app (Google Authenticator, Authy, Microsoft Authenticator) to scan this QR code
              </p>
              <div className="bg-gray-50 p-4 rounded-lg flex items-center justify-center mb-4">
                <img src={setupData.qrCodeURL} alt="2FA QR Code" className="w-48 h-48" />
              </div>
            </div>

            {/* Manual Entry Section */}
            <div className="bg-white p-6 rounded-lg border border-gray-200">
              <p className="text-sm text-gray-600 mb-4 font-medium">Can't scan? Enter manually</p>
              <div className="bg-gray-50 p-4 rounded-lg flex items-center justify-between">
                <code className="text-sm font-mono text-gray-900">{setupData.secret}</code>
                <button
                  onClick={() => copyToClipboard(setupData.secret, 'secret')}
                  className="p-2 hover:bg-gray-200 rounded transition-colors"
                >
                  {copiedSecret ? (
                    <Check className="w-4 h-4 text-green-600" />
                  ) : (
                    <Copy className="w-4 h-4 text-gray-600" />
                  )}
                </button>
              </div>
            </div>

            {/* Next Button */}
            <Button
              onClick={() => setStep('verify')}
              className="w-full py-3 bg-purple-600 hover:bg-purple-700 text-white font-semibold rounded-lg"
            >
              I've Scanned the QR Code
            </Button>
          </div>
        )}

        {step === 'verify' && (
          <div className="space-y-6">
            {/* Step Indicator */}
            <div className="flex gap-2 justify-center">
              <div className="w-2 h-2 bg-purple-600 rounded-full"></div>
              <div className="w-2 h-2 bg-purple-600 rounded-full"></div>
              <div className="w-2 h-2 bg-gray-300 rounded-full"></div>
            </div>

            {/* Backup Codes Section */}
            <div className="bg-blue-50 border border-blue-200 p-6 rounded-lg">
              <p className="text-sm text-blue-900 font-medium mb-4">Step 2: Save Backup Codes</p>
              <p className="text-xs text-blue-800 mb-4">
                Save these codes in a safe place. You can use them to access your account if you lose access to your authenticator app.
              </p>

              <div className="grid grid-cols-2 gap-2 bg-white p-4 rounded border border-blue-100 mb-4">
                {setupData.backupCodes.map((code, index) => (
                  <code
                    key={index}
                    className="text-xs font-mono text-gray-900 py-2 px-2 hover:bg-gray-50 cursor-pointer rounded transition-colors"
                    onClick={() => copyToClipboard(code, 'code')}
                  >
                    {code}
                  </code>
                ))}
              </div>

              <Button
                onClick={downloadBackupCodes}
                variant="outline"
                className="w-full py-2 border-blue-300 text-blue-700 hover:bg-blue-50"
              >
                Download Backup Codes
              </Button>
            </div>

            {/* Warning */}
            <div className="bg-amber-50 border border-amber-200 p-4 rounded-lg">
              <p className="text-xs text-amber-900">
                ⚠️ <strong>Important:</strong> Save these codes securely. They are your only backup if you lose access to your authenticator app.
              </p>
            </div>

            {/* Next Button */}
            <Button
              onClick={() => setStep('complete')}
              className="w-full py-3 bg-purple-600 hover:bg-purple-700 text-white font-semibold rounded-lg"
            >
              I've Saved My Backup Codes
            </Button>
          </div>
        )}

        {step === 'complete' && (
          <div className="space-y-6">
            {/* Step Indicator */}
            <div className="flex gap-2 justify-center">
              <div className="w-2 h-2 bg-purple-600 rounded-full"></div>
              <div className="w-2 h-2 bg-purple-600 rounded-full"></div>
              <div className="w-2 h-2 bg-purple-600 rounded-full"></div>
            </div>

            {/* Success Message */}
            <div className="bg-green-50 border border-green-200 p-6 rounded-lg text-center">
              <div className="w-12 h-12 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <Check className="w-6 h-6 text-green-600" />
              </div>
              <p className="text-sm text-green-900 font-medium mb-2">Almost Done!</p>
              <p className="text-xs text-green-800">
                Click the button below to enable two-factor authentication on your account.
              </p>
            </div>

            {/* Complete Button */}
            <Button
              onClick={completeSetup}
              disabled={isLoading}
              className="w-full py-3 bg-purple-600 hover:bg-purple-700 text-white font-semibold rounded-lg disabled:opacity-50"
            >
              {isLoading ? 'Enabling 2FA...' : 'Enable Two-Factor Authentication'}
            </Button>

            {/* Back Button */}
            <Button
              onClick={() => setStep('verify')}
              disabled={isLoading}
              variant="outline"
              className="w-full py-3"
            >
              Back
            </Button>
          </div>
        )}
      </div>
    </div>
  );
}
