# 🧪 **CREATE PAGE TESTING GUIDE**

Quick reference for testing the `/dashboard/create` page

---

## **🚀 Quick Start**

### **1. Start Dev Server**
```bash
cd techbirdsfly-frontend-nextjs
npm run dev
```

### **2. Navigate to Create Page**
```
http://localhost:3000/dashboard/create
```

### **3. Login (if needed)**
```
Email: test@example.com
Password: (from your setup)
```

---

## **✅ Test Cases**

### **Test 1: Prompt Validation**
**Goal:** Verify minimum character requirement

**Steps:**
1. Type 5 characters (e.g., "hello")
2. Verify "Continue to Style" button is disabled
3. Type 10+ characters (e.g., "hello world")
4. Verify "Continue to Style" button is enabled
5. Clear field
6. Verify button disables again

**Expected Result:** ✅ Button toggles enable/disable based on character count

---

### **Test 2: Style Selection**
**Goal:** Verify style selection works

**Steps:**
1. Complete prompt step
2. Click "Continue to Style"
3. Click "Modern" card
4. Verify border turns purple and background turns light purple
5. Click "Minimal" card
6. Verify selection changes to "Minimal"
7. Try clicking "Continue to Industry" without selecting style
8. Verify button is disabled

**Expected Result:** ✅ Only one style can be selected at a time

---

### **Test 3: Industry Selection**
**Goal:** Verify industry selection

**Steps:**
1. Complete style step
2. Click "Continue to Industry"
3. Click "Tech Startup" (🚀)
4. Verify selection highlights
5. Try "E-commerce" (🛒)
6. Verify selection changes
7. Try clicking "Continue to Colors" without industry
8. Verify button is disabled

**Expected Result:** ✅ Industry selection works correctly

---

### **Test 4: Color Palette Selection**
**Goal:** Verify palette selection

**Steps:**
1. Complete industry step
2. Click "Continue to Colors"
3. Click "Calm" palette
4. Verify selection highlights with purple border
5. Click "Sunset" palette
6. Verify selection changes
7. Verify "Create My Website" button is now enabled

**Expected Result:** ✅ Palette selection enables generation

---

### **Test 5: Generation Flow**
**Goal:** Verify complete generation process

**Steps:**
1. Fill all steps:
   - Prompt: "A modern e-commerce website for selling organic coffee"
   - Style: "Modern"
   - Industry: "E-commerce"
   - Palette: "Warm"
2. Click "Create My Website"
3. Verify button shows "Creating Website..." with spinner
4. Wait 2 seconds (simulated API call)
5. Verify redirect to `/dashboard/editor`
6. Verify page title shows "Website Editor"

**Expected Result:** ✅ Completes generation and redirects

---

### **Test 6: Back Navigation**
**Goal:** Verify back button works

**Steps:**
1. Complete prompt step
2. Go to style step
3. Click "Back"
4. Verify you're back at prompt step
5. Verify prompt text is still there
6. Continue through all steps
7. At palette step, click "Back" multiple times
8. Verify you can go back through all steps

**Expected Result:** ✅ Back button preserves data and navigates correctly

---

### **Test 7: Progress Bar**
**Goal:** Verify progress bar updates

**Steps:**
1. At prompt step, verify progress bar shows ~20%
2. Move to style step, verify ~40%
3. Move to industry step, verify ~60%
4. Move to palette step, verify ~80%
5. Start generating, verify ~100%

**Expected Result:** ✅ Progress bar increments with each step

---

### **Test 8: Character Count**
**Goal:** Verify character counter on prompt

**Steps:**
1. On prompt step, verify "0/500" display
2. Type 10 characters
3. Verify counter shows "10/500"
4. Type to 100 characters
5. Verify counter shows "100/500"

**Expected Result:** ✅ Character count updates in real-time

---

### **Test 9: Toast Notifications**
**Goal:** Verify notifications during generation

**Steps:**
1. Complete all steps
2. Click "Create My Website"
3. Verify toast appears (optional - depends on implementation)
4. Wait for completion

**Expected Result:** ✅ Toast shows during generation process

---

### **Test 10: Responsive Design**
**Goal:** Verify mobile layout

