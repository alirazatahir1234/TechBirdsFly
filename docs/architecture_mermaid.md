# TechBirdsFly Architecture — Mermaid Diagrams

## System Diagram

```mermaid
flowchart LR
  subgraph Frontend
    A[React SPA] -->|HTTP| G[API Gateway]
  end
  subgraph Services
    G --> Auth[Auth Service]
    G --> User[User/Profile Service]
    G --> Gen[Generator Service]
    G --> Img[Image Service]
    G --> Billing[Billing Service]
    G --> Admin[Admin Service]
  end
  subgraph Infra
    Gen --> Bus[Event Bus (RabbitMQ / Azure SB)]
    Gen --> Blob[Blob Storage]
    Img --> Blob
    Billing --> Stripe[Stripe]
    Auth --> AuthDB[(AuthDB)]
    User --> UserDB[(UserDB)]
    Billing --> BillingDB[(BillingDB)]
  end
```

## Sequence Diagram — Generate website

```mermaid
sequenceDiagram
  participant User
  participant FE as Frontend
  participant GW as Gateway
  participant AuthSvc
  participant GenSvc
  participant Bus as EventBus
  participant Worker
  participant OpenAI
  participant Blob
  participant Billing

  User->>FE: Submit prompt
  FE->>GW: POST /api/projects {prompt}
  GW->>AuthSvc: validate token
  GW->>GenSvc: POST /projects (creates job)
  GenSvc->>Bus: Publish GenerateWebsiteJob(jobId)
  Worker->>Bus: pickup job
  Worker->>OpenAI: generate code & content
  OpenAI-->>Worker: return content
  Worker->>Blob: upload files (ZIP)
  Blob-->>Worker: artifactUrl
  Worker->>GenSvc: update project status + artifactUrl
  Worker->>Billing: emit usage event
  GenSvc-->>FE: project ready (poll/push)
```
