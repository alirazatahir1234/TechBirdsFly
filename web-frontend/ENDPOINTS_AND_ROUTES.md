# 📋 TechBirdsFly Frontend - API Endpoints & Routes Documentation

> **Last Updated:** November 17, 2025  
> **Project:** TechBirdsFly Frontend (Next.js 15.5.6)  
> **Stack:** Next.js, Tailwind CSS, shadcn/ui, TypeScript

---

## 🚀 Quick Links

- **Local Development:** http://localhost:3000
- **Network Access:** http://192.168.70.244:3000
- **Dashboard:** http://localhost:3000/dashboard

---

## 📱 Frontend Routes (Pages)

### Public Routes (No Authentication Required)

| Route | Component | Purpose | Status |
|-------|-----------|---------|--------|
| `/` | `app/page.tsx` | Landing page with hero & features | ✅ Live |
| `/login` | `app/login/page.tsx` | User login page | ✅ Live |
| `/register` | `app/register/page.tsx` | User registration page | ✅ Live |
| `/forgot-password` | `app/forgot-password/page.tsx` | Forgot password recovery | ✅ Live |

### Protected Routes (Authentication Required)

| Route | Component | Purpose | Status |
|-------|-----------|---------|--------|
| `/dashboard` | `app/dashboard/page.tsx` | Main dashboard with analytics | ✅ Live |
| `/builder` | Protected | Website builder (planned) | 🔲 Planned |
| `/projects` | Protected | Projects management (planned) | 🔲 Planned |
| `/settings` | Protected | User settings (planned) | 🔲 Planned |
| `/profile` | Protected | User profile (planned) | 🔲 Planned |

---

## 🔐 Authentication API Routes

### NextAuth Configuration
- **Route:** `/api/auth/[...nextauth]`
- **Provider:** NextAuth.js v5.0.0-beta.29
- **Location:** `app/api/auth/[...nextauth]/route.ts`

### Available Auth Endpoints

| Endpoint | Method | Purpose | Response |
|----------|--------|---------|----------|
| `/api/auth/signin` | GET/POST | Sign in user | Session token |
| `/api/auth/signout` | GET/POST | Sign out user | Redirect to login |
| `/api/auth/session` | GET | Get current session | Session object or null |
| `/api/auth/providers` | GET | List auth providers | Provider list |
| `/api/auth/csrf` | GET | Get CSRF token | CSRF token |
| `/api/auth/callback/[provider]` | GET/POST | OAuth callback | Auth result |

---

## 🎨 Dashboard Components & Features

### Layout Components

#### 1. **Sidebar Navigation**
- **File:** `components/layout/Sidebar.tsx`
- **Features:**
  - Logo with gradient (M icon)
  - Navigation menu with active states
  - Submenu collapse/expand
  - User profile card at bottom
  - Responsive mobile support

**Menu Items:**
- Home
- Dashboard (active)
- Analytics
- Pages
- Applications
- E-commerce
- Authentication

#### 2. **Top Navigation Bar**
- **File:** `components/layout/Topbar.tsx`
- **Features:**
  - Page title display
  - Search input
  - Notification bell icon
  - Settings gear icon
  - Responsive design

#### 3. **Dashboard Layout Wrapper**
- **File:** `components/layout/DashboardLayout.tsx`
- **Features:**
  - Sidebar + Topbar integration
  - Mobile hamburger menu toggle
  - Responsive sidebar (collapsible on mobile)
  - Overlay for mobile sidebar

---

### Dashboard Widgets

#### 1. **Active Users Widget**
- **File:** `components/dashboard/ActiveUsers.tsx`
- **Displays:**
  - Active user count: **300**
  - Purple bar chart visualization
  - "Page views per minute" metric
  - Payout upgrade suggestion

#### 2. **Statistics Cards**
- **File:** `components/dashboard/StatsCards.tsx`
- **Displays (4 Cards):**
  - **Users:** 35k (purple icon)
  - **Clicks:** 1m (green icon)
  - **Sales:** 345$ (red icon)
  - **Items:** 68 (blue icon)

