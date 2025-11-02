# Step-7 Part 3: End-to-End U1 Testing Plan

**Status:** Ready to Execute  
**Test Objective:** Validate complete UserRegistered → Profile Creation flow  
**Scope:** Auth Service → Event Bus → Kafka → User Service  
**Expected Duration:** 15-20 minutes

---

## 🎯 Test Objectives

| Objective | Success Criteria |
|-----------|-----------------|
| **Event Publishing** | Auth Service publishes UserRegisteredEvent after user registration |
| **Event Storage** | Event Bus stores event in Outbox table |
| **Kafka Publishing** | Outbox background worker publishes to USER_REGISTERED topic |
| **Event Consumption** | User Service consumer receives event from Kafka |
| **Profile Creation** | User profile created in User Service database |
| **Distributed Tracing** | Correlation ID flows through all logs |
| **Idempotency** | Retry doesn't create duplicate profiles |
| **Error Handling** | Errors logged without crashing services |

---

## 📋 Pre-Test Requirements

### Services Status
- [ ] **Kafka** running on `localhost:9092`
  - Topics created: `user_registered`, `user_updated`, `user_deactivated`, `subscription_started`, `website_generated`
- [ ] **Schema Registry** running on `localhost:8081`
- [ ] **PostgreSQL** running on `localhost:5432`
  - Database: `eventbus`
  - Username: `postgres`
- [ ] **Event Bus Service** running on `http://localhost:5020`
- [ ] **Auth Service** running on `http://localhost:5000`
- [ ] **User Service** running on `http://localhost:5008`
- [ ] **Redis** running on `localhost:6379` (optional, for User Service cache)

### Databases
- [ ] Event Bus database migrated (`EventBusDbContext`)
- [ ] User Service database migrated (`UserDbContext`)
- [ ] All tables created and accessible

### Network
- [ ] All services can reach each other
- [ ] HTTP calls work between services (Event Bus → Auth/User)
- [ ] Kafka is accessible from all services

---

## 🚀 Test Execution Steps

### Phase 1: Startup Verification (5 minutes)

#### Step 1.1: Verify Event Bus Health
```bash
# Check Event Bus is responding
curl -s http://localhost:5020/health | jq .

# Expected output:
# {
#   "status": "Healthy",
#   "service": "event-bus-service",
#   "timestamp": "2025-11-02T14:00:00Z",
#   "version": "1.0.0"
# }
```

#### Step 1.2: Verify Auth Service Health
```bash
# Check Auth Service is responding
curl -s http://localhost:5000/api/auth/health | jq .

# Expected: Service running, no errors
```

#### Step 1.3: Verify User Service Health
```bash
# Check User Service is responding
curl -s http://localhost:5008/health | jq .

# Expected: Service running, database connected
```

#### Step 1.4: Check Kafka Topics
```bash
# List topics
kafka-topics --bootstrap-server localhost:9092 --list

# Expected topics:
# - user_registered
# - user_updated
# - user_deactivated
# - subscription_started
# - website_generated
```

#### Step 1.5: Verify No Pre-Existing Events
```bash
# Clear any previous test data (optional)
# Reset User Service database
cd services/user-service/src/UserService
dotnet ef database drop -f
dotnet ef database update

# Reset Event Bus database
cd services/event-bus-service/src/EventBusService
dotnet ef database drop -f
dotnet ef database update
```

---

### Phase 2: User Registration Test (5 minutes)

#### Step 2.1: Register New User
```bash
# Generate unique email (to avoid duplicates)
TIMESTAMP=$(date +%s%N)
EMAIL="testuser_${TIMESTAMP}@example.com"

# Register user in Auth Service
REGISTER_RESPONSE=$(curl -s -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d "{
    \"email\": \"$EMAIL\",
    \"password\": \"TestPassword123!\",
    \"firstName\": \"Test\",
    \"lastName\": \"User\"
  }")

echo "Registration Response:"
echo "$REGISTER_RESPONSE" | jq .

# Extract UserId for later use
USER_ID=$(echo "$REGISTER_RESPONSE" | jq -r '.data.userId')
echo "User ID: $USER_ID"
```

