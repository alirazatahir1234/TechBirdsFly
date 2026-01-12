# Google Cloud Deployment Guide for TechBirdsFly

## 🎯 Overview

This guide provides step-by-step instructions for deploying TechBirdsFly to Google Cloud Platform (GCP) Free Tier.

**Project Details:**
- **GCP Project:** `techbirdsfly`
- **Region:** `us-central1`
- **Services:** Cloud Run, Artifact Registry, Cloud Build, Cloud SQL (optional)

---

## 📋 Prerequisites

### 1. Google Cloud CLI Installation

```bash
# macOS (using Homebrew)
brew install --cask google-cloud-sdk

# Initialize and login
gcloud init
gcloud auth login
```

### 2. Set Project

```bash
gcloud config set project techbirdsfly
```

### 3. Enable Required APIs

```bash
gcloud services enable \
  run.googleapis.com \
  cloudbuild.googleapis.com \
  artifactregistry.googleapis.com \
  secretmanager.googleapis.com \
  iam.googleapis.com
```

### 4. Create Artifact Registry Repository

```bash
gcloud artifacts repositories create techbirdsfly-repo \
  --repository-format=docker \
  --location=us-central1 \
  --description="TechBirdsFly Docker images"
```

---

## 🔐 GitHub Secrets Configuration

You need to set up these secrets in your GitHub repository:

