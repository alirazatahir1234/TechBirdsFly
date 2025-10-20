# React Dashboard - Quick Start

## ⚡ 2-Minute Setup

### Prerequisites
- Node.js 16+
- YARP Gateway running on port 5000
- All microservices running

### Install & Run

```bash
# Navigate to frontend
cd web-frontend/techbirdsfly-frontend

# Install dependencies
npm install

# Start development server
npm start
```

Browser opens at `http://localhost:3000`

## 🧪 Quick Test

### 1. Register Account
```
URL: http://localhost:3000/register
Enter:
- Full Name: Test User
- Email: test@example.com
- Password: Test123!
- Confirm: Test123!
Click: Sign Up
Result: "Account Created!" → Redirect to login
```

### 2. Login
```
URL: http://localhost:3000/login
Enter:
- Email: test@example.com
- Password: Test123!
Click: Sign In
Result: Redirected to dashboard
```

### 3. Dashboard Overview
```
You should see:
- User profile card with subscription
- Statistics (total images, spending, etc.)
- Quick action buttons
- Getting started guide
```

### 4. Generate Image
```
Click: "Generate Image" button
Or navigate to: http://localhost:3000/images

Enter prompt: "A beautiful sunset over mountains"
Select size: 1024x1024
Select quality: Standard
Click: "Generate Image"
Wait: ~2-5 seconds
See: New image in gallery
```

### 5. View & Delete
```
Hover over image in gallery
See: Prompt, size, cost, creation date
Click: Delete button (trash icon)
Confirm: Yes
Result: Image removed from gallery
```

### 6. Logout
```
Header: Click user avatar or logout button
Result: Redirected to login page
```

## 📚 File Structure

```
src/
├── api/client.ts              - API communication with YARP
├── context/AuthContext.tsx    - Global auth state
├── components/                - Reusable components
│   ├── LoginForm.tsx
│   ├── RegisterForm.tsx
│   ├── DashboardHeader.tsx
│   ├── ProfileCard.tsx
│   ├── GenerateImageForm.tsx
│   └── ImageGallery.tsx
├── pages/                     - Full pages
│   ├── DashboardPage.tsx
│   └── ImagesPage.tsx
├── types/index.ts             - TypeScript definitions
├── App.tsx                    - Router & auth logic
└── index.tsx                  - Entry point
```

## 🔗 API Routes

| Action | Endpoint | Service |
|--------|----------|---------|
| Register | POST /api/auth/register | Auth (5001) |
| Login | POST /api/auth/login | Auth (5001) |
| Get Profile | GET /api/users/me | User (5008) |
| Generate Image | POST /api/images/generate | Image (5007) |
| List Images | GET /api/images/list | Image (5007) |
| Delete Image | DELETE /api/images/{id} | Image (5007) |
| Image Stats | GET /api/images/stats/summary | Image (5007) |

**All requests flow through YARP Gateway (5000)**

## 🐛 Troubleshooting

### "npm install" fails
```bash
# Clear cache and retry
npm cache clean --force
npm install
```

### "Cannot reach API"
```bash
# Check gateway is running
cd gateway/yarp-gateway/src
dotnet run --urls http://localhost:5000

# In another terminal, check it's working
curl http://localhost:5000/health
```

### Blank page at localhost:3000
```bash
# Check browser console for errors (F12)
# Ensure all services are running
# Try hard refresh: Cmd+Shift+R (Mac) or Ctrl+Shift+R (Windows)
```

### Login doesn't work
```bash
# Check Auth Service is running on 5001
curl http://localhost:5001/swagger

# Verify YARP is routing correctly
curl http://localhost:5000/api/auth/login -X POST
```

### Images won't generate
```bash
# Check Image Service running on 5007
curl http://localhost:5007/health

# Check User Service running on 5008
curl http://localhost:5008/health

# Check rate limits aren't exceeded
# Wait 60 seconds and try again
```

## 🚀 Production Build

```bash
# Create optimized build
npm run build

# Serve locally to test
npm install -g serve
serve -s build -l 3000
```

## 📖 Documentation

- **Full Documentation**: [PHASE3_3_DASHBOARD_IMPLEMENTATION.md](PHASE3_3_DASHBOARD_IMPLEMENTATION.md)
- **Dashboard README**: [techbirdsfly-frontend/README.md](techbirdsfly-frontend/README.md)
- **API Documentation**: Access Swagger at http://localhost:5000/swagger

## ✅ Checklist

- [ ] Node.js 16+ installed
- [ ] npm install completed
- [ ] All services running
- [ ] YARP Gateway running on 5000
- [ ] http://localhost:3000 opens in browser
- [ ] Can register new account
- [ ] Can login with credentials
- [ ] Dashboard displays user data
- [ ] Can generate images
- [ ] Gallery shows generated images
- [ ] Can delete images
- [ ] Can logout

## 🎯 Next Steps

1. **Customize**: Edit components to match your branding
2. **Test**: Generate images and verify gallery updates
3. **Deploy**: Push to GitHub and deploy via CI/CD
4. **Monitor**: Check logs and metrics in production
5. **Iterate**: Collect user feedback and enhance features

---

**Ready to build? Start with:**
```bash
cd web-frontend/techbirdsfly-frontend && npm install && npm start
```
