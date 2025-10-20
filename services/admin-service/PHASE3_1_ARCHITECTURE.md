# Phase 3.1 Architecture Diagram

## System Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          ADMIN DASHBOARD (React)                            │
│                                                                              │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐          │
│  │   Metrics View   │  │  Activity Feed   │  │  Alert Center    │          │
│  └────────┬─────────┘  └────────┬─────────┘  └────────┬─────────┘          │
│           │                     │                     │                     │
│           └─────────┬───────────┴─────────────────────┘                     │
│                     │                                                        │
│         WebSocket Client Connection Manager                                │
│         (signalR.HubConnectionBuilder)                                     │
│                     │                                                        │
└─────────────────────┼────────────────────────────────────────────────────────┘
                      │
                      │ WebSocket Connection
                      │ ws://localhost:5006/hubs/admin
                      │ Bearer Token (JWT)
                      ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│                        ADMIN SERVICE (Backend)                               │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                           AdminHub (SignalR)                        │   │
│  │                                                                      │   │
│  │  OnConnectedAsync() → Add to "admins" group                        │   │
│  │                                                                      │   │
│  │  ┌──────────────────────────────────────────────────────────────┐  │   │
│  │  │                    Subscription Groups                        │  │   │
│  │  │                                                               │  │   │
│  │  │  Group: "admins"          Group: "user-activity"            │  │   │
│  │  │  ├─ Client 1              ├─ Client 2                       │  │   │
│  │  │  ├─ Client 2              ├─ Client 5                       │  │   │
│  │  │  └─ Client 3              └─ Client 8                       │  │   │
│  │  │                                                               │  │   │
│  │  │  Group: "analytics"       Group: "alerts"                   │  │   │
│  │  │  ├─ Client 1              ├─ Client 1                       │  │   │
│  │  │  ├─ Client 3              ├─ Client 4                       │  │   │
│  │  │  ├─ Client 4              └─ Client 7                       │  │   │
│  │  │  └─ Client 6                                                 │  │   │
│  │  │                                                               │  │   │
│  │  └──────────────────────────────────────────────────────────────┘  │   │
│  │                                                                      │   │
│  │  Hub Methods:                                                        │   │
│  │  ├─ SubscribeToUserActivity()                                       │   │
│  │  ├─ SubscribeToAnalytics()                                          │   │
│  │  ├─ SubscribeToAlerts()                                             │   │
│  │  ├─ BroadcastUserCountUpdate(totalUsers, activeUsers, newToday)   │   │
│  │  ├─ BroadcastAnalyticsUpdate(data)                                 │   │
│  │  ├─ BroadcastGenerationMetrics(websites, images, failed, time)    │   │
│  │  ├─ BroadcastRevenueUpdate(dailyRevenue, total30d)                │   │
│  │  ├─ NotifyUserAction(userId, action, details)                     │   │
│  │  ├─ NotifyRoleAssignment(userId, roleName, action)                │   │
│  │  ├─ NotifySystemEvent(eventType, description, data)               │   │
│  │  ├─ SendSystemAlert(type, message, data)                          │   │
│  │  ├─ SendWarning(message, data)                                    │   │
│  │  ├─ SendError(message, data)                                      │   │
│  │  └─ GetPlatformStats()                                             │   │
│  │                                                                      │   │
│  └──────────────────────────────────┬───────────────────────────────────┘   │
│                                      │                                       │
│                    IHubContext<AdminHub>                                    │
│                           ↓                                                  │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                        RealTimeService                              │   │
│  │                                                                      │   │
│  │  public class RealTimeService : IRealTimeService {                 │   │
│  │    ├─ BroadcastUserActivityAsync(data)                             │   │
│  │    │  → _hubContext.Clients.Group("user-activity").SendAsync()    │   │
│  │    │                                                                │   │
│  │    ├─ BroadcastAnalyticsUpdateAsync(data)                          │   │
│  │    │  → _hubContext.Clients.Group("analytics").SendAsync()        │   │
│  │    │                                                                │   │
│  │    ├─ BroadcastUserCountUpdateAsync(...)                           │   │
│  │    │  → _hubContext.Clients.Group("analytics").SendAsync()        │   │
│  │    │                                                                │   │
│  │    ├─ BroadcastGenerationMetricsAsync(...)                         │   │
│  │    │  → _hubContext.Clients.Group("analytics").SendAsync()        │   │
│  │    │                                                                │   │
│  │    ├─ BroadcastRevenueUpdateAsync(daily, total)                    │   │
│  │    │  → _hubContext.Clients.Group("analytics").SendAsync()        │   │
│  │    │                                                                │   │
│  │    ├─ NotifyUserActionAsync(userId, action, details)              │   │
│  │    │  → _hubContext.Clients.Group("user-activity").SendAsync()    │   │
│  │    │                                                                │   │
│  │    ├─ NotifyRoleAssignmentAsync(userId, roleName, action)         │   │
│  │    │  → _hubContext.Clients.Group("admins").SendAsync()           │   │
│  │    │                                                                │   │
│  │    ├─ NotifySystemEventAsync(eventType, description, data)        │   │
│  │    │  → _hubContext.Clients.Group("admins").SendAsync()           │   │
│  │    │                                                                │   │
│  │    ├─ SendAlertAsync(type, message, data)                          │   │
│  │    │  → _hubContext.Clients.Group("alerts").SendAsync()           │   │
│  │    │                                                                │   │
│  │    └─ SendPerformanceAlertAsync(metric, value, threshold)         │   │
│  │       → _hubContext.Clients.Group("alerts").SendAsync()           │   │
│  │  }                                                                   │   │
│  │                                                                      │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                      │                                       │
│                           Injected in Controllers                            │
│                                      │                                       │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                      RealTimeController                             │   │
│  │                                                                      │   │
│  │  Endpoints:                                                          │   │
│  │  ├─ GET  /api/admin/realtime/health                                │   │
│  │  ├─ GET  /api/admin/realtime/status                               │   │
│  │  ├─ GET  /api/admin/realtime/platform-snapshot                    │   │
│  │  ├─ POST /api/admin/realtime/broadcast/platform-update            │   │
│  │  ├─ POST /api/admin/realtime/broadcast/user-count                 │   │
│  │  ├─ POST /api/admin/realtime/broadcast/generation-metrics         │   │
│  │  ├─ POST /api/admin/realtime/broadcast/revenue                    │   │
│  │  ├─ POST /api/admin/realtime/alert                                │   │
│  │  ├─ POST /api/admin/realtime/alert/warning                        │   │
│  │  ├─ POST /api/admin/realtime/alert/error                          │   │
│  │  ├─ POST /api/admin/realtime/alert/performance                    │   │
│  │  ├─ POST /api/admin/realtime/notify/user-action                   │   │
│  │  ├─ POST /api/admin/realtime/notify/role-assignment               │   │
│  │  └─ POST /api/admin/realtime/notify/system-event                  │   │
│  │                                                                      │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                    Dependent Services                               │   │
│  │                                                                      │   │
│  │  ├─ IUserManagementService                                          │   │
│  │  │  └─ Used for user data in notifications                         │   │
│  │  │                                                                   │   │
│  │  ├─ IAnalyticsService                                               │   │
│  │  │  └─ Used for platform stats broadcasting                        │   │
│  │  │                                                                   │   │
│  │  └─ ILogger<T>                                                      │   │
│  │     └─ Used for comprehensive logging                              │   │
│  │                                                                      │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘

                            Configuration (Program.cs)
                            ├─ builder.Services.AddSignalR()
                            ├─ builder.Services.AddScoped<IRealTimeService, RealTimeService>()
                            └─ app.MapHub<AdminHub>("/hubs/admin")