**Expected Response:**
```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "email": "testuser_1699891935@example.com",
    "message": "User registered successfully. Please confirm your email."
  }
}
```

#### Step 2.2: Wait for Event Processing
```bash
# Wait for:
# 1. Event Bus to receive and store
# 2. Outbox worker to publish to Kafka (10s cycle)
# 3. User Service to consume (5s poll)

sleep 15
echo "✓ Waiting for event processing..."
```

---

### Phase 3: Verification Tests (5 minutes)

#### Step 3.1: Verify Event in Event Bus Outbox
```bash
# Check Event Bus outbox stats
curl -s http://localhost:5020/api/outbox/stats | jq .

# Expected output:
# {
#   "totalPending": 0,    # Should be 0 if processed
#   "totalProcessed": 1,  # Should be 1
#   "totalFailed": 0
# }
```

#### Step 3.2: Verify Event on Kafka Topic
```bash
# Check for UserRegistered event on Kafka
# Run this in a new terminal to monitor
kafka-console-consumer --bootstrap-server localhost:9092 \
  --topic user_registered \
  --from-beginning \
  --max-messages 1

# Or use event-bus consumer API
curl -s http://localhost:5020/api/subscriptions/health | jq .

# Expected:
# {
#   "isHealthy": true,
#   "consumersActive": 1,
#   "totalEventsConsumed": 1
# }
```

#### Step 3.3: Verify User Profile Created in User Service
```bash
# Query user by email
curl -s "http://localhost:5008/api/users/email/$EMAIL" \
  -H "Authorization: Bearer $TOKEN" | jq .

# Alternative: Get user by ID (if auth not required)
curl -s "http://localhost:5008/api/users/$USER_ID" | jq .

# Expected:
# {
#   "id": "550e8400-e29b-41d4-a716-446655440000",
#   "email": "testuser_1699891935@example.com",
#   "firstName": "Test",
#   "lastName": "User",
#   "status": "active",
#   "role": "user",
#   "isEmailVerified": false,
#   "createdAt": "2025-11-02T14:32:16Z"
# }
```

**If user NOT found:**
- ❌ Event not consumed by User Service
- ❌ Profile creation failed
- Check logs for errors

#### Step 3.4: Verify Correlation ID in Logs

**Auth Service Logs:**
```bash
# Check Auth Service logs for UserRegistered event publishing
docker logs auth-service 2>&1 | grep -i "userregistered\|eventpublish" | tail -5

# Expected log entries:
# [14:32:15 INF] 📢 Publishing UserRegistered event for new user - UserId: 550e8400-e29b-41d4-a716-446655440000, Email: testuser_1699891935@example.com
# [14:32:15 INF] ✅ UserRegistered event published - UserId: 550e8400-e29b-41d4-a716-446655440000
```

**Event Bus Logs:**
```bash
# Check Event Bus logs for event storage and publishing
docker logs event-bus-service 2>&1 | grep -i "user_registered\|outbox" | tail -10

# Expected log entries:
# [14:32:15 INF] 📥 Event published: UserRegistered
# [14:32:15 INF] ✅ Event stored in outbox
# [14:32:25 INF] 📤 Publishing to Kafka topic: user_registered
```

**User Service Logs:**
```bash
# Check User Service logs for event consumption
docker logs user-service 2>&1 | grep -i "userregistered\|eventconsumer" | tail -10

# Expected log entries:
# [14:32:30 INF] 📨 Received UserRegistered event - UserId: 550e8400-e29b-41d4-a716-446655440000, Email: testuser_1699891935@example.com
# [14:32:30 INF] 🔄 Creating user profile from event - UserId: 550e8400-e29b-41d4-a716-446655440000
# [14:32:31 INF] ✅ User profile created successfully from event - UserId: 550e8400-e29b-41d4-a716-446655440000
```

