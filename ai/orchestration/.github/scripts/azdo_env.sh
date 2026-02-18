#!/bin/bash
# Azure DevOps Board Update Helper
# Makes it easy to update task states during development

# Set these environment variables persistently
export AZDO_PAT=""
export AZDO_BOARD_URL=""

echo "✅ Azure DevOps environment configured"
echo "   Board: Factory Team Stories"
echo ""
echo "Available commands:"
echo "  task_start <id>      - Move task to Active"
echo "  task_done <id>       - Move task to Closed"  
echo "  task_show <id>       - Show task details"
echo "  board_status         - Show current sprint status"
