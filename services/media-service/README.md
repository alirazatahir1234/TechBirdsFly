# Image Service

Generates and manages AI-generated images for website components.

## Responsibilities

- Hero image generation (DALL·E integration)
- Icon generation
- Image storage and retrieval
- Image CDN distribution
- Image caching

## API Endpoints

```
POST   /api/images/generate         - Generate image from prompt
GET    /api/images/{imageId}        - Get image metadata
DELETE /api/images/{imageId}        - Delete image
GET    /api/images/user/{userId}    - List user images
POST   /api/images/{imageId}/cdn    - Get CDN URL
```

## Database

- **Primary DB**: SQL Server / PostgreSQL
- **Tables**: Images, ImageMetadata, ImageCache
- **Storage**: Azure Blob / S3
- **Cache**: Redis (CDN URLs, metadata)

## Dependencies

- Auth Service (JWT validation)
- Azure OpenAI (DALL·E)
- Blob Storage

## Status

🟡 **Phase 2** - Scaffolding ready, implementation pending

## Local Development

```bash
cd src
dotnet restore
dotnet run --urls http://localhost:5004
```

## Environment Variables

```
ConnectionStrings__ImageDb=Data Source=image.db
AzureOpenAI__Endpoint=https://your-instance.openai.azure.com/
AzureOpenAI__ApiKey=your-api-key
BlobStorage__ConnectionString=DefaultEndpointsProtocol=https;...
Jwt__Key=your-secret-key
```

## Related

- [Architecture](/docs/architecture.md)
- [API Docs](/docs/README.md)
