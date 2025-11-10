#!/bin/bash

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

set -e

PROJECT_PATH="$(cd "$(dirname "${BASH_SOURCE[0]}")/src/AdminService" && pwd)"
echo -e "${BLUE}================================${NC}"
echo -e "${BLUE}Admin Service Setup${NC}"
echo -e "${BLUE}================================${NC}"
echo ""

# Step 1: Restore packages
echo -e "${YELLOW}Step 1: Restoring NuGet packages...${NC}"
cd "$PROJECT_PATH"
dotnet restore
echo -e "${GREEN}✓ Packages restored${NC}"
echo ""

# Step 2: Build project
echo -e "${YELLOW}Step 2: Building project...${NC}"
dotnet build --no-restore --configuration Release
echo -e "${GREEN}✓ Project built successfully${NC}"
echo ""

# Step 3: Run migrations
echo -e "${YELLOW}Step 3: Creating database migration...${NC}"
dotnet ef migrations add InitialCreate --no-build
echo -e "${GREEN}✓ Migration created${NC}"
echo ""

# Step 4: Update database
echo -e "${YELLOW}Step 4: Applying database migrations...${NC}"
dotnet ef database update --no-build
echo -e "${GREEN}✓ Database updated${NC}"
echo ""

echo -e "${BLUE}================================${NC}"
echo -e "${GREEN}Setup complete!${NC}"
echo -e "${BLUE}================================${NC}"
echo ""
echo -e "${YELLOW}To start the service, run:${NC}"
echo "cd $PROJECT_PATH"
echo "dotnet run"
