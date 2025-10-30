# Debug Configuration Guide

This guide explains how to run all TechBirdsFly services in debugging mode from VS Code.

## Prerequisites

- ✅ .NET 8 SDK installed
- ✅ Node.js 18+ installed
- ✅ Docker & Docker Compose installed
- ✅ VS Code with C# and JavaScript debugger extensions
- ✅ All services built successfully

## Quick Start: Launch All Services in Debug Mode

### Option 1: Single Click (Recommended)

1. Open VS Code
2. Click on **Run and Debug** (Ctrl+Shift+D)
3. Select **"🔵 All .NET Services + Frontend"** from the dropdown
4. Click the **green play button**

This will:
- Start Docker observability stack (Seq + Jaeger)
- Build all 6 .NET services
- Launch all services in debug mode
- Start Next.js frontend
- Open integrated terminals for each service

### Option 2: Launch Individual Services

If you want to debug specific services:

1. Open **Run and Debug** (Ctrl+Shift+D)
2. Select any individual service:
   - `.NET Auth Service` (Port 5000-5001)
   - `.NET Billing Service` (Port 5001-5002)
   - `.NET Generator Service` (Port 5002-5003)
   - `.NET Admin Service` (Port 5003-5004)
   - `.NET Image Service` (Port 5004-5005)
   - `.NET User Service` (Port 5005-5006)
   - `Next.js Frontend` (Port 3000)

## Service Ports

| Service | HTTP | HTTPS | Status |
|---------|------|-------|--------|
| Auth Service | 5000 | 5001 | Debug Ready |
| Billing Service | 5001 | 5002 | Debug Ready |
| Generator Service | 5002 | 5003 | Debug Ready |
| Admin Service | 5003 | 5004 | Debug Ready |
| Image Service | 5004 | 5005 | Debug Ready |
| User Service | 5005 | 5006 | Debug Ready |
| Next.js Frontend | 3000 | - | Debug Ready |
| Seq (Logs) | 5341 | - | Internal |
| Jaeger (Traces) | 16686 | - | Internal |

## Debugging Features

### .NET Services

- **Breakpoints**: Click line numbers to set breakpoints
- **Step Through**: F10 (step over), F11 (step into), Shift+F11 (step out)
- **Variables**: Inspect locals and watch expressions
- **Call Stack**: View execution path
- **Exception Helper**: Debug with full exception context

### Next.js Frontend

- **Breakpoints**: Set in TypeScript/JavaScript code
- **Console**: View logs in VS Code debug console
- **Network**: Monitor API calls to backend services
- **Elements**: Use browser DevTools (F12)

### Cross-Service Debugging

- **Correlation IDs**: Track requests across all services via X-Correlation-ID header
- **Seq Logs**: View all service logs at http://localhost:5341
- **Jaeger Traces**: View distributed traces at http://localhost:16686

## Available Tasks

Run from Command Palette (Ctrl+Shift+P) → "Tasks: Run Task":

- **build-all-services**: Build entire solution
- **build-[service-name]**: Build specific service
- **start-observability-stack**: Start Docker services (Seq + Jaeger)
- **stop-observability-stack**: Stop Docker services
- **view-logs-seq**: Open Seq dashboard
- **view-logs-jaeger**: Open Jaeger dashboard

## Common Debugging Workflows

### Debug a Single Service

```
1. Ctrl+Shift+D
2. Select ".NET [Service Name]"
3. Click play button
4. Set breakpoints in code
5. Trigger API calls through frontend or Swagger
6. Debug in VS Code
```

### Debug Frontend + Backend Together

```
1. Launch composite configuration "All .NET Services + Frontend"
2. Set breakpoints in:
   - Frontend: src/app/api/*, src/lib/services/*
   - Backend: Controllers/*, Services/*
3. Use browser DevTools (F12) for frontend debugging
4. Use VS Code for backend debugging
5. Track requests via X-Correlation-ID in Seq
```

### Trace Cross-Service Calls

```
1. Run all services
2. Set breakpoints in Auth Service
3. Set breakpoints in called service
4. Make request from frontend
5. Observe execution path across services
6. View complete trace in Jaeger at http://localhost:16686
```

## Troubleshooting

### Port Already in Use

```bash
# Kill process on specific port (e.g., 5000)
lsof -ti:5000 | xargs kill -9
```

### Services Won't Start

1. Ensure Docker is running: `docker ps`
2. Build all services: Ctrl+Shift+P → "Tasks: Run Task" → "build-all-services"
3. Check logs in Seq: http://localhost:5341

### Breakpoints Not Hitting

1. Ensure code is built: Rebuild via Tasks menu
2. Check if process is paused correctly
3. Try "Debug → Restart" (Ctrl+Shift+F5)

### Next.js Frontend Not Updating

1. Check terminal for build errors
2. Refresh browser (Cmd+R or Ctrl+R)
3. Clear .next folder: `rm -rf web-frontend/techbirdsfly-frontend-nextjs/.next`
4. Restart frontend debug session

## Configuration Files

- **launch.json**: Debug configurations for all services
- **tasks.json**: Build and utility tasks
- **settings.json**: VS Code debug settings and formatters

## Performance Tips

- Launch only services you're debugging to reduce resource usage
- Use conditional breakpoints to avoid stopping too frequently
- Set logpoints (breakpoints that log) instead of regular breakpoints for performance-sensitive code
- Monitor memory in Debug Console: `process.memoryUsage()`

## Additional Resources

- [VS Code Debugging](https://code.visualstudio.com/docs/editor/debugging)
- [.NET Debugging](https://code.visualstudio.com/docs/languages/csharp#_debugger-support)
- [Node.js Debugging](https://nodejs.org/en/docs/guides/debugging-getting-started/)
- [Seq Documentation](https://docs.datalust.co/docs/)
- [Jaeger Documentation](https://www.jaegertracing.io/)

---

**Happy Debugging! 🚀**