| Secret Name | Description | How to Get |
|-------------|-------------|------------|
| `GCP_PROJECT_ID` | Your GCP project ID | `techbirdsfly` |
| `GCP_SERVICE_ACCOUNT_KEY` | Service account JSON key | See below |
| `JWT_SECRET_KEY` | JWT signing key | Generate: `openssl rand -base64 64` |
| `GROQ_API_KEY` | Groq API key for AI | From [console.groq.com](https://console.groq.com) |

### Creating Service Account Key

```bash
# Create service account
gcloud iam service-accounts create github-actions \
  --display-name="GitHub Actions"

# Grant necessary roles
PROJECT_ID=techbirdsfly
SA_EMAIL="github-actions@${PROJECT_ID}.iam.gserviceaccount.com"

gcloud projects add-iam-policy-binding $PROJECT_ID \
  --member="serviceAccount:$SA_EMAIL" \
  --role="roles/run.admin"

gcloud projects add-iam-policy-binding $PROJECT_ID \
  --member="serviceAccount:$SA_EMAIL" \
  --role="roles/iam.serviceAccountUser"

gcloud projects add-iam-policy-binding $PROJECT_ID \
  --member="serviceAccount:$SA_EMAIL" \
  --role="roles/artifactregistry.writer"

gcloud projects add-iam-policy-binding $PROJECT_ID \
  --member="serviceAccount:$SA_EMAIL" \
  --role="roles/cloudbuild.builds.builder"

# Create and download key
gcloud iam service-accounts keys create key.json \
  --iam-account=$SA_EMAIL

# Copy the content of key.json to GitHub Secrets as GCP_SERVICE_ACCOUNT_KEY
cat key.json
```

---

## 🚀 Deployment Methods

### Method 1: Automatic via GitHub Actions (Recommended)

Once you push to the `main` branch:

1. **CI Workflow** (`ci.yml`) runs tests
2. **Deploy Workflow** (`deploy-gcp.yml`) builds and deploys

**Monitor progress:**
- Go to GitHub → Actions tab
- Watch the workflows run

### Method 2: Manual Deployment via gcloud CLI

#### Deploy Backend Services

```bash
# Set variables
export PROJECT_ID=techbirdsfly
export REGION=us-central1
export REGISTRY=us-central1-docker.pkg.dev/${PROJECT_ID}/techbirdsfly-repo

# Build and push auth-service
cd services/auth-service/src
docker build -t ${REGISTRY}/auth-service:latest .
docker push ${REGISTRY}/auth-service:latest

# Deploy auth-service
gcloud run deploy auth-service \
  --image=${REGISTRY}/auth-service:latest \
  --region=${REGION} \
  --platform=managed \
  --allow-unauthenticated \
  --memory=512Mi \
  --cpu=1 \
  --min-instances=0 \
  --max-instances=2 \
  --set-env-vars="ASPNETCORE_ENVIRONMENT=Production"

# Repeat for other services...
```

#### Deploy Frontend

```bash
cd web-frontend/techbirdsfly-frontend-nextjs

# Build Docker image
docker build -t ${REGISTRY}/frontend:latest .
docker push ${REGISTRY}/frontend:latest

# Deploy to Cloud Run
gcloud run deploy frontend \
  --image=${REGISTRY}/frontend:latest \
  --region=${REGION} \
  --platform=managed \
  --allow-unauthenticated \
  --memory=512Mi \
  --cpu=1 \
  --port=3000
```

---

## 🌐 Service URLs (After Deployment)

After deployment, your services will be available at:

| Service | URL Pattern |
|---------|-------------|
| Frontend | `https://frontend-XXXXX-uc.a.run.app` |
| Auth Service | `https://auth-service-XXXXX-uc.a.run.app` |
| Generator Service | `https://generator-service-XXXXX-uc.a.run.app` |
| Gateway | `https://gateway-XXXXX-uc.a.run.app` |

**Get actual URLs:**
```bash
gcloud run services list --region=us-central1
```

---

## ⚙️ Environment Variables for Cloud Run

### Auth Service
```bash
gcloud run services update auth-service \
  --region=us-central1 \
  --set-env-vars="
    ASPNETCORE_ENVIRONMENT=Production,
    JWT_SECRET_KEY=your-jwt-secret,
    DATABASE_URL=your-db-connection-string
  "
```

### Generator Service
```bash
gcloud run services update generator-service \
  --region=us-central1 \
  --set-env-vars="
    ASPNETCORE_ENVIRONMENT=Production,
    GROQ_API_KEY=your-groq-api-key
  "
```

### Frontend
```bash
gcloud run services update frontend \
  --region=us-central1 \
  --set-env-vars="
    NEXT_PUBLIC_API_URL=https://gateway-XXXXX-uc.a.run.app
  "
```

---

## 💾 Database Options (Free Tier)

### Option 1: Cloud SQL (Not Free)
- Cloud SQL has no free tier
- Minimum cost: ~$7/month

### Option 2: Free Alternatives

**Neon (PostgreSQL) - Recommended:**
- Free tier: 512MB storage, 100 hours/month
- Sign up: [neon.tech](https://neon.tech)

```bash
# Example connection string
DATABASE_URL="postgresql://user:pass@ep-xxx.us-east-1.aws.neon.tech/neondb"
```

**Supabase:**
- Free tier: 500MB storage
- Sign up: [supabase.com](https://supabase.com)

**PlanetScale (MySQL):**
- Free tier: 5GB storage
- Sign up: [planetscale.com](https://planetscale.com)

---

## 💰 Cost Management (Free Tier Limits)

### Cloud Run Free Tier (per month)
- 2 million requests
- 360,000 GB-seconds of memory
- 180,000 vCPU-seconds
- 1 GB network egress (North America)

### Tips to Stay Free
1. Set `min-instances=0` for all services
2. Use `--memory=256Mi` or `512Mi`
3. Monitor usage in Cloud Console

```bash
# Check billing
gcloud beta billing projects describe techbirdsfly
```

---

## 🔍 Monitoring & Logs

### View Logs
```bash
# All services
gcloud run services logs tail frontend --region=us-central1

# Specific service
gcloud run services logs read auth-service --region=us-central1 --limit=50
```

### Cloud Console
- **Cloud Run:** https://console.cloud.google.com/run
- **Logs:** https://console.cloud.google.com/logs
- **Billing:** https://console.cloud.google.com/billing

---

## 🐛 Troubleshooting

### Build Failures
```bash
# Check Cloud Build logs
gcloud builds list --limit=5

# Get specific build logs
gcloud builds log BUILD_ID
```

### Deployment Issues
```bash
# Check service status
gcloud run services describe SERVICE_NAME --region=us-central1

# Check revisions
gcloud run revisions list --service=SERVICE_NAME --region=us-central1
```

### Common Errors

**1. Permission Denied**
```bash
# Add Cloud Build service account permissions
gcloud projects add-iam-policy-binding techbirdsfly \
  --member="serviceAccount:PROJECT_NUMBER@cloudbuild.gserviceaccount.com" \
  --role="roles/run.admin"
```

**2. Image Not Found**
```bash
# Verify image exists
gcloud artifacts docker images list us-central1-docker.pkg.dev/techbirdsfly/techbirdsfly-repo
```

**3. Container Failed to Start**
- Check logs: `gcloud run services logs read SERVICE_NAME`
- Verify PORT environment variable
- Check memory limits

---

## 📝 Quick Commands Reference

```bash
# List all services
gcloud run services list --region=us-central1

# Get service URL
gcloud run services describe SERVICE_NAME --region=us-central1 --format='value(status.url)'

# Update service
gcloud run services update SERVICE_NAME --region=us-central1 --memory=1Gi

# Delete service
gcloud run services delete SERVICE_NAME --region=us-central1

# View images in registry
gcloud artifacts docker images list us-central1-docker.pkg.dev/techbirdsfly/techbirdsfly-repo
```

---

## 🎉 Success Checklist

After successful deployment:

- [ ] All GitHub Actions workflows pass ✅
- [ ] Services are running in Cloud Run
- [ ] Frontend is accessible
- [ ] API endpoints respond correctly
- [ ] Database connections work
- [ ] Authentication flows work

**Test your deployment:**
```bash
# Test frontend
curl https://frontend-XXXXX-uc.a.run.app

# Test auth health
curl https://auth-service-XXXXX-uc.a.run.app/health

# Test generator
curl https://generator-service-XXXXX-uc.a.run.app/health
```

---

## 📞 Support

If you encounter issues:
1. Check GitHub Actions logs
2. Review Cloud Run logs in GCP Console
3. Verify all environment variables are set
4. Ensure all secrets are configured in GitHub

**Useful Links:**
- [Cloud Run Documentation](https://cloud.google.com/run/docs)
- [GitHub Actions + Cloud Run](https://cloud.google.com/build/docs/automating-builds/github/connect-repo-github)
- [Free Tier Details](https://cloud.google.com/free)
