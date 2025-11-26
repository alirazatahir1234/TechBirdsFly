# 🐳 TechBirdsFly Docker & Debug Mode Setup Guide

## Overview

This guide explains how to run the entire TechBirdsFly microservices architecture in Docker containers with all services, databases, and observability tools.

---

## 📋 Prerequisites

- **Docker**: Version 20.10 or higher
- **Docker Compose**: Version 2.0 or higher
- **Disk Space**: Minimum 10GB free space
- **RAM**: Minimum 8GB (16GB recommended)
- **macOS**: Via Docker Desktop, or Linux with Docker Engine

### Install Docker

**macOS:**
```bash
# Install Docker Desktop
brew install --cask docker

# Or download from: https://www.docker.com/products/docker-desktop
```

**Linux:**
```bash
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
```

**Verify Installation:**
```bash
docker --version
docker-compose --version
```

---

## 🚀 Quick Start

### 1. Start All Services

```bash
cd /Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly

# Make script executable (first time only)
chmod +x docker-compose-manager.sh

# Start all containers
./docker-compose-manager.sh up
```

### 2. Access the Services

Once started, services will be available at:

#### 🔐 **Authentication**
- **Auth Service**: `http://localhost:5001`
- **Swagger UI**: `http://localhost:5001/swagger`

#### 👥 **User Management**
- **User Service**: `http://localhost:5008`
- **Swagger UI**: `http://localhost:5008/swagger`

#### 💳 **Billing**
- **Billing Service**: `http://localhost:5002`
- **Swagger UI**: `http://localhost:5002/swagger`

#### 📨 **Event Processing**
- **Event Bus Service**: `http://localhost:5020`
- **Swagger UI**: `http://localhost:5020/swagger`

#### ⚙️ **Code Generation**
- **Generator Service**: `http://localhost:5003`
- **Swagger UI**: `http://localhost:5003/swagger`

#### 🛠️ **Administration**
- **Admin Service**: `http://localhost:5006`
- **Swagger UI**: `http://localhost:5006/swagger`

#### 🖼️ **Image Processing**
- **Image Service**: `http://localhost:5007`
- **Swagger UI**: `http://localhost:5007/swagger`

#### 📤 **Project Exporting**
- **Export Service**: `http://localhost:5004`
- **Swagger UI**: `http://localhost:5004/swagger`

#### 📁 **Project Management**
- **Project Service**: `http://localhost:5009`
- **Swagger UI**: `http://localhost:5009/swagger`

#### ⚡ **Caching**
- **Cache Service**: `http://localhost:5021`
- **Swagger UI**: `http://localhost:5021/swagger`

#### 🎬 **Media Management**
- **Media Service**: `http://localhost:5022`
- **Swagger UI**: `http://localhost:5022/swagger`

#### 🚪 **API Gateway**
- **Gateway**: `http://localhost:9000`
- **Swagger UI**: `http://localhost:9000/swagger`
- **Routes**: All services accessible via gateway

#### 🌐 **Frontend**
- **Web App**: `http://localhost:3000`

---

## 📊 Observability & Monitoring

### Seq Logging
- **URL**: `http://localhost:5341`
- **Username**: `admin`
- **Password**: (default)
- **Purpose**: View structured logs from all services

### Jaeger Distributed Tracing
- **URL**: `http://localhost:16686`
- **Purpose**: Trace requests across services

### Health Checks
All services have health checks configured. To view status:

```bash
./docker-compose-manager.sh status
```

---

## 🛠️ Docker Compose Manager Script

### Available Commands

#### **Start Services**
```bash
./docker-compose-manager.sh up
```
Starts all containers:
- Infrastructure (PostgreSQL, Kafka, Redis, MongoDB, Seq, Jaeger)
- All 12 microservices
- API Gateway
- Frontend application

#### **Stop Services**
```bash
./docker-compose-manager.sh down
```
Gracefully stops all running containers.

#### **View Logs**
```bash
# View all logs in real-time
./docker-compose-manager.sh logs

# View logs for specific service
./docker-compose-manager.sh logs auth-service
./docker-compose-manager.sh logs api-gateway
./docker-compose-manager.sh logs frontend
```

#### **Build Images**
```bash
# Build all images (uses cache)
./docker-compose-manager.sh build

# Rebuild all images (no cache - slower)
./docker-compose-manager.sh rebuild
```

#### **List Containers**
```bash
./docker-compose-manager.sh ps
```

#### **Check Service Health**
```bash
./docker-compose-manager.sh status
```

#### **Clean Up (Destructive)**
```bash
# Remove all containers, volumes, and images
./docker-compose-manager.sh clean
```

---

## 📁 Docker Compose Configuration

### File Location
`/docker/docker-compose.debug.yml`

### Service Structure

