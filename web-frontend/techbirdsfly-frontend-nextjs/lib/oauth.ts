/**
 * OAuth2 Client - Google Authentication Flow
 * ============================================================================
 * Handles Google OAuth2 authentication including:
 * - Authorization URL generation (PKCE flow)
 * - Token exchange
 * - Profile fetching
 * - Error handling
 * 
 * Requirements:
 * - NEXT_PUBLIC_GOOGLE_CLIENT_ID environment variable
 * - NEXT_PUBLIC_GOOGLE_REDIRECT_URI environment variable
 */

// Configuration
const GOOGLE_CLIENT_ID = process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID || '';
const GOOGLE_REDIRECT_URI = process.env.NEXT_PUBLIC_GOOGLE_REDIRECT_URI || `${typeof window !== 'undefined' ? window.location.origin : ''}/api/auth/oauth/google/callback`;
const GOOGLE_OAUTH_ENDPOINT = 'https://accounts.google.com/o/oauth2/v2/auth';
const GOOGLE_TOKEN_ENDPOINT = 'https://oauth2.googleapis.com/token';
const GOOGLE_USERINFO_ENDPOINT = 'https://www.googleapis.com/oauth2/v2/userinfo';

// Local storage keys
const OAUTH_STATE_KEY = 'oauth_state';
const OAUTH_PKCE_KEY = 'oauth_pkce';

/**
 * Generate random string for PKCE code challenge
 */
function generateRandomString(length: number = 32): string {
  const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~';
  let result = '';
  for (let i = 0; i < length; i++) {
    result += chars.charAt(Math.floor(Math.random() * chars.length));
  }
  return result;
}

/**
 * Generate PKCE code challenge from verifier
 */
async function generateCodeChallenge(codeVerifier: string): Promise<string> {
  const encoder = new TextEncoder();
  const data = encoder.encode(codeVerifier);
  const hashBuffer = await crypto.subtle.digest('SHA-256', data);
  const hashArray = Array.from(new Uint8Array(hashBuffer));
  const hashString = hashArray.map(b => String.fromCharCode(b)).join('');
  return btoa(hashString).replace(/\+/g, '-').replace(/\//g, '_').replace(/=/g, '');
}

/**
 * Generate Google OAuth authorization URL
 */
export async function generateGoogleAuthUrl(redirectPath: string = '/dashboard'): Promise<string> {
  // Generate PKCE code verifier and challenge
  const codeVerifier = generateRandomString(43);
  const codeChallenge = await generateCodeChallenge(codeVerifier);
  
  // Generate state for CSRF protection
  const state = generateRandomString(32);
  
  // Store values in localStorage for verification later
  if (typeof window !== 'undefined') {
    localStorage.setItem(OAUTH_STATE_KEY, state);
    localStorage.setItem(OAUTH_PKCE_KEY, codeVerifier);
    // Store redirect path for post-login redirect
    sessionStorage.setItem('oauth_redirect_path', redirectPath);
  }
  
  // Build authorization URL
  const params = new URLSearchParams({
    client_id: GOOGLE_CLIENT_ID,
    redirect_uri: GOOGLE_REDIRECT_URI,
    response_type: 'code',
    scope: 'openid email profile',
    state: state,
    code_challenge: codeChallenge,
    code_challenge_method: 'S256',
    access_type: 'offline',
    prompt: 'consent',
  });
  
  return `${GOOGLE_OAUTH_ENDPOINT}?${params.toString()}`;
}

/**
 * Validate OAuth state from callback
 */
export function validateOAuthState(returnedState: string): boolean {
  if (typeof window === 'undefined') return false;
  
  const storedState = localStorage.getItem(OAUTH_STATE_KEY);
  
  if (!storedState || storedState !== returnedState) {
    console.error('❌ OAuth state mismatch - potential CSRF attack');
    return false;
  }
  
  // Clear the stored state
  localStorage.removeItem(OAUTH_STATE_KEY);
  return true;
}

/**
 * Exchange authorization code for tokens
 */
export async function exchangeGoogleAuthCode(code: string): Promise<{
  access_token: string;
  refresh_token?: string;
  expires_in: number;
  token_type: string;
}> {
  if (typeof window === 'undefined') {
    throw new Error('This function can only be called in the browser');
  }
  
  const codeVerifier = localStorage.getItem(OAUTH_PKCE_KEY);
  if (!codeVerifier) {
    throw new Error('PKCE code verifier not found');
  }
  
  const response = await fetch(GOOGLE_TOKEN_ENDPOINT, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded',
    },
    body: new URLSearchParams({
      client_id: GOOGLE_CLIENT_ID,
      code: code,
      code_verifier: codeVerifier,
      grant_type: 'authorization_code',
      redirect_uri: GOOGLE_REDIRECT_URI,
    }).toString(),
  });
  
  if (!response.ok) {
    const error = await response.json();
    throw new Error(`Token exchange failed: ${error.error_description || error.error}`);
  }
  
  const tokens = await response.json();
  
  // Clear PKCE verifier
  localStorage.removeItem(OAUTH_PKCE_KEY);
  
  return tokens;
}

/**
 * Fetch user profile from Google
 */
export async function fetchGoogleUserProfile(accessToken: string): Promise<{
  id: string;
  email: string;
  name: string;
  picture?: string;
  given_name?: string;
  family_name?: string;
}> {
  const response = await fetch(GOOGLE_USERINFO_ENDPOINT, {
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
  });
  
  if (!response.ok) {
    const error = await response.json();
    throw new Error(`Failed to fetch user profile: ${error.error_description}`);
  }
  
  return response.json();
}

/**
 * Complete OAuth login flow (frontend)
 * 1. Exchange code for tokens
 * 2. Fetch user profile
 * 3. Return combined profile and tokens
 */
export async function completeGoogleOAuthFlow(code: string): Promise<{
  profile: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    avatar?: string;
  };
  tokens: {
    access_token: string;
    refresh_token?: string;
    expires_in: number;
  };
}> {
  try {
    // Exchange authorization code for tokens
    const tokens = await exchangeGoogleAuthCode(code);
    
    // Fetch user profile using access token
    const googleProfile = await fetchGoogleUserProfile(tokens.access_token);
    
    // Transform Google profile to our user format
    const profile = {
      id: googleProfile.id,
      email: googleProfile.email,
      firstName: googleProfile.given_name || googleProfile.name.split(' ')[0] || '',
      lastName: googleProfile.family_name || googleProfile.name.split(' ').slice(1).join(' ') || '',
      avatar: googleProfile.picture,
    };
    
    return {
      profile,
      tokens: {
        access_token: tokens.access_token,
        refresh_token: tokens.refresh_token,
        expires_in: tokens.expires_in,
      },
    };
  } catch (error) {
    console.error('❌ OAuth flow error:', error);
    throw error;
  }
}

/**
 * Get the redirect path from session storage
 */
export function getOAuthRedirectPath(): string {
  if (typeof window === 'undefined') return '/dashboard';
  const path = sessionStorage.getItem('oauth_redirect_path');
  sessionStorage.removeItem('oauth_redirect_path');
  return path || '/dashboard';
}

/**
 * Clear OAuth-related storage (for logout)
 */
export function clearOAuthStorage(): void {
  if (typeof window === 'undefined') return;
  localStorage.removeItem(OAUTH_STATE_KEY);
  localStorage.removeItem(OAUTH_PKCE_KEY);
  sessionStorage.removeItem('oauth_redirect_path');
}

export type GoogleProfile = {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  avatar?: string;
};
