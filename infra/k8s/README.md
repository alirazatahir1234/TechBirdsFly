# Kubernetes Manifests

Production Kubernetes deployment configuration for TechBirdsFly.AI

## Files

- `namespace.yaml` - TechBirdsFly namespace
- `secrets.yaml` - API keys, connection strings, JWT secrets
- `configmap.yaml` - Application configuration
- `services/auth-deployment.yaml` - Auth service
- `services/generator-deployment.yaml` - Generator service
- `services/user-deployment.yaml` - User service (Phase 2)
- `services/image-deployment.yaml` - Image service (Phase 2)
- `services/billing-deployment.yaml` - Billing service (Phase 2)
- `services/admin-deployment.yaml` - Admin service (Phase 2)
- `ingress.yaml` - Ingress configuration
- `pvc.yaml` - Persistent volumes for databases

## Deployment

```bash
# Create namespace
kubectl apply -f namespace.yaml

# Create secrets and configmaps
kubectl apply -f secrets.yaml
kubectl apply -f configmap.yaml

# Deploy all services
kubectl apply -f services/

# Deploy ingress
kubectl apply -f ingress.yaml
```

## Current Status

🟡 **Phase 2** - Templates ready for implementation

## Related

- [Docker Compose](/infra/docker-compose.yml)
- [Services](/services/README.md)