#### Infrastructure Layer
- **PostgreSQL** (Port 5433) - Relational database
- **Kafka** (Port 9092) - Message broker
- **Zookeeper** (Port 2181) - Kafka coordination
- **Schema Registry** (Port 8081) - Kafka schema management
- **Redis** (Port 6379) - Caching
- **MongoDB** (Port 27017) - NoSQL database
- **Seq** (Port 5341) - Log aggregation
- **Jaeger** (Port 16686) - Distributed tracing

#### Microservices Layer (12 Services)
Each service configured with:
- Health checks every 10 seconds
- Proper dependencies (waits for infrastructure)
- Environment variables for configuration
- Network connectivity via Docker Compose network
- Volume mounts for data persistence

#### Gateway & Frontend Layer
- **API Gateway** (Port 9000) - Routes to all services
- **Frontend** (Port 3000) - Next.js application

---

## 🔧 Environment Configuration

All services use the following environment setup:

### Database Connections
```
Host: postgres (Docker service name)
Port: 5432 (internal)
Database: techbirdsfly_[service-name]
Username: postgres
Password: postgres123
```

### Kafka Configuration
```
Bootstrap Servers: kafka:29092 (internal)
Schema Registry: http://schema-registry:8081
```

### Redis Configuration
```
Host: redis
Port: 6379
```

### MongoDB Configuration
```
ConnectionString: mongodb://mongodb:27017
Database: techbirdsfly_media
```

---

## 🐛 Debugging

### Enable Debug Logging

Edit `docker/docker-compose.debug.yml` and set logging level:

```yaml
environment:
  Logging:LogLevel:Default: Debug
```

Then rebuild and restart:
```bash
./docker-compose-manager.sh rebuild
./docker-compose-manager.sh down
./docker-compose-manager.sh up
```

### View Container Logs

```bash
# Real-time logs for specific service
docker logs -f techbirdsfly-auth-service-debug

# Last 100 lines
docker logs --tail 100 techbirdsfly-auth-service-debug

# Follow with timestamps
docker logs -f --timestamps techbirdsfly-auth-service-debug
```

### Inspect Container

```bash
# See container details
docker inspect techbirdsfly-auth-service-debug

# Check network
docker inspect techbirdsfly_debug

# Check volume
docker inspect postgres_data_debug
```

### Execute Commands in Container

```bash
# Open shell in running container
docker exec -it techbirdsfly-postgres-debug psql -U postgres

# Run health check manually
docker exec techbirdsfly-auth-service-debug curl http://localhost:5001/health
```

---

## 🔌 Network Architecture

### Service-to-Service Communication

Services communicate via Docker Compose network `techbirdsfly_debug`:

```
Frontend (3000)
    ↓
API Gateway (9000)
    ↓
┌───────────────────────────────────────────┐
│   Auth  │  User  │  Billing  │  Admin     │
│  (5001) │ (5008) │  (5002)   │  (5006)    │
│───────────────────────────────────────────│
│ Generator │ Image │ Export │ Project     │
│  (5003)   │(5007) │(5004)  │  (5009)     │
│───────────────────────────────────────────│
│ Event Bus │ Cache │ Media                 │
│  (5020)   │(5021) │(5022)                 │
└───────────────────────────────────────────┘
    ↓
    Database Layer
    (PostgreSQL, Redis, MongoDB, Kafka)
```

### External Access (Host Machine)

Use `localhost` to access from host:
- Frontend: `http://localhost:3000`
- Gateway: `http://localhost:9000`
- Services: `http://localhost:50XX` (specific ports)

---

## 📈 Performance Tuning

### Resource Allocation

Edit `docker/docker-compose.debug.yml`:

```yaml
services:
  auth-service:
    deploy:
      resources:
        limits:
          cpus: '0.5'
          memory: 512M
        reservations:
          cpus: '0.25'
          memory: 256M
```

### Restart Policies

Current: `restart: unless-stopped`

Options:
- `no` - Do not automatically restart
- `always` - Always restart if stopped
- `unless-stopped` - Always unless explicitly stopped
- `on-failure` - Restart only on failure

---

## 🚨 Troubleshooting

### Services Won't Start

```bash
# Check logs
./docker-compose-manager.sh logs

# Check if ports are in use
lsof -i :5001
lsof -i :3000
lsof -i :9000

# Stop existing containers
docker stop $(docker ps -aq)
```

### Database Connection Issues

```bash
# Test PostgreSQL connection
docker exec -it techbirdsfly-postgres-debug \
  psql -U postgres -c "SELECT 1"

# Check database
docker exec -it techbirdsfly-postgres-debug \
  psql -U postgres -l
```

### Kafka Issues

```bash
# Check Kafka status
docker exec -it techbirdsfly-kafka-debug \
  kafka-broker-api-versions.sh --bootstrap-server localhost:9092

# List topics
docker exec -it techbirdsfly-kafka-debug \
  kafka-topics.sh --list --bootstrap-server localhost:9092
```

