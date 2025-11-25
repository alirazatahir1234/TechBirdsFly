# 🐦 AppLogo Component - Quick Reference

## Basic Usage

```tsx
import { AppLogoIcon, AppLogoHorizontal, AppLogoText } from "@/components/AppLogo";

// Icon only (sidebar, compact)
<AppLogoIcon size="md" />

// Logo + text (headers, branding)
<AppLogoHorizontal size="lg" />

// Text only (subtle)
<AppLogoText size="sm" />
```

---

## Component Props

### `AppLogo` (Main Component)

```tsx
<AppLogo
  variant="icon" | "horizontal" | "text"  // default: "icon"
  size="sm" | "md" | "lg"                  // default: "md"
  className="..."                          // additional classes
/>
```

### Quick Variants

| Component | Usage |
|-----------|-------|
| `<AppLogoIcon />` | Icon only |
| `<AppLogoHorizontal />` | Logo + text |
| `<AppLogoText />` | Text only |

---

## Sizes Reference

### Icon Sizes
- `sm`: 32×32px
- `md`: 48×48px (default)
- `lg`: 64×64px

### Horizontal Sizes
- `sm`: 120px wide
- `md`: 150px wide (default)
- `lg`: 200px wide

---

## Common Patterns

### 1. Sidebar Logo
```tsx
<div className="p-6 border-b">
  <AppLogoIcon size="md" />
</div>
```

### 2. Navbar with Text
```tsx
<div className="flex items-center gap-3">
  <AppLogoIcon size="sm" />
  <span className="font-bold">Dashboard</span>
</div>
```

### 3. Login Page
```tsx
<div className="flex justify-center mb-8">
  <AppLogoIcon size="lg" />
</div>
```

### 4. Header with Branding
```tsx
<header className="flex items-center justify-between">
  <AppLogoHorizontal size="md" />
  <nav>...</nav>
</header>
```

### 5. Responsive Logo
```tsx
<div className="hidden md:block">
  <AppLogoHorizontal size="md" />
</div>
<div className="md:hidden">
  <AppLogoIcon size="sm" />
</div>
```

---

## File Locations

| File | Purpose |
|------|---------|
| `/components/AppLogo.tsx` | Logo component |
| `/public/images/techbirdsfly/logo.svg` | Logo image |
| `/app/layout.tsx` | Favicon config |
| `/components/layout/Sidebar.tsx` | Uses AppLogoIcon |
| `/app/login/page.tsx` | Uses AppLogoIcon |
| `/components/layout/Topbar.tsx` | Uses AppLogoIcon |

---

## Styling Examples

### Custom Classes
```tsx
<AppLogoIcon 
  size="md" 
  className="rounded-full border-2 border-purple-500"
/>
```

### With Dark Mode
```tsx
<div className="block dark:hidden">
  <AppLogoIcon size="md" />
</div>
<div className="hidden dark:block opacity-90">
  <AppLogoIcon size="md" />
</div>
```

### Interactive
```tsx
<button className="hover:opacity-80 transition">
  <AppLogoIcon size="md" />
</button>
```

---

## Future Updates

### Replace SVG with PNG
In `/components/AppLogo.tsx`, change:
```tsx
src="/images/techbirdsfly/logo.svg"
↓
src="/images/techbirdsfly/logo.png"
```

### Add Dark Mode Variants
Save:
- `/public/images/techbirdsfly/logo-dark.svg`
- `/public/images/techbirdsfly/logo-white.svg`

Then add conditional rendering in component.

---

## Component Files Structure

```
components/
├── AppLogo.tsx                    ← Main component
├── layout/
│   ├── Sidebar.tsx               ← Uses AppLogoIcon
│   └── Topbar.tsx                ← Uses AppLogoIcon
└── ...

public/
└── images/
    └── techbirdsfly/
        ├── logo.svg              ← Current logo
        └── [future variants...]

app/
├── layout.tsx                    ← Favicon config
└── login/
    └── page.tsx                  ← Uses AppLogoIcon
```

---

## One-Liner Examples

```tsx
// Sidebar
<AppLogoIcon size="md" />

// Login header
<AppLogoIcon size="lg" />

// Navbar branding
<AppLogoHorizontal size="md" />

// Footer
<AppLogoText size="sm" />

// Mobile nav
<AppLogoIcon size="sm" className="rounded" />
```

---

## TypeScript Support

Full type safety:
```tsx
import type { AppLogoProps } from "@/components/AppLogo";

// IntelliSense provides autocomplete for:
// - variant: "icon" | "horizontal" | "text"
// - size: "sm" | "md" | "lg"
// - className: string (optional)
```

---

**Logo is production-ready!** 🚀