#### Step 3.5: Extract and Verify Correlation ID
```bash
# Extract correlation ID from logs
CORR_ID=$(docker logs auth-service 2>&1 | grep -i "userregistered" | grep "CorrelationId" | head -1 | sed 's/.*CorrelationId: //' | cut -d' ' -f1)

echo "Correlation ID: $CORR_ID"

# Verify same ID appears in all services
echo "Auth Service:"
docker logs auth-service 2>&1 | grep "$CORR_ID" | wc -l

echo "Event Bus Service:"
docker logs event-bus-service 2>&1 | grep "$CORR_ID" | wc -l

echo "User Service:"
docker logs user-service 2>&1 | grep "$CORR_ID" | wc -l

# Expected: All services show correlation ID
```

---

### Phase 4: Idempotency & Error Handling (5 minutes)

#### Step 4.1: Test Idempotency
```bash
# Simulate event retry (publish same event again)
# Get the stored event from Event Bus
curl -s http://localhost:5020/api/events/types | jq .

# Manually re-publish the event to User Service
# This would trigger the handler again

# Expected behavior:
# - Handler detects user already exists
# - Logs: "ℹ️ User profile already exists, skipping creation"
# - No duplicate created
# - Service remains healthy
```

#### Step 4.2: Check for Duplicate Users
```bash
# Query all users and verify no duplicates
curl -s "http://localhost:5008/api/users?page=1&pageSize=100" | jq '.users | length'

# Should show only 1 user created for this test
# If more than 1, idempotency failed
```

---

## 📊 Test Results Template

### Summary

```
╔════════════════════════════════════════════════════════════╗
║         END-TO-END U1 TEST RESULTS                         ║
╠════════════════════════════════════════════════════════════╣
║                                                            ║
║ Test Date:              ______________________             ║
║ Test Duration:          ______________________             ║
║ Overall Result:         ✅ PASS / ❌ FAIL                  ║
║                                                            ║
╠════════════════════════════════════════════════════════════╣
║ TEST PHASES                                                ║
╠════════════════════════════════════════════════════════════╣
║                                                            ║
║ Phase 1: Startup Verification                             ║
║   ✅ Event Bus healthy                                     ║
║   ✅ Auth Service healthy                                  ║
║   ✅ User Service healthy                                  ║
║   ✅ Kafka topics exist                                    ║
║   Result: ✅ PASS                                          ║
║                                                            ║
║ Phase 2: User Registration                                ║
║   ✅ User registered in Auth Service                       ║
║   ✅ UserRegisteredEvent published                         ║
║   ✅ Event stored in outbox                                ║
║   Result: ✅ PASS                                          ║
║                                                            ║
║ Phase 3: Verification Tests                               ║
║   ✅ Event found in outbox                                 ║
║   ✅ Event published to Kafka                              ║
║   ✅ Event consumed by User Service                        ║
║   ✅ User profile created in User Service                  ║
║   ✅ Correlation ID flows through logs                     ║
║   Result: ✅ PASS                                          ║
║                                                            ║
║ Phase 4: Idempotency & Error Handling                     ║
║   ✅ Retry doesn't create duplicates                       ║
║   ✅ Error messages logged correctly                       ║
║   Result: ✅ PASS                                          ║
║                                                            ║
╠════════════════════════════════════════════════════════════╣
║ DETAILED RESULTS                                           ║
╠════════════════════════════════════════════════════════════╣
║                                                            ║
║ Event Publishing:       ✅                                 ║
║ Event Storage (Outbox): ✅                                 ║
║ Kafka Publishing:       ✅                                 ║
║ Event Consumption:      ✅                                 ║
║ Profile Creation:       ✅                                 ║
║ Correlation Tracing:    ✅                                 ║
║ Idempotency:            ✅                                 ║
║ Error Handling:         ✅                                 ║
║                                                            ║
╠════════════════════════════════════════════════════════════╣
║ PERFORMANCE METRICS                                        ║
╠════════════════════════════════════════════════════════════╣
║                                                            ║
║ Registration → Publication:     _____ ms                   ║
║ Publication → Outbox:           _____ ms                   ║
║ Outbox → Kafka:                 _____ ms (bg worker 10s)  ║
║ Kafka → Consumption:            _____ ms (poll 5s)        ║
║ Total E2E Latency:              _____ ms                   ║
║                                                            ║
╠════════════════════════════════════════════════════════════╣
║ ISSUES FOUND                                               ║
╠════════════════════════════════════════════════════════════╣
║                                                            ║
║ None / List any issues here...                             ║
║                                                            ║
╚════════════════════════════════════════════════════════════╝
```

