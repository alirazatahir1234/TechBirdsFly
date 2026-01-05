# TechBirdsFly - Google Cloud CI/CD Quick Start

## 🚀 One-Command Setup

```bash
# Make the script executable and run
chmod +x scripts/setup-gcp.sh
./scripts/setup-gcp.sh
```

---

## 📋 Manual Setup Steps

### Step 1: Create Project
```bash
# Login to Google Cloud
gcloud auth login

# Create project
gcloud projects create techbirdsfly --name="TechBirdsFly"

# Set as default
gcloud config set project techbirdsfly
```

### Step 2: Enable APIs
```bash
gcloud services enable \
  run.googleapis.com \
  artifactregistry.googleapis.com \
  cloudbuild.googleapis.com \
  iam.googleapis.com \
  secretmanager.googleapis.com \
  compute.googleapis.com \
  cloudresourcemanager.googleapis.com
```

### Step 3: Create Artifact Registry
```bash
gcloud artifacts repositories create techbirdsfly-repo \
  --repository-format=docker \
  --location=us-central1 \
  --description="TechBirdsFly Docker images"

# Configure Docker
gcloud auth configure-docker us-central1-docker.pkg.dev
```

### Step 4: Setup Service Account
```bash
# Create service account
gcloud iam service-accounts create techbirdsfly-cicd \
  --display-name="TechBirdsFly CI/CD"

# Grant roles
SA_EMAIL="techbirdsfly-cicd@techbirdsfly.iam.gserviceaccount.com"

gcloud projects add-iam-policy-binding techbirdsfly \
  --member="serviceAccount:${SA_EMAIL}" \
  --role="roles/run.admin"

gcloud projects add-iam-policy-binding techbirdsfly \
  --member="serviceAccount:${SA_EMAIL}" \
  --role="roles/artifactregistry.writer"

gcloud projects add-iam-policy-binding techbirdsfly \
  --member="serviceAccount:${SA_EMAIL}" \
  --role="roles/iam.serviceAccountUser"

# Create key for GitHub Actions
gcloud iam service-accounts keys create ~/techbirdsfly-key.json \
  --iam-account=${SA_EMAIL}
```

### Step 5: Grant Cloud Build Permissions
```bash
PROJECT_NUMBER=$(gcloud projects describe techbirdsfly --format='value(projectNumber)')
CLOUDBUILD_SA="${PROJECT_NUMBER}@cloudbuild.gserviceaccount.com"

gcloud projects add-iam-policy-binding techbirdsfly \
  --member="serviceAccount:${CLOUDBUILD_SA}" \
  --role="roles/run.admin"

gcloud projects add-iam-policy-binding techbirdsfly \
  --member="serviceAccount:${CLOUDBUILD_SA}" \
  --role="roles/iam.serviceAccountUser"
```

---

## 🔐 GitHub Secrets

Add these to **Settings → Secrets → Actions** in your GitHub repo:

| Secret Name | Value |
|-------------|-------|
| `GCP_PROJECT_ID` | `techbirdsfly` |
| `GCP_SA_KEY` | Contents of `~/techbirdsfly-key.json` |
| `GCP_REGION` | `us-central1` |

---

## 🚀 Deploy

### Via Cloud Build (Automatic)
```bash
# Push to main branch - triggers automatic deployment
git push origin main
```

### Via Cloud Build (Manual)
```bash
gcloud builds submit --config=cloudbuild.yaml
```

### Check Deployment Status
```bash
# View build logs
gcloud builds list --limit=5

# View service status
gcloud run services list --region=us-central1
```

---

## 📊 Service URLs

After deployment, get URLs:

```bash
# All services
gcloud run services list --region=us-central1

# Specific service
gcloud run services describe techbirdsfly-frontend \
  --region=us-central1 \
  --format='value(status.url)'
```

---

## 💰 Free Tier Limits

| Service | Free Allowance |
|---------|---------------|
| Cloud Run | 2M requests/month |
| Cloud Build | 120 min/day |
| Artifact Registry | 500 MB storage |

---

## 🔧 Useful Commands

```bash
# View logs
gcloud run services logs read techbirdsfly-frontend --region=us-central1

# Update service
gcloud run deploy SERVICE_NAME --image=IMAGE_URL --region=us-central1

# Scale service
gcloud run services update SERVICE_NAME --min-instances=1 --region=us-central1

# Delete service
gcloud run services delete SERVICE_NAME --region=us-central1
```

---

## 📁 Files Created

```
TechBirdsFly/
├── cloudbuild.yaml              # Cloud Build configuration
├── scripts/
│   └── setup-gcp.sh             # Setup script
├── services/
│   ├── auth-service/
│   │   └── Dockerfile           # Auth service container
│   └── generator-service/
│       └── Dockerfile           # Generator service container
├── gateway/
│   └── yarp-gateway/
│       └── Dockerfile           # Gateway container
└── docs/
    └── CICD_GOOGLE_CLOUD_PLAN.md # Full documentation
```

---

## 🆘 Troubleshooting

### Build Fails
```bash
# Check build logs
gcloud builds log BUILD_ID

# Re-grant permissions
./scripts/setup-gcp.sh
```

### Service Won't Start
```bash
# Check service logs
gcloud run services logs read SERVICE_NAME --region=us-central1

# Check revision status
gcloud run revisions list --service=SERVICE_NAME --region=us-central1
```

### Authentication Error
```bash
# Re-authenticate
gcloud auth login
gcloud auth configure-docker us-central1-docker.pkg.dev
```

---

**Full documentation:** `docs/CICD_GOOGLE_CLOUD_PLAN.md`
