#!/bin/bash

# Test Project Service Endpoints
BASE_URL="http://localhost:5010/api/projects"
USER_ID="550e8400-e29b-41d4-a716-446655440000"
PROJECT_NAME="My AI Website"
INDUSTRY="Technology"
STYLE="Modern"
PALETTE="Blue-White"

echo "================================"
echo "Project Service API Test Suite"
echo "================================"
echo ""

# 1. Health Check
echo "1️⃣  Testing Health Check..."
curl -X GET "$BASE_URL/health/status" \
  -H "Content-Type: application/json" \
  -w "\nStatus: %{http_code}\n\n"

# 2. Create Project
echo "2️⃣  Creating New Project..."
CREATE_RESPONSE=$(curl -s -X POST "$BASE_URL/create" \
  -H "Content-Type: application/json" \
  -d "{
    \"name\": \"$PROJECT_NAME\",
    \"industry\": \"$INDUSTRY\",
    \"style\": \"$STYLE\",
    \"palette\": \"$PALETTE\",
    \"html\": \"<html><body><h1>Generated Website</h1></body></html>\",
    \"userId\": \"$USER_ID\"
  }")

echo "$CREATE_RESPONSE"
PROJECT_ID=$(echo "$CREATE_RESPONSE" | grep -o '"data":"[^"]*' | cut -d'"' -f4)
echo "Project ID: $PROJECT_ID"
echo ""

# If project creation failed, exit
if [ -z "$PROJECT_ID" ]; then
  echo "❌ Failed to create project. Exiting."
  exit 1
fi

# 3. Get Project (Retrieve latest version)
echo "3️⃣  Retrieving Project (Latest Version)..."
curl -s -X GET "$BASE_URL/$PROJECT_ID" \
  -H "Content-Type: application/json" | jq '.'
echo ""

# 4. List User Projects
echo "4️⃣  Listing All User Projects..."
curl -s -X GET "$BASE_URL/user/$USER_ID" \
  -H "Content-Type: application/json" | jq '.'
echo ""

# 5. Save Version
echo "5️⃣  Saving New Version..."
SAVE_RESPONSE=$(curl -s -X POST "$BASE_URL/$PROJECT_ID/versions" \
  -H "Content-Type: application/json" \
  -d "{
    \"html\": \"<html><body><h1>Updated Website v2</h1></body></html>\"
  }")

echo "$SAVE_RESPONSE"
echo ""

# 6. Get Project Again (Should show latest version)
echo "6️⃣  Retrieving Project Again (Should show v2)..."
curl -s -X GET "$BASE_URL/$PROJECT_ID" \
  -H "Content-Type: application/json" | jq '.'
echo ""

# 7. Delete Project
echo "7️⃣  Deleting Project..."
curl -s -X DELETE "$BASE_URL/$PROJECT_ID" \
  -H "Content-Type: application/json" | jq '.'
echo ""

echo "================================"
echo "✅ All tests completed!"
echo "================================"
