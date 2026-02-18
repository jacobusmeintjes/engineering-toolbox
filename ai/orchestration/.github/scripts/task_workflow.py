#!/usr/bin/env python3
"""
Task Workflow Helper - Updates Azure DevOps board in real-time

Usage:
    python3 task_workflow.py start <task_id>    # Move task to Active
    python3 task_workflow.py done <task_id>     # Move task to Closed
    python3 task_workflow.py status             # Show current board status
"""

import os
import sys
import subprocess
import json

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
AZDO_SCRIPT = os.path.join(SCRIPT_DIR, "../skills/scrum-master/azdo_workitems.py")
MANAGE_SCRIPT = os.path.join(SCRIPT_DIR, "manage_task_state.py")
BOARD_STATUS_SCRIPT = os.path.join(SCRIPT_DIR, "board_status.py")

def run_command(cmd):
    """Run a shell command and return output"""
    result = subprocess.run(cmd, shell=True, capture_output=True, text=True)
    return result.returncode, result.stdout, result.stderr

def get_work_item(task_id):
    """Get work item details"""
    rc, stdout, stderr = run_command(f"python3 {AZDO_SCRIPT} show --id {task_id}")
    if rc == 0:
        try:
            return json.loads(stdout)
        except:
            pass
    return None

def start_task(task_id):
    """Mark a task as Active (started)"""
    print(f"🚀 Starting Task #{task_id}")
    print("=" * 60)
    
    # Get task details first
    task = get_work_item(task_id)
    if not task:
        print(f"❌ Error: Could not find task #{task_id}")
        return False
    
    print(f"Task: {task.get('title', 'Unknown')}")
    print(f"Type: {task.get('type', 'Unknown')}")
    print(f"Current State: {task.get('state', 'Unknown')}")
    
    # Move to Active
    print("\n⏱️  Moving to Active state...")
    rc, stdout, stderr = run_command(f"python3 {MANAGE_SCRIPT} --id {task_id} --state Active --force")
    
    if rc == 0:
        print("✅ Task moved to Active")
        print(f"📊 View on board: https://dev.azure.com/phoenixcode/Factory/_workitems/edit/{task_id}")
    else:
        print(f"❌ Error: {stderr}")
        return False
    
    print("=" * 60)
    return True

def complete_task(task_id):
    """Mark a task as Closed (completed)"""
    print(f"✅ Completing Task #{task_id}")
    print("=" * 60)
    
    # Get task details first
    task = get_work_item(task_id)
    if not task:
        print(f"❌ Error: Could not find task #{task_id}")
        return False
    
    print(f"Task: {task.get('title', 'Unknown')}")
    print(f"Type: {task.get('type', 'Unknown')}")
    print(f"Current State: {task.get('state', 'Unknown')}")
    
    # Move to Closed
    print("\n🏁 Moving to Closed state...")
    rc, stdout, stderr = run_command(f"python3 {MANAGE_SCRIPT} --id {task_id} --state Closed --force")
    
    if rc == 0:
        print("✅ Task completed and closed")
        print(f"📊 View on board: https://dev.azure.com/phoenixcode/Factory/_workitems/edit/{task_id}")
    else:
        print(f"❌ Error: {stderr}")
        return False
    
    print("=" * 60)
    return True

def show_status():
    """Show current board status"""
    rc, stdout, stderr = run_command(f"python3 {BOARD_STATUS_SCRIPT}")
    print(stdout)
    if stderr:
        print(f"Errors: {stderr}", file=sys.stderr)

def main():
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(1)
    
    command = sys.argv[1].lower()
    
    if command == "start":
        if len(sys.argv) < 3:
            print("Error: Missing task ID")
            print("Usage: python3 task_workflow.py start <task_id>")
            sys.exit(1)
        task_id = sys.argv[2]
        success = start_task(task_id)
        sys.exit(0 if success else 1)
    
    elif command == "done":
        if len(sys.argv) < 3:
            print("Error: Missing task ID")
            print("Usage: python3 task_workflow.py done <task_id>")
            sys.exit(1)
        task_id = sys.argv[2]
        success = complete_task(task_id)
        sys.exit(0 if success else 1)
    
    elif command == "status":
        show_status()
        sys.exit(0)
    
    else:
        print(f"Unknown command: {command}")
        print(__doc__)
        sys.exit(1)

if __name__ == "__main__":
    main()
