#!/bin/bash
# Quick task state management functions

source "$(dirname "$0")/azdo_env.sh"

SCRIPT_DIR="$(dirname "$0")"
AZDO_SCRIPT="$SCRIPT_DIR/../skills/scrum-master/azdo_workitems.py"
STATE_SCRIPT="$SCRIPT_DIR/manage_task_state.py"

# Start working on a task
task_start() {
    local task_id=$1
    echo "🚀 Starting Task #$task_id"
    python3 "$STATE_SCRIPT" --id "$task_id" --state Active --force
    python3 "$AZDO_SCRIPT" show --id "$task_id" | grep -E '"title"|"type"|"state"'
}

# Complete a task
task_done() {
    local task_id=$1
    echo "✅ Completing Task #$task_id"
    python3 "$STATE_SCRIPT" --id "$task_id" --state Closed --force
    python3 "$AZDO_SCRIPT" show --id "$task_id" | grep -E '"title"|"type"|"state"'
}

# Show task details
task_show() {
    local task_id=$1
    python3 "$AZDO_SCRIPT" show --id "$task_id"
}

# Board status summary
board_status() {
    echo "📊 Azure DevOps Board Status"
    echo ""
    echo "Foundation & Infrastructure (Feature #426):"
    for id in 426 427 428 429 430 431; do
        echo -n "  #$id "
        python3 "$AZDO_SCRIPT" show --id "$id" 2>/dev/null | grep '"state"' | sed 's/.*"state": "\([^"]*\)".*/\1/'
    done
    echo ""
    echo "Authentication & User Management (Feature #432):"
    for id in 432 433 434 435 436 437; do
        echo -n "  #$id "
        python3 "$AZDO_SCRIPT" show --id "$id" 2>/dev/null | grep '"state"' | sed 's/.*"state": "\([^"]*\)".*/\1/'
    done
}

# Export functions
export -f task_start
export -f task_done
export -f task_show
export -f board_status

# If called with arguments, execute the command
if [ $# -gt 0 ]; then
    "$@"
fi