```

## Message Flow Example: Broadcasting Analytics Update

```
1. Backend Service / Controller
   └─ Call: await realTimeService.BroadcastAnalyticsUpdateAsync(data)
                         ↓
2. RealTimeService
   └─ await _hubContext.Clients.Group("analytics").SendAsync("AnalyticsUpdate", data)
                         ↓
3. AdminHub (via IHubContext)
   └─ Find all clients in "analytics" group
                         ↓
4. WebSocket Protocol
   └─ Send message to all connected clients in group
                         ↓
5. Browser Client (SignalR Client)
   ├─ Receive message
   ├─ Trigger: connection.on("AnalyticsUpdate", handler)
   └─ Update React component state
                         ↓
6. React Component
   └─ Re-render with new data
        └─ Update dashboard metrics/charts
```

## Client Subscription Flow

```
Client Connection
         │
         ├─ WebSocket connection established
         │
         ├─ Auto-added to "admins" group
         │
         ├─ User subscribes to "analytics" group
         │   └─ await connection.invoke("SubscribeToAnalytics")
         │
         ├─ Server adds client to group
         │   └─ await Groups.AddToGroupAsync(connectionId, "analytics")
         │
         ├─ Client now receives broadcasts to "analytics" group
         │   └─ connection.on("AnalyticsUpdate", (data) => {...})
         │
         └─ Client can unsubscribe
             └─ await connection.invoke("UnsubscribeFromAnalytics")
```

## Deployment Architecture (Future)

```
┌────────────────────────────────────────────────────────────┐
│                        Load Balancer                       │
└────────────────────────────────────────────────────────────┘
           │                           │
           ↓                           ↓
┌──────────────────────┐    ┌──────────────────────┐
│   Admin Service 1    │    │   Admin Service 2    │
│  :5006 /hubs/admin   │    │  :5007 /hubs/admin   │
└──────────────────────┘    └──────────────────────┘
           │                           │
           ├───────────┬───────────────┤
           │           │               │
           ↓           ↓               ↓
      ┌────────────────────────────────────┐
      │      Redis Backplane (Future)      │
      │   (For multi-server coordination)  │
      └────────────────────────────────────┘
           │
           ↓
      ┌──────────────┐
      │  Shared DB   │
      └──────────────┘
```

## Technology Stack

```
┌─────────────────────────┐
│   WebSocket Layer       │
├─────────────────────────┤
│  ASP.NET Core SignalR   │
│  (Microsoft.AspNetCore  │
│   .SignalR.Core v1.1.0) │
└────────┬────────────────┘
         │
┌────────▼─────────────────┐
│   Transport Layer        │
├──────────────────────────┤
│  WebSocket Protocol      │
│  (TCP + HTTP Upgrade)    │
└────────┬─────────────────┘
         │
┌────────▼──────────────────┐
│   Authentication          │
├───────────────────────────┤
│  JWT Bearer Tokens        │
│  (via existing Auth Svc)  │
└────────┬──────────────────┘
         │
┌────────▼──────────────────┐
│   Data Serialization      │
├───────────────────────────┤
│  JSON                     │
└───────────────────────────┘
```

---

**Architecture Diagram for Phase 3.1**
Complete real-time monitoring infrastructure with WebSocket support, group-based messaging, and REST API integration.
