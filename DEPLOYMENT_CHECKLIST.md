# TechBirdsFly - Deployment Checklist

## ✅ GitHub Secrets Verification

Make sure these secrets are added to your GitHub repository:

**Settings → Secrets and variables → Actions → Repository secrets**

| Secret Name | Value | Status |
|-------------|-------|--------|
| `GCP_PROJECT_ID` | `techbirdsfly` | ✅ Added |
| `GCP_SA_KEY` | (JSON key content) | ✅ Added |
| `GCP_REGION` | `us-central1` | ⚠️ Verify |

---

## 🚀 Deploy Now

### Option 1: Automatic Deployment (Recommended)

Push to main branch to trigger GitHub Actions:

```bash
# Make sure all changes are committed
git add .
git commit -m "feat: Add Google Cloud CI/CD pipeline"
git push origin main
```

Then watch the deployment at:
```
https://github.com/alirazatahir1234/TechBirdsFly/actions
```

---

### Option 2: Manual Deployment via Cloud Build

If you prefer to deploy manually first:

```bash
# Authenticate with Google Cloud
gcloud auth login

# Set project
gcloud config set project techbirdsfly

# Submit build manually
gcloud builds submit --config=cloudbuild.yaml
```

---

## 📊 Monitor Deployment

### Watch GitHub Actions
```bash
# Open in browser
open https://github.com/alirazatahir1234/TechBirdsFly/actions
```

### Check Cloud Build
```bash
# View recent builds
gcloud builds list --limit=5

# Stream logs for a specific build
gcloud builds log BUILD_ID --stream
```

### Check Cloud Run Services
```bash
# List all services
gcloud run services list --region=us-central1

# Get service URLs
gcloud run services describe techbirdsfly-frontend --region=us-central1 --format='value(status.url)'
gcloud run services describe techbirdsfly-gateway --region=us-central1 --format='value(status.url)'
gcloud run services describe techbirdsfly-auth --region=us-central1 --format='value(status.url)'
gcloud run services describe techbirdsfly-generator --region=us-central1 --format='value(status.url)'
```

---

## 🔍 Troubleshooting

### If GitHub Action Fails

1. **Check workflow logs** in GitHub Actions tab
2. **Verify secrets** are correctly set (no extra spaces)
3. **Check GCP permissions**:
   ```bash
   ./scripts/setup-gcp.sh
   ```

### If Build Fails

```bash
# Check Cloud Build logs
gcloud builds list --limit=1
gcloud builds log BUILD_ID

# Verify Docker builds locally
docker build -f services/auth-service/Dockerfile services/auth-service
```

### If Service Won't Start

```bash
# Check service logs
gcloud run services logs read techbirdsfly-frontend --region=us-central1 --limit=50

# Check service configuration
gcloud run services describe techbirdsfly-frontend --region=us-central1
```

---

## 🎯 Expected Results

After successful deployment, you should see:

1. ✅ GitHub Actions workflow completes successfully
2. ✅ 4 Docker images in Artifact Registry
3. ✅ 4 Cloud Run services deployed and running
4. ✅ Public URLs for each service

### Service URLs Format
```
Frontend: https://techbirdsfly-frontend-<hash>-uc.a.run.app
Gateway:  https://techbirdsfly-gateway-<hash>-uc.a.run.app
Auth:     https://techbirdsfly-auth-<hash>-uc.a.run.app
Generator: https://techbirdsfly-generator-<hash>-uc.a.run.app
```

---

## 📝 Post-Deployment Tasks

After first successful deployment:

1. **Update Frontend Config** with Gateway URL:
   ```bash
   # Get Gateway URL
   GATEWAY_URL=$(gcloud run services describe techbirdsfly-gateway --region=us-central1 --format='value(status.url)')
   
   # Update frontend environment variable
   gcloud run services update techbirdsfly-frontend \
     --region=us-central1 \
     --set-env-vars="NEXT_PUBLIC_API_URL=$GATEWAY_URL"
   ```

2. **Configure Gateway Routes** to point to Cloud Run service URLs

3. **Test the application** by visiting the Frontend URL

4. **Set up custom domain** (optional):
   ```bash
   gcloud run domain-mappings create \
     --service=techbirdsfly-frontend \
     --domain=yourdomain.com \
     --region=us-central1
   ```

---

## 💰 Cost Monitoring

Monitor your usage to stay within free tier:

```bash
# Check Cloud Run request count
gcloud monitoring time-series list \
  --filter='metric.type="run.googleapis.com/request_count"' \
  --format=table

# View current month billing
gcloud billing accounts list
```

**Free Tier Limits:**
- Cloud Run: 2M requests/month
- Cloud Build: 120 min/day
- Artifact Registry: 500 MB

---

## 🔐 Security Best Practices

- [ ] Rotate service account keys regularly
- [ ] Enable Cloud Armor for DDoS protection (if needed)
- [ ] Set up Cloud SQL for production database
- [ ] Configure Secret Manager for sensitive data
- [ ] Enable Cloud Audit Logs

---

## 📚 Documentation

- Quick Start: `CICD_QUICK_START.md`
- Full Guide: `docs/CICD_GOOGLE_CLOUD_PLAN.md`
- Architecture: `CURRENT_ARCHITECTURE.md`

---

**Ready to deploy? Run:**
```bash
git push origin main
```
