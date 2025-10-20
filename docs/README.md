# TechBirdsFly.AI — Documentation

This folder contains architecture notes, diagrams, and infra snippets for the TechBirdsFly AI Website Generator project.

Files:
- `architecture.md` - high-level architecture and responsibilities
- `architecture_mermaid.md` - mermaid diagrams you can paste into markdown to render visuals
- `infra/docker-compose.yml` - local dev compose file (starter)

Next steps:
- Scaffold Auth Service (minimal .NET 8 API with JWT)
- Scaffold Generator Service (minimal job publisher + worker)
- Hook up frontend to local API Gateway (YARP) or direct to services for early testing

Run locally (suggestion):
1. Start infra: `docker compose -f infra/docker-compose.yml up -d`
2. Run services locally from IDE (VS Code / Rider) or build Docker images and run via compose.
