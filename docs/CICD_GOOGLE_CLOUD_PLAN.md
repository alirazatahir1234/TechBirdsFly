# TechBirdsFly CI/CD Complete Guide - Google Cloud Free Tier

**Date:** January 5, 2026  
**Platform:** Google Cloud Platform (GCP)  
**Tier:** Free Tier  
**Project:** techbirdsfly

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Step 1: Create GCP Project](#step-1-create-gcp-project)
4. [Step 2: Enable Required APIs](#step-2-enable-required-apis)
5. [Step 3: Setup Artifact Registry](#step-3-setup-artifact-registry)
6. [Step 4: Configure Cloud Build](#step-4-configure-cloud-build)
7. [Step 5: Setup Cloud Run Services](#step-5-setup-cloud-run-services)
8. [Step 6: Configure IAM & Service Accounts](#step-6-configure-iam--service-accounts)
9. [Step 7: Create Dockerfiles](#step-7-create-dockerfiles)
10. [Step 8: Create Cloud Build Configuration](#step-8-create-cloud-build-configuration)
11. [Step 9: Setup GitHub Actions Integration](#step-9-setup-github-actions-integration)
12. [Step 10: Deploy & Verify](#step-10-deploy--verify)
13. [Cost Optimization](#cost-optimization)
14. [Troubleshooting](#troubleshooting)

---

## Overview

### Architecture Overview

```
┌──────────────────────────────────────────────────────────────────────┐
│                        CI/CD Pipeline Flow                           │
└──────────────────────────────────────────────────────────────────────┘

┌─────────┐    ┌──────────┐    ┌───────────────┐    ┌─────────────┐
│ GitHub  │───▶│ Cloud    │───▶│   Artifact    │───▶│  Cloud Run  │
│  Push   │    │  Build   │    │   Registry    │    │  Services   │
└─────────┘    └──────────┘    └───────────────┘    └─────────────┘
                    │                                      │
                    │         ┌──────────────┐            │
                    └────────▶│   Tests &    │            │
                              │   Quality    │            │
                              └──────────────┘            │
                                                         │
                              ┌──────────────────────────┘
                              ▼
┌──────────────────────────────────────────────────────────────────────┐
│                      Cloud Run Services                              │
├──────────────────────────────────────────────────────────────────────┤
│  ┌────────────┐  ┌────────────┐  ┌────────────┐  ┌────────────┐    │
│  │   Auth     │  │ Generator  │  │   User     │  │  Frontend  │    │
│  │  Service   │  │  Service   │  │  Service   │  │  (Next.js) │    │
│  └────────────┘  └────────────┘  └────────────┘  └────────────┘    │
└──────────────────────────────────────────────────────────────────────┘
```

### Services to Deploy

| Service | Type | Port | Cloud Run Service Name |
|---------|------|------|------------------------|
| Auth Service | .NET 8 | 5001 | `techbirdsfly-auth` |
| Generator Service | .NET 8 | 5289 | `techbirdsfly-generator` |
| User Service | .NET 8 | 5002 | `techbirdsfly-user` |
| API Gateway | .NET 8 (YARP) | 8000 | `techbirdsfly-gateway` |
| Frontend | Next.js | 3000 | `techbirdsfly-frontend` |

### Google Cloud Free Tier Limits

| Service | Free Tier Allowance |
|---------|---------------------|
| Cloud Run | 2 million requests/month, 360,000 GB-seconds memory |
| Cloud Build | 120 build-minutes/day |
| Artifact Registry | 500 MB storage/month |
| Cloud Storage | 5 GB storage, 1 GB egress/month |

---

## Prerequisites

Before starting, ensure you have:

- [ ] Google Cloud Account (with billing enabled for free tier)
- [ ] GitHub Repository (`alirazatahir1234/TechBirdsFly`)
- [ ] `gcloud` CLI installed locally
- [ ] Docker installed locally (for testing)
- [ ] Node.js 20+ and .NET 8 SDK installed

### Install Google Cloud CLI

```bash
# macOS (using Homebrew)
brew install google-cloud-sdk

# Or download from Google
curl https://sdk.cloud.google.com | bash
exec -l $SHELL

# Verify installation
gcloud version
```

---

## Step 1: Create GCP Project

### 1.1 Create Project via Console

1. Go to [Google Cloud Console](https://console.cloud.google.com)
2. Click **Select a Project** → **New Project**
3. Enter details:
   - **Project Name:** `techbirdsfly`
   - **Project ID:** `techbirdsfly` (or auto-generated)
   - **Organization:** Leave as default
4. Click **Create**

### 1.2 Create Project via CLI

```bash
# Login to Google Cloud
gcloud auth login

# Create the project
gcloud projects create techbirdsfly --name="TechBirdsFly"

# Set as default project
gcloud config set project techbirdsfly

# Verify project
gcloud config get-value project
```

### 1.3 Link Billing Account

```bash
# List billing accounts
gcloud billing accounts list

# Link billing to project (required even for free tier)
gcloud billing projects link techbirdsfly --billing-account=YOUR_BILLING_ACCOUNT_ID
```

Or via Console:
1. Go to **Billing** in Cloud Console
2. Link your project to a billing account
3. Free tier credits apply automatically

---

## Step 2: Enable Required APIs

### 2.1 Enable APIs via Console

1. Go to [APIs & Services](https://console.cloud.google.com/apis/library)
2. Search and enable each API:

**Required APIs:**

| API | Purpose | Search Term |
|-----|---------|-------------|
| Cloud Run API | Run containerized services | "Cloud Run" |
| Artifact Registry API | Store Docker images | "Artifact Registry" |
| Cloud Build API | Build containers | "Cloud Build" |
| IAM API | Identity management | "Identity and Access Management" |
| Secret Manager API | Store secrets | "Secret Manager" |
| Cloud SQL Admin API | Managed PostgreSQL (optional) | "Cloud SQL Admin" |

### 2.2 Enable APIs via CLI (Recommended)

```bash
# Set project
gcloud config set project techbirdsfly

# Enable all required APIs at once
gcloud services enable \
  run.googleapis.com \
  artifactregistry.googleapis.com \
  cloudbuild.googleapis.com \
  iam.googleapis.com \
  secretmanager.googleapis.com \
  compute.googleapis.com \
  cloudresourcemanager.googleapis.com

# Verify enabled APIs
gcloud services list --enabled
```

### 2.3 Expected Output

```
NAME                                 TITLE
run.googleapis.com                   Cloud Run Admin API
artifactregistry.googleapis.com      Artifact Registry API
cloudbuild.googleapis.com            Cloud Build API
iam.googleapis.com                   Identity and Access Management (IAM) API
secretmanager.googleapis.com         Secret Manager API
compute.googleapis.com               Compute Engine API
cloudresourcemanager.googleapis.com  Cloud Resource Manager API
```

---

## Step 3: Setup Artifact Registry

### 3.1 Create Docker Repository

```bash
# Create Artifact Registry repository for Docker images
gcloud artifacts repositories create techbirdsfly-repo \
  --repository-format=docker \
  --location=us-central1 \
  --description="TechBirdsFly Docker images"

# Verify repository
gcloud artifacts repositories list --location=us-central1
```

### 3.2 Configure Docker Authentication

```bash
# Configure Docker to use gcloud for authentication
gcloud auth configure-docker us-central1-docker.pkg.dev

# This adds the registry to your Docker config
cat ~/.docker/config.json
```

### 3.3 Repository URL Format

Your images will be stored at:
```
us-central1-docker.pkg.dev/techbirdsfly/techbirdsfly-repo/IMAGE_NAME:TAG
```

Example image URLs:
- `us-central1-docker.pkg.dev/techbirdsfly/techbirdsfly-repo/auth-service:latest`
- `us-central1-docker.pkg.dev/techbirdsfly/techbirdsfly-repo/generator-service:v1.0.0`
- `us-central1-docker.pkg.dev/techbirdsfly/techbirdsfly-repo/frontend:main`

---

## Step 4: Configure Cloud Build

### 4.1 Grant Cloud Build Permissions

```bash
# Get the Cloud Build service account
PROJECT_NUMBER=$(gcloud projects describe techbirdsfly --format='value(projectNumber)')
CLOUDBUILD_SA="${PROJECT_NUMBER}@cloudbuild.gserviceaccount.com"

echo "Cloud Build Service Account: $CLOUDBUILD_SA"

# Grant Cloud Run Admin role
gcloud projects add-iam-policy-binding techbirdsfly \
  --member="serviceAccount:${CLOUDBUILD_SA}" \
  --role="roles/run.admin"

# Grant Service Account User role
gcloud projects add-iam-policy-binding techbirdsfly \
  --member="serviceAccount:${CLOUDBUILD_SA}" \
  --role="roles/iam.serviceAccountUser"

# Grant Artifact Registry Writer role
gcloud projects add-iam-policy-binding techbirdsfly \
  --member="serviceAccount:${CLOUDBUILD_SA}" \
  --role="roles/artifactregistry.writer"

# Grant Secret Manager Access (for environment variables)
gcloud projects add-iam-policy-binding techbirdsfly \
  --member="serviceAccount:${CLOUDBUILD_SA}" \
  --role="roles/secretmanager.secretAccessor"
```

### 4.2 Create Build Trigger (Optional - for automatic builds)

```bash
# Connect GitHub repository first via Console
# Then create trigger via CLI

gcloud builds triggers create github \
  --name="techbirdsfly-main-trigger" \
  --repo-name="TechBirdsFly" \
  --repo-owner="alirazatahir1234" \
  --branch-pattern="^main$" \
  --build-config="cloudbuild.yaml"
```

---

## Step 5: Setup Cloud Run Services

### 5.1 Create Cloud Run Services (Initial Setup)

We'll create placeholder services that will be updated by CI/CD:

```bash
# Set region
REGION="us-central1"

# Create Auth Service
gcloud run deploy techbirdsfly-auth \
  --image=gcr.io/cloudrun/hello \
  --platform=managed \
  --region=$REGION \
  --allow-unauthenticated \
  --port=5001 \
  --memory=256Mi \
  --cpu=1 \
  --min-instances=0 \
  --max-instances=2

# Create Generator Service
gcloud run deploy techbirdsfly-generator \
  --image=gcr.io/cloudrun/hello \
  --platform=managed \
  --region=$REGION \
  --allow-unauthenticated \
  --port=5289 \
  --memory=512Mi \
  --cpu=1 \
  --min-instances=0 \
  --max-instances=2

# Create API Gateway
gcloud run deploy techbirdsfly-gateway \
  --image=gcr.io/cloudrun/hello \
  --platform=managed \
  --region=$REGION \
  --allow-unauthenticated \
  --port=8000 \
  --memory=256Mi \
  --cpu=1 \
  --min-instances=0 \
  --max-instances=3

# Create Frontend Service
gcloud run deploy techbirdsfly-frontend \
  --image=gcr.io/cloudrun/hello \
  --platform=managed \
  --region=$REGION \
  --allow-unauthenticated \
  --port=3000 \
  --memory=256Mi \
  --cpu=1 \
  --min-instances=0 \
  --max-instances=3
```

### 5.2 Get Service URLs

```bash
# List all Cloud Run services
gcloud run services list --region=us-central1

# Get specific service URL
gcloud run services describe techbirdsfly-frontend --region=us-central1 --format='value(status.url)'
```

---

## Step 6: Configure IAM & Service Accounts

### 6.1 Create Service Account for CI/CD

```bash
# Create service account
gcloud iam service-accounts create techbirdsfly-cicd \
  --display-name="TechBirdsFly CI/CD Service Account" \
  --description="Service account for CI/CD pipeline"

# Get service account email
SA_EMAIL="techbirdsfly-cicd@techbirdsfly.iam.gserviceaccount.com"
```

### 6.2 Assign Roles to Service Account

```bash
# Cloud Run Admin (deploy services)
gcloud projects add-iam-policy-binding techbirdsfly \
  --member="serviceAccount:${SA_EMAIL}" \
  --role="roles/run.admin"

# Artifact Registry Writer (push images)
gcloud projects add-iam-policy-binding techbirdsfly \
  --member="serviceAccount:${SA_EMAIL}" \
  --role="roles/artifactregistry.writer"

# Service Account User (act as service account)
gcloud projects add-iam-policy-binding techbirdsfly \
  --member="serviceAccount:${SA_EMAIL}" \
  --role="roles/iam.serviceAccountUser"

# Storage Admin (for build artifacts)
gcloud projects add-iam-policy-binding techbirdsfly \
  --member="serviceAccount:${SA_EMAIL}" \
  --role="roles/storage.admin"

# Secret Manager Accessor (for secrets)
gcloud projects add-iam-policy-binding techbirdsfly \
  --member="serviceAccount:${SA_EMAIL}" \
  --role="roles/secretmanager.secretAccessor"
```

### 6.3 Create Service Account Key for GitHub Actions

```bash
# Create key file
gcloud iam service-accounts keys create ~/techbirdsfly-cicd-key.json \
  --iam-account=${SA_EMAIL}

# View the key (you'll add this to GitHub Secrets)
cat ~/techbirdsfly-cicd-key.json

# IMPORTANT: This key is sensitive! Add to GitHub Secrets, then delete local file
# rm ~/techbirdsfly-cicd-key.json
```

### 6.4 Add Secrets to GitHub Repository

Go to your GitHub repository → **Settings** → **Secrets and variables** → **Actions**

Add these secrets:

| Secret Name | Value |
|-------------|-------|
| `GCP_PROJECT_ID` | `techbirdsfly` |
| `GCP_SA_KEY` | Contents of `techbirdsfly-cicd-key.json` |
| `GCP_REGION` | `us-central1` |

---

## Step 7: Create Dockerfiles

### 7.1 Auth Service Dockerfile

Create: `services/auth-service/Dockerfile`

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY src/AuthService.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY src/ ./
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5001
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 5001

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5001/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "AuthService.dll"]
```

### 7.2 Generator Service Dockerfile

Create: `services/generator-service/Dockerfile`

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY src/GeneratorService.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY src/ ./
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published files
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5289
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 5289

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:5289/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "GeneratorService.dll"]
```

### 7.3 API Gateway Dockerfile

Create: `gateway/yarp-gateway/Dockerfile`

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY src/YarpGateway.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY src/ ./
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published files
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8000
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 8000

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8000/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "YarpGateway.dll"]
```

### 7.4 Frontend Dockerfile

Create: `web-frontend/techbirdsfly-frontend-nextjs/Dockerfile`

```dockerfile
# Dependencies stage
FROM node:20-alpine AS deps
RUN apk add --no-cache libc6-compat
WORKDIR /app

# Copy package files
COPY package.json package-lock.json ./
RUN npm ci --only=production

# Builder stage
FROM node:20-alpine AS builder
WORKDIR /app

# Copy dependencies from deps stage
COPY --from=deps /app/node_modules ./node_modules
COPY . .

# Set environment variables for build
ENV NEXT_TELEMETRY_DISABLED=1
ENV NODE_ENV=production

# Build the application
RUN npm run build

# Runner stage
FROM node:20-alpine AS runner
WORKDIR /app

ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1

# Create non-root user
RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 nextjs

# Copy necessary files
COPY --from=builder /app/public ./public
COPY --from=builder /app/.next/standalone ./
COPY --from=builder /app/.next/static ./.next/static

# Set ownership
USER nextjs

# Expose port
EXPOSE 3000

ENV PORT=3000
ENV HOSTNAME="0.0.0.0"

# Start the application
CMD ["node", "server.js"]
```

---

## Step 8: Create Cloud Build Configuration

### 8.1 Main Cloud Build File

Create: `cloudbuild.yaml` (in repository root)

```yaml
# TechBirdsFly Cloud Build Configuration
# Builds and deploys all services to Cloud Run

substitutions:
  _REGION: us-central1
  _REPO: techbirdsfly-repo

options:
  logging: CLOUD_LOGGING_ONLY
  machineType: 'E2_HIGHCPU_8'

steps:
  # ============================================
  # Step 1: Build Auth Service
  # ============================================
  - name: 'gcr.io/cloud-builders/docker'
    id: 'build-auth-service'
    args:
      - 'build'
      - '-t'
      - '${_REGION}-docker.pkg.dev/${PROJECT_ID}/${_REPO}/auth-service:${SHORT_SHA}'
      - '-t'
      - '${_REGION}-docker.pkg.dev/${PROJECT_ID}/${_REPO}/auth-service:latest'
      - '-f'
      - 'services/auth-service/Dockerfile'
      - 'services/auth-service'

  # ============================================
  # Step 2: Build Generator Service
  # ============================================
  - name: 'gcr.io/cloud-builders/docker'
    id: 'build-generator-service'
    args:
      - 'build'
      - '-t'
      - '${_REGION}-docker.pkg.dev/${PROJECT_ID}/${_REPO}/generator-service:${SHORT_SHA}'
      - '-t'
      - '${_REGION}-docker.pkg.dev/${PROJECT_ID}/${_REPO}/generator-service:latest'
      - '-f'
      - 'services/generator-service/Dockerfile'
      - 'services/generator-service'

  # ============================================
  # Step 3: Build API Gateway
  # ============================================
  - name: 'gcr.io/cloud-builders/docker'
    id: 'build-gateway'
    args:
      - 'build'
      - '-t'
      - '${_REGION}-docker.pkg.dev/${PROJECT_ID}/${_REPO}/gateway:${SHORT_SHA}'
      - '-t'
      - '${_REGION}-docker.pkg.dev/${PROJECT_ID}/${_REPO}/gateway:latest'
      - '-f'
      - 'gateway/yarp-gateway/Dockerfile'
      - 'gateway/yarp-gateway'

  # ============================================
  # Step 4: Build Frontend
  # ============================================
  - name: 'gcr.io/cloud-builders/docker'
    id: 'build-frontend'
    args:
      - 'build'
      - '-t'
      - '${_REGION}-docker.pkg.dev/${PROJECT_ID}/${_REPO}/frontend:${SHORT_SHA}'
      - '-t'
      - '${_REGION}-docker.pkg.dev/${PROJECT_ID}/${_REPO}/frontend:latest'
      - '-f'
      - 'web-frontend/techbirdsfly-frontend-nextjs/Dockerfile'
      - 'web-frontend/techbirdsfly-frontend-nextjs'
    waitFor: ['-'] # Run in parallel

  # ============================================
  # Step 5: Push All Images
  # ============================================
  - name: 'gcr.io/cloud-builders/docker'
    id: 'push-auth-service'
    args: ['push', '--all-tags', '${_REGION}-docker.pkg.dev/${PROJECT_ID}/${_REPO}/auth-service']
    waitFor: ['build-auth-service']

  - name: 'gcr.io/cloud-builders/docker'
    id: 'push-generator-service'
    args: ['push', '--all-tags', '${_REGION}-docker.pkg.dev/${PROJECT_ID}/${_REPO}/generator-service']
    waitFor: ['build-generator-service']

  - name: 'gcr.io/cloud-builders/docker'
    id: 'push-gateway'
    args: ['push', '--all-tags', '${_REGION}-docker.pkg.dev/${PROJECT_ID}/${_REPO}/gateway']
    waitFor: ['build-gateway']

  - name: 'gcr.io/cloud-builders/docker'
    id: 'push-frontend'
    args: ['push', '--all-tags', '${_REGION}-docker.pkg.dev/${PROJECT_ID}/${_REPO}/frontend']
    waitFor: ['build-frontend']

  # ============================================
  # Step 6: Deploy to Cloud Run
  # ============================================
  - name: 'gcr.io/google.com/cloudsdktool/cloud-sdk'
    id: 'deploy-auth-service'
    entrypoint: gcloud
    args:
      - 'run'
      - 'deploy'
      - 'techbirdsfly-auth'
      - '--image=${_REGION}-docker.pkg.dev/${PROJECT_ID}/${_REPO}/auth-service:${SHORT_SHA}'
      - '--region=${_REGION}'
      - '--platform=managed'
      - '--port=5001'
      - '--memory=256Mi'
      - '--cpu=1'
      - '--min-instances=0'
      - '--max-instances=2'
      - '--allow-unauthenticated'
    waitFor: ['push-auth-service']

  - name: 'gcr.io/google.com/cloudsdktool/cloud-sdk'
    id: 'deploy-generator-service'
    entrypoint: gcloud
    args:
      - 'run'
      - 'deploy'
      - 'techbirdsfly-generator'
      - '--image=${_REGION}-docker.pkg.dev/${PROJECT_ID}/${_REPO}/generator-service:${SHORT_SHA}'
      - '--region=${_REGION}'
      - '--platform=managed'
      - '--port=5289'
      - '--memory=512Mi'
      - '--cpu=1'
      - '--min-instances=0'
      - '--max-instances=2'
      - '--allow-unauthenticated'
    waitFor: ['push-generator-service']

  - name: 'gcr.io/google.com/cloudsdktool/cloud-sdk'
    id: 'deploy-gateway'
    entrypoint: gcloud
    args:
      - 'run'
      - 'deploy'
      - 'techbirdsfly-gateway'
      - '--image=${_REGION}-docker.pkg.dev/${PROJECT_ID}/${_REPO}/gateway:${SHORT_SHA}'
      - '--region=${_REGION}'
      - '--platform=managed'
      - '--port=8000'
      - '--memory=256Mi'
      - '--cpu=1'
      - '--min-instances=0'
      - '--max-instances=3'
      - '--allow-unauthenticated'
    waitFor: ['push-gateway']

  - name: 'gcr.io/google.com/cloudsdktool/cloud-sdk'
    id: 'deploy-frontend'
    entrypoint: gcloud
    args:
      - 'run'
      - 'deploy'
      - 'techbirdsfly-frontend'
      - '--image=${_REGION}-docker.pkg.dev/${PROJECT_ID}/${_REPO}/frontend:${SHORT_SHA}'
      - '--region=${_REGION}'
      - '--platform=managed'
      - '--port=3000'
      - '--memory=256Mi'
      - '--cpu=1'
      - '--min-instances=0'
      - '--max-instances=3'
      - '--allow-unauthenticated'
    waitFor: ['push-frontend']

# Images to be pushed to Artifact Registry
images:
  - '${_REGION}-docker.pkg.dev/${PROJECT_ID}/${_REPO}/auth-service:${SHORT_SHA}'
  - '${_REGION}-docker.pkg.dev/${PROJECT_ID}/${_REPO}/auth-service:latest'
  - '${_REGION}-docker.pkg.dev/${PROJECT_ID}/${_REPO}/generator-service:${SHORT_SHA}'
  - '${_REGION}-docker.pkg.dev/${PROJECT_ID}/${_REPO}/generator-service:latest'
  - '${_REGION}-docker.pkg.dev/${PROJECT_ID}/${_REPO}/gateway:${SHORT_SHA}'
  - '${_REGION}-docker.pkg.dev/${PROJECT_ID}/${_REPO}/gateway:latest'
  - '${_REGION}-docker.pkg.dev/${PROJECT_ID}/${_REPO}/frontend:${SHORT_SHA}'
  - '${_REGION}-docker.pkg.dev/${PROJECT_ID}/${_REPO}/frontend:latest'

timeout: 1800s # 30 minutes
```

---

## Step 9: Setup GitHub Actions Integration

### 9.1 Update GitHub Actions Workflow

Replace: `.github/workflows/ci.yml`

```yaml
name: TechBirdsFly CI/CD Pipeline

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

env:
  NODE_VERSION: '20'
  DOTNET_VERSION: '8.0'
  GCP_REGION: us-central1
  GCP_PROJECT_ID: techbirdsfly
  ARTIFACT_REGISTRY: us-central1-docker.pkg.dev/techbirdsfly/techbirdsfly-repo

jobs:
  # ============================================
  # Job 1: Lint and Type Check
  # ============================================
  lint:
    name: 🔍 Lint & Type Check
    runs-on: ubuntu-latest
    
    steps:
      - name: 📥 Checkout code
        uses: actions/checkout@v4
      
      - name: 📦 Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
          cache: 'npm'
          cache-dependency-path: web-frontend/techbirdsfly-frontend-nextjs/package-lock.json
      
      - name: 📥 Install dependencies
        working-directory: web-frontend/techbirdsfly-frontend-nextjs
        run: npm ci
      
      - name: 🔍 Run ESLint
        working-directory: web-frontend/techbirdsfly-frontend-nextjs
        run: npm run lint
      
      - name: 📝 Check TypeScript
        working-directory: web-frontend/techbirdsfly-frontend-nextjs
        run: npx tsc --noEmit

  # ============================================
  # Job 2: Build and Test .NET Services
  # ============================================
  build-dotnet:
    name: 🔨 Build .NET Services
    runs-on: ubuntu-latest
    
    steps:
      - name: 📥 Checkout code
        uses: actions/checkout@v4
      
      - name: 📦 Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      
      - name: 📦 Restore dependencies
        run: dotnet restore TechBirdsFly.sln
      
      - name: 🔨 Build solution
        run: dotnet build TechBirdsFly.sln --configuration Release --no-restore
      
      - name: 🧪 Run tests
        run: dotnet test TechBirdsFly.sln --configuration Release --no-build --verbosity normal
        continue-on-error: true

  # ============================================
  # Job 3: Build and Test Frontend
  # ============================================
  build-frontend:
    name: 🎨 Build Frontend
    runs-on: ubuntu-latest
    
    steps:
      - name: 📥 Checkout code
        uses: actions/checkout@v4
      
      - name: 📦 Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
          cache: 'npm'
          cache-dependency-path: web-frontend/techbirdsfly-frontend-nextjs/package-lock.json
      
      - name: 📥 Install dependencies
        working-directory: web-frontend/techbirdsfly-frontend-nextjs
        run: npm ci
      
      - name: 🔨 Build frontend
        working-directory: web-frontend/techbirdsfly-frontend-nextjs
        run: npm run build
        env:
          NEXT_PUBLIC_API_BASE: https://techbirdsfly-gateway-xxxxx-uc.a.run.app/api
          NEXT_PUBLIC_GATEWAY_URL: https://techbirdsfly-gateway-xxxxx-uc.a.run.app
      
      - name: 🧪 Run tests
        working-directory: web-frontend/techbirdsfly-frontend-nextjs
        run: npm test -- --run
        continue-on-error: true

  # ============================================
  # Job 4: Deploy to Google Cloud (Main branch only)
  # ============================================
  deploy-gcp:
    name: 🚀 Deploy to Google Cloud
    runs-on: ubuntu-latest
    needs: [lint, build-dotnet, build-frontend]
    if: github.ref == 'refs/heads/main' && github.event_name == 'push'
    
    permissions:
      contents: read
      id-token: write
    
    steps:
      - name: 📥 Checkout code
        uses: actions/checkout@v4
      
      - name: 🔐 Authenticate to Google Cloud
        uses: google-github-actions/auth@v2
        with:
          credentials_json: ${{ secrets.GCP_SA_KEY }}
      
      - name: ☁️ Setup Google Cloud SDK
        uses: google-github-actions/setup-gcloud@v2
        with:
          project_id: ${{ env.GCP_PROJECT_ID }}
      
      - name: 🐳 Configure Docker for Artifact Registry
        run: |
          gcloud auth configure-docker ${{ env.GCP_REGION }}-docker.pkg.dev --quiet
      
      # Build and Push Auth Service
      - name: 🔨 Build Auth Service Image
        run: |
          docker build \
            -t ${{ env.ARTIFACT_REGISTRY }}/auth-service:${{ github.sha }} \
            -t ${{ env.ARTIFACT_REGISTRY }}/auth-service:latest \
            -f services/auth-service/Dockerfile \
            services/auth-service
      
      - name: 📤 Push Auth Service Image
        run: |
          docker push ${{ env.ARTIFACT_REGISTRY }}/auth-service:${{ github.sha }}
          docker push ${{ env.ARTIFACT_REGISTRY }}/auth-service:latest
      
      # Build and Push Generator Service
      - name: 🔨 Build Generator Service Image
        run: |
          docker build \
            -t ${{ env.ARTIFACT_REGISTRY }}/generator-service:${{ github.sha }} \
            -t ${{ env.ARTIFACT_REGISTRY }}/generator-service:latest \
            -f services/generator-service/Dockerfile \
            services/generator-service
      
      - name: 📤 Push Generator Service Image
        run: |
          docker push ${{ env.ARTIFACT_REGISTRY }}/generator-service:${{ github.sha }}
          docker push ${{ env.ARTIFACT_REGISTRY }}/generator-service:latest
      
      # Build and Push Gateway
      - name: 🔨 Build Gateway Image
        run: |
          docker build \
            -t ${{ env.ARTIFACT_REGISTRY }}/gateway:${{ github.sha }} \
            -t ${{ env.ARTIFACT_REGISTRY }}/gateway:latest \
            -f gateway/yarp-gateway/Dockerfile \
            gateway/yarp-gateway
      
      - name: 📤 Push Gateway Image
        run: |
          docker push ${{ env.ARTIFACT_REGISTRY }}/gateway:${{ github.sha }}
          docker push ${{ env.ARTIFACT_REGISTRY }}/gateway:latest
      
      # Build and Push Frontend
      - name: 🔨 Build Frontend Image
        run: |
          docker build \
            -t ${{ env.ARTIFACT_REGISTRY }}/frontend:${{ github.sha }} \
            -t ${{ env.ARTIFACT_REGISTRY }}/frontend:latest \
            -f web-frontend/techbirdsfly-frontend-nextjs/Dockerfile \
            web-frontend/techbirdsfly-frontend-nextjs
      
      - name: 📤 Push Frontend Image
        run: |
          docker push ${{ env.ARTIFACT_REGISTRY }}/frontend:${{ github.sha }}
          docker push ${{ env.ARTIFACT_REGISTRY }}/frontend:latest
      
      # Deploy Services to Cloud Run
      - name: 🚀 Deploy Auth Service
        run: |
          gcloud run deploy techbirdsfly-auth \
            --image=${{ env.ARTIFACT_REGISTRY }}/auth-service:${{ github.sha }} \
            --region=${{ env.GCP_REGION }} \
            --platform=managed \
            --port=5001 \
            --memory=256Mi \
            --cpu=1 \
            --min-instances=0 \
            --max-instances=2 \
            --allow-unauthenticated
      
      - name: 🚀 Deploy Generator Service
        run: |
          gcloud run deploy techbirdsfly-generator \
            --image=${{ env.ARTIFACT_REGISTRY }}/generator-service:${{ github.sha }} \
            --region=${{ env.GCP_REGION }} \
            --platform=managed \
            --port=5289 \
            --memory=512Mi \
            --cpu=1 \
            --min-instances=0 \
            --max-instances=2 \
            --allow-unauthenticated
      
      - name: 🚀 Deploy Gateway
        run: |
          gcloud run deploy techbirdsfly-gateway \
            --image=${{ env.ARTIFACT_REGISTRY }}/gateway:${{ github.sha }} \
            --region=${{ env.GCP_REGION }} \
            --platform=managed \
            --port=8000 \
            --memory=256Mi \
            --cpu=1 \
            --min-instances=0 \
            --max-instances=3 \
            --allow-unauthenticated
      
      - name: 🚀 Deploy Frontend
        run: |
          gcloud run deploy techbirdsfly-frontend \
            --image=${{ env.ARTIFACT_REGISTRY }}/frontend:${{ github.sha }} \
            --region=${{ env.GCP_REGION }} \
            --platform=managed \
            --port=3000 \
            --memory=256Mi \
            --cpu=1 \
            --min-instances=0 \
            --max-instances=3 \
            --allow-unauthenticated
      
      - name: 📋 Get Service URLs
        run: |
          echo "🎉 Deployment Complete!"
          echo ""
          echo "Service URLs:"
          echo "============="
          gcloud run services describe techbirdsfly-auth --region=${{ env.GCP_REGION }} --format='value(status.url)' | xargs -I {} echo "Auth Service: {}"
          gcloud run services describe techbirdsfly-generator --region=${{ env.GCP_REGION }} --format='value(status.url)' | xargs -I {} echo "Generator Service: {}"
          gcloud run services describe techbirdsfly-gateway --region=${{ env.GCP_REGION }} --format='value(status.url)' | xargs -I {} echo "Gateway: {}"
          gcloud run services describe techbirdsfly-frontend --region=${{ env.GCP_REGION }} --format='value(status.url)' | xargs -I {} echo "Frontend: {}"
```

---

## Step 10: Deploy & Verify

### 10.1 Manual First Deployment

```bash
# Navigate to project root
cd /Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly

# Submit build to Cloud Build
gcloud builds submit --config=cloudbuild.yaml

# Monitor build progress
gcloud builds list --limit=5

# Get build logs
gcloud builds log BUILD_ID
```

### 10.2 Verify Deployments

```bash
# List all Cloud Run services
gcloud run services list --region=us-central1

# Get service URLs
echo "=== TechBirdsFly Service URLs ==="
echo "Auth: $(gcloud run services describe techbirdsfly-auth --region=us-central1 --format='value(status.url)')"
echo "Generator: $(gcloud run services describe techbirdsfly-generator --region=us-central1 --format='value(status.url)')"
echo "Gateway: $(gcloud run services describe techbirdsfly-gateway --region=us-central1 --format='value(status.url)')"
echo "Frontend: $(gcloud run services describe techbirdsfly-frontend --region=us-central1 --format='value(status.url)')"
```

### 10.3 Test Endpoints

```bash
# Get Gateway URL
GATEWAY_URL=$(gcloud run services describe techbirdsfly-gateway --region=us-central1 --format='value(status.url)')

# Test health endpoint
curl -s "$GATEWAY_URL/health"

# Test auth endpoint
curl -X POST "$GATEWAY_URL/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!","name":"Test User"}'
```

---

## Cost Optimization

### Free Tier Maximization Tips

1. **Set min-instances=0** for all services (scales to zero when not in use)
2. **Use small memory** allocations (256Mi for most services)
3. **Limit max-instances** to prevent runaway scaling
4. **Use regional deployments** (us-central1 has best free tier support)
5. **Clean up old images** in Artifact Registry regularly

### Estimated Monthly Costs (Free Tier)

| Resource | Free Tier | Typical Usage | Estimated Cost |
|----------|-----------|---------------|----------------|
| Cloud Run | 2M requests | ~50K requests | $0 |
| Artifact Registry | 500MB | ~200MB | $0 |
| Cloud Build | 120 min/day | ~30 min/day | $0 |
| Cloud Storage | 5GB | ~1GB | $0 |
| **Total** | - | - | **$0/month** |

### Cleanup Commands

```bash
# Delete old images (keep last 3 versions)
gcloud artifacts docker images list \
  us-central1-docker.pkg.dev/techbirdsfly/techbirdsfly-repo/auth-service \
  --sort-by=~CREATE_TIME \
  --limit=100 | tail -n +4 | awk '{print $1}' | xargs -I {} gcloud artifacts docker images delete {} --quiet

# Delete all revisions except latest
gcloud run revisions list --service=techbirdsfly-auth --region=us-central1 \
  | tail -n +2 | head -n -1 | awk '{print $2}' \
  | xargs -I {} gcloud run revisions delete {} --region=us-central1 --quiet
```

---

## Troubleshooting

### Common Issues

#### 1. Build Fails - Permission Denied

```bash
# Re-grant Cloud Build permissions
PROJECT_NUMBER=$(gcloud projects describe techbirdsfly --format='value(projectNumber)')
gcloud projects add-iam-policy-binding techbirdsfly \
  --member="serviceAccount:${PROJECT_NUMBER}@cloudbuild.gserviceaccount.com" \
  --role="roles/run.admin"
```

#### 2. Service Fails to Start

```bash
# Check service logs
gcloud run services logs read techbirdsfly-auth --region=us-central1 --limit=50

# Check revision status
gcloud run revisions describe REVISION_NAME --region=us-central1
```

#### 3. GitHub Actions Fails - Authentication Error

```bash
# Verify service account key is valid
gcloud auth activate-service-account --key-file=techbirdsfly-cicd-key.json
gcloud projects list
```

#### 4. Image Push Fails

```bash
# Re-configure Docker authentication
gcloud auth configure-docker us-central1-docker.pkg.dev

# Verify repository exists
gcloud artifacts repositories list --location=us-central1
```

### Useful Commands

```bash
# View all builds
gcloud builds list --limit=10

# Cancel running build
gcloud builds cancel BUILD_ID

# View service metrics
gcloud run services describe techbirdsfly-frontend --region=us-central1

# Set environment variables
gcloud run services update techbirdsfly-gateway \
  --region=us-central1 \
  --set-env-vars="AUTH_SERVICE_URL=https://techbirdsfly-auth-xxxx.run.app"
```

---

## Quick Reference Card

### Project Info
```
Project ID:     techbirdsfly
Region:         us-central1
Repository:     techbirdsfly-repo
```

### Service URLs (After Deployment)
```
Frontend:   https://techbirdsfly-frontend-xxxx-uc.a.run.app
Gateway:    https://techbirdsfly-gateway-xxxx-uc.a.run.app
Auth:       https://techbirdsfly-auth-xxxx-uc.a.run.app
Generator:  https://techbirdsfly-generator-xxxx-uc.a.run.app
```

### Key Commands
```bash
# Deploy
gcloud builds submit --config=cloudbuild.yaml

# View logs
gcloud run services logs read SERVICE_NAME --region=us-central1

# List services
gcloud run services list --region=us-central1

# Update service
gcloud run deploy SERVICE_NAME --image=IMAGE_URL --region=us-central1
```

### GitHub Secrets
```
GCP_PROJECT_ID:   techbirdsfly
GCP_SA_KEY:       (service account JSON key)
GCP_REGION:       us-central1
```

---

## Next Steps

After completing this setup:

1. ✅ Push code to GitHub `main` branch
2. ✅ Watch GitHub Actions run the CI/CD pipeline
3. ✅ Verify all services are deployed to Cloud Run
4. ✅ Test endpoints using service URLs
5. ✅ Setup custom domain (optional)
6. ✅ Configure Cloud SQL for PostgreSQL (optional)
7. ✅ Setup Cloud Monitoring alerts (recommended)

---

**Document Version:** 1.0  
**Last Updated:** January 5, 2026  
**Author:** TechBirdsFly Team
