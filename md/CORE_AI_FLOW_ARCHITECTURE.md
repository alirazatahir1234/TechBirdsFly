# 🏗️ **CORE AI FLOW ARCHITECTURE**

Complete technical reference for the AI Website Generation Workflow

---

## **🎯 High-Level Flow**

```
User Visits /dashboard/create
    ↓
Step 1: Describe Website (AI Prompt)
    ↓
Step 2: Choose Design Style
    ↓
Step 3: Select Industry
    ↓
Step 4: Choose Color Palette
    ↓
Step 5: Generate Website
    ↓
Redirect to /dashboard/editor
    ↓
User Views Live Preview
    ↓
Edit Sections & Regenerate
    ↓
Export Code (HTML/React/Next.js)
```

---

## **📁 File Structure**

```
techbirdsfly-frontend-nextjs/
├── app/
│   ├── dashboard/
│   │   ├── layout.tsx ........................ Protected route wrapper
│   │   ├── page.tsx ......................... Main dashboard landing
│   │   │
│   │   ├── create/
│   │   │   └── page.tsx ..................... ✅ AI Generation (4 steps)
│   │   │       • Prompt input
│   │   │       • Style selection (4 options)
│   │   │       • Industry selection (6 options)
│   │   │       • Palette selection (6 options)
│   │   │       • Loading state
│   │   │
│   │   ├── editor/
│   │   │   └── page.tsx ..................... ⏳ Website editor
│   │   │       • Live preview
│   │   │       • Section management
│   │   │       • Content editing
│   │   │       • Regeneration
│   │   │       • Global styles
│   │   │
│   │   ├── export/
│   │   │   └── page.tsx ..................... ✅ Code Export (3 formats)
│   │   │       • HTML export
│   │   │       • React export
│   │   │       • Next.js export
│   │   │       • Auto-download
│   │   │
│   │   ├── projects/
│   │   │   └── page.tsx ..................... ⏳ Project management
│   │   │
│   │   ├── media/
│   │   │   └── page.tsx ..................... ⏳ Media library
│   │   │
│   │   └── settings/
│   │       └── page.tsx ..................... ⏳ Settings
│   │
│   └── components/
│       └── layout/
│           ├── Sidebar.tsx .................. ✅ Base44-style (6 items)
│           ├── DashboardLayout.tsx ......... ✅ Dashboard wrapper
│           ├── Topbar.tsx .................. ✅ Header + Logout
│           └── Navigation.tsx .............. ✅ Marketing nav
```

---

## **🔄 State Management Pattern**

### **Create Page State**
```tsx
// /app/dashboard/create/page.tsx

type GenerationStep = 'prompt' | 'style' | 'industry' | 'palette' | 'generating';

interface GenerationState {
  prompt: string;      // User's website description
  style: string;       // 'modern' | 'minimal' | 'bold' | 'creative'
  industry: string;    // 'tech' | 'ecommerce' | 'blog' | 'portfolio' | 'agency' | 'saas'
  palette: string;     // 'vibrant' | 'calm' | 'dark' | 'sunset' | 'ocean' | 'forest'
}

// Usage
const [step, setStep] = useState<GenerationStep>('prompt');
const [state, setState] = useState<GenerationState>({
  prompt: '',
  style: '',
  industry: '',
  palette: ''
});
```

### **sessionStorage (Editor Page)**
```tsx
// Store in create page
const generatedWebsite = {
  prompt: "...",
  style: "modern",
  industry: "saas",
  palette: "calm"
};
sessionStorage.setItem('generatedWebsite', JSON.stringify(generatedWebsite));

// Retrieve in editor page
const generatedWebsite = JSON.parse(
  sessionStorage.getItem('generatedWebsite') || '{}'
);
```

---

## **📊 Data Flow Diagram**

