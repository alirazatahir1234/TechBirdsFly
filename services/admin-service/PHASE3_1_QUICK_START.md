# Phase 3.1 Quick Start Guide: WebSocket Real-Time Monitoring

## Overview

This guide walks you through setting up and testing the real-time WebSocket infrastructure for the Admin Service.

## Prerequisites

- .NET 8 SDK installed
- Visual Studio Code or Visual Studio
- Postman or REST Client extension for testing
- Node.js (for JavaScript client examples)

## Setup

### 1. Build the Project

```bash
cd services/admin-service/src/AdminService
dotnet build
```

**Expected Output**:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### 2. Run the Admin Service

```bash
# From AdminService directory
dotnet run
```

**Expected Console Output**:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5006
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to stop.
```

## Testing Endpoints

### Quick Health Check

```bash
curl http://localhost:5006/api/admin/realtime/health
```

**Expected Response**:
```json
{
  "status": "healthy",
  "service": "real-time",
  "timestamp": "2025-10-17T10:30:00Z",
  "websocketEndpoint": "/hubs/admin"
}
```

### Test with REST Client (VS Code)

Open `AdminService.http` and use the real-time section:

1. Click "Send Request" on any test
2. Check the response in the sidebar
3. Examples provided for all endpoints

### Example: Broadcast Platform Update

```
POST http://localhost:5006/api/admin/realtime/broadcast/platform-update
Authorization: Bearer {your-jwt-token}
```

Response:
```json
{
  "message": "Platform update broadcasted"
}
```

## WebSocket Client Setup

### Option 1: Using VS Code REST Client with JavaScript

Create `test-realtime.js`:

```javascript
import * as signalR from '@microsoft/signalr';

async function connectToHub() {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5006/hubs/admin", {
      accessTokenFactory: () => "YOUR_JWT_TOKEN"
    })
    .withAutomaticReconnect([0, 2000, 10000, 30000])
    .build();

  connection.on("ConnectionEstablished", (msg) => {
    console.log("✓ Connected:", msg);
  });

  connection.on("AnalyticsUpdate", (data) => {
    console.log("📊 Analytics Update:", data);
  });

  connection.on("SystemAlert", (data) => {
    console.log("⚠️  Alert:", data);
  });

  connection.onreconnecting(() => {
    console.log("🔄 Reconnecting...");
  });

  connection.onreconnected(() => {
    console.log("✓ Reconnected!");
  });

  try {
    await connection.start();
    console.log("✓ Connected to real-time hub");

    // Subscribe to updates
    await connection.invoke("SubscribeToAnalytics");
    await connection.invoke("SubscribeToAlerts");
    console.log("✓ Subscribed to analytics and alerts");
  } catch (error) {
    console.error("✗ Connection failed:", error);
  }
}

connectToHub();
```

### Option 2: Browser Console

```javascript
// In your React admin dashboard
const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5006/hubs/admin", {
    accessTokenFactory: () => localStorage.getItem('token')
  })
  .withAutomaticReconnect()
  .build();

connection.on("AnalyticsUpdate", (data) => {
  console.log("Real-time data:", data);
  // Update UI with data
});

await connection.start();
await connection.invoke("SubscribeToAnalytics");
```

## Common Tasks

### Subscribe to User Activity

```javascript
// Client side
await connection.invoke("SubscribeToUserActivity");

connection.on("UserActivityUpdate", (data) => {
  console.log("User Activity:", data);
  // { timestamp, userId, action, details }
});

// Or trigger from API
// POST /api/admin/realtime/notify/user-action
// { userId: "...", action: "website_generated", details: {...} }
```

### Send an Alert

```bash
curl -X POST http://localhost:5006/api/admin/realtime/alert \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "type": "warning",
    "message": "High CPU usage detected",
    "data": { "cpuUsage": 92 }
  }'
