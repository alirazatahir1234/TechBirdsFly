# 🚀 Event Bus Service

Distributed event bus for TechBirdsFly microservices using Apache Kafka, Avro schemas, and the Outbox pattern.

## 📋 Architecture

```
Event-Bus-Service
├── Domain/                          # Business entities
│   ├── Entities/
│   │   ├── OutboxEvent.cs          # Outbox pattern entity
│   │   └── EventSubscription.cs    # Subscription management
│   └── Events/
├── Application/                     # Use cases & interfaces
│   ├── Interfaces/
│   │   ├── IKafkaProducer.cs
│   │   ├── IOutboxEventRepository.cs
│   │   └── IEventSubscriptionRepository.cs
│   ├── Services/
│   └── DTOs/
├── Infrastructure/                  # External integrations
│   ├── Persistence/
│   │   └── EventBusDbContext.cs
│   ├── Kafka/
│   │   ├── KafkaProducer.cs
│   │   └── KafkaSettings.cs
│   └── Repositories/
├── WebAPI/                          # API endpoints
│   ├── Controllers/
│   ├── Middlewares/
│   └── DI/
└── Tests/
```

## 🔑 Key Features

- **Kafka Producer**: Publish events to Kafka topics
- **Outbox Pattern**: Guaranteed event delivery with database transaction
- **Subscription Management**: Services can subscribe to specific event types
- **REST Webhooks**: Deliver events via HTTP to subscribers
- **Avro Schemas**: Contract-first event design with Schema Registry
- **Health Checks**: Monitor Kafka, PostgreSQL, and service health
- **JWT Authentication**: Secure event publishing
- **Serilog Integration**: Centralized logging with Seq

## 🛠️ Configuration

### Environment Variables / appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=techbirdsfly_eventbus;Username=postgres;Password=Alisheikh@123"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "SchemaRegistry": {
      "Url": "http://localhost:8081"
    },
    "Topics": {
      "UserEvents": "user-events",
      "SubscriptionEvents": "subscription-events",
      "WebsiteEvents": "website-events"
    }
  }
}
```

## 🚀 Running the Service

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Create database
dotnet ef database update

# Run
dotnet run
```

Service runs on:
- HTTP: `http://localhost:5030`
- HTTPS: `https://localhost:7030`
- Swagger: `http://localhost:5030/swagger`

## 📡 API Endpoints

### Health Check
```
GET /health
GET /api/health/status
GET /api/health/info
```

### Events (Coming in Step-4)
```
POST /api/events/publish           # Publish an event
GET  /api/events/{eventId}         # Get event details
GET  /api/events/replay            # Replay events
```

### Subscriptions (Coming in Step-6)
```
POST /api/subscriptions            # Subscribe to events
GET  /api/subscriptions            # List subscriptions
PUT  /api/subscriptions/{id}       # Update subscription
DELETE /api/subscriptions/{id}     # Unsubscribe
```

## 🔄 Outbox Pattern Flow

1. **Produce**: Service publishes event → creates OutboxEvent in DB
2. **Persist**: Event stored in outbox table (same transaction)
3. **Poll**: Background worker queries unpublished events
4. **Publish**: Events published to Kafka
5. **Mark**: After successful publish, mark as published
6. **Deliver**: Kafka consumers process events
7. **Webhook**: Events delivered to subscribed services via REST

## 📚 Topics

- **user-events**: UserRegistered, UserUpdated, UserDeleted
- **subscription-events**: SubscriptionStarted, SubscriptionEnded
- **website-events**: WebsiteGenerated, WebsitePublished

## 🔗 Integration Points

### Auth Service → Event Bus
- Publishes: `UserRegistered`, `UserUpdated`, `UserDeactivated`
- Consumes: None yet

### Profile Service → Event Bus
- Subscribes to: `UserRegistered`
- Creates: User profile on registration

### Email Service → Event Bus
- Subscribes to: `UserRegistered`, `SubscriptionStarted`
- Sends: Welcome email, subscription confirmation

## 🛡️ Security

- JWT Bearer authentication required for publishing events
- Services authenticate with their service token
- Event signatures for integrity verification
- HTTPS in production

## 📊 Monitoring

- Serilog logs to Seq (`http://localhost:5341`)
- OpenTelemetry tracing to Jaeger
- Prometheus metrics (planned)
- Health checks at `/health`

## 🗄️ Database Schema

### OutboxEvents Table
```sql
- Id (UUID, PK)
- EventType (VARCHAR 256)
- EventPayload (TEXT, JSON)
- Topic (VARCHAR 256)
- PartitionKey (VARCHAR 256)
- IsPublished (BOOLEAN)
- PublishedAt (TIMESTAMP)
- OccurredAt (TIMESTAMP)
- CreatedAt (TIMESTAMP)
- PublishAttempts (INT)
- LastErrorMessage (TEXT)
```

### EventSubscriptions Table
```sql
- Id (UUID, PK)
- ServiceName (VARCHAR 256)
- EventType (VARCHAR 256)
- WebhookUrl (VARCHAR 2048)
- IsActive (BOOLEAN)
- RetryCount (INT)
- TimeoutSeconds (INT)
- LastDeliveredAt (TIMESTAMP)
- LastFailedAt (TIMESTAMP)
- FailureReason (TEXT)
- CreatedAt (TIMESTAMP)
```

## 🧪 Testing

```bash
# Unit tests
dotnet test ./Tests/UnitTests

# Integration tests
dotnet test ./Tests/IntegrationTests
```

## 📖 References

- [Outbox Pattern](https://microservices.io/patterns/data/transactional-outbox.html)
- [Kafka Documentation](https://kafka.apache.org/documentation/)
- [Confluent Schema Registry](https://docs.confluent.io/platform/current/schema-registry/)
- [Avro Format](https://avro.apache.org/)

## 🔄 Next Steps

- Step-2: Docker Compose setup for Kafka & Schema Registry
- Step-3: Avro schemas and event contracts
- Step-4: Event publishing APIs
- Step-5: Outbox background worker
- Step-6: Kafka consumer & webhook delivery
- Step-7: Auth Service integration
