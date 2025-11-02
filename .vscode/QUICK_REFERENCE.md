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

### .NET Microservices
```
🔐 Auth Service            → localhost:5000 (HTTP)
📨 Event Bus Service       → localhost:5020 (HTTP)
👤 User Service            → localhost:5008 (HTTP)
💳 Billing Service         → localhost:5002 (HTTP)
⚙️  Generator Service       → localhost:5003 (HTTP)
🖼  Image Service           → localhost:5007 (HTTP)
🛠  Admin Service           → localhost:5006 (HTTP)
🚪 API Gateway (YARP)      → localhost:8000 (HTTP)
🌐 Next.js Frontend        → localhost:3000
```

### Event Streaming & Infrastructure
```
🔄 Kafka Bootstrap         → localhost:9092
📊 Schema Registry         → localhost:8081
🗄  PostgreSQL              → localhost:5432
💾 Redis Cache             → localhost:6379
```

### Observability & Monitoring
```
📝 Seq Logs                → localhost:5341 (Dashboard)
🔍 Jaeger Traces           → localhost:16686 (Dashboard)
📍 Zookeeper               → localhost:2181
```

---

## Run Individual Services

### Via VS Code Debugger
1. Ctrl+Shift+D
2. Select service name from dropdown
3. Click ▶️ (Green Play Button)

**Example:** Select "🔐 .NET Auth Service (Port 5000)"

### Via Command Line

**Start Infrastructure First:**
```bash
cd /Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly
docker compose -f docker-compose.debug.yml up -d
```

**Then Start Services (each in separate terminal):**
```bash
# Auth Service (Port 5000)
cd services/auth-service/src
dotnet run

# Event Bus Service (Port 5020)
cd services/event-bus-service/src
dotnet run

# User Service (Port 5008)
cd services/user-service/src/UserService
dotnet run

# Billing Service (Port 5002)
cd services/billing-service/src
dotnet run

# Generator Service (Port 5003)
cd services/generator-service/src
dotnet run

# Image Service (Port 5007)
cd services/image-service/src
dotnet run

# Admin Service (Port 5006)
cd services/admin-service/src
dotnet run

# API Gateway (Port 8000)
cd gateway/yarp-gateway/src
dotnet run

# Frontend (Port 3000)
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
- **Purpose:** Real-time logs from all 8 microservices
- **Features:** Structured logging, full-text search, filtering, alerting
- **Includes:** Request tracing, error tracking, performance metrics

### Jaeger (Distributed Tracing)
- **URL:** http://localhost:16686
- **Purpose:** Track requests across all services with correlation IDs
- **Features:** Request timing, service dependencies, error tracking, latency analysis
- **Integration:** All services publish traces via OpenTelemetry

### Kafka Topics (Event Streaming)
```bash
# List all topics
docker exec techbirdsfly-kafka-debug kafka-topics.sh \
  --bootstrap-server localhost:9092 --list

# Monitor USER_REGISTERED topic
docker exec techbirdsfly-kafka-debug kafka-console-consumer.sh \
  --bootstrap-server localhost:9092 \
  --topic USER_REGISTERED \
  --from-beginning

# Check consumer group status
docker exec techbirdsfly-kafka-debug kafka-consumer-groups.sh \
  --bootstrap-server localhost:9092 \
  --group event-bus-service-group --describe
```

### PostgreSQL (Event Bus Outbox)
```bash
# Connect to database
psql -h localhost -U postgres -d techbirdsfly_eventbus

# View outbox events
SELECT id, event_type, status, created_at FROM outbox ORDER BY created_at DESC LIMIT 10;
```

### Redis Cache
```bash
# Connect to Redis CLI
redis-cli

# Check connection
ping

# View all keys
KEYS *
```

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
# macOS/Linux: Kill process on port
lsof -ti:5000 | xargs kill -9  # Port 5000
lsof -ti:5020 | xargs kill -9  # Port 5020
lsof -ti:9092 | xargs kill -9  # Port 9092 (Kafka)
```

### Kafka Connection Refused?
```bash
# Start Docker infrastructure
docker compose -f docker-compose.debug.yml up -d

# Verify Kafka is running
docker ps | grep kafka

# Check Kafka logs
docker logs techbirdsfly-kafka-debug
```

### Database Does Not Exist?
```bash
# Create Event Bus database
cd services/event-bus-service/src
dotnet ef database update

# Create Auth database (auto-migrated)
# Create User database (auto-migrated)
```

### Can't Connect to PostgreSQL?
```bash
# Verify PostgreSQL is running
docker ps | grep postgres

# Test connection
psql -h localhost -U postgres -c "SELECT version();"
```

### Services Won't Start?
1. **Clean & rebuild:**
   ```bash
   dotnet clean TechBirdsFly.sln
   dotnet build TechBirdsFly.sln
   ```

2. **Check port availability:**
   ```bash
   netstat -an | grep LISTEN  # macOS/Linux
   ```

3. **View service logs:**
   - Open Seq Dashboard: http://localhost:5341
   - Search for error messages
   - Filter by service name

---

## Configuration Files

All settings in `.vscode/` and service directories:

### VS Code Debug Configs
- **launch.json** - All 9 debug configurations (8 services + frontend)
  - Includes Kafka URLs for each service
  - Pre-configured ports (5000, 5020, 5008, 5002, 5003, 5007, 5006, 8000, 3000)
  - Environment variables for each service

### Service Configuration Files
- **Auth Service:** `/services/auth-service/src/Properties/launchSettings.json`
- **Event Bus:** `/services/event-bus-service/src/Properties/launchSettings.json`
- **User Service:** `/services/user-service/src/Properties/launchSettings.json`
- **Billing Service:** `/services/billing-service/src/Properties/launchSettings.json`
- **Generator Service:** `/services/generator-service/src/Properties/launchSettings.json`
- **Image Service:** `/services/image-service/src/Properties/launchSettings.json`
- **Admin Service:** `/services/admin-service/src/Properties/launchSettings.json`
- **API Gateway:** `/gateway/yarp-gateway/src/Properties/launchSettings.json`

### Docker Infrastructure
- **docker-compose.debug.yml** - Development infrastructure setup
  - PostgreSQL (5432)
  - Kafka (9092) + Zookeeper (2181)
  - Schema Registry (8081)
  - Seq (5341)
  - Jaeger (16686)
  - Redis (6379)

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

**Total Services:** 8 (.NET) + 1 (Next.js Frontend) + 8 (Infrastructure)

**Event-Driven Architecture:** ✅ Implemented
- Auth Service → Event Bus (HTTP) → PostgreSQL Outbox → Kafka → User Service
- Full correlation ID tracking across all services
- Distributed tracing with Jaeger

*Last Updated: November 2, 2025*
