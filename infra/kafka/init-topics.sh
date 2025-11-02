#!/bin/bash
# Kafka topics initialization script for TechBirdsFly
# This script creates all necessary topics after Kafka is ready

set -e

echo "⏳ Waiting for Kafka to be ready..."
until kafka-broker-api-versions.sh --bootstrap-server kafka:9092 > /dev/null 2>&1; do
  echo "Waiting for Kafka..."
  sleep 2
done

echo "✅ Kafka is ready!"
echo ""

# Function to create topic
create_topic() {
  local topic=$1
  local partitions=$2
  local replication=$3
  
  echo "📝 Creating topic: $topic (partitions: $partitions, replication: $replication)"
  kafka-topics.sh --create \
    --bootstrap-server kafka:9092 \
    --topic $topic \
    --partitions $partitions \
    --replication-factor $replication \
    --if-not-exists \
    --config retention.ms=604800000 \
    --config compression.type=snappy
}

echo "🚀 Creating Kafka topics..."
echo ""

# User domain events
create_topic "user-events" 3 1
create_topic "user-registered" 3 1
create_topic "user-updated" 3 1
create_topic "user-deactivated" 3 1

# Subscription domain events
create_topic "subscription-events" 3 1
create_topic "subscription-started" 3 1
create_topic "subscription-ended" 3 1
create_topic "subscription-upgraded" 3 1

# Website domain events
create_topic "website-events" 3 1
create_topic "website-generated" 3 1
create_topic "website-published" 3 1
create_topic "website-deleted" 3 1

# Billing events
create_topic "billing-events" 3 1
create_topic "payment-processed" 3 1
create_topic "invoice-created" 3 1

# System events
create_topic "system-events" 3 1
create_topic "health-check" 1 1

echo ""
echo "✅ All topics created successfully!"
echo ""
echo "📊 Topic list:"
kafka-topics.sh --list --bootstrap-server kafka:9092
