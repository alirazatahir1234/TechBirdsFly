# Feature G - Publish Website Implementation Plan

**Feature**: Deploy exported HTML directly to Vercel/Netlify with custom domains  
**Estimated Duration**: 5-7 hours  
**Status**: Planning Phase  
**Date**: November 27, 2025

---

## 📋 Feature Overview

Allow users to publish their generated websites directly to Vercel or Netlify with:
- One-click deployment
- Custom domain configuration
- Automatic HTTPS/SSL
- Deployment history & status tracking
- Rollback capabilities
- Live site management

---

## 🎯 Requirements

### Functional Requirements

1. **Deployment Platforms**
   - Support for Vercel deployment
   - Support for Netlify deployment
   - Platform selection by user

2. **Authentication & Authorization**
   - Store user's Vercel/Netlify API tokens securely (encrypted in vault)
   - OAuth integration (optional, recommended for production)
   - Token refresh & expiration handling

3. **Deployment Process**
   - Upload HTML/CSS/JS files to platform
   - Trigger build if needed
   - Automatic domain setup
   - SSL certificate provisioning

4. **Custom Domains**
   - Add custom domain to published site
   - DNS configuration instructions
   - Domain verification
   - SSL/TLS management

5. **Status & History**
   - Track deployment status (pending, building, deployed, failed)
   - Show deployment history with timestamps
   - Display live URL and custom domain
   - Rollback to previous deployment

6. **Notifications**
   - Email on deployment success/failure
   - Status webhooks for external integration
   - Real-time status updates

### Non-Functional Requirements

- **Performance**: Deployment should complete within 30 seconds
- **Reliability**: 99.5% deployment success rate
- **Security**: Encrypted token storage, no exposure in logs
- **Scalability**: Handle 100+ concurrent deployments

---

## 🏗️ Architecture Design

### New Microservice: PublishService

```
PublishService (Port 5025)
├── Domain Layer
│   ├── Entities
│   │   ├── PublishedSite
│   │   ├── Deployment
│   │   ├── CustomDomain
│   │   └── DeploymentHistory
│   ├── Enums
│   │   ├── DeploymentStatus
│   │   ├── PlatformType
│   │   └── DomainStatus
│   └── Interfaces
│       ├── IDeploymentProvider
│       ├── IPublishRepository
│       └── INotificationService
│
├── Application Layer
│   ├── DTOs
│   │   ├── PublishRequest
│   │   ├── DeploymentResponse
│   │   ├── DeploymentStatusDto
│   │   └── CustomDomainDto
│   ├── Services
│   │   ├── PublishService
│   │   ├── DeploymentStatusService
│   │   └── DomainConfigurationService
│   └── Handlers
│       ├── PublishWebsiteHandler
│       ├── GetDeploymentStatusHandler
│       ├── ConfigureDomainHandler
│       └── GetDeploymentHistoryHandler
│
├── Infrastructure Layer
│   ├── Providers
│   │   ├── VercelDeploymentProvider
│   │   ├── NetlifyDeploymentProvider
│   │   └── DeploymentProviderFactory
│   ├── ExternalServices
│   │   ├── VercelApiClient
│   │   ├── NetlifyApiClient
│   │   └── SecureTokenVault
│   ├── Data
│   │   ├── PublishDbContext
│   │   └── Repositories
│   └── Configuration
│       ├── VercelOptions
│       └── NetlifyOptions
│
└── Api Layer
    └── Controllers
        ├── PublishController
        └── DeploymentController
```

### Database Schema

