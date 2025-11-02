# 🐳 Docker Compose Setup Guide - TechBirdsFly Infrastructure

## 📋 Services Overview

This Docker Compose stack includes:

### Database & Caching
- **PostgreSQL 17** - Primary relational database for all microservices
- **Redis 7.4** - In-memory cache and session store

### Event Streaming
- **Zookeeper 7.5** - Kafka coordination
- **Kafka 7.5** - Distributed event broker (3+ topics)
- **Schema Registry 7.5** - Avro schema management and versioning

### Message Queue
- **RabbitMQ 3.13** - AMQP message broker

### Observability
- **Seq 2024.1** - Centralized structured logging
- **Jaeger** - Distributed tracing visualization

## 🚀 Quick Start

### Prerequisites
```bash
# Check Docker and Docker Compose versions
docker --version  # >= 20.10
docker-compose --version  # >= 2.0
```

### Start Infrastructure

```bash
# Navigate to infrastructure directory
cd infra

# Start all services (development mode)
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up -d

# Or production mode
docker-compose -f docker-compose.yml up -d

# View logs
docker-compose logs -f

# View logs for specific service
docker-compose logs -f kafka
```

### Initialize Kafka Topics

```bash
# Option 1: Run the initialization script
docker-compose exec kafka bash /init-topics.sh

# Option 2: Create topics manually
docker-compose exec kafka kafka-topics.sh --create \
  --bootstrap-server localhost:9092 \
  --topic user-events \
  --partitions 3 \
  --replication-factor 1

# List all topics
docker-compose exec kafka kafka-topics.sh --list --bootstrap-server localhost:9092
```

## 🔌 Connection Strings & Endpoints

### PostgreSQL
```
Host: localhost
Port: 5432
User: postgres
Password: Alisheikh@123
```

**Databases Created:**
- `techbirdsfly_auth` - Auth Service
- `techbirdsfly_eventbus` - Event Bus Service
- `techbirdsfly_billing` - Billing Service
- `techbirdsfly_generator` - Generator Service
- `techbirdsfly_admin` - Admin Service
- `techbirdsfly_user` - User Service
- `techbirdsfly_image` - Image Service
- `techbirdsfly_gateway` - API Gateway

```bash
# Connect with psql
psql -h localhost -U postgres -d techbirdsfly_auth
```

### Kafka
```
Bootstrap Servers: localhost:9092
Internal: kafka:29092
```

### Schema Registry
```
URL: http://localhost:8081
REST API: http://localhost:8081/subjects
```

### Redis
```
Host: localhost
Port: 6379
```

```bash
# Connect with redis-cli
redis-cli -h localhost -p 6379
```

### Observability Platforms

| Service | URL | Purpose |
|---------|-----|---------|
| Seq | http://localhost:80 | Centralized logging |
| Jaeger UI | http://localhost:16686 | Distributed tracing |
| RabbitMQ Management | http://localhost:15672 | Message queue admin |

## 📊 Kafka Topics

### User Domain
- `user-events` - All user events
- `user-registered` - New user registration
- `user-updated` - User profile updates
- `user-deactivated` - User deactivation

### Subscription Domain
- `subscription-events` - All subscription events
- `subscription-started` - Subscription creation
- `subscription-ended` - Subscription cancellation
- `subscription-upgraded` - Plan upgrades

### Website Domain
- `website-events` - All website events
- `website-generated` - Website generation complete
- `website-published` - Website published
- `website-deleted` - Website deletion

### Billing Domain
- `billing-events` - All billing events
- `payment-processed` - Payment completion
- `invoice-created` - Invoice generation

### System Topics
- `system-events` - Infrastructure events
- `health-check` - Health monitoring

## 🛠️ Common Commands

### Start/Stop Services
```bash
# Start all services
docker-compose up -d

# Stop all services
docker-compose down

# Stop and remove volumes
docker-compose down -v

# Restart a specific service
docker-compose restart kafka
```

### View Service Status
```bash
# Check all services
docker-compose ps

# View resource usage
docker stats

# Check service health
docker-compose ps
```

### Access Service Shells
```bash
# PostgreSQL
docker-compose exec postgres psql -U postgres

# Kafka
docker-compose exec kafka bash

# Redis
docker-compose exec redis redis-cli
```

### View Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f kafka

# Follow last 100 lines
docker-compose logs -f --tail=100 kafka

