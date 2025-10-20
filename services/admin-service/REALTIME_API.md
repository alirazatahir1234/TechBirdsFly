# Real-Time API Documentation (Phase 3.1)

## Overview

The Real-Time API provides WebSocket-based bidirectional communication between the admin dashboard and the backend services. Built on ASP.NET Core SignalR, it enables live updates, notifications, and monitoring without polling.

## WebSocket Endpoint

**Base URL**: `ws://localhost:5006/hubs/admin`  
**Protocol**: WebSocket over HTTP/HTTPS  
**Authentication**: JWT Bearer Token (required)

### Connection

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5006/hubs/admin", {
        accessTokenFactory: () => localStorage.getItem('token')
    })
    .withAutomaticReconnect([0, 2000, 10000, 30000])
    .build();

connection.on("ConnectionEstablished", (msg) => {
    console.log("Connected:", msg);
});

await connection.start();
```

## Subscription Groups

Clients can subscribe to different groups to receive targeted updates:

### User Activity Group
**Name**: `user-activity`  
**Purpose**: Real-time user actions and events

**Subscribe**:
```javascript
await connection.invoke("SubscribeToUserActivity");
```

**Listen**:
```javascript
connection.on("UserActivityUpdate", (data) => {
    console.log("User Action:", data);
    // {
    //   timestamp: "2025-10-17T10:30:00Z",
    //   userId: "550e8400-e29b-41d4-a716-446655440000",
    //   action: "website_generated",
    //   details: {...}
    // }
});
```

### Analytics Group
**Name**: `analytics`  
**Purpose**: Platform metrics and analytics updates

**Subscribe**:
```javascript
await connection.invoke("SubscribeToAnalytics");
```

**Listen**:
```javascript
connection.on("AnalyticsUpdate", (data) => {
    console.log("Analytics:", data);
    // {
    //   timestamp: "2025-10-17T10:30:00Z",
    //   totalUsers: 5432,
    //   activeUsers: 234,
    //   websitesGenerated: 1203,
    //   imagesGenerated: 3847,
    //   revenue: 12345.67
    // }
});

connection.on("UserCountUpdate", (data) => {
    console.log("User Metrics:", data);
    // { totalUsers: 5432, activeUsers: 234, newUsersToday: 12 }
});
```

### Alerts Group
**Name**: `alerts`  
**Purpose**: System alerts and warnings

**Subscribe**:
```javascript
await connection.invoke("SubscribeToAlerts");
```

**Listen**:
```javascript
connection.on("SystemAlert", (data) => {
    console.log("Alert:", data);
    // { type: "warning", message: "...", severity: "high", timestamp: "..." }
});
```

### Admin Group (Auto-joined)
**Name**: `admins`  
**Purpose**: Admin-only notifications

All connected admin users are automatically added to this group.

**Listen**:
```javascript
connection.on("AdminNotification", (data) => {
    console.log("Admin Notification:", data);
});
```

## REST Endpoints for Broadcasting

All endpoints require `Admin` role authorization.

### Health & Status

#### Real-Time Service Health Check
```
GET /api/admin/realtime/health
```

**Response**:
```json
{
  "status": "healthy",
  "service": "real-time",
  "timestamp": "2025-10-17T10:30:00Z",
  "websocketEndpoint": "/hubs/admin"
}
```

#### Connection Status
```
GET /api/admin/realtime/status
```

**Response**:
```json
{
  "status": "connected",
  "timestamp": "2025-10-17T10:30:00Z",
  "message": "Real-time service is operational"
}
```

### Platform Data

#### Get Platform Snapshot
```
GET /api/admin/realtime/platform-snapshot
```

**Response**:
```json
{
  "timestamp": "2025-10-17T10:30:00Z",
  "data": {
    "totalUsers": 5432,
    "activeUsers": 234,
    "newUsersToday": 12,
    "websitesGenerated": 1203,
    "imagesGenerated": 3847,
    "failedGenerations": 5,
    "totalRevenue": 98765.43,
    "revenueToday": 1234.56
  }
}
```

### Broadcasting Endpoints

#### Broadcast Platform Update
```
POST /api/admin/realtime/broadcast/platform-update
Authorization: Bearer {token}
```

Broadcasts current platform statistics to all connected clients in `analytics` group.

**Response**:
```json
{
  "message": "Platform update broadcasted"
}
```

#### Broadcast User Count Update
```
POST /api/admin/realtime/broadcast/user-count
Authorization: Bearer {token}
```

Broadcasts user statistics update to `analytics` group.

**Response**:
```json
{
  "message": "User count update broadcasted",
  "totalUsers": 5432,
  "activeUsers": 234,
  "newUsersToday": 12
}
```

#### Broadcast Generation Metrics
```
POST /api/admin/realtime/broadcast/generation-metrics
Authorization: Bearer {token}
Content-Type: application/json

