#!/usr/bin/env python3
"""
Task State Management Script

Manages Azure DevOps work item state transitions following SDLC workflow.
Used by the Orchestrator to move tasks through: New → Active → Completed

Usage:
    python3 manage_task_state.py --id 428 --state Active
    python3 manage_task_state.py --id 428 --state Completed --comment "Solution created"
    python3 manage_task_state.py --id 428 --state New --comment "Reset due to failure"
"""

import sys
import os
import subprocess
import argparse
import json

# Valid state transitions
VALID_STATES = ["New", "Active", "Resolved", "Completed", "Closed"]

# SDLC workflow for this project
WORKFLOW = {
    "New": ["Active"],
    "Active": ["Completed", "New"],  # Can go back to New on failure
    "Completed": ["Closed"],
    "Resolved": ["Completed", "Closed"],
}

def run_command(cmd, capture=True):
    """Run shell command and return output"""
    if capture:
        result = subprocess.run(cmd, shell=True, capture_output=True, text=True)
        return result.stdout.strip(), result.stderr.strip(), result.returncode
    else:
        result = subprocess.run(cmd, shell=True)
        return "", "", result.returncode

def get_task_info(task_id):
    """Get current task state from Azure DevOps"""
    script_path = os.path.join(
        os.path.dirname(__file__),
        "../skills/scrum-master/azdo_workitems.py"
    )
    
    cmd = f"python3 {script_path} show --id {task_id}"
    stdout, stderr, code = run_command(cmd)
    
    if code != 0:
        return None, f"Failed to get task info: {stderr}"
    
    # Parse JSON output
    try:
        task_info = json.loads(stdout)
        state = task_info.get("state")
        if state:
            return state, None
        return None, "No 'state' field in work item"
    except json.JSONDecodeError as e:
        return None, f"Failed to parse JSON: {e}"

def validate_transition(current_state, new_state):
    """Validate if state transition is allowed"""
    if new_state not in VALID_STATES:
        return False, f"Invalid state: {new_state}. Must be one of {VALID_STATES}"
    
    # Allow transition from None (new task)
    if current_state is None:
        return True, None
    
    allowed = WORKFLOW.get(current_state, [])
    if new_state in allowed or new_state == current_state:
        return True, None
    
    return False, f"Invalid transition: {current_state} → {new_state}. Allowed: {allowed}"

def update_task_state(task_id, new_state, comment=None):
    """Update task state in Azure DevOps"""
    script_path = os.path.join(
        os.path.dirname(__file__),
        "../skills/scrum-master/azdo_workitems.py"
    )
    
    # Move to new state
    cmd = f"python3 {script_path} move --id {task_id} --state '{new_state}'"
    stdout, stderr, code = run_command(cmd)
    
    if code != 0:
        return False, f"Failed to move task: {stderr}"
    
    # Add comment if provided
    if comment:
        comment_cmd = f"python3 {script_path} comment --id {task_id} --text '{comment}'"
        stdout, stderr, code = run_command(comment_cmd)
        if code != 0:
            print(f"Warning: Failed to add comment: {stderr}", file=sys.stderr)
    
    return True, None

def main():
    parser = argparse.ArgumentParser(
        description="Manage Azure DevOps task state transitions"
    )
    parser.add_argument(
        "--id",
        required=True,
        type=int,
        help="Azure DevOps work item ID"
    )
    parser.add_argument(
        "--state",
        required=True,
        choices=VALID_STATES,
        help="Target state"
    )
    parser.add_argument(
        "--comment",
        help="Optional comment to add to work item"
    )
    parser.add_argument(
        "--force",
        action="store_true",
        help="Skip validation (use with caution)"
    )
    
    args = parser.parse_args()
    
    print(f"🔄 Managing Task {args.id}: → {args.state}")
    
    # Get current state
    if not args.force:
        current_state, error = get_task_info(args.id)
        if error:
            print(f"❌ Error: {error}", file=sys.stderr)
            return 1
        
        print(f"   Current state: {current_state}")
        
        # Validate transition
        valid, error = validate_transition(current_state, args.state)
        if not valid:
            print(f"❌ {error}", file=sys.stderr)
            print(f"   Use --force to override (not recommended)", file=sys.stderr)
            return 1
        
        if current_state == args.state:
            print(f"✅ Task already in '{args.state}' state")
            if args.comment:
                print(f"   Adding comment...")
                script_path = os.path.join(
                    os.path.dirname(__file__),
                    "../skills/scrum-master/azdo_workitems.py"
                )
                cmd = f"python3 {script_path} comment --id {args.id} --text '{args.comment}'"
                run_command(cmd, capture=False)
            return 0
    
    # Update state
    success, error = update_task_state(args.id, args.state, args.comment)
    if not success:
        print(f"❌ {error}", file=sys.stderr)
        return 1
    
    print(f"✅ Task {args.id} moved to '{args.state}' state")
    if args.comment:
        print(f"   Comment added: {args.comment}")
    
    return 0

if __name__ == "__main__":
    sys.exit(main())