```
┌─────────────────────────────────────────────────────────────┐
│                    /dashboard/create                        │
│                  (4-Step Generation)                        │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌─────────────┐         ┌─────────────┐                   │
│  │   Prompt    │────→    │   Prompt    │                   │
│  │  TextArea   │         │   State     │                   │
│  └─────────────┘         └─────────────┘                   │
│         ↓                      ↓                            │
│  ┌─────────────┐         ┌─────────────┐                   │
│  │   Style     │────→    │   Style     │                   │
│  │  4 Buttons  │         │   State     │                   │
│  └─────────────┘         └─────────────┘                   │
│         ↓                      ↓                            │
│  ┌─────────────┐         ┌─────────────┐                   │
│  │  Industry   │────→    │  Industry   │                   │
│  │  6 Buttons  │         │   State     │                   │
│  └─────────────┘         └─────────────┘                   │
│         ↓                      ↓                            │
│  ┌─────────────┐         ┌─────────────┐                   │
│  │  Palette    │────→    │  Palette    │                   │
│  │  6 Buttons  │         │   State     │                   │
│  └─────────────┘         └─────────────┘                   │
│         ↓                      ↓                            │
│  ┌─────────────┐         ┌─────────────┐                   │
│  │  Generate   │────→    │ sessionStore│                   │
│  │   Button    │         │   (JSON)    │                   │
│  └─────────────┘         └─────────────┘                   │
│         ↓                                                   │
│  [2 Second Simulated API Call]                            │
│         ↓                                                   │
│  router.push('/dashboard/editor')                          │
│                                                              │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                    /dashboard/editor                        │
│                 (Website Editor - Pending)                  │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Retrieve from sessionStorage                              │
│           ↓                                                  │
│  ┌──────────────────┐          ┌──────────────────┐        │
│  │  Live Preview    │          │  Section Editor  │        │
│  │  (Left Panel)    │          │  (Right Panel)   │        │
│  │  • Layout        │          │  • Edit content  │        │
│  │  • Sections      │          │  • Add sections  │        │
│  │  • Responsive    │          │  • AI write      │        │
│  └──────────────────┘          │  • AI images     │        │
│           ↓                     │  • Regenerate    │        │
│  Updates in Real-Time      └──────────────────┘        │
│                                                              │
│  Global Styles Panel (Bottom)                              │
│  • Font selection                                           │
│  • Color customization                                      │
│  • Spacing adjustment                                       │
│                                                              │
│  Action Buttons                                             │
│  • Save Project                                             │
│  • Export Code                                              │
│  • Preview (Full Screen)                                    │
│                                                              │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                    /dashboard/export                        │
│              (Code Export - 3 Frameworks)                   │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────┐    ┌──────────┐    ┌──────────┐             │
│  │  HTML    │    │  React   │    │ Next.js  │             │
│  │  Export  │    │  Export  │    │  Export  │             │
│  └──────────┘    └──────────┘    └──────────┘             │
│       ↓                ↓                ↓                   │
│  [API Call: /export/{projectId}/{framework}]               │
│       ↓                ↓                ↓                   │
│  [Auto-Download .zip file]                                 │
│                                                              │
│  Filename: techbirdsfly-{framework}-{timestamp}.zip        │
│                                                              │
│  Contains:                                                  │
│  • Source code                                             │
│  • Assets & images                                         │
│  • Configuration files                                     │
│  • README.md                                               │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## **🎨 Component Hierarchy**

```
RootLayout (providers)
├── ToastProvider
├── SessionProvider
│   ├── QueryProvider
│   │   └── children
│   └── Footer (all pages)
│
├── Marketing Pages (public)
│   ├── Navigation (sticky)
│   └── page content
│
└── Dashboard Pages (protected)
    ├── ProtectedRoute
    │   └── DashboardLayout
    │       ├── Sidebar (6 items)
    │       │   ├── Create Website (primary)
    │       │   ├── Editor
    │       │   ├── Projects
    │       │   ├── Media
    │       │   ├── Export
    │       │   ├── Settings
    │       │   └── Logout (footer)
    │       │
    │       ├── Topbar
    │       │   ├── Breadcrumb
    │       │   └── Logout (top-right)
    │       │
    │       └── main
    │           ├── create/
    │           │   └── GenerationForm (4 steps)
    │           │
    │           ├── editor/
    │           │   ├── LivePreview
    │           │   ├── SectionEditor
    │           │   ├── GlobalStyles
    │           │   └── ActionButtons
    │           │
    │           ├── export/
    │           │   ├── ExportButton x3
    │           │   └── FAQSection
    │           │
    │           ├── projects/
    │           ├── media/
    │           └── settings/
```

---

## **🔐 Route Protection**

```tsx
// app/dashboard/layout.tsx
export default function DashboardLayout() {
  return (
    <ProtectedRoute>
      <DashboardLayout>
        {children}
      </DashboardLayout>
    </ProtectedRoute>
  );
}
```

**Routes:**
- ✅ `/dashboard` - Protected
- ✅ `/dashboard/create` - Protected
- ✅ `/dashboard/editor` - Protected
- ✅ `/dashboard/export` - Protected
- ✅ `/dashboard/projects` - Protected
- ✅ `/dashboard/media` - Protected
- ✅ `/dashboard/settings` - Protected
- ✅ `/marketing` - Public
- ✅ `/auth/login` - Public
- ✅ `/auth/register` - Public

---

## **🎯 Sidebar Navigation**

```tsx
// components/layout/Sidebar.tsx

