/**
 * Email Verification Utilities
 * ============================================================================
 * Handles email verification flows including:
 * - Code generation
 * - Code validation
 * - Resend attempts tracking
 * - Error handling
 */

// Local storage keys
const EMAIL_VERIFY_ATTEMPTS_KEY = 'email_verify_attempts';
const EMAIL_VERIFY_TIMESTAMP_KEY = 'email_verify_timestamp';
const MAX_VERIFY_ATTEMPTS = 3;
const VERIFY_ATTEMPT_WINDOW = 60 * 60 * 1000; // 1 hour in ms
const RESEND_COOLDOWN = 5 * 60 * 1000; // 5 minutes in ms

/**
 * Generate a random verification code (6 digits)
 */
export function generateVerificationCode(): string {
  return Math.floor(100000 + Math.random() * 900000).toString();
}

/**
 * Check if user can send a new verification code
 */
export function canResendVerificationCode(): {
  canResend: boolean;
  remainingSeconds: number;
} {
  if (typeof window === 'undefined') {
    return { canResend: true, remainingSeconds: 0 };
  }

  const lastResendTime = sessionStorage.getItem('last_verification_resend');
  if (!lastResendTime) {
    return { canResend: true, remainingSeconds: 0 };
  }

  const elapsed = Date.now() - parseInt(lastResendTime);
  const remaining = RESEND_COOLDOWN - elapsed;

  if (remaining <= 0) {
    sessionStorage.removeItem('last_verification_resend');
    return { canResend: true, remainingSeconds: 0 };
  }

  return {
    canResend: false,
    remainingSeconds: Math.ceil(remaining / 1000),
  };
}

/**
 * Set resend cooldown
 */
export function setResendCooldown(): void {
  if (typeof window === 'undefined') return;
  sessionStorage.setItem('last_verification_resend', Date.now().toString());
}

/**
 * Check verification attempts
 */
export function checkVerificationAttempts(): {
  canVerify: boolean;
  attemptsRemaining: number;
  lockoutUntil?: Date;
} {
  if (typeof window === 'undefined') {
    return { canVerify: true, attemptsRemaining: MAX_VERIFY_ATTEMPTS };
  }

  const attemptsStr = localStorage.getItem(EMAIL_VERIFY_ATTEMPTS_KEY);
  const timestampStr = localStorage.getItem(EMAIL_VERIFY_TIMESTAMP_KEY);

  if (!attemptsStr || !timestampStr) {
    return { canVerify: true, attemptsRemaining: MAX_VERIFY_ATTEMPTS };
  }

  const attempts = parseInt(attemptsStr);
  const timestamp = parseInt(timestampStr);
  const now = Date.now();

  // Check if lockout window has expired
  if (now - timestamp > VERIFY_ATTEMPT_WINDOW) {
    localStorage.removeItem(EMAIL_VERIFY_ATTEMPTS_KEY);
    localStorage.removeItem(EMAIL_VERIFY_TIMESTAMP_KEY);
    return { canVerify: true, attemptsRemaining: MAX_VERIFY_ATTEMPTS };
  }

  // Check if user has exceeded attempts
  if (attempts >= MAX_VERIFY_ATTEMPTS) {
    const lockoutUntil = new Date(timestamp + VERIFY_ATTEMPT_WINDOW);
    return {
      canVerify: false,
      attemptsRemaining: 0,
      lockoutUntil,
    };
  }

  return {
    canVerify: true,
    attemptsRemaining: MAX_VERIFY_ATTEMPTS - attempts,
  };
}

/**
 * Record a verification attempt
 */
export function recordVerificationAttempt(): void {
  if (typeof window === 'undefined') return;

  const attemptsStr = localStorage.getItem(EMAIL_VERIFY_ATTEMPTS_KEY);
  const attempts = attemptsStr ? parseInt(attemptsStr) + 1 : 1;

  localStorage.setItem(EMAIL_VERIFY_ATTEMPTS_KEY, attempts.toString());
  localStorage.setItem(EMAIL_VERIFY_TIMESTAMP_KEY, Date.now().toString());
}

/**
 * Clear verification attempts
 */
export function clearVerificationAttempts(): void {
  if (typeof window === 'undefined') return;
  localStorage.removeItem(EMAIL_VERIFY_ATTEMPTS_KEY);
  localStorage.removeItem(EMAIL_VERIFY_TIMESTAMP_KEY);
}

/**
 * Format time remaining for display
 */
export function formatTimeRemaining(seconds: number): string {
  if (seconds <= 0) return 'Ready';
  if (seconds < 60) return `${seconds}s`;
  const minutes = Math.ceil(seconds / 60);
  return `${minutes}m`;
}

/**
 * Validate email format
 */
export function isValidEmail(email: string): boolean {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
}

/**
 * Validate verification code format
 */
export function isValidVerificationCode(code: string): boolean {
  return /^\d{6}$/.test(code);
}

export type VerificationCodeCheckResult = {
  canVerify: boolean;
  attemptsRemaining: number;
  lockoutUntil?: Date;
};

export type ResendCheckResult = {
  canResend: boolean;
  remainingSeconds: number;
};
