# 🚀 Debug Quick Reference Card

## ONE-CLICK DEBUG START

1. **Ctrl+Shift+D** (Run and Debug)
2. Select **"🔵 All .NET Services + Frontend"**
3. Click **▶️ Green Play Button**

**Done!** All 6 services + frontend debugging in one click.

---

## Keyboard Shortcuts

| Action | Windows/Linux | macOS |
|--------|---------------|-------|
| Run/Debug | Ctrl+Shift+D | Cmd+Shift+D |
| Start Debugging | F5 | F5 |
| Continue | F5 | F5 |
| Pause | Ctrl+Alt+Break | Cmd+Alt+Break |
| Stop | Shift+F5 | Shift+F5 |
| Restart | Ctrl+Shift+F5 | Cmd+Shift+F5 |
| Step Over | F10 | F10 |
| Step Into | F11 | F11 |
| Step Out | Shift+F11 | Shift+F11 |
| Toggle Breakpoint | Ctrl+B | Cmd+B |

---

## Service Ports @ a Glance

```
Auth Service       → localhost:5000 (HTTP)  / :5001 (HTTPS)
Billing Service    → localhost:5001 (HTTP)  / :5002 (HTTPS)
Generator Service  → localhost:5002 (HTTP)  / :5003 (HTTPS)
Admin Service      → localhost:5003 (HTTP)  / :5004 (HTTPS)
Image Service      → localhost:5004 (HTTP)  / :5005 (HTTPS)
User Service       → localhost:5005 (HTTP)  / :5006 (HTTPS)
Frontend           → localhost:3000 (Next.js)

Seq Logs           → localhost:5341 (Dashboard)
Jaeger Traces      → localhost:16686 (Dashboard)
```

---

## Run Individual Services

### Via VS Code Debugger
1. Ctrl+Shift+D
2. Select service name
3. Click ▶️

### Via Command Line
```bash
cd services/auth-service/src
dotnet run

cd services/billing-service/src/BillingService
dotnet run

cd services/generator-service/src
dotnet run

cd services/admin-service/src/AdminService
dotnet run

cd services/image-service/src/ImageService
dotnet run

cd services/user-service/src/UserService
dotnet run

cd web-frontend/techbirdsfly-frontend-nextjs
npm run dev
```

---

## Build All Services

**Ctrl+Shift+P** → "Tasks: Run Task" → "build-all-services"

Or from terminal:
```bash
cd /path/to/TechBirdsFly
dotnet build TechBirdsFly.sln
```

---

## Observability Dashboards

### Seq (Centralized Logs)
- **URL:** http://localhost:5341
- **Purpose:** View all structured logs from all 6 services
- **Features:** Full-text search, filtering, alerting

### Jaeger (Distributed Tracing)
- **URL:** http://localhost:16686
- **Purpose:** Trace requests across all services
- **Features:** Request timing, service calls, error tracking

---

## Debugging Workflows

### Debug & Hit Breakpoint

1. Set breakpoint (click line number)
2. Run composite config
3. Trigger action from frontend
4. Execution pauses at breakpoint
5. Inspect variables in left panel
6. Use F10/F11 to step

### Debug Cross-Service Call

1. Set breakpoint in Auth Service
2. Set breakpoint in called service
3. Run composite config
4. Make request from frontend
5. Auth Service hits breakpoint
6. Step into next service
7. View full call stack

### View Request Trace

1. Make API call from frontend
2. Open Jaeger: http://localhost:16686
3. Select service from dropdown
4. View timing for all services involved

### Search Logs

1. Open Seq: http://localhost:5341
2. Use search box for queries
3. Filter by correlation ID or service name
4. View complete request flow

---

## Common Tasks (Command Palette)

**Ctrl+Shift+P** then type:

- `Tasks: Run Task` → build-all-services
- `Tasks: Run Task` → start-observability-stack
- `Tasks: Run Task` → stop-observability-stack
- `Tasks: Run Task` → view-logs-seq
- `Tasks: Run Task` → view-logs-jaeger
- `Debug: Start Debugging` → Launch selected config
- `Debug: Stop` → Stop debugging session

---

## Troubleshooting

### Port in Use?
```bash
lsof -ti:5000 | xargs kill -9  # Kill port 5000
```

### Can't Debug Frontend?
1. Ensure Node.js installed: `node --version`
2. Refresh browser: Cmd+R / Ctrl+R
3. Check console for errors: F12

### Breakpoints Not Hitting?
1. Rebuild service: Ctrl+Shift+P → "build-[service]"
2. Restart debug: Shift+F5
3. Ensure debug build: Check bin/Debug/ folder

### Services Won't Start?
1. Check Docker: `docker ps`
2. View error logs: Ctrl+Shift+P → "view-logs-seq"
3. Rebuild all: Ctrl+Shift+P → "build-all-services"

---

## Configuration Files

All settings in `.vscode/` folder:

- **launch.json** - All debug configurations
- **tasks.json** - Build and utility tasks
- **settings.json** - VS Code preferences
- **DEBUG_GUIDE.md** - Full documentation
- **LAUNCH_CONFIG_SUMMARY.md** - Setup details

---

## Supported Debugging

✅ C# (.NET 8) - Full support
✅ TypeScript - Full support
✅ JavaScript - Full support
✅ Breakpoints - All languages
✅ Step debugging - All languages
✅ Variables inspection - All languages
✅ Call stacks - All languages
✅ Conditionals - All languages

---

## Need Help?

- **DEBUG_GUIDE.md** - Comprehensive guide with examples
- **LAUNCH_CONFIG_SUMMARY.md** - Setup reference
- VS Code Help: Ctrl+Shift+P → "Help: Welcome"

---

**Debug Status: ✅ READY**

*Last Updated: October 30, 2025*