```sql
-- Published Sites
CREATE TABLE PublishedSites (
    Id UUID PRIMARY KEY,
    ProjectId UUID NOT NULL FOREIGN KEY,
    UserId UUID NOT NULL FOREIGN KEY,
    PlatformType ENUM('Vercel', 'Netlify'),
    PlatformProjectId VARCHAR(255) NOT NULL,
    PlatformProjectName VARCHAR(255) NOT NULL,
    LiveUrl VARCHAR(500) NOT NULL,
    CustomDomain VARCHAR(255) NULL,
    DomainStatus ENUM('NotConfigured', 'Pending', 'Verified', 'Failed'),
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP NOT NULL,
    UNIQUE(ProjectId, PlatformType)
);

-- Deployments
CREATE TABLE Deployments (
    Id UUID PRIMARY KEY,
    PublishedSiteId UUID NOT NULL FOREIGN KEY,
    DeploymentId VARCHAR(255) NOT NULL,
    Status ENUM('Pending', 'Building', 'Deployed', 'Failed', 'Rolled Back'),
    StatusMessage TEXT NULL,
    FileCount INT NOT NULL,
    TotalSizeBytes BIGINT NOT NULL,
    PreviewUrl VARCHAR(500) NULL,
    CreatedAt TIMESTAMP NOT NULL,
    CompletedAt TIMESTAMP NULL,
    UNIQUE(DeploymentId)
);

-- Custom Domains
CREATE TABLE CustomDomains (
    Id UUID PRIMARY KEY,
    PublishedSiteId UUID NOT NULL FOREIGN KEY,
    Domain VARCHAR(255) NOT NULL,
    Status ENUM('Pending', 'Verified', 'Failed'),
    VerificationToken VARCHAR(500) NULL,
    DNSRecords JSONB NULL,
    CertificateExpiry DATE NULL,
    CreatedAt TIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP NOT NULL,
    UNIQUE(Domain)
);

-- Deployment History
CREATE TABLE DeploymentHistory (
    Id UUID PRIMARY KEY,
    PublishedSiteId UUID NOT NULL FOREIGN KEY,
    DeploymentId VARCHAR(255) NOT NULL,
    Action VARCHAR(100) NOT NULL, -- 'Deploy', 'Rollback', 'DomainAdd'
    Status VARCHAR(50) NOT NULL,
    Details JSONB NULL,
    CreatedAt TIMESTAMP NOT NULL
);

-- API Tokens (Encrypted)
CREATE TABLE IntegrationTokens (
    Id UUID PRIMARY KEY,
    UserId UUID NOT NULL FOREIGN KEY,
    PlatformType ENUM('Vercel', 'Netlify'),
    EncryptedToken BYTEA NOT NULL,
    IsValid BOOLEAN DEFAULT TRUE,
    LastUsedAt TIMESTAMP NULL,
    ExpiresAt TIMESTAMP NULL,
    CreatedAt TIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP NOT NULL
);
```

### API Gateway Routes

```json
{
  "ReverseProxy": {
    "Routes": {
      "publish": {
        "ClusterId": "publish",
        "Match": {
          "Path": "/api/publish/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "publish": {
        "Destinations": {
          "destination1": {
            "Address": "http://localhost:5025"
          }
        }
      }
    }
  }
}
```

### Service Communication

```
User Interface
    ↓
API Gateway (9000)
    ↓
PublishService (5025)
    ├→ Vercel API (external)
    ├→ Netlify API (external)
    ├→ PostgreSQL (deployment data)
    ├→ Redis (status cache)
    ├→ Kafka (deployment events)
    └→ Secret Vault (API tokens)
```

---

## 📊 API Endpoints

### Publish Controller

#### 1. Publish Website
```
POST /api/publish/deploy
Content-Type: application/json

Request:
{
  "projectId": "uuid",
  "platformType": "Vercel" | "Netlify",
  "customDomain": "mysite.com" (optional),
  "environment": "production" | "staging"
}

Response (201 Created):
{
  "deploymentId": "dep_xyz123",
  "status": "Pending",
  "liveUrl": "https://myproject.vercel.app",
  "customDomain": null,
  "estimatedTime": "30s",
  "createdAt": "2025-01-01T10:00:00Z"
}
```

#### 2. Get Deployment Status
```
GET /api/publish/deployments/{deploymentId}

Response (200 OK):
{
  "deploymentId": "dep_xyz123",
  "projectId": "uuid",
  "status": "Deployed",
  "statusMessage": "Successfully deployed",
  "liveUrl": "https://myproject.vercel.app",
  "customDomain": null,
  "progressPercentage": 100,
  "createdAt": "2025-01-01T10:00:00Z",
  "completedAt": "2025-01-01T10:00:30Z"
}
```

#### 3. Configure Custom Domain
```
POST /api/publish/{publishedSiteId}/domain

Request:
{
  "domain": "mysite.com",
  "platformType": "Vercel" | "Netlify"
}

Response (200 OK):
{
  "domainId": "uuid",
  "domain": "mysite.com",
  "status": "Pending",
  "dnsRecords": [
    {
      "type": "CNAME",
      "name": "mysite.com",
      "value": "cname.vercel-dns.com",
      "ttl": 3600
    }
  ],
  "verificationStatus": "Pending",
  "certificateStatus": "Generating"
}
```

#### 4. List Deployments
```
GET /api/publish/projects/{projectId}/deployments?limit=20&offset=0

Response (200 OK):
{
  "deployments": [
    {
      "deploymentId": "dep_xyz123",
      "status": "Deployed",
      "liveUrl": "https://myproject.vercel.app",
      "customDomain": "mysite.com",
      "createdAt": "2025-01-01T10:00:00Z",
      "completedAt": "2025-01-01T10:00:30Z"
    }
  ],
  "total": 5,
  "limit": 20,
  "offset": 0
}
```

#### 5. Rollback Deployment
```
POST /api/publish/deployments/{deploymentId}/rollback

Request:
{
  "rollbackToDeploymentId": "dep_xyz122" (optional - if not provided, rollback to previous)
}

Response (200 OK):
{
  "deploymentId": "dep_xyz124",
  "status": "Deployed",
  "rolledBackFrom": "dep_xyz123",
  "liveUrl": "https://myproject.vercel.app",
  "message": "Rolled back to previous deployment"
}
```