---

## 🔍 Troubleshooting Guide

### Issue: User profile not created in User Service

**Symptom:** Step 3.3 returns 404 for user email

**Debugging Steps:**
1. Check User Service logs:
   ```bash
   docker logs user-service 2>&1 | tail -50
   ```

2. Verify EventConsumerService started:
   ```bash
   docker logs user-service 2>&1 | grep "Event Consumer"
   ```

3. Check Event Bus connectivity:
   ```bash
   docker logs user-service 2>&1 | grep "Event Bus"
   ```

4. Verify Kafka connection:
   ```bash
   docker logs user-service 2>&1 | grep "Kafka"
   ```

**Common Causes:**
- Event Bus not running → Start Event Bus service
- Kafka not running → Start Kafka broker
- Event not published → Check Auth Service logs
- Handler exception → Check User Service logs for errors

---

### Issue: Event not appearing in Outbox

**Symptom:** Step 3.1 shows totalProcessed = 0

**Debugging Steps:**
1. Check Auth Service published:
   ```bash
   docker logs auth-service 2>&1 | grep "publishing\|UserRegistered"
   ```

2. Verify Event Bus received:
   ```bash
   curl http://localhost:5020/api/outbox/stats
   ```

3. Check database connection:
   ```bash
   docker logs event-bus-service 2>&1 | grep "database\|outbox"
   ```

**Common Causes:**
- Event Bus HTTP endpoint not responding
- Database migration not applied
- Connection string incorrect
- Auth Service can't reach Event Bus

---

### Issue: Correlation ID not appearing in logs

**Symptom:** Logs don't show correlation IDs across services

**Debugging Steps:**
1. Verify correlation ID set in Auth Service:
   ```bash
   docker logs auth-service 2>&1 | grep "CorrelationId" | head -1
   ```

2. Check Event Bus forwarding:
   ```bash
   docker logs event-bus-service 2>&1 | grep "correlation\|CorrelationId"
   ```

3. Verify User Service receives:
   ```bash
   docker logs user-service 2>&1 | grep "CorrelationId" | head -1
   ```

**Common Causes:**
- CorrelationId not serialized in event
- Logging context not properly configured
- Correlation header not forwarded

---

## ✅ Success Criteria

All tests PASS when:

1. ✅ **User registered** in Auth Service returns valid UserId
2. ✅ **Event published** within 1 second
3. ✅ **Event in outbox** within 2 seconds
4. ✅ **Event on Kafka** within 15 seconds (10s bg worker + buffer)
5. ✅ **User profile created** within 20 seconds total (5s poll + processing)
6. ✅ **Same correlation ID** appears in logs from all 3 services
7. ✅ **No duplicates** created on retry
8. ✅ **All services healthy** throughout test

---

## 📝 Documentation

**Key Files:**
- Event Consumer: `/services/user-service/src/UserService/EventConsumers/`
- Auth Integration: `/services/auth-service/src/Application/Services/AuthApplicationService.cs`
- Event Bus: `/services/event-bus-service/src/Infrastructure/BackgroundServices/`
- Event Contracts: `/src/Shared/Events/Contracts/UserRegisteredEvent.cs`

**Related Tests:**
- Unit tests: `.../EventConsumers.Tests/`
- Integration tests: `.../Services.Tests/`
- Load tests: `./infra/load-tests/`

---

## 🎯 Next Steps After Testing

- [ ] Fix any issues found
- [ ] Document any deviations
- [ ] Performance optimization (if latency > 30s)
- [ ] Deploy to staging environment
- [ ] Run automated test suite
- [ ] Load testing (100+ concurrent registrations)
- [ ] Chaos engineering (service failures)
- [ ] Production deployment

---

**Test Plan Status:** ✅ Ready to Execute  
**Last Updated:** November 2, 2025