### Health Checks Failing

```bash
# Check service health
./docker-compose-manager.sh status

# Manual health check
curl http://localhost:5001/health
curl http://localhost:9000/health
```

---

## 🔄 Common Workflows

### Restart Everything

```bash
./docker-compose-manager.sh down
./docker-compose-manager.sh up
```

### Rebuild After Code Changes

```bash
./docker-compose-manager.sh rebuild
./docker-compose-manager.sh down
./docker-compose-manager.sh up
```

### View Specific Service Logs

```bash
# Auth service
./docker-compose-manager.sh logs auth-service

# Gateway
./docker-compose-manager.sh logs api-gateway

# Follow new logs only
docker-compose -f docker/docker-compose.debug.yml logs -f --tail=50 auth-service
```

### Update Database Schema

```bash
# Access database
docker exec -it techbirdsfly-postgres-debug psql -U postgres

# Run migrations (service-specific)
docker exec -it techbirdsfly-auth-service-debug \
  dotnet ef database update
```

---

## 📊 Docker Compose File Structure

```yaml
version: '3.9'

services:
  # Infrastructure services
  postgres          # Primary database
  kafka            # Message broker
  zookeeper        # Kafka coordination
  schema-registry  # Schema management
  redis            # Cache
  mongodb          # NoSQL
  seq              # Logging
  jaeger           # Tracing
  
  # Microservices (12 total)
  auth-service
  user-service
  billing-service
  event-bus-service
  generator-service
  admin-service
  image-service
  export-service
  project-service
  cache-service
  media-service
  
  # Gateway & Frontend
  api-gateway
  frontend

volumes:
  postgres_data_debug
  kafka_data_debug
  redis_data_debug
  mongo_data_debug

networks:
  techbirdsfly_debug
    driver: bridge
    subnet: 172.25.0.0/16
```

---

## 🎯 Service Port Reference

| Service | Port | Type |
|---------|------|------|
| **Frontend** | 3000 | Web |
| **API Gateway** | 9000 | HTTP/gRPC |
| **Auth Service** | 5001 | HTTP |
| **Billing Service** | 5002 | HTTP |
| **Generator Service** | 5003 | HTTP |
| **Export Service** | 5004 | HTTP |
| **Admin Service** | 5006 | HTTP |
| **Image Service** | 5007 | HTTP |
| **User Service** | 5008 | HTTP |
| **Project Service** | 5009 | HTTP |
| **Event Bus Service** | 5020 | HTTP |
| **Cache Service** | 5021 | HTTP |
| **Media Service** | 5022 | HTTP |
| **PostgreSQL** | 5433 | Database |
| **Redis** | 6379 | Cache |
| **MongoDB** | 27017 | Database |
| **Kafka** | 9092 | Message Broker |
| **Zookeeper** | 2181 | Coordination |
| **Schema Registry** | 8081 | Schema |
| **Seq** | 5341 | Logging |
| **Jaeger** | 16686 | Tracing |

---

## 🔐 Security Notes

⚠️ **For Development Only**

- Default credentials (`postgres123`) should NOT be used in production
- Use strong, unique passwords in production
- Enable network policies and firewalls
- Use secrets management (Azure Key Vault, etc.)
- Enable TLS/SSL for all connections
- Restrict port exposure

---

## 📝 Dockerfile Details

### Standard Dockerfile Pattern

All services follow this pattern:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

# Publish stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Health check
HEALTHCHECK --interval=10s --timeout=5s --retries=3 --start-period=30s \
    CMD curl -f http://localhost:PORT/health || exit 1

ENTRYPOINT ["dotnet", "ServiceName.dll"]
```

### Frontend Dockerfile

Multi-stage build:
1. Build stage (node:20-alpine) - Run `npm run build`
2. Runtime stage (node:20-alpine) - Run production server

---

## 🎓 Next Steps

1. **Verify all services are running**: `./docker-compose-manager.sh status`
2. **Check logs for errors**: `./docker-compose-manager.sh logs`
3. **Access frontend**: `http://localhost:3000`
4. **Review swagger docs**: `http://localhost:9000/swagger`
5. **Monitor logs**: `http://localhost:5341` (Seq)
6. **View traces**: `http://localhost:16686` (Jaeger)

---

## 📚 Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [Node.js Docker Images](https://hub.docker.com/_/node)

---

## 🆘 Support

If you encounter issues:

1. Check logs: `./docker-compose-manager.sh logs`
2. Check service health: `./docker-compose-manager.sh status`
3. Review Docker events: `docker events`
4. Inspect containers: `docker inspect [container-name]`
5. Check disk space: `docker system df`

---

**Last Updated**: November 27, 2025
**Version**: 1.0
**Status**: Production Ready
