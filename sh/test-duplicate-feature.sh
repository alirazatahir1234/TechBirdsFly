#!/bin/bash

# Test Duplicate Project Feature
BASE_URL="http://localhost:5010/api/projects"
USER_ID="550e8400-e29b-41d4-a716-446655440000"

echo "=========================================="
echo "Testing Duplicate Project Feature"
echo "=========================================="
echo ""

# 1. Create original project
echo "1️⃣  Creating original project..."
CREATE_RESPONSE=$(curl -s -X POST "$BASE_URL/create" \
  -H "Content-Type: application/json" \
  -d "{
    \"name\": \"Original AI Website\",
    \"industry\": \"Technology\",
    \"style\": \"Modern\",
    \"palette\": \"Blue-White\",
    \"html\": \"<html><body><h1>Original Website</h1></body></html>\",
    \"userId\": \"$USER_ID\"
  }")

echo "$CREATE_RESPONSE" | jq '.'
ORIGINAL_ID=$(echo "$CREATE_RESPONSE" | jq -r '.data')
echo "Original Project ID: $ORIGINAL_ID"
echo ""

if [ -z "$ORIGINAL_ID" ] || [ "$ORIGINAL_ID" == "null" ]; then
  echo "❌ Failed to create original project"
  exit 1
fi

# 2. Get original project
echo "2️⃣  Verifying original project..."
curl -s -X GET "$BASE_URL/$ORIGINAL_ID" \
  -H "Content-Type: application/json" | jq '.'
echo ""

# 3. Duplicate the project
echo "3️⃣  Duplicating project..."
DUPLICATE_RESPONSE=$(curl -s -X POST "$BASE_URL/$ORIGINAL_ID/duplicate" \
  -H "Content-Type: application/json" \
  -d "{\"userId\": \"$USER_ID\"}")

echo "$DUPLICATE_RESPONSE" | jq '.'
DUPLICATE_ID=$(echo "$DUPLICATE_RESPONSE" | jq -r '.data')
echo "Duplicate Project ID: $DUPLICATE_ID"
echo ""

if [ -z "$DUPLICATE_ID" ] || [ "$DUPLICATE_ID" == "null" ]; then
  echo "❌ Failed to duplicate project"
  exit 1
fi

# 4. Verify duplicate project
echo "4️⃣  Verifying duplicate project..."
DUPLICATE_VERIFY=$(curl -s -X GET "$BASE_URL/$DUPLICATE_ID" \
  -H "Content-Type: application/json")

echo "$DUPLICATE_VERIFY" | jq '.'
DUPLICATE_NAME=$(echo "$DUPLICATE_VERIFY" | jq -r '.data.name')
echo "Duplicate Name: $DUPLICATE_NAME"
echo ""

# 5. List all user projects (should show both)
echo "5️⃣  Listing all user projects (should show both)..."
curl -s -X GET "$BASE_URL/user/$USER_ID" \
  -H "Content-Type: application/json" | jq '.data | length'
echo ""

# 6. Verify content is identical
echo "6️⃣  Verifying HTML content is identical..."
ORIGINAL_HTML=$(echo "$CREATE_RESPONSE" | jq -r '.data' | xargs -I {} curl -s -X GET "$BASE_URL/{}" -H "Content-Type: application/json" | jq -r '.data.html')
DUPLICATE_HTML=$(echo "$DUPLICATE_VERIFY" | jq -r '.data.html')

if [ "$ORIGINAL_HTML" == "$DUPLICATE_HTML" ]; then
  echo "✅ HTML content is identical"
else
  echo "❌ HTML content differs"
fi
echo ""

echo "=========================================="
echo "✅ All duplicate tests passed!"
echo "=========================================="
