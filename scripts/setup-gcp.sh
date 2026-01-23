#!/bin/bash
# ============================================
# TechBirdsFly - Google Cloud Setup Script
# ============================================
# This script sets up all required Google Cloud
# resources for the CI/CD pipeline
# ============================================

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
PROJECT_ID="techbirdsfly"
REGION="us-central1"
REPO_NAME="TechBirdsFly"
SA_NAME="techbirdsfly-cicd"

echo -e "${BLUE}============================================${NC}"
echo -e "${BLUE}  TechBirdsFly Google Cloud Setup${NC}"
echo -e "${BLUE}============================================${NC}"
echo ""

# Function to print step
print_step() {
    echo -e "${GREEN}▶ $1${NC}"
}

# Function to print warning
print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

# Function to print error
print_error() {
    echo -e "${RED}✖ $1${NC}"
}

# Function to print success
print_success() {
    echo -e "${GREEN}✔ $1${NC}"
}

# ============================================
# Step 1: Check Prerequisites
# ============================================
print_step "Step 1: Checking prerequisites..."

if ! command -v gcloud &> /dev/null; then
    print_error "gcloud CLI is not installed. Please install it first."
    echo "Visit: https://cloud.google.com/sdk/docs/install"
    exit 1
fi
print_success "gcloud CLI is installed"

# Check if logged in
if ! gcloud auth list --filter=status:ACTIVE --format="value(account)" &> /dev/null; then
    print_warning "Not logged in to gcloud. Running 'gcloud auth login'..."
    gcloud auth login
fi
print_success "Authenticated with Google Cloud"

# ============================================
# Step 2: Set Project
# ============================================
print_step "Step 2: Setting project to ${PROJECT_ID}..."

# Check if project exists
if gcloud projects describe $PROJECT_ID &> /dev/null; then
    print_success "Project '$PROJECT_ID' exists"
else
    print_warning "Project '$PROJECT_ID' does not exist. Creating..."
    gcloud projects create $PROJECT_ID --name="TechBirdsFly"
    print_success "Project created"
fi

gcloud config set project $PROJECT_ID
print_success "Project set to $PROJECT_ID"

# ============================================
# Step 3: Enable APIs
# ============================================
print_step "Step 3: Enabling required APIs..."

APIS=(
    "run.googleapis.com"
    "artifactregistry.googleapis.com"
    "cloudbuild.googleapis.com"
    "iam.googleapis.com"
    "secretmanager.googleapis.com"
    "compute.googleapis.com"
    "cloudresourcemanager.googleapis.com"
)

for api in "${APIS[@]}"; do
    echo "  Enabling $api..."
    gcloud services enable $api --quiet
done
print_success "All APIs enabled"

# ============================================
# Step 4: Create Artifact Registry Repository
# ============================================
print_step "Step 4: Creating Artifact Registry repository..."

if gcloud artifacts repositories describe $REPO_NAME --location=$REGION &> /dev/null; then
    print_success "Repository '$REPO_NAME' already exists"
else
    gcloud artifacts repositories create $REPO_NAME \
        --repository-format=docker \
        --location=$REGION \
        --description="TechBirdsFly Docker images"
    print_success "Repository created"
fi

# Configure Docker authentication
gcloud auth configure-docker ${REGION}-docker.pkg.dev --quiet
print_success "Docker authentication configured"

# ============================================
# Step 5: Create Service Account
# ============================================
print_step "Step 5: Creating service account..."

SA_EMAIL="${SA_NAME}@${PROJECT_ID}.iam.gserviceaccount.com"

if gcloud iam service-accounts describe $SA_EMAIL &> /dev/null; then
    print_success "Service account '$SA_NAME' already exists"
else
    gcloud iam service-accounts create $SA_NAME \
        --display-name="TechBirdsFly CI/CD Service Account" \
        --description="Service account for CI/CD pipeline"
    print_success "Service account created"
fi

# ============================================
# Step 6: Grant IAM Roles
# ============================================
print_step "Step 6: Granting IAM roles..."

ROLES=(
    "roles/run.admin"
    "roles/artifactregistry.writer"
    "roles/iam.serviceAccountUser"
    "roles/storage.admin"
    "roles/secretmanager.secretAccessor"
)

