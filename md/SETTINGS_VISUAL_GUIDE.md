# 🎨 SETTINGS PAGE - VISUAL OVERVIEW & QUICK REFERENCE

---

## 📸 UI Layout Diagram

### Desktop Layout (1024px+)

```
┌─────────────────────────────────────────────────────────────────────┐
│ ⚙️ Settings                    🔍 Search         🔔 👤 ⚙️           │
├─────────────────────────────────────────────────────────────────────┤
│                                                                       │
│  Account Settings                                                    │
│  Update your personal information and preferences                    │
│                                                                       │
│  ┌──────────────────────────────────────┬──────────────────────────┐│
│  │ LEFT COLUMN (2/3 width)              │ RIGHT COLUMN (1/3 width)││
│  │                                      │                          ││
│  │ ┌─ Personal Information ──────────┐  │ ┌─ Your Photo ────────┐ ││
│  │ │ Full Name: [____________]      │  │ │    [Cloud Icon]      │ ││
│  │ │ Last Name: [____________]      │  │ │  [________Image______] │ ││
│  │ │ Email:     [____________]      │  │ │ Drag & drop upload   │ ││
│  │ │ Username:  [____________]      │  │ │ [Upload Button]      │ ││
│  │ │ Phone:     [____________]      │  │ └──────────────────────┘ ││
│  │ │ Bio:       [____________]      │  │                          ││
│  │ │              [Cancel] [Save]  │  │ ┌─ Connected Accounts ─┐ ││
│  │ └─────────────────────────────────┘  │ │ [Google Button]      │ ││
│  │                                      │ │ [GitHub Button]      │ ││
│  │ ┌─ Profile Details ───────────────┐  │ │ [Microsoft Button]   │ ││
│  │ │ City:      [____________]       │  │ └──────────────────────┘ ││
│  │ │ Country:   [____________]       │  │                          ││
│  │ │ Zip Code:  [____________]       │  │                          ││
│  │ │              [Cancel] [Save]   │  │                          ││
│  │ └─────────────────────────────────┘  │                          ││
│  │                                      │                          ││
│  │ ┌─ Professional Information ──────┐  │                          ││
│  │ │ Company:   [____________]       │  │                          ││
│  │ │ Department:[____________]       │  │                          ││
│  │ │ Job Title: [____________]       │  │                          ││
│  │ │ Website:   [____________]       │  │                          ││
│  │ │              [Cancel] [Save]   │  │                          ││
│  │ └─────────────────────────────────┘  │                          ││
│  │                                      │                          ││
│  │ ┌─ Social Media Links ────────────┐  │                          ││
│  │ │ Facebook:  [________________]   │  │                          ││
│  │ │ Instagram: [________________]   │  │                          ││
│  │ │ Twitter:   [________________]   │  │                          ││
│  │ │ LinkedIn:  [________________]   │  │                          ││
│  │ │              [Cancel] [Save]   │  │                          ││
│  │ └─────────────────────────────────┘  │                          ││
│  │                                      │                          ││
│  │ ┌─ Preferences ──────────────────┐   │                          ││
│  │ │ Timezone: [Dropdown ▼]         │   │                          ││
│  │ │ Theme:    [Dropdown ▼]         │   │                          ││
│  │ │ Language: [Dropdown ▼]         │   │                          ││
│  │ │              [Cancel] [Save]   │   │                          ││
│  │ └─────────────────────────────────┘   │                          ││
│  │                                      │                          ││
│  │ ┌─ Notification Settings ────────┐   │                          ││
│  │ │ ☑ Enable All Notifications     │   │                          ││
│  │ │ ☑ Email Notifications          │   │                          ││
│  │ │ ☑ Push Notifications           │   │                          ││
│  │ │ ☐ SMS Notifications            │   │                          ││
│  │ │              [Cancel] [Save]   │   │                          ││
│  │ └─────────────────────────────────┘   │                          ││
│  │                                      │                          ││
│  └──────────────────────────────────────┴──────────────────────────┘│
│                                                                       │
└─────────────────────────────────────────────────────────────────────┘
```

---

### Mobile Layout (<768px)

```
┌──────────────────────┐
│ ☰ Settings     🔍    │
├──────────────────────┤
│                      │
│ Account Settings     │
│ Update your info     │
│                      │
│ Personal Information │
│ Full Name:           │
│ [______________]     │
│ Last Name:           │
│ [______________]     │
│ Email:               │
│ [______________]     │
│ Username:            │
│ [______________]     │
│ Phone:               │
│ [______________]     │
│ Bio:                 │
│ [______________]     │
│  [Cancel] [Save]     │
│                      │
│ Profile Details      │
│ City:                │
│ [______________]     │
│ Country:             │
│ [______________]     │
│ Zip Code:            │
│ [______________]     │
│  [Cancel] [Save]     │
│                      │
│ ... (rest scrolls)   │
│                      │
│ Your Photo           │
│   [Cloud Icon]       │
│ [______Image_____]   │
│ [Upload Button]      │
│                      │
│ Connected Accounts   │
│ [Google]             │
│ [GitHub]             │
│ [Microsoft]          │
│                      │
└──────────────────────┘
```

