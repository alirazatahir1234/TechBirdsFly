#!/bin/bash
# GitHub Actions Status Checker (No CLI needed)

echo "🔍 Checking GitHub Actions Status..."
echo ""

# Fetch latest workflow runs
RESPONSE=$(curl -s "https://api.github.com/repos/alirazatahir1234/TechBirdsFly/actions/runs?per_page=5")

# Parse and display results
echo "$RESPONSE" | python3 -c "
import sys, json
try:
    data = json.load(sys.stdin)
    runs = data.get('workflow_runs', [])
    
    if not runs:
        print('❌ No workflow runs found')
        sys.exit(1)
    
    print('📊 Recent Workflow Runs:\n')
    for run in runs[:5]:
        name = run.get('name', 'Unknown')
        status = run.get('status', 'unknown')
        conclusion = run.get('conclusion', 'none')
        created = run.get('created_at', '')[:10]
        url = run.get('html_url', '')
        
        # Status emoji
        if status == 'completed':
            if conclusion == 'success':
                emoji = '✅'
            elif conclusion == 'failure':
                emoji = '❌'
            else:
                emoji = '⚠️'
        else:
            emoji = '🟡'
        
        print(f'{emoji} {name}')
        print(f'   Status: {status} | Result: {conclusion}')
        print(f'   Date: {created}')
        print(f'   URL: {url}')
        print()
        
except Exception as e:
    print(f'Error parsing response: {e}')
    sys.exit(1)
"

echo ""
echo "📖 View full details at: https://github.com/alirazatahir1234/TechBirdsFly/actions"
