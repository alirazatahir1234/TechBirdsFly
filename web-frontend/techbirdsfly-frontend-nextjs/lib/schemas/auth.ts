import { z } from 'zod';

// Login Form Schema
export const loginSchema = z.object({
  email: z
    .string()
    .min(1, { message: 'Email is required' })
    .email({ message: 'Please enter a valid email address' }),
  password: z
    .string()
    .min(1, { message: 'Password is required' })
    .min(6, { message: 'Password must be at least 6 characters' }),
  rememberMe: z.boolean().default(false),
});

export type LoginFormData = z.infer<typeof loginSchema>;

// Register Form Schema
export const registerSchema = z
  .object({
    firstName: z
      .string()
      .min(1, { message: 'First name is required' })
      .min(2, { message: 'First name must be at least 2 characters' })
      .max(50, { message: 'First name must not exceed 50 characters' }),
    lastName: z
      .string()
      .min(1, { message: 'Last name is required' })
      .min(2, { message: 'Last name must be at least 2 characters' })
      .max(50, { message: 'Last name must not exceed 50 characters' }),
    email: z
      .string()
      .min(1, { message: 'Email is required' })
      .email({ message: 'Please enter a valid email address' }),
    password: z
      .string()
      .min(1, { message: 'Password is required' })
      .min(8, { message: 'Password must be at least 8 characters' })
      .regex(/[A-Z]/, { message: 'Password must contain at least one uppercase letter' })
      .regex(/[a-z]/, { message: 'Password must contain at least one lowercase letter' })
      .regex(/[0-9]/, { message: 'Password must contain at least one number' })
      .regex(/[^A-Za-z0-9]/, { message: 'Password must contain at least one special character' }),
    confirmPassword: z
      .string()
      .min(1, { message: 'Please confirm your password' }),
    agreeToTerms: z
      .boolean()
      .refine((val) => val === true, {
        message: 'You must agree to the terms and conditions',
      }),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  });

export type RegisterFormData = z.infer<typeof registerSchema>;

// Password Reset Schema
export const passwordResetSchema = z.object({
  email: z
    .string()
    .min(1, { message: 'Email is required' })
    .email({ message: 'Please enter a valid email address' }),
});

export type PasswordResetData = z.infer<typeof passwordResetSchema>;

// New Password Schema (for after reset link)
export const newPasswordSchema = z
  .object({
    password: z
      .string()
      .min(1, { message: 'Password is required' })
      .min(8, { message: 'Password must be at least 8 characters' })
      .regex(/[A-Z]/, { message: 'Password must contain at least one uppercase letter' })
      .regex(/[a-z]/, { message: 'Password must contain at least one lowercase letter' })
      .regex(/[0-9]/, { message: 'Password must contain at least one number' })
      .regex(/[^A-Za-z0-9]/, { message: 'Password must contain at least one special character' }),
    confirmPassword: z
      .string()
      .min(1, { message: 'Please confirm your password' }),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  });

export type NewPasswordData = z.infer<typeof newPasswordSchema>;

// Email Verification Schema
export const emailVerificationSchema = z.object({
  code: z
    .string()
    .min(1, { message: 'Verification code is required' })
    .regex(/^\d{6}$/, { message: 'Code must be 6 digits' }),
});

export type EmailVerificationData = z.infer<typeof emailVerificationSchema>;

// 2FA TOTP Setup Schema
export const twoFactorSetupSchema = z.object({
  secret: z
    .string()
    .min(1, { message: 'Secret is required' }),
  qrCodeURL: z
    .string()
    .url({ message: 'Invalid QR code URL' }),
  backupCodes: z
    .array(z.string())
    .min(1, { message: 'Backup codes are required' }),
});

export type TwoFactorSetupData = z.infer<typeof twoFactorSetupSchema>;

// 2FA Verification Schema (TOTP code)
export const twoFactorVerificationSchema = z.object({
  code: z
    .string()
    .min(1, { message: '2FA code is required' })
    .regex(/^\d{6}$/, { message: 'Code must be 6 digits' }),
});

export type TwoFactorVerificationData = z.infer<typeof twoFactorVerificationSchema>;

// 2FA Backup Code Schema
export const twoFactorBackupCodeSchema = z.object({
  code: z
    .string()
    .min(1, { message: 'Backup code is required' })
    .regex(/^[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}$/i, { message: 'Invalid backup code format' }),
});

export type TwoFactorBackupCodeData = z.infer<typeof twoFactorBackupCodeSchema>;
