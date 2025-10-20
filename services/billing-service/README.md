# Billing Service# Billing Service



Handles usage tracking, billing calculations, and Stripe payment integration for TechBirdsFly.AI.Handles usage tracking, billing, and Stripe payment integration.



## Status## Responsibilities



✅ **Phase 1** - Fully scaffolded and operational- Usage tracking and metering

- Billing calculations

## Overview- Invoice generation

- Stripe payment processing

The Billing Service manages:- Subscription management

- **Billing Accounts** - User subscription and payment status- Usage alerts and limits

- **Usage Tracking** - Track website generations, image creations, etc.

- **Invoices** - Monthly billing and payment records## API Endpoints

- **Quotas** - Monthly generation limits per subscription tier

- **Stripe Integration** - Payment processing and webhooks```

GET    /api/billing/user/{userId}           - Get billing info

## ArchitectureGET    /api/billing/invoices/{userId}       - List invoices

POST   /api/billing/usage/track             - Track usage event

### Data ModelsGET    /api/billing/usage/{userId}/current  - Get current usage

POST   /api/billing/subscribe               - Create subscription

#### BillingAccountPOST   /api/billing/webhook                 - Stripe webhook

- Id (Guid)```

- UserId (Guid)

- StripeCustomerId (string)## Database

- SubscriptionStatus (active, inactive, cancelled)

- PlanType (free, pro, enterprise)- **Primary DB**: SQL Server / PostgreSQL

- MonthlyGenerations (int)- **Tables**: BillingAccounts, Invoices, UsageMetrics, StripeEvents

- MonthlyGenerationsLimit (int)- **Cache**: Redis (usage counters, rates)

- MonthlyBill (decimal)

- CreatedAt, UpdatedAt, CancelledAt## Dependencies



#### UsageMetric- Auth Service (JWT validation)

- Id (Guid)- Stripe API

- UserId (Guid)- User Service (quota updates)

- EventType (website_generated, image_generated, etc.)- Generator Service (usage events)

- Count (int)

- CostPerUnit (decimal)## Status

- TotalCost (decimal)

- EventDate (DateTime)🟡 **Phase 2** - Scaffolding ready, implementation pending

- Metadata (string - JSON)

## Local Development

#### Invoice

- Id (Guid)```bash

- BillingAccountId (Guid)cd src

- StripeInvoiceId (string)dotnet restore

- Amount (decimal)dotnet run --urls http://localhost:5005

- BilledDate, DueDate (DateTime)```

- Status (draft, open, paid, uncollectible, void)

## Environment Variables

## API Endpoints

```

### Billing AccountConnectionStrings__BillingDb=Data Source=billing.db

Stripe__ApiKey=sk_test_...

```Stripe__WebhookSecret=whsec_...

GET  /api/billing/user/{userId}Jwt__Key=your-secret-key

     Returns billing account info (creates if doesn't exist)```

```

## Related

### Usage Tracking

- [Architecture](/docs/architecture.md)

```- [API Docs](/docs/README.md)

POST /api/billing/track-usage
     Body: { userId, eventType, count, costPerUnit }
     Tracks usage event and updates billing

GET  /api/billing/usage/{userId}
     Returns current month usage and quota status
```

### Invoices

```
GET  /api/billing/invoices/{userId}
     Returns list of user invoices
```

### Webhooks

```
POST /api/billing/webhook/stripe
     Receives Stripe webhook events
```

## Database

- **Type**: SQLite (development), SQL Server (production)
- **Tables**: BillingAccounts, Invoices, UsageMetrics
- **Migrations**: EF Core Code-First
- **File**: `billing.db`

## Configuration

```json
{
  "ConnectionStrings": {
    "BillingDb": "Data Source=billing.db"
  },
  "Jwt": {
    "Key": "your-secret-key",
    "Issuer": "TechBirdsFly"
  },
  "Stripe": {
    "ApiKey": "sk_test_...",
    "WebhookSecret": "whsec_..."
  }
}
```

## Local Development

### Prerequisites
- .NET 8.0 SDK
- SQLite (included with .NET)

### Setup
```bash
cd src/BillingService

dotnet restore
dotnet build
dotnet run --urls http://localhost:5005
```

### Access
- Swagger UI: http://localhost:5005/swagger
- API: http://localhost:5005/api/billing/

### Test
Use `BillingService.http` in VS Code REST Client extension:
```http
GET http://localhost:5005/api/billing/user/550e8400-e29b-41d4-a716-446655440000
```

## Dependencies

### NuGet Packages
- Microsoft.EntityFrameworkCore.Sqlite 9.0.10
- Microsoft.EntityFrameworkCore.Design 9.0.10
- Microsoft.AspNetCore.Authentication.JwtBearer 8.0.8
- System.IdentityModel.Tokens.Jwt 8.14.0

### Services
- Auth Service (JWT validation)
- Generator Service (usage event publishing)
- User Service (quota updates)

## Phase 2 Implementation

- [ ] Real Stripe API integration (payment processing)
- [ ] Monthly invoice generation job
- [ ] Webhook event processing from Stripe
- [ ] Email invoice delivery
- [ ] Usage alerts and notifications
- [ ] Subscription upgrades/downgrades
- [ ] Refund processing
- [ ] Analytics dashboard
- [ ] RabbitMQ message consumer for usage events

## Testing

### Example Usage Flow
```bash
# 1. Get/Create billing account
curl http://localhost:5005/api/billing/user/{userId}

# 2. Track website generation (1 @ $0.50)
curl -X POST http://localhost:5005/api/billing/track-usage \
  -H "Content-Type: application/json" \
  -d '{"userId":"{userId}","eventType":"website_generated","count":1,"costPerUnit":0.50}'

# 3. Track image generation (3 @ $0.10 each)
curl -X POST http://localhost:5005/api/billing/track-usage \
  -H "Content-Type: application/json" \
  -d '{"userId":"{userId}","eventType":"image_generated","count":3,"costPerUnit":0.10}'

# 4. Check usage
curl http://localhost:5005/api/billing/usage/{userId}

# 5. Get invoices
curl http://localhost:5005/api/billing/invoices/{userId}
```

## Deployment

### Docker Build
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY src/*.csproj ./
RUN dotnet restore
COPY src/ ./
RUN dotnet publish -c Release -o publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5005
ENTRYPOINT ["dotnet", "BillingService.dll"]
```

### Environment Variables (Production)
```bash
ConnectionStrings__BillingDb=<sql-server-connection>
Jwt__Key=<your-jwt-key>
Jwt__Issuer=TechBirdsFly
Stripe__ApiKey=<stripe-api-key>
Stripe__WebhookSecret=<stripe-webhook-secret>
ASPNETCORE_ENVIRONMENT=Production
```

## Build Status

✅ Builds successfully  
✅ Database migrations created  
✅ Service runs on port 5005  
✅ Swagger documentation available  
✅ Ready for testing

## Related Documentation

- [Main README](/README.md)
- [Architecture](/docs/architecture.md)
- [Quick Start](/QUICK_START.md)

---

**Framework**: .NET 8.0  
**Port**: 5005 (local)  
**Status**: ✅ Operational