---

## 🎯 Form Components Map

### Component Structure

```
Settings Page (app/settings/page.tsx)
├── DashboardLayout (wrapper)
│   ├── Topbar (search, notifications, settings)
│   ├── Sidebar (navigation menu)
│   └── Main Content (3-column grid)
│       ├── LEFT COLUMN (col-span-2)
│       │   ├── PersonalInfoForm ✅
│       │   ├── ProfileDetailsForm ✅
│       │   ├── ProfessionalForm ✅
│       │   ├── SocialLinksForm ✅
│       │   ├── PreferencesForm ✅
│       │   └── NotificationForm ✅
│       │
│       └── RIGHT COLUMN (col-span-1, sticky)
│           ├── ProfileImageUploader ✅
│           └── Connected Accounts (hardcoded)
```

---

## 🔄 Data Flow Diagram

```
USER INTERACTION
│
├─→ Form Input Changes
│   └─→ handleChange() triggers
│       └─→ setFormData() updates state
│           └─→ Component re-renders with new value
│
├─→ Form Submission
│   └─→ onSubmit() triggered
│       ├─→ e.preventDefault()
│       ├─→ setIsSaving(true)
│       ├─→ API Call: PUT /api/users/{id}
│       │   └─→ Send formData as JSON
│       ├─→ Wait for response
│       └─→ setIsSaving(false)
│           └─→ Button re-enabled
│
└─→ Cancel Button
    └─→ Reset form to initial state
```

---

## 📊 Field Mapping Reference Table

```
┌─────────────────────────┬──────────────────┬───────────────────────┐
│ UI Field                │ Form Component   │ Backend Entity/Field  │
├─────────────────────────┼──────────────────┼───────────────────────┤
│ First Name              │ PersonalInfo     │ User.firstName        │
│ Last Name               │ PersonalInfo     │ User.lastName         │
│ Email Address           │ PersonalInfo     │ User.email            │
│ Username                │ PersonalInfo     │ User.username         │
│ Phone No                │ PersonalInfo     │ User.phone            │
│ Bio                     │ PersonalInfo     │ User.bio              │
│                         │                  │                       │
│ City                    │ ProfileDetails   │ UserProfile.location  │
│ Country                 │ ProfileDetails   │ UserProfile.location  │
│ Zip Code                │ ProfileDetails   │ Preferences.zipcode   │
│                         │                  │                       │
│ Company Name            │ Professional    │ UserProfile.company   │
│ Department              │ Professional    │ UserProfile.department│
│ Job Title               │ Professional    │ UserProfile.jobTitle  │
│ Website                 │ Professional    │ UserProfile.website   │
│                         │                  │                       │
│ Facebook URL            │ SocialLinks     │ SocialMediaLinks.fb   │
│ Instagram URL           │ SocialLinks     │ SocialMediaLinks.ig   │
│ Twitter/X URL           │ SocialLinks     │ SocialMediaLinks.tw   │
│ LinkedIn URL            │ SocialLinks     │ SocialMediaLinks.li   │
│                         │                  │                       │
│ Timezone                │ Preferences     │ Preferences.timezone  │
│ Theme                   │ Preferences     │ Preferences.theme     │
│ Language                │ Preferences     │ Preferences.language  │
│                         │                  │                       │
│ Enable All Notif        │ Notification    │ notificationsEnabled  │
│ Email Notif             │ Notification    │ emailNotifications    │
│ Push Notif              │ Notification    │ pushNotifications     │
│ SMS Notif               │ Notification    │ smsNotifications      │
│                         │                  │                       │
│ Profile Image           │ ImageUploader   │ User.profileImageUrl  │
└─────────────────────────┴──────────────────┴───────────────────────┘
```

---

## 🎨 Color & Styling Reference

### Color Palette

```
Primary Purple:   #7c3aed (purple-600)
Primary Purple Hover: #8b5cf6 (purple-700)

Background:       #ffffff (white)
Input Background: #f3f4f6 (gray-50)
Input Border:     #e5e7eb (gray-200)
Input Text:       #111827 (gray-900)
Placeholder:      #9ca3af (gray-400)

Label Text:       #374151 (gray-700)
Section Title:    #111827 (gray-900)
Muted Text:       #6b7280 (gray-600)

Section Border:   #f3f4f6 (gray-100)
Divider:          #e5e7eb (gray-200)
```

### Typography Sizes

```
Page Title:        text-3xl font-bold
Section Title:     text-xl font-semibold
Field Label:       text-sm font-medium
Field Input:       text-base (default)
Placeholder:       text-sm
Button Text:       font-medium
```

### Spacing

```
Form Gap:          gap-4 (1rem)
Section Gap:       space-y-8 (2rem)
Component Padding: p-6 (1.5rem)
Input Padding:     px-4 py-2
Border Radius:     rounded-lg
```