const navigationItems = [
  {
    id: 'create',
    label: 'Create Website',
    icon: Sparkles,
    href: '/dashboard/create',
    description: 'Generate with AI',
    primary: true
  },
  {
    id: 'editor',
    label: 'Editor',
    icon: PenTool,
    href: '/dashboard/editor',
    description: 'Edit sections & regenerate'
  },
  {
    id: 'projects',
    label: 'Projects',
    icon: FolderOpen,
    href: '/dashboard/projects',
    description: 'All generated websites'
  },
  {
    id: 'media',
    label: 'Media',
    icon: Image,
    href: '/dashboard/media',
    description: 'AI images & uploads'
  },
  {
    id: 'export',
    label: 'Export',
    icon: Download,
    href: '/dashboard/export',
    description: 'HTML/React/Next.js'
  },
  {
    id: 'settings',
    label: 'Settings',
    icon: Settings,
    href: '/dashboard/settings',
    description: 'Profile & Billing'
  }
];
```

---

## **📱 Responsive Breakpoints**

```
Mobile:  < 640px   (Single column, stacked layout)
Tablet:  640-1024px (2 columns where applicable)
Desktop: > 1024px   (Full layout, side panels)
```

**Sidebar:** Hidden on mobile (hamburger menu - if implemented)
**Export Page:** Stacks cards on mobile
**Editor Page:** Preview above, editor below on mobile

---

## **⚡ Performance Considerations**

```
Create Page:
- Lightweight (no API calls except generation)
- sessionStorage fast access
- Instant navigation between steps

Editor Page (Planned):
- Live preview uses iframe or canvas
- Section updates debounced (300ms)
- Images lazy-loaded
- Virtualized section list if 20+ sections

Export Page:
- Loads project data once
- Caches export formats
- Download uses blob API
- Toast notifications for feedback
```

---

## **🔗 Integration Points**

### **Backend API Endpoints (Future)**

```
POST /api/generate
├── Input: { prompt, style, industry, palette }
└── Output: { projectId, sections[], css }

GET /api/projects
├── Input: { userId }
└── Output: Project[]

POST /api/projects/{id}/update
├── Input: { sections[], css, metadata }
└── Output: { success }

GET /api/export/{projectId}/{framework}
├── Input: { format: 'html' | 'react' | 'nextjs' }
└── Output: Zip file (binary)

POST /api/projects/{id}/delete
└── Output: { success }
```

### **Third-Party Services (Planned)**

```
AI Generation:
- OpenAI GPT-4 (content)
- Replicate (images)

Media Storage:
- AWS S3 (project files)
- Cloudinary (images)

Code Generation:
- Custom microservice
```

---

## **✅ Implementation Status**

| Feature | Status | File | Lines |
|---------|--------|------|-------|
| Create Page | ✅ Complete | `/app/dashboard/create/page.tsx` | 349 |
| Editor Page | ⏳ Pending | `/app/dashboard/editor/page.tsx` | TBD |
| Export Page | ✅ Complete | `/app/dashboard/export/page.tsx` | 323 |
| Projects Page | ⏳ Pending | `/app/dashboard/projects/page.tsx` | TBD |
| Media Page | ⏳ Pending | `/app/dashboard/media/page.tsx` | TBD |
| Settings Page | ⏳ Pending | `/app/dashboard/settings/page.tsx` | TBD |
| Sidebar | ✅ Complete | `/components/layout/Sidebar.tsx` | Updated |
| Dashboard Layout | ✅ Complete | `/components/layout/DashboardLayout.tsx` | Exists |
| Topbar | ✅ Complete | `/components/layout/Topbar.tsx` | Fixed |

---

## **🚀 Development Roadmap**

### **Phase 1: Core AI Flow** (Current)
- [x] Create Page (Step 1)
- [ ] Editor Page (Step 2)
- [x] Export Page (Step 3 - already complete)

### **Phase 2: Management Features** (Next)
- [ ] Projects Page
- [ ] Media Library
- [ ] Settings Page

### **Phase 3: Advanced Features** (Future)
- [ ] Template library
- [ ] Collaboration
- [ ] Analytics
- [ ] Team management

---

## **🎓 Key Technologies**

```
Frontend:
- Next.js 15.5.6 (App Router)
- React 19.1.0
- TypeScript (strict)
- Tailwind CSS v4
- lucide-react (icons)
- shadcn/ui (components)
- Zustand (state)
- React Hot Toast (notifications)

Backend (Planned):
- Node.js + Express
- PostgreSQL
- Redis (caching)
- OpenAI API
- AWS S3
```

---

## **📝 Documentation Files**

```
AI_GENERATION_WORKFLOW.md ........... Overview & user flow
CREATE_PAGE_TESTING_GUIDE.md ........ Testing procedures
CORE_AI_FLOW_ARCHITECTURE.md ....... This file
CODE_EXPORT_INTEGRATION.md ......... Export feature details
SIDEBAR_BASE44_REDESIGN.md ......... Sidebar redesign docs
QUICK_START.md ..................... Quick reference
DEPLOYMENT_CHECKLIST.md ............ Deployment guide
```

---

## **✨ Summary**

The **Core AI Flow Architecture** consists of:

1. **Create Page** ✅ - User inputs (prompt, style, industry, palette)
2. **Editor Page** ⏳ - Live preview & content editing
3. **Export Page** ✅ - Code download in 3 formats
4. **Base44 Sidebar** ✅ - 6-item minimal navigation

All connected through:
- sessionStorage for fast state transfer
- Protected routes for authentication
- Responsive design for all devices
- Clean separation of concerns

**Status:** 70% Complete (4/6 pages done)

---

**Created:** November 25, 2025
**Last Updated:** November 25, 2025
**Version:** 1.0