#### 3. **Earnings Card**
- **File:** `components/dashboard/EarningsCard.tsx`
- **Displays:**
  - Monthly earnings: **735.2$**
  - "Withdraw All Earnings" button
  - **Earnings by Item List:**
    - Bento 3D Kit - Illustration
    - Bento 3D Kit - Coded Template
    - Bento 3D Kit - Illustration

#### 4. **Sales by Age Chart**
- **File:** `components/dashboard/SalesByAgeChart.tsx`
- **Displays:**
  - Smooth purple curve line chart
  - Age groups (10-15, 15-20, 20-25, etc.)
  - Interactive data visualization
  - Grid lines for reference

#### 5. **Impression Chart**
- **File:** `components/dashboard/ImpressionChart.tsx`
- **Displays:**
  - Weekly bar chart (Mon-Thu)
  - Purple bars for active day
  - Light purple bars for other days
  - Y-axis labels (0, 10, 20)

---

## 📂 File Structure

```
techbirdsfly-frontend-nextjs/
├── app/
│   ├── page.tsx                           # Landing page
│   ├── layout.tsx                         # Root layout
│   ├── globals.css                        # Global styles with Tailwind v4
│   ├── login/
│   │   └── page.tsx                       # Login page
│   ├── register/
│   │   └── page.tsx                       # Register page
│   ├── forgot-password/
│   │   └── page.tsx                       # Forgot password page
│   ├── dashboard/
│   │   └── page.tsx                       # Dashboard page
│   └── api/
│       └── auth/
│           └── [...nextauth]/
│               └── route.ts               # NextAuth handler
│
├── components/
│   ├── Navigation.tsx                     # Main navigation
│   ├── layout/
│   │   ├── Sidebar.tsx                    # Sidebar navigation
│   │   ├── Topbar.tsx                     # Top navigation bar
│   │   └── DashboardLayout.tsx            # Dashboard wrapper
│   ├── dashboard/
│   │   ├── ActiveUsers.tsx                # Active users widget
│   │   ├── StatsCards.tsx                 # Stats cards (4x)
│   │   ├── EarningsCard.tsx               # Earnings display
│   │   ├── SalesByAgeChart.tsx            # Sales chart
│   │   └── ImpressionChart.tsx            # Impression chart
│   ├── ui/
│   │   ├── button.tsx                     # shadcn button
│   │   ├── card.tsx                       # shadcn card
│   │   ├── input.tsx                      # shadcn input
│   │   ├── dropdown-menu.tsx              # shadcn dropdown
│   │   └── navigation-menu.tsx            # shadcn navigation
│   └── forms/
│       ├── FormCheckbox.tsx               # Form checkbox
│       └── FormInput.tsx                  # Form input
│
├── lib/
│   ├── utils.ts                           # Utility functions
│   ├── hooks/
│   │   ├── useCanvasTransform.ts          # Canvas transform hook
│   │   ├── useNextAuthSession.ts          # Auth session hook
│   │   └── useQueries.ts                  # Query hooks
│   ├── providers/
│   │   ├── QueryProvider.tsx              # React Query provider
│   │   └── SessionProvider.tsx            # NextAuth session provider
│   ├── schemas/
│   │   └── auth.ts                        # Zod schemas for auth
│   ├── store/
│   │   ├── authStore.ts                   # Zustand auth store
│   │   └── builderStore.ts                # Zustand builder store
│   └── utils/
│       └── PostMessageBridge.ts           # Message bridge utility
│
├── public/                                # Static assets
├── middleware.ts                          # NextAuth middleware
├── auth.ts                                # NextAuth config
├── next.config.ts                         # Next.js config
├── tailwind.config.ts                     # Tailwind config
├── tsconfig.json                          # TypeScript config
├── package.json                           # Dependencies
└── README.md                              # Project README
```

---

## 🔗 Component Routes & Props

### Dashboard Page
**Route:** `/dashboard`
```tsx
<DashboardLayout title="Dashboard">
  <Grid>
    <ActiveUsers />
    <EarningsCard />
    <StatsCards />
    <SalesByAgeChart />
    <ImpressionChart />
  </Grid>
</DashboardLayout>
```