for role in "${ROLES[@]}"; do
    echo "  Granting $role..."
    gcloud projects add-iam-policy-binding $PROJECT_ID \
        --member="serviceAccount:${SA_EMAIL}" \
        --role="$role" \
        --quiet &> /dev/null
done
print_success "All IAM roles granted"

# Grant Cloud Build service account permissions
print_step "Step 6b: Granting Cloud Build permissions..."
PROJECT_NUMBER=$(gcloud projects describe $PROJECT_ID --format='value(projectNumber)')
CLOUDBUILD_SA="${PROJECT_NUMBER}@cloudbuild.gserviceaccount.com"

gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:${CLOUDBUILD_SA}" \
    --role="roles/run.admin" \
    --quiet &> /dev/null

gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:${CLOUDBUILD_SA}" \
    --role="roles/iam.serviceAccountUser" \
    --quiet &> /dev/null

gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:${CLOUDBUILD_SA}" \
    --role="roles/artifactregistry.writer" \
    --quiet &> /dev/null

print_success "Cloud Build permissions granted"

# ============================================
# Step 7: Create Service Account Key
# ============================================
print_step "Step 7: Creating service account key..."

KEY_FILE="$HOME/techbirdsfly-cicd-key.json"

if [ -f "$KEY_FILE" ]; then
    print_warning "Key file already exists at $KEY_FILE"
    read -p "Do you want to regenerate it? (y/n) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        rm "$KEY_FILE"
        gcloud iam service-accounts keys create "$KEY_FILE" \
            --iam-account=$SA_EMAIL
        print_success "New key created at $KEY_FILE"
    fi
else
    gcloud iam service-accounts keys create "$KEY_FILE" \
        --iam-account=$SA_EMAIL
    print_success "Key created at $KEY_FILE"
fi

# ============================================
# Step 8: Create Initial Cloud Run Services
# ============================================
print_step "Step 8: Creating placeholder Cloud Run services..."

SERVICES=(
    "techbirdsfly-auth:5001:256Mi"
    "techbirdsfly-generator:5289:512Mi"
    "techbirdsfly-gateway:8000:256Mi"
    "techbirdsfly-frontend:3000:256Mi"
)

for service_config in "${SERVICES[@]}"; do
    IFS=':' read -r service_name port memory <<< "$service_config"
    
    if gcloud run services describe $service_name --region=$REGION &> /dev/null 2>&1; then
        echo "  $service_name already exists"
    else
        echo "  Creating $service_name..."
        gcloud run deploy $service_name \
            --image=gcr.io/cloudrun/hello \
            --platform=managed \
            --region=$REGION \
            --allow-unauthenticated \
            --port=$port \
            --memory=$memory \
            --cpu=1 \
            --min-instances=0 \
            --max-instances=2 \
            --quiet &> /dev/null
    fi
done
print_success "Cloud Run services created"

# ============================================
# Summary
# ============================================
echo ""
echo -e "${BLUE}============================================${NC}"
echo -e "${BLUE}  Setup Complete!${NC}"
echo -e "${BLUE}============================================${NC}"
echo ""
echo -e "${GREEN}Project ID:${NC} $PROJECT_ID"
echo -e "${GREEN}Region:${NC} $REGION"
echo -e "${GREEN}Repository:${NC} $REPO_NAME"
echo -e "${GREEN}Service Account:${NC} $SA_EMAIL"
echo ""
echo -e "${YELLOW}Next Steps:${NC}"
echo "1. Add the following secrets to your GitHub repository:"
echo "   - GCP_PROJECT_ID: $PROJECT_ID"
echo "   - GCP_SA_KEY: (contents of $KEY_FILE)"
echo "   - GCP_REGION: $REGION"
echo ""
echo "2. The key file is at: $KEY_FILE"
echo "   Run: cat $KEY_FILE | pbcopy  (to copy to clipboard on macOS)"
echo ""
echo "3. Push to main branch to trigger deployment!"
echo ""
echo -e "${GREEN}Service URLs (after first deployment):${NC}"
gcloud run services list --region=$REGION --format="table(SERVICE,URL)" 2>/dev/null || echo "  (Run deployment first)"
echo ""
echo -e "${BLUE}============================================${NC}"