---

## ✅ Checklist for Testing

### Desktop (1024px+)
- [ ] 3-column layout displays correctly
- [ ] Right sidebar is sticky while scrolling
- [ ] Forms align properly
- [ ] Image uploader positioned correctly

### Tablet (768px)
- [ ] Layout adjusts to tablet width
- [ ] Columns reflow appropriately
- [ ] Touch targets are large enough

### Mobile (< 768px)
- [ ] Single column layout
- [ ] Full-width inputs
- [ ] Sections stack vertically
- [ ] Image uploader on top
- [ ] Buttons are touch-friendly

### Form Functionality
- [ ] Typing in fields updates state
- [ ] Save button submits data
- [ ] Cancel button resets form
- [ ] Save button disables while saving
- [ ] Loading text shows "Saving..."

### Navigation
- [ ] Settings visible in sidebar
- [ ] Settings link in top navigation
- [ ] Page accessible via /settings route
- [ ] DashboardLayout wraps content

---

## 🚀 Quick Navigation

**Access Settings Page:**
```
URL: http://localhost:3000/settings

Via Sidebar:
Home → Settings ⚙️

Via Topbar:
Logo → Settings
```

**Edit Forms:**
```
PersonalInfoForm   → components/settings/PersonalInfoForm.tsx
ProfileDetailsForm → components/settings/ProfileDetailsForm.tsx
ProfessionalForm   → components/settings/ProfessionalForm.tsx
SocialLinksForm    → components/settings/SocialLinksForm.tsx
PreferencesForm    → components/settings/PreferencesForm.tsx
NotificationForm   → components/settings/NotificationForm.tsx
ImageUploader      → components/settings/ProfileImageUploader.tsx
```

**Update Navigation:**
```
Add Sidebar Item   → components/layout/Sidebar.tsx (line 27+)
Add Nav Link       → components/Navigation.tsx (add <Link>)
```

---

## 📝 Form Templates

### Adding a New Field to PersonalInfoForm

```tsx
// 1. Add to useState
const [formData, setFormData] = useState({
  // ... existing fields
  newField: user?.newField || "",  // ADD THIS
});

// 2. Add input element
<div>
  <label className="text-sm font-medium text-gray-700 block mb-1">
    Field Label
  </label>
  <Input
    name="newField"
    placeholder="Enter value"
    value={formData.newField}
    onChange={handleChange}
    className="bg-gray-50 border-gray-200 text-gray-900 placeholder-gray-400"
  />
</div>

// 3. Already included in submission:
await updateUser(formData); // includes newField automatically
```

---

## 🔗 API Endpoints Quick Reference

```
GET    /api/users/{userId}
GET    /api/users/{userId}/profile
PUT    /api/users/{userId}
PUT    /api/users/{userId}/profile

Request Body Examples:

// Update User (PersonalInfo, ProfileImage)
PUT /api/users/123
{
  firstName: "John",
  lastName: "Doe",
  email: "john@example.com",
  username: "johndoe",
  phone: "+1234567890",
  bio: "...",
  profileImageUrl: "https://..."
}

// Update Profile (All other forms)
PUT /api/users/123/profile
{
  location: "San Francisco, USA",
  companyName: "...",
  department: "...",
  jobTitle: "...",
  website: "...",
  socialMediaLinks: {
    facebook: "...",
    instagram: "...",
    twitter: "...",
    linkedin: "..."
  },
  preferences: {
    timezone: "pst",
    theme: "light",
    language: "english",
    zipcode: "94105"
  },
  notificationsEnabled: true,
  emailNotifications: true
}
```

---

## 🎓 Component Prop Reference

```typescript
// PersonalInfoForm
interface PersonalInfoFormProps {
  user?: {
    firstName?: string;
    lastName?: string;
    email?: string;
    username?: string;
    phone?: string;
    bio?: string;
  };
}

// ProfileDetailsForm
interface ProfileDetailsFormProps {
  user?: any;
  profile?: {
    location?: string;
    preferences?: { zipcode?: string };
  };
}

// ProfessionalForm
interface ProfessionalFormProps {
  profile?: {
    companyName?: string;
    department?: string;
    jobTitle?: string;
    website?: string;
  };
}

// SocialLinksForm
interface SocialLinksFormProps {
  profile?: {
    socialMediaLinks?: {
      facebook?: string;
      instagram?: string;
      twitter?: string;
      linkedin?: string;
    };
  };
}

// PreferencesForm
interface PreferencesFormProps {
  profile?: {
    preferences?: {
      timezone?: string;
      theme?: string;
      language?: string;
    };
  };
}

// NotificationForm
interface NotificationFormProps {
  profile?: {
    notificationsEnabled?: boolean;
    emailNotifications?: boolean;
  };
}

// ProfileImageUploader
interface ProfileImageUploaderProps {
  user?: {
    profileImageUrl?: string;
  };
}
```

---

**Version:** 1.0.0  
**Last Updated:** November 17, 2025  
**Status:** ✅ COMPLETE
