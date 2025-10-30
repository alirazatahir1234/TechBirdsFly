# Redis Cache Service

Shared Redis caching service for all TechBirdsFly microservices.

## Quick Start

### Start Redis

```bash
cd infra/redis
docker-compose up -d
```

### Verify Redis is running

```bash
docker exec -it techbirdsfly-redis redis-cli ping
# Response: PONG
```

## Configuration

- **Port**: 6379
- **Memory**: 256MB (configurable in `redis.conf`)
- **Persistence**: Enabled (AOF + RDB)
- **Eviction Policy**: `allkeys-lru` (removes least recently used keys when full)

## Redis CLI Commands

```bash
# Access Redis CLI
docker exec -it techbirdsfly-redis redis-cli

# View all keys
KEYS *

# Get specific key
GET auth:token:admin

# Monitor in real-time
MONITOR

# Check memory usage
INFO MEMORY

# Flush all data (careful!)
FLUSHALL
```

## Connection String

All services connect to Redis via:
```
redis://techbirdsfly-redis:6379/0
```

Or from host machine:
```
redis://localhost:6379/0
```

## Production Considerations

For production deployments:

1. **Password Protection**: Uncomment `requirepass` in `redis.conf`
2. **Replication**: Set up master-slave replication
3. **Persistence**: Adjust `save` directives based on durability needs
4. **Memory Limits**: Tune `maxmemory` based on workload
5. **Monitoring**: Use RedisInsight or Prometheus exporter
6. **Security**: Run Redis in private VPC, not exposed to internet

## Monitoring

### Using RedisInsight (GUI)

```bash
# Download and run RedisInsight from https://redis.io/insight/
# Connect to localhost:6379
```

### Using redis-cli (CLI)

```bash
# Monitor operations in real-time
docker exec -it techbirdsfly-redis redis-cli MONITOR

# View stats
docker exec -it techbirdsfly-redis redis-cli INFO
```

## Scaling Redis

### For High Availability
- Use Redis Sentinel for automatic failover
- Configure multiple replicas

### For Large Datasets
- Use Redis Cluster for horizontal scaling
- Shard data across multiple Redis instances

### For Cloud Deployments
- Azure: Use Azure Cache for Redis
- AWS: Use ElastiCache for Redis
- GCP: Use Cloud Memorystore for Redis

## Troubleshooting

### Redis connection refused
```bash
# Check if container is running
docker ps | grep redis

# Check logs
docker logs techbirdsfly-redis

# Restart if needed
docker restart techbirdsfly-redis
```

### High memory usage
```bash
# Check memory stats
docker exec -it techbirdsfly-redis redis-cli INFO MEMORY

# Adjust maxmemory in redis.conf
# Restart container to apply changes
```

### Data persistence issues
```bash
# Check AOF file
docker exec -it techbirdsfly-redis ls -la /data

# Verify AOF is writing
docker exec -it techbirdsfly-redis redis-cli CONFIG GET appendonly
```

## References

- [Redis Documentation](https://redis.io/documentation)
- [Redis Best Practices](https://redis.io/docs/management/admin-hints/)
- [StackExchange.Redis Documentation](https://stackexchange.github.io/StackExchange.Redis/)