---

## 📦 Dependencies

### Core
- `next@15.5.6` - React framework
- `react@19.1.0` - UI library
- `react-dom@19.1.0` - DOM rendering

### Authentication
- `next-auth@5.0.0-beta.29` - Authentication

### UI & Styling
- `tailwindcss@4` - CSS framework
- `@tailwindcss/postcss@4` - Tailwind PostCSS
- `class-variance-authority@0.7.1` - Variant utilities
- `clsx@2.1.1` - Conditional classnames
- `tailwind-merge@3.3.1` - Merge utilities

### Icons
- `lucide-react@0.546.0` - Icon library

### Forms & Validation
- `react-hook-form@7.65.0` - Form management
- `@hookform/resolvers@5.2.2` - Form resolvers
- `zod@4.1.12` - Data validation

### State Management
- `zustand@5.0.8` - State store
- `@tanstack/react-query@5.90.5` - Data fetching
- `@tanstack/react-query-devtools@5.90.2` - Query debugging

### UI Components (shadcn/ui)
- `@radix-ui/react-dropdown-menu@2.1.16`
- `@radix-ui/react-navigation-menu@1.2.14`
- `@radix-ui/react-slot@1.2.3`

---

## 🧪 Testing Setup

### Unit Testing
- **Tool:** Vitest
- **Config:** `vitest.config.ts`
- **Test Utils:** `__tests__/utils/test-utils.tsx`
- **Mock Data:** `__tests__/utils/mock-data.ts`

**Test Files:**
- `__tests__/unit/builderStore.test.ts`
- `__tests__/unit/CanvasToolbar.test.tsx`

### E2E Testing
- **Tool:** Playwright
- **Config:** `playwright.config.ts`
- **Test Files:**
  - `e2e/auth.spec.ts` - Authentication flows
  - `e2e/builder.spec.ts` - Builder functionality

---

## 🚀 Available Commands

```bash
# Development
npm run dev              # Start development server with Turbopack

# Building
npm run build            # Build for production with Turbopack

# Production
npm start                # Start production server

# Code Quality
npm run lint             # Run ESLint
npm run lint:fix         # Fix ESLint issues
npm run type-check       # TypeScript type checking

# Testing
npm test                 # Run unit tests
npm run test:ui          # Run tests with UI
npm run test:coverage    # Run tests with coverage
npm run test:e2e         # Run E2E tests
npm run test:e2e:ui      # Run E2E tests with UI
npm run test:e2e:headed  # Run E2E tests with browser
npm run test:e2e:debug   # Debug E2E tests

# CI Pipeline
npm run ci               # Run: lint + type-check + test + build
```

---

## 🎯 Features by Page

### Landing Page (`/`)
- ✅ Hero section with gradient
- ✅ Feature showcase cards
- ✅ CTA sections
- ✅ Navigation menu
- ✅ Footer with links

### Login Page (`/login`)
- ✅ Email input field
- ✅ Password input field
- ✅ "Remember me" checkbox
- ✅ Sign in button
- ✅ Forgot password link
- ✅ Sign up link

### Register Page (`/register`)
- ✅ Full name input
- ✅ Email input
- ✅ Password input
- ✅ Confirm password input
- ✅ Terms acceptance checkbox
- ✅ Sign up button
- ✅ Sign in link

### Forgot Password Page (`/forgot-password`)
- ✅ Email input
- ✅ Reset password button
- ✅ Loading state with spinner
- ✅ Back to login link
- ✅ Error handling

### Dashboard Page (`/dashboard`)
- ✅ Sidebar with 7 menu items
- ✅ Top navigation with search
- ✅ Active users count (300)
- ✅ Bar chart visualization
- ✅ 4 Stats cards
- ✅ Earnings display (735.2$)
- ✅ Earnings by item list (3 items)
- ✅ Sales by age curve chart
- ✅ Weekly impression chart
- ✅ Mobile responsive layout

