TechBirdsFly.AI — Microservice Architecture

This document captures the microservice architecture, APIs, data ownership, communication patterns, auth model, infra suggestions, and developer guidance for the TechBirdsFly AI Website Generator MVP.

1. High-level service map

```
Frontend (React SPA)
         |
      API Gateway (YARP / Azure API Management)
         |
---------------------------------------------
|     |       |         |         |         |
Auth  User  Generator  Image   Billing  Admin
Svc   Profile  Svc      Svc      Svc      Svc
|     |       |         |         |         |
AuthDB UserDB Blob    (Image Store) BillingDB Logging / Metrics
```

Responsibilities, data ownership, communication patterns, and endpoints are described in the full architecture doc in this folder.
