import NextAuth from 'next-auth';
import Credentials from 'next-auth/providers/credentials';
import Google from 'next-auth/providers/google';
import Facebook from 'next-auth/providers/facebook';

const API_BASE = process.env.NEXT_PUBLIC_API_BASE || 'http://localhost:5000/api';

export const { handlers, auth, signIn, signOut } = NextAuth({
  providers: [
    // Credentials Provider (JWT with backend)
    Credentials({
      name: 'Credentials',
      credentials: {
        email: { label: 'Email', type: 'email' },
        password: { label: 'Password', type: 'password' },
      },
      async authorize(credentials) {
        if (!credentials?.email || !credentials?.password) {
          throw new Error('Invalid credentials');
        }

        try {
          const response = await fetch(`${API_BASE}/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
              email: credentials.email,
              password: credentials.password,
            }),
          });

          if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Login failed');
          }

          const user = await response.json();

          return {
            id: user.id,
            email: user.email,
            name: user.fullName,
            image: user.avatar,
            accessToken: user.accessToken,
            refreshToken: user.refreshToken,
          };
        } catch (error) {
          throw new Error(error instanceof Error ? error.message : 'Authentication failed');
        }
      },
    }),

    // Google OAuth Provider
    Google({
      clientId: process.env.GOOGLE_CLIENT_ID || '',
      clientSecret: process.env.GOOGLE_CLIENT_SECRET || '',
      allowDangerousEmailAccountLinking: true,
    }),

    // Facebook OAuth Provider
    Facebook({
      clientId: process.env.FACEBOOK_CLIENT_ID || '',
      clientSecret: process.env.FACEBOOK_CLIENT_SECRET || '',
      allowDangerousEmailAccountLinking: true,
    }),
  ],

  pages: {
    signIn: '/login',
    error: '/auth/error',
  },

  callbacks: {
    // JWT callback - add tokens to JWT
    async jwt({ token, user, account }) {
      if (user) {
        token.id = user.id;
        token.accessToken = (user as any).accessToken;
        token.refreshToken = (user as any).refreshToken;
      }

      // Handle OAuth providers
      if (account?.provider === 'google' || account?.provider === 'facebook') {
        token.provider = account.provider;
        token.providerAccountId = account.providerAccountId;
      }

      // Refresh token if expired
      if (token.accessToken && !token.accessTokenExpires) {
        // Set expiry to 1 hour from now
        token.accessTokenExpires = Date.now() + 60 * 60 * 1000;
      }

      return token;
    },

    // Session callback - add JWT data to session
    async session({ session, token }) {
      if (session.user) {
        session.user.id = token.id as string;
        session.user.email = token.email as string;
        (session as any).accessToken = token.accessToken;
        (session as any).refreshToken = token.refreshToken;
        (session as any).provider = token.provider;
      }
      return session;
    },

    // Redirect after signin
    async redirect({ url, baseUrl }) {
      // Redirect to dashboard after successful login
      if (url.startsWith('/')) return `${baseUrl}${url}`;
      if (new URL(url).origin === baseUrl) return url;
      return baseUrl + '/dashboard';
    },

    // SignIn callback - handle OAuth
    async signIn({ user, account, profile }) {
      if (account?.provider === 'google' || account?.provider === 'facebook') {
        try {
          // Register or link OAuth account with backend
          const response = await fetch(`${API_BASE}/auth/oauth-callback`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
              provider: account.provider,
              providerAccountId: account.providerAccountId,
              email: user.email,
              name: user.name,
              image: user.image,
            }),
          });

          if (!response.ok) {
            console.error('OAuth callback failed:', await response.json());
            return false;
          }

          const data = await response.json();
          user.id = data.id;
          (user as any).accessToken = data.accessToken;
          (user as any).refreshToken = data.refreshToken;

          return true;
        } catch (error) {
          console.error('OAuth error:', error);
          return false;
        }
      }

      return true;
    },
  },

  events: {
    async signOut() {
      // Clear tokens on sign out
      if (typeof window !== 'undefined') {
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
      }
    },
  },

  session: {
    strategy: 'jwt',
    maxAge: 24 * 60 * 60, // 24 hours
    updateAge: 60 * 60, // Update every hour
  },

  jwt: {
    maxAge: 24 * 60 * 60, // 24 hours
  },

  trustHost: true,

  debug: process.env.NODE_ENV === 'development',
});