---

## 🔒 Authentication Flow

### Protected Route Flow
```
User Request → Middleware Check
├─ Session Exists? 
│  ├─ Yes → Allow Access ✅
│  └─ No → Redirect to /login?callbackUrl={route}
└─ After Login → Redirect to callbackUrl
```

### Current Auth Status
- **Mode:** TESTING (Authentication temporarily disabled)
- **Setting:** `DISABLE_AUTH_FOR_TESTING = true` in `middleware.ts`
- **To Enable:** Change to `false` when ready for production

---

## 🎨 Design System

### Color Scheme
- **Primary:** Purple (#7c3aed)
- **Secondary:** Gray (#6b7280)
- **Success:** Green (#10b981)
- **Warning:** Red (#ef4444)
- **Info:** Blue (#3b82f6)
- **Background:** Light gray (#f9fafb)

### Typography
- **Font Family:** Geist Sans / Geist Mono (variable fonts)
- **Base Size:** 16px
- **Heading Sizes:** 2xl, xl, lg, base, sm, xs

### Spacing Scale
- **xs:** 0.25rem (4px)
- **sm:** 0.5rem (8px)
- **base:** 1rem (16px)
- **lg:** 1.5rem (24px)
- **xl:** 2rem (32px)

### Border Radius
- **sm:** 0.375rem
- **base:** 0.625rem
- **lg:** 0.875rem
- **xl:** 1.25rem

---

## 📊 Data Models

### User Profile
```typescript
{
  id: string;
  name: string;
  email: string;
  avatar?: string;
  subscription?: string;
}
```

### Dashboard Stats
```typescript
{
  activeUsers: number;
  pageViewsPerMinute: number;
  earnings: number;
  users: number;
  clicks: number;
  sales: number;
  items: number;
}
```

### Chart Data
```typescript
{
  timestamp: string;
  value: number;
}
```

---

## 🔄 State Management

### Zustand Stores
- **`authStore.ts`** - User authentication state
- **`builderStore.ts`** - Website builder state

### React Query
- Configured in `lib/providers/QueryProvider.tsx`
- Dev tools enabled for debugging

### Next Auth Session
- Provider: `lib/providers/SessionProvider.tsx`
- Hook: `useNextAuthSession.ts`

---

## 🌐 Environment Variables

**File:** `.env.local`

```env
# NextAuth Configuration
NEXTAUTH_URL=http://localhost:3000
NEXTAUTH_SECRET=your-secret-key

# API Endpoints (if needed)
NEXT_PUBLIC_API_URL=http://localhost:3000/api
```

---

## 📝 Notes

### Testing Mode
- Authentication is **currently disabled** for UI testing
- To enable: Change `DISABLE_AUTH_FOR_TESTING = false` in `middleware.ts`
- All routes are accessible without login

### Browser Support
- ✅ Chrome/Edge (latest)
- ✅ Firefox (latest)
- ✅ Safari (latest)
- ✅ Mobile browsers (iOS Safari, Chrome Mobile)

### Performance
- Turbopack enabled for faster builds
- Next.js 15.5.6 with latest optimizations
- Server-side rendering (SSR) for all pages
- React 19.1.0 with latest features

---

## 🚨 Known Limitations

- API integration not yet connected
- Charts use mock data
- Form submissions don't persist data
- No backend connection yet

---

## ✅ Checklist for Production

- [ ] Re-enable authentication (`DISABLE_AUTH_FOR_TESTING = false`)
- [ ] Connect to backend API
- [ ] Set environment variables
- [ ] Run full test suite
- [ ] Performance testing
- [ ] Security audit
- [ ] Deploy to production

---

## 📞 Support & Next Steps

**Need to:**
1. Connect to backend API? → Update API endpoints
2. Add more routes? → Create new `app/[route]/page.tsx`
3. Modify dashboard? → Edit components in `components/dashboard/`
4. Change styling? → Update `app/globals.css` or component styles

---

**Created:** November 17, 2025  
**Project:** TechBirdsFly Frontend  
**Version:** 1.0.0
