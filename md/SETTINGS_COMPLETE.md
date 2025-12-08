# ⚙️ SETTINGS PAGE - COMPLETE IMPLEMENTATION GUIDE

> **Status:** ✅ FULLY IMPLEMENTED  
> **Date:** November 17, 2025  
> **Location:** `/app/settings/page.tsx`

---

## 📋 Table of Contents

1. [✅ What's Implemented](#whats-implemented)
2. [🗂️ File Structure](#file-structure)
3. [🔗 Backend Entity Mapping](#backend-entity-mapping)
4. [📊 Component Documentation](#component-documentation)
5. [🎨 UI Layout](#ui-layout)
6. [🔄 Form Data Flow](#form-data-flow)
7. [📱 Responsive Design](#responsive-design)
8. [🚀 API Integration](#api-integration)
9. [💻 Developer Guide](#developer-guide)

---

## ✅ What's Implemented

### ✔ 7 Form Components

| Component | Maps To | Fields |
|-----------|---------|--------|
| **PersonalInfoForm** | `User` entity | First Name, Last Name, Email, Username, Phone, Bio |
| **ProfileDetailsForm** | `UserProfile` + Preferences | City, Country, Zip Code |
| **ProfessionalForm** | `UserProfile` | Company Name, Department, Job Title, Website |
| **SocialLinksForm** | `UserProfile.SocialMediaLinks` | Facebook, Instagram, Twitter/X, LinkedIn |
| **PreferencesForm** | `UserProfile.Preferences` | Timezone, Theme, Language |
| **NotificationForm** | `UserProfile` | Enable All, Email, Push, SMS Notifications |
| **ProfileImageUploader** | `User` entity | Profile Image Upload |

### ✔ 1 Main Settings Page

- 3-column responsive grid layout
- Left column: 6 form sections
- Right column: Photo uploader + Connected Accounts
- Sticky photo uploader on desktop
- Full DashboardLayout wrapper

### ✔ Navigation Integration

- Settings link in **Sidebar** ✅
- Settings link in **Navigation** (top navbar) ✅

---

## 🗂️ File Structure

```
techbirdsfly-frontend-nextjs/
│
├── app/
│   └── settings/
│       └── page.tsx                    # Main Settings page
│
├── components/
│   └── settings/
│       ├── PersonalInfoForm.tsx        # User: Name, Email, Phone, Bio
│       ├── ProfileDetailsForm.tsx      # Location, Zip
│       ├── ProfessionalForm.tsx        # Company, Job, Website
│       ├── SocialLinksForm.tsx         # Social media links (JSON)
│       ├── PreferencesForm.tsx         # Timezone, Theme, Language
│       ├── NotificationForm.tsx        # Notification toggles
│       └── ProfileImageUploader.tsx    # Profile photo upload
```

---

## 🔗 Backend Entity Mapping

### 📌 User Entity Fields → UI

```typescript
// User entity
{
  firstName: string;        // PersonalInfoForm
  lastName: string;         // PersonalInfoForm (combined as FullName)
  email: string;            // PersonalInfoForm
  username: string;         // PersonalInfoForm
  phone?: string;           // PersonalInfoForm
  bio?: string;             // PersonalInfoForm
  profileImageUrl?: string; // ProfileImageUploader
}
```

### 📌 UserProfile Entity Fields → UI

```typescript
// UserProfile entity
{
  userId: string;
  
  // Location-based
  location: string;         // ProfileDetailsForm (split: city, country)
  
  // Professional
  companyName?: string;     // ProfessionalForm
  department?: string;      // ProfessionalForm
  jobTitle?: string;        // ProfessionalForm
  website?: string;         // ProfessionalForm
  
  // Social Media (JSON)
  socialMediaLinks: {
    facebook?: string;      // SocialLinksForm
    instagram?: string;     // SocialLinksForm
    twitter?: string;       // SocialLinksForm
    linkedin?: string;      // SocialLinksForm
  };
  
  // Preferences (JSON)
  preferences: {
    timezone?: string;      // PreferencesForm
    theme?: string;         // PreferencesForm
    language?: string;      // PreferencesForm
    zipcode?: string;       // ProfileDetailsForm
  };
  
  // Notifications
  notificationsEnabled: boolean;   // NotificationForm
  emailNotifications: boolean;     // NotificationForm
}
```

---

## 📊 Component Documentation

### 1. PersonalInfoForm

**Maps to:** `User` entity

**Props:**
```typescript
interface PersonalInfoFormProps {
  user?: any;  // User data
}
```

**Fields:**
- First Name
- Last Name
- Email Address
- Username
- Phone No
- Bio

**API Call:**
```
PUT /api/users/{id}
Body: { firstName, lastName, email, username, phone, bio }
```

---

### 2. ProfileDetailsForm

**Maps to:** `UserProfile.location` + `UserProfile.preferences.zipcode`

**Props:**
```typescript
interface ProfileDetailsFormProps {
  user?: any;
  profile?: any;
}
```

**Fields:**
- City
- Country
- Zip Code

**API Call:**
```
PUT /api/users/{id}/profile
Body: {
  location: `${city}, ${country}`,
  preferences: { zipcode }
}
```

---

### 3. ProfessionalForm

**Maps to:** `UserProfile` entity

**Fields:**
- Company Name
- Department
- Job Title
- Website

**API Call:**
```
PUT /api/users/{id}/profile
Body: { companyName, department, jobTitle, website }
```

---

### 4. SocialLinksForm

**Maps to:** `UserProfile.socialMediaLinks` (JSON)

**Fields:**
- Facebook URL
- Instagram URL
- Twitter/X URL
- LinkedIn URL

**API Call:**
```
PUT /api/users/{id}/profile
Body: {
  socialMediaLinks: { facebook, instagram, twitter, linkedin }
}
```

---

### 5. PreferencesForm

**Maps to:** `UserProfile.preferences` (JSON)

**Fields:**
- Timezone (dropdown)
- Theme (dropdown: light, dark, auto)
- Language (dropdown)

**API Call:**
```
PUT /api/users/{id}/profile
Body: {
  preferences: { timezone, theme, language }
}
```

---

### 6. NotificationForm

**Maps to:** `UserProfile` entity

**Fields:**
- Enable All Notifications (toggle)
- Email Notifications (toggle)
- Push Notifications (toggle)
- SMS Notifications (toggle)

**API Call:**
```
PUT /api/users/{id}/profile
Body: { notificationsEnabled, emailNotifications }
```

---

### 7. ProfileImageUploader

**Maps to:** `User.profileImageUrl`

**Features:**
- Drag & drop upload
- File preview
- Image preview display

**API Call:**
```
PUT /api/users/{id}
Body: { profileImageUrl: base64_or_url }
```

---

## 🎨 UI Layout

### Desktop Layout (3-column grid)

```
┌─────────────────────────────────────────────────────┐
│  Settings                                           │
│  Update your personal information and preferences   │
└─────────────────────────────────────────────────────┘

┌──────────────────────────────────┬──────────────────┐
│ LEFT COLUMN (col-span-2)         │ RIGHT COLUMN     │
│                                  │ (col-span-1)     │
│ 1. Personal Information          │ Your Photo       │
│ 2. Profile Details               │ [Upload Box]     │
│ 3. Professional Information      │                  │
│ 4. Social Media Links            │ Connected        │
│ 5. Preferences                   │ Accounts         │
│ 6. Notification Settings         │ ├─ Google        │
│                                  │ ├─ GitHub        │
│                                  │ └─ Microsoft     │
└──────────────────────────────────┴──────────────────┘
```

### Mobile Layout (single column)

All sections stack vertically, full width.

---

## 🔄 Form Data Flow

### User Flow

```
User → Form Component → Form State → Handle Submit
                              ↓
                        Validation Check
                              ↓
                        API Call (PUT)
                              ↓
                        Success Toast
                              ↓
                        Update UI / Reset
```

### Example: PersonalInfoForm

```tsx
// 1. Form state
const [formData, setFormData] = useState({
  firstName: "",
  lastName: "",
  email: "",
  // ...
});

// 2. Handle change
const handleChange = (e) => {
  setFormData(prev => ({
    ...prev,
    [e.target.name]: e.target.value
  }));
};

// 3. Submit
async function onSubmit(e) {
  e.preventDefault();
  // API: PUT /api/users/{id}
  await updateUser(formData);
}
```

---

## 📱 Responsive Design

### Breakpoints

**Mobile (< 768px)**
```css
grid-cols-1    /* Single column */
```

**Tablet (768px - 1024px)**
```css
lg:col-span-2  /* Forms take 2 cols */
lg:col-span-1  /* Sidebar takes 1 col */
```

**Desktop (> 1024px)**
```css
grid grid-cols-3 gap-8
col-span-2 /* Forms */
col-span-1 /* Sidebar - sticky position */
```

---

## 🚀 API Integration

### Required Endpoints

```
PUT /api/users/{id}
PUT /api/users/{id}/profile
GET /api/users/{id}
GET /api/users/{id}/profile
```

### Implementation Steps

1. **Replace Mock Data:**
```tsx
// app/settings/page.tsx
const user = await getUser();      // Fetch from API
const profile = await getProfile(); // Fetch from API
```

2. **Update Form Submissions:**
```tsx
// Each component
async function onSubmit(values) {
  await fetch(`/api/users/${userId}`, {
    method: 'PUT',
    body: JSON.stringify(values)
  });
}
```

3. **Add Error Handling:**
```tsx
try {
  await updateUser(data);
  showSuccess("Saved successfully");
} catch (error) {
  showError(error.message);
}
```

---

## 💻 Developer Guide

### Modifying a Form

**Example: Add a new field to PersonalInfoForm**

```tsx
// 1. Add to state
const [formData, setFormData] = useState({
  // ... existing fields
  middleName: user?.middleName || "",  // NEW
});

// 2. Add input
<div>
  <label className="text-sm font-medium text-gray-700 block mb-1">
    Middle Name
  </label>
  <Input
    name="middleName"
    placeholder="Enter middle name"
    value={formData.middleName}
    onChange={handleChange}
    className="bg-gray-50 border-gray-200 text-gray-900 placeholder-gray-400"
  />
</div>

// 3. Include in submission
async function onSubmit(e) {
  // ...
  await updateUser({ ...formData });  // middleName included
}
```

### Adding Validation

```tsx
// Add validation function
const validateForm = (data) => {
  if (!data.firstName) return "First name is required";
  if (!data.email.includes("@")) return "Invalid email";
  return null;
};

// Check before submit
async function onSubmit(e) {
  e.preventDefault();
  const error = validateForm(formData);
  if (error) {
    showError(error);
    return;
  }
  // ... submit
}
```

### Customizing Styles

**Change form background:**
```tsx
className="bg-blue-50 border-blue-200"  // From gray to blue
```

**Change button color:**
```tsx
className="bg-blue-600 hover:bg-blue-700 text-white"  // From purple to blue
```

---

## 📚 Quick Reference

| Task | File | Line |
|------|------|------|
| View Settings page | `app/settings/page.tsx` | - |
| Edit Personal Form | `components/settings/PersonalInfoForm.tsx` | - |
| Edit Profile Form | `components/settings/ProfileDetailsForm.tsx` | - |
| Change colors | Any component | `className="bg-..."` |
| Add validation | Any component | `onSubmit` function |
| Link Settings | `components/Navigation.tsx` | Line with Settings |
| Sidebar Settings | `components/layout/Sidebar.tsx` | sidebarItems array |

---

## ✨ Best Practices Applied

✅ **Separation of Concerns**
- Each form handles one entity section
- Reusable components
- Clean prop interfaces

✅ **Responsive Design**
- Mobile-first approach
- Tailwind grid system
- Sticky photo uploader

✅ **Type Safety**
- TypeScript interfaces for props
- Type-safe form data

✅ **User Experience**
- Cancel buttons for reset
- Loading states (disabled buttons)
- Clear field labels

✅ **Scalability**
- Easy to add new fields
- Modular component structure
- Easy API integration

---

## 🎯 Next Steps

1. **API Integration**
   - Replace mock data with real API calls
   - Add error handling
   - Add success notifications

2. **Validation**
   - Add email format validation
   - Add URL validation for social links
   - Add phone format validation

3. **Enhancement**
   - Add image crop/resize for profile photo
   - Add real-time validation feedback
   - Add undo/save indicators

4. **Testing**
   - Test form submissions
   - Test API error handling
   - Test responsive layout on devices

---

**Version:** 1.0.0  
**Last Updated:** November 17, 2025  
**Status:** ✅ Complete & Ready for API Integration