# Timestamp and service name
docker-compose logs -f -t kafka
```

### Manage Volumes
```bash
# List volumes
docker volume ls | grep techbirdsfly

# Inspect volume
docker volume inspect techbirdsfly_postgres_data

# Remove volume
docker volume rm techbirdsfly_postgres_data
```

## 🔍 Kafka Operations

### Topic Management
```bash
# Create topic
docker-compose exec kafka kafka-topics.sh \
  --create \
  --bootstrap-server localhost:9092 \
  --topic my-topic \
  --partitions 3 \
  --replication-factor 1

# List topics
docker-compose exec kafka kafka-topics.sh \
  --list \
  --bootstrap-server localhost:9092

# Describe topic
docker-compose exec kafka kafka-topics.sh \
  --describe \
  --bootstrap-server localhost:9092 \
  --topic user-events

# Delete topic
docker-compose exec kafka kafka-topics.sh \
  --delete \
  --bootstrap-server localhost:9092 \
  --topic my-topic
```

### Consumer Operations
```bash
# Read messages from topic (from beginning)
docker-compose exec kafka kafka-console-consumer.sh \
  --bootstrap-server localhost:9092 \
  --topic user-events \
  --from-beginning

# Read messages in consumer group
docker-compose exec kafka kafka-console-consumer.sh \
  --bootstrap-server localhost:9092 \
  --topic user-events \
  --group my-group

# List consumer groups
docker-compose exec kafka kafka-consumer-groups.sh \
  --bootstrap-server localhost:9092 \
  --list
```

### Producer Operations
```bash
# Produce messages
docker-compose exec kafka kafka-console-producer.sh \
  --bootstrap-server localhost:9092 \
  --topic user-events
  
# Input messages (one per line, Ctrl+C to exit):
# {"userId": "123", "email": "user@example.com"}
```

## 🐳 Docker Network

All services connect via bridge network: `techbirdsfly_network` (172.25.0.0/16)

Internal service communication:
- PostgreSQL: `postgres:5432`
- Kafka: `kafka:9092` (internal) or `kafka:29092` (external)
- Redis: `redis:6379`
- Schema Registry: `schema-registry:8081`
- Seq: `seq:5341`

## 📈 Health Checks

Services have health checks configured. View status:

```bash
docker-compose ps
```

Look for `(healthy)` or `(starting)` status.

## 🔐 Security Notes

**Development Only:**
- Default PostgreSQL password: `Alisheikh@123`
- RabbitMQ defaults: `guest/guest`
- No authentication on Kafka or Redis

**For Production:**
- Use strong database passwords
- Enable Kafka SASL/SSL
- Enable Redis password authentication
- Use network policies and firewall rules
- Run services in separate networks
- Use secrets management (Docker Secrets, Vault)

## 🐛 Troubleshooting

### Service won't start
```bash
# Check logs
docker-compose logs kafka

# Verify port availability
lsof -i :9092  # Check Kafka port

# Rebuild images
docker-compose up -d --build
```

### Connection refused
```bash
# Verify service is running
docker-compose ps

# Wait for service to be healthy
docker-compose ps kafka
# Should show "(healthy)" status

# Test connectivity
docker-compose exec postgres psql -U postgres -d postgres -c "SELECT 1"
```

### Disk space issues
```bash
# Remove unused volumes
docker system prune -a --volumes

# Check volume sizes
du -sh /var/lib/docker/volumes/techbirdsfly*
```

### Reset everything
```bash
# Stop and remove everything
docker-compose down -v

# Remove images
docker rmi $(docker images | grep techbirdsfly)

# Start fresh
docker-compose up -d
```

## 📚 References

- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [Kafka Documentation](https://kafka.apache.org/documentation/)
- [Confluent Platform](https://docs.confluent.io/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Seq Documentation](https://docs.datalust.co/docs/getting-started)
- [Jaeger Documentation](https://www.jaegertracing.io/docs/)

## 🚀 Next Steps

1. **Verify all services are running:**
   ```bash
   docker-compose ps
   ```

2. **Test database connectivity:**
   ```bash
   docker-compose exec postgres psql -U postgres -l
   ```

3. **Test Kafka:**
   ```bash
   docker-compose exec kafka kafka-topics.sh --list --bootstrap-server localhost:9092
   ```

4. **Access observability dashboards:**
   - Seq: http://localhost
   - Jaeger: http://localhost:16686

5. **Proceed to Step-3: Shared Event Contracts**