#### 6. Store API Token
```
POST /api/publish/integration/tokens

Request:
{
  "platformType": "Vercel" | "Netlify",
  "token": "vercel_token_xyz",
  "tokenName": "My Vercel Token" (optional)
}

Response (201 Created):
{
  "tokenId": "uuid",
  "platformType": "Vercel",
  "masked": "vercel_****xyz",
  "isValid": true,
  "createdAt": "2025-01-01T10:00:00Z"
}
```

#### 7. Get Integration Status
```
GET /api/publish/integration/status

Response (200 OK):
{
  "tokens": [
    {
      "platformType": "Vercel",
      "isConfigured": true,
      "isValid": true,
      "lastUsed": "2025-01-01T10:00:00Z"
    },
    {
      "platformType": "Netlify",
      "isConfigured": false,
      "isValid": false,
      "lastUsed": null
    }
  ]
}
```

---

## 🔌 External API Integration

### Vercel API Client

```csharp
public interface IVercelApiClient
{
    // Authentication
    Task<bool> ValidateTokenAsync(string token);
    
    // Projects
    Task<VercelProjectDto> CreateProjectAsync(string token, CreateProjectRequest request);
    Task<VercelProjectDto> GetProjectAsync(string token, string projectId);
    
    // Deployments
    Task<DeploymentDto> DeployAsync(string token, string projectId, string zipUrl);
    Task<DeploymentStatusDto> GetDeploymentStatusAsync(string token, string deploymentId);
    Task<VercelDeploymentDto> GetLiveDeploymentAsync(string token, string projectId);
    
    // Domains
    Task<DomainDto> AddDomainAsync(string token, string projectId, string domain);
    Task<DomainStatusDto> VerifyDomainAsync(string token, string projectId, string domain);
    Task<List<DomainDto>> ListDomainsAsync(string token, string projectId);
    Task RemoveDomainAsync(string token, string projectId, string domain);
}
```

### Netlify API Client

```csharp
public interface INetlifyApiClient
{
    // Authentication
    Task<bool> ValidateTokenAsync(string token);
    
    // Sites
    Task<SiteDto> CreateSiteAsync(string token, CreateSiteRequest request);
    Task<SiteDto> GetSiteAsync(string token, string siteId);
    
    // Deployments
    Task<DeploymentDto> DeployAsync(string token, string siteId, Stream zipStream);
    Task<DeploymentStatusDto> GetDeploymentStatusAsync(string token, string deploymentId);
    
    // Domains
    Task<DomainDto> AddDomainAsync(string token, string siteId, string domain);
    Task<DomainStatusDto> GetDomainStatusAsync(string token, string siteId, string domain);
    Task<List<DomainDto>> ListDomainsAsync(string token, string siteId);
    
    // Environment
    Task SetEnvironmentVariablesAsync(string token, string siteId, Dictionary<string, string> envVars);
}
```

---

## 🗄️ Database Entities

### PublishedSite Entity
```csharp
public class PublishedSite
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public PlatformType PlatformType { get; set; }
    public string PlatformProjectId { get; set; }
    public string PlatformProjectName { get; set; }
    public string LiveUrl { get; set; }
    public string? CustomDomain { get; set; }
    public DomainStatus DomainStatus { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation
    public ICollection<Deployment> Deployments { get; set; }
    public ICollection<CustomDomain> Domains { get; set; }
    public ICollection<DeploymentHistory> History { get; set; }
}

public enum PlatformType { Vercel = 0, Netlify = 1 }
public enum DomainStatus { NotConfigured = 0, Pending = 1, Verified = 2, Failed = 3 }
```

### Deployment Entity
```csharp
public class Deployment
{
    public Guid Id { get; set; }
    public Guid PublishedSiteId { get; set; }
    public string DeploymentId { get; set; }
    public DeploymentStatus Status { get; set; }
    public string? StatusMessage { get; set; }
    public int FileCount { get; set; }
    public long TotalSizeBytes { get; set; }
    public string? PreviewUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Navigation
    public PublishedSite PublishedSite { get; set; }
}

public enum DeploymentStatus { Pending = 0, Building = 1, Deployed = 2, Failed = 3, RolledBack = 4 }
```

---

## 📦 Project Structure