{
  "websitesGenerated": 125,
  "imagesGenerated": 342,
  "failedGenerations": 5,
  "averageGenerationTime": 4.2
}
```

Broadcasts website/image generation metrics to `analytics` group.

**Response**:
```json
{
  "message": "Generation metrics broadcasted"
}
```

#### Broadcast Revenue Update
```
POST /api/admin/realtime/broadcast/revenue
Authorization: Bearer {token}
Content-Type: application/json

{
  "dailyRevenue": 1250.75,
  "totalRevenue30d": 35420.50
}
```

Broadcasts revenue data to `analytics` group.

**Response**:
```json
{
  "message": "Revenue update broadcasted"
}
```

### Alert Endpoints

#### Send Generic Alert
```
POST /api/admin/realtime/alert
Authorization: Bearer {token}
Content-Type: application/json

{
  "type": "info",
  "message": "Test alert from API",
  "data": {
    "timestamp": "2025-10-17T10:30:00Z",
    "severity": "low"
  }
}
```

**Alert Types**: `info`, `warning`, `error`, `success`

**Response**:
```json
{
  "message": "Alert sent successfully"
}
```

#### Send Warning Alert
```
POST /api/admin/realtime/alert/warning
Authorization: Bearer {token}
Content-Type: application/json

{
  "message": "System performance degradation detected",
  "data": {
    "cpuUsage": 92,
    "memoryUsage": 88,
    "threshold": 85
  }
}
```

#### Send Error Alert
```
POST /api/admin/realtime/alert/error
Authorization: Bearer {token}
Content-Type: application/json

{
  "message": "API rate limit exceeded for multiple users",
  "data": {
    "affectedUsers": 3,
    "limitExceeded": "1000 req/hour"
  }
}
```

#### Send Performance Alert
```
POST /api/admin/realtime/alert/performance
Authorization: Bearer {token}
Content-Type: application/json

{
  "metric": "ApiResponseTime",
  "value": 2500,
  "threshold": "2000ms"
}
```

### Notification Endpoints

#### Notify User Action
```
POST /api/admin/realtime/notify/user-action
Authorization: Bearer {token}
Content-Type: application/json

{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "action": "website_generated",
  "details": {
    "projectId": "proj-12345",
    "templateId": "tmpl-789",
    "generationTime": 3.5
  }
}
```

Notifies subscribed clients of specific user actions.

#### Notify Role Assignment
```
POST /api/admin/realtime/notify/role-assignment
Authorization: Bearer {token}
Content-Type: application/json

{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "roleName": "Moderator",
  "action": "assigned"
}
```

**Action**: `assigned` or `revoked`

#### Notify System Event
```
POST /api/admin/realtime/notify/system-event
Authorization: Bearer {token}
Content-Type: application/json