```

**Clients listening to alerts will receive**:
```javascript
connection.on("SystemAlert", (data) => {
  // { 
  //   type: "warning", 
  //   message: "High CPU usage detected", 
  //   timestamp: "2025-10-17T...", 
  //   data: { cpuUsage: 92 }
  // }
});
```

### Get Platform Snapshot

```bash
curl http://localhost:5006/api/admin/realtime/platform-snapshot \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

Response:
```json
{
  "timestamp": "2025-10-17T10:30:00Z",
  "data": {
    "totalUsers": 5432,
    "activeUsers": 234,
    "websitesGenerated": 1203,
    "imagesGenerated": 3847,
    "failedGenerations": 5,
    "totalRevenue": 98765.43,
    "revenueToday": 1234.56
  }
}
```

## Architecture Overview

```
┌─────────────┐
│   Browser   │
│  Dashboard  │
└──────┬──────┘
       │ WebSocket
       │ /hubs/admin
       ↓
┌─────────────────────┐
│   AdminHub (Server) │
├─────────────────────┤
│ Groups:             │
│ - admins (auto)     │
│ - user-activity     │
│ - analytics         │
│ - alerts            │
└─────┬───────────────┘
      ↑
      │ (IHubContext)
      │
┌─────────────────────┐
│ RealTimeService     │
├─────────────────────┤
│ Broadcast Methods   │
│ - User Activity     │
│ - Analytics         │
│ - Alerts            │
│ - Notifications     │
└─────┬───────────────┘
      ↑
      │ (Injected)
┌─────────────────────┐
│ Controllers/Services│
│ - RealTimeController│
│ - Analytics Service │
│ - User Service      │
└─────────────────────┘
```

## Troubleshooting

### Connection Refused
```
Error: System.Net.Http.HttpRequestException: No connection could be made
```

**Solution**: Ensure Admin Service is running on port 5006
```bash
lsof -i :5006  # Check if port is in use
```

### 401 Unauthorized
```
Error: 401 Unauthorized
```

**Solution**: Provide valid JWT token in Authorization header
```bash
curl -H "Authorization: Bearer YOUR_VALID_TOKEN" ...
```

### WebSocket Upgrade Failed
```
WebSocket connection failed: error during WebSocket handshake: ...
```

**Solution**: Check browser console for errors, verify CORS settings

### Messages Not Received
1. Verify client is subscribed to the correct group
   ```javascript
   await connection.invoke("SubscribeToAnalytics");
   ```
2. Check that broadcast is being sent
   ```bash
   curl -X POST http://localhost:5006/api/admin/realtime/broadcast/platform-update \
     -H "Authorization: Bearer TOKEN"
   ```
3. Check browser console for JavaScript errors

## Files Reference

| File | Purpose | Location |
|------|---------|----------|
| AdminHub.cs | WebSocket hub implementation | `/Hubs/` |
| IRealTimeService.cs | Broadcast management | `/Services/` |
| RealTimeController.cs | REST endpoints | `/Controllers/` |
| AdminService.http | Test examples | Project root |
| REALTIME_API.md | Complete API documentation | Service docs |
| PHASE3_1_COMPLETION.md | Implementation details | Service docs |

## Next Steps

After verifying real-time communication:

1. **Phase 3.2**: Implement React Admin Dashboard UI
2. **Phase 3.3**: Add Advanced Reporting System
3. **Phase 3.4**: Implement Security Enhancements

## Resources

- [SignalR Documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr)
- [REALTIME_API.md](./REALTIME_API.md) - Complete API reference
- [PHASE3_1_COMPLETION.md](./PHASE3_1_COMPLETION.md) - Implementation details

## Support

For issues or questions:
1. Check `REALTIME_API.md` for API documentation
2. Review `AdminService.http` for testing examples
3. Check `PHASE3_1_COMPLETION.md` for architecture details
4. Review application logs for error details

---

**Ready to start?** Run `dotnet run` and test your first WebSocket connection! 🚀