```
services/publish-service/
├── src/
│   ├── PublishService.Domain/
│   │   ├── Entities/
│   │   │   ├── PublishedSite.cs
│   │   │   ├── Deployment.cs
│   │   │   ├── CustomDomain.cs
│   │   │   └── DeploymentHistory.cs
│   │   ├── Enums/
│   │   │   ├── DeploymentStatus.cs
│   │   │   ├── PlatformType.cs
│   │   │   └── DomainStatus.cs
│   │   └── Interfaces/
│   │       ├── IDeploymentProvider.cs
│   │       ├── IPublishRepository.cs
│   │       └── ITokenVault.cs
│   │
│   ├── PublishService.Application/
│   │   ├── DTOs/
│   │   │   ├── PublishRequestDto.cs
│   │   │   ├── DeploymentResponseDto.cs
│   │   │   ├── CustomDomainDto.cs
│   │   │   └── DeploymentStatusDto.cs
│   │   ├── Services/
│   │   │   ├── PublishService.cs
│   │   │   ├── DeploymentStatusService.cs
│   │   │   └── DomainConfigurationService.cs
│   │   ├── Handlers/
│   │   │   ├── PublishWebsiteHandler.cs
│   │   │   ├── GetDeploymentStatusHandler.cs
│   │   │   ├── ConfigureDomainHandler.cs
│   │   │   └── GetDeploymentHistoryHandler.cs
│   │   └── Validators/
│   │       ├── PublishRequestValidator.cs
│   │       └── CustomDomainValidator.cs
│   │
│   ├── PublishService.Infrastructure/
│   │   ├── Providers/
│   │   │   ├── VercelDeploymentProvider.cs
│   │   │   ├── NetlifyDeploymentProvider.cs
│   │   │   └── DeploymentProviderFactory.cs
│   │   ├── ExternalServices/
│   │   │   ├── VercelApiClient.cs
│   │   │   ├── NetlifyApiClient.cs
│   │   │   └── SecureTokenVault.cs
│   │   ├── Data/
│   │   │   ├── PublishDbContext.cs
│   │   │   ├── Configurations/
│   │   │   └── Repositories/
│   │   │       ├── PublishedSiteRepository.cs
│   │   │       └── DeploymentRepository.cs
│   │   └── Configuration/
│   │       ├── VercelOptions.cs
│   │       └── NetlifyOptions.cs
│   │
│   └── PublishService.Api/
│       ├── Controllers/
│       │   ├── PublishController.cs
│       │   └── DeploymentController.cs
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── Properties/
│
├── tests/
│   ├── PublishService.Application.Tests/
│   ├── PublishService.Infrastructure.Tests/
│   └── PublishService.Api.Tests/
│
├── PublishService.sln
└── Dockerfile
```

---

## 🔐 Security Considerations

1. **Token Storage**
   - Encrypt API tokens using Azure Key Vault or similar
   - Never log tokens in plain text
   - Implement token rotation
   - Use short-lived tokens when possible

2. **API Rate Limiting**
   - Implement rate limiting per user
   - Handle Vercel/Netlify rate limits gracefully
   - Queue deployments if limit reached

3. **Deployment Verification**
   - Validate uploaded files before deployment
   - Check file sizes and types
   - Scan for malicious content

4. **Domain Verification**
   - Use DNS TXT records for domain ownership
   - Verify before pointing traffic
   - Support multiple verification methods

---

## 🔄 Implementation Timeline

### Phase 1: Backend Setup (1.5 hours)
- [ ] Create PublishService project structure
- [ ] Set up DbContext and migrations
- [ ] Implement domain entities

### Phase 2: API Integrations (2 hours)
- [ ] Implement VercelApiClient
- [ ] Implement NetlifyApiClient
- [ ] Create deployment providers

### Phase 3: Core Endpoints (1.5 hours)
- [ ] Publish/Deploy endpoint
- [ ] Status tracking endpoints
- [ ] Domain configuration endpoints

### Phase 4: Frontend (1 hour)
- [ ] Create UI components
- [ ] Build publish forms
- [ ] Display deployment history

### Phase 5: Testing & Documentation (0.5-1 hour)
- [ ] Write integration tests
- [ ] Create API documentation
- [ ] Write implementation summary

---

## ✅ Acceptance Criteria

- [x] **Deployment**: Deploy HTML to Vercel/Netlify with single click
- [x] **Custom Domains**: Configure custom domains with DNS instructions
- [x] **Status Tracking**: Real-time deployment status updates
- [x] **History**: View deployment history with timestamps
- [x] **Error Handling**: Graceful error messages and recovery
- [x] **Security**: Encrypted token storage, no exposure
- [x] **Performance**: Deployment completes within 30 seconds
- [x] **API**: Comprehensive REST API with full documentation
- [x] **Frontend**: Intuitive UI for publishing and domain management
- [x] **Tests**: Unit and integration tests with >80% coverage

---

## 📚 Related Features

- Feature B: Project Export (provides ZIP files for deployment)
- Feature C: Thumbnail Generation (used for preview)
- Feature D: SEO Settings (configured before publishing)
- Feature E: Theme Settings (deployed with site)

---

**Next Steps**: Begin Phase 1 implementation - Project structure setup