{
  "eventType": "database_maintenance",
  "description": "Scheduled maintenance completed successfully",
  "data": {
    "duration": "15 minutes",
    "affectedServices": ["auth-service", "generator-service"],
    "status": "completed"
  }
}
```

## Hub Methods (Client-side invocation)

### Subscription Management

#### SubscribeToUserActivity
Subscribe to user activity updates.
```javascript
await connection.invoke("SubscribeToUserActivity");
```

#### SubscribeToAnalytics
Subscribe to analytics updates.
```javascript
await connection.invoke("SubscribeToAnalytics");
```

#### SubscribeToAlerts
Subscribe to system alerts.
```javascript
await connection.invoke("SubscribeToAlerts");
```

#### UnsubscribeFromUserActivity
Unsubscribe from user activity updates.
```javascript
await connection.invoke("UnsubscribeFromUserActivity");
```

#### UnsubscribeFromAnalytics
Unsubscribe from analytics updates.
```javascript
await connection.invoke("UnsubscribeFromAnalytics");
```

#### UnsubscribeFromAlerts
Unsubscribe from alert updates.
```javascript
await connection.invoke("UnsubscribeFromAlerts");
```

### Data Retrieval

#### GetPlatformStats
Get current platform statistics (immediate snapshot).
```javascript
const stats = await connection.invoke("GetPlatformStats");
// { totalUsers: 5432, activeUsers: 234, ... }
```

#### GetConnectionCount
Get current connection information.
```javascript
const info = await connection.invoke("GetConnectionCount");
// { connectionId: "...", timestamp: "...", ...}
```

#### SendTestMessage
Send test message for connectivity verification.
```javascript
const response = await connection.invoke("SendTestMessage", "Test payload");
// { timestamp: "...", message: "Test received" }
```

## Client Implementation Example

```javascript
// React component example
import React, { useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';

function AdminDashboard() {
  const [connection, setConnection] = useState(null);
  const [analytics, setAnalytics] = useState(null);
  const [alerts, setAlerts] = useState([]);

  useEffect(() => {
    // Create connection
    const conn = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5006/hubs/admin", {
        accessTokenFactory: () => localStorage.getItem('token')
      })
      .withAutomaticReconnect([0, 2000, 10000, 30000])
      .build();

    // Set up event listeners
    conn.on("ConnectionEstablished", (msg) => {
      console.log("Connected:", msg);
    });

    conn.on("AnalyticsUpdate", (data) => {
      setAnalytics(data);
    });

    conn.on("SystemAlert", (data) => {
      setAlerts(prev => [data, ...prev].slice(0, 10));
    });

    conn.onreconnecting(() => {
      console.log("Attempting to reconnect...");
    });

    conn.onreconnected(() => {
      console.log("Reconnected to real-time service");
    });

    // Start connection
    conn.start()
      .then(() => {
        console.log("Connected to real-time hub");
        // Subscribe to updates
        conn.invoke("SubscribeToAnalytics");
        conn.invoke("SubscribeToAlerts");
      })
      .catch(err => console.error("Connection error:", err));

    setConnection(conn);

    return () => {
      if (conn) {
        conn.stop();
      }
    };
  }, []);

  return (
    <div>
      <h1>Admin Dashboard</h1>
      {analytics && (
        <div>
          <p>Active Users: {analytics.activeUsers}</p>
          <p>Total Users: {analytics.totalUsers}</p>
          <p>Revenue: ${analytics.revenue}</p>
        </div>
      )}
      <div>
        <h2>Recent Alerts</h2>
        {alerts.map((alert, i) => (
          <div key={i} className={`alert alert-${alert.type}`}>
            {alert.message}
          </div>
        ))}
      </div>
    </div>
  );
}

export default AdminDashboard;
```

## Error Handling

### Hub Errors
```javascript
connection.on("ReceiveMessage", (message) => {
  // Handle message
});

connection.on("ErrorOccurred", (error) => {
  console.error("Hub error:", error);
  // { type: "error", message: "...", timestamp: "..." }
});
```

### Connection Errors
```javascript
connection.onreconnecting((error) => {
  console.error("Connection lost:", error);
  // Automatically attempt to reconnect
});

connection.onreconnected(() => {
  console.log("Reconnected successfully");
  // Re-subscribe to groups
});
```

## Security Considerations

1. **Authentication Required**: All WebSocket connections require a valid JWT bearer token
2. **Authorization**: Broadcasting endpoints require `Admin` role
3. **HTTPS/WSS**: Use WSS (WebSocket Secure) in production
4. **Rate Limiting**: Consider implementing rate limiting for frequent broadcasts
5. **Token Refresh**: Implement token refresh mechanism for long-lived connections

## Scaling Considerations

- **Backplane**: For multi-server deployments, implement SignalR backplane (Redis recommended)
- **Connection Limits**: Monitor concurrent connections per server
- **Message Size**: Large payloads may impact performance; consider pagination
- **Broadcast Frequency**: Limit update frequency to prevent overwhelming clients

## Troubleshooting

### Connection Refused
- Verify server is running on port 5006
- Check firewall settings
- Ensure JWT token is valid and not expired

### Messages Not Received
- Verify client is subscribed to the correct group
- Check browser console for JavaScript errors
- Verify server authorization (Admin role required)

### Frequent Disconnections
- Check network stability
- Verify WebSocket support in browser
- Monitor server logs for errors
- Consider adjusting reconnect timing

## Testing with HTTP Client

See `AdminService.http` for complete testing examples:

```
### Real-Time Service Health Check
GET http://localhost:5006/api/admin/realtime/health

### Broadcast Platform Update
POST http://localhost:5006/api/admin/realtime/broadcast/platform-update
Authorization: Bearer {token}
```

## Next Steps (Phase 3.2)

- Implement React Dashboard UI with WebSocket integration
- Create real-time data visualization components
- Add connection state indicator
- Implement alert notification system
- Add subscription management UI