**Steps:**
1. Open dev tools (F12)
2. Toggle device toolbar (Ctrl+Shift+M)
3. Set to iPhone SE (375px width)
4. Verify layout is single column
5. Verify all text is readable
6. Verify buttons are touch-friendly
7. Verify cards stack vertically
8. Resize to iPad (768px)
9. Verify layout becomes 2 columns where applicable

**Expected Result:** ✅ Layout adapts properly to screen size

---

## **📋 Test Data**

### **Sample Prompt**
```
A modern SaaS platform for project management with a clean, 
minimalist design. The website should highlight key features, 
pricing plans, and a call-to-action for free signup.
```

### **All Style Options**
- Modern (Blue gradient)
- Minimal (Gray gradient)
- Bold (Orange gradient)
- Creative (Purple gradient)

### **All Industry Options**
- Tech Startup 🚀
- E-commerce 🛒
- Blog/Magazine 📝
- Portfolio 🎨
- Agency 🏢
- SaaS 💻

### **All Palette Options**
- Vibrant (Red, Teal, Yellow)
- Calm (Blue grays, Muted)
- Dark & Bold (Black, Red, White)
- Sunset (Orange, Gold, Yellow)
- Ocean (Deep Blue, Cyan)
- Forest (Green shades)

---

## **🐛 Common Issues**

### **Issue: "Continue" button stays disabled**
**Solution:** Verify at least 10 characters are entered (trim whitespace)

### **Issue: Selection doesn't highlight**
**Solution:** Clear browser cache and restart dev server

### **Issue: Redirect doesn't happen**
**Solution:** Verify `/dashboard/editor` page exists or is being built

### **Issue: Spinner won't stop**
**Solution:** Check browser console for errors, clear localStorage if needed

### **Issue: sessionStorage not persisting**
**Solution:** Verify browser allows sessionStorage (not in incognito in some cases)

---

## **🔍 Browser DevTools Checklist**

### **Console**
```
Check for errors: Should be clean or only warnings
Check for logs: Should show generation progress
```

### **Network Tab**
```
Watch for API calls when "Create My Website" is clicked
Should show mock call simulation
```

### **Application Tab (Storage)**
```
sessionStorage → generatedWebsite key
Should contain JSON with prompt, style, industry, palette
```

### **Elements Inspector**
```
Verify correct Tailwind classes are applied
Verify step container is visible
Verify buttons have correct state classes
```

---

## **✅ Full Test Run (5 minutes)**

```
1. Load page (http://localhost:3000/dashboard/create)
2. Type prompt (10+ chars)
3. Click "Continue to Style"
4. Select style
5. Click "Continue to Industry"
6. Select industry
7. Click "Continue to Colors"
8. Select palette
9. Verify all buttons enabled
10. Click "Create My Website"
11. Verify loading state
12. Wait for redirect to editor
13. Verify sessionStorage has data
14. Check responsive on mobile
15. Test back navigation
```

---

## **📝 Test Results Template**

```markdown
## Test Run: [DATE]

### Environment
- Browser: [Chrome/Safari/Firefox]
- OS: [macOS/Windows/Linux]
- Screen Size: [Desktop/Mobile/Tablet]
- Dev Server: [Running on port 3000]

### Results
- [ ] Prompt validation: ✅/❌
- [ ] Style selection: ✅/❌
- [ ] Industry selection: ✅/❌
- [ ] Palette selection: ✅/❌
- [ ] Generation flow: ✅/❌
- [ ] Back navigation: ✅/❌
- [ ] Progress bar: ✅/❌
- [ ] Responsive design: ✅/❌
- [ ] Redirect to editor: ✅/❌

### Issues Found
- [Issue 1]
- [Issue 2]

### Notes
- [Any observations]
```

---

## **🎯 Success Criteria**

- ✅ All form validations work
- ✅ Navigation between steps works
- ✅ All selections are visually indicated
- ✅ Generation completes and redirects
- ✅ Data persists to sessionStorage
- ✅ Responsive on all screen sizes
- ✅ No console errors
- ✅ All buttons are accessible

---

## **🚀 Ready to Test!**

The create page is fully functional and ready for comprehensive testing. Run through the test cases above to ensure everything works as expected.

**Status:** ✅ Ready for Testing

---

**Last Updated:** November 25, 2025
**File:** `/app/dashboard/create/page.tsx`
**Test Document:** `CREATE_PAGE_TESTING_GUIDE.md`
