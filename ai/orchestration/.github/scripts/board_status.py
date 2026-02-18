#!/usr/bin/env python3
"""
Real-time Azure DevOps Board Status Display

Shows current status of all work items in a visual format.
"""

import os
import sys
import subprocess
import json

AZDO_SCRIPT = os.path.join(os.path.dirname(__file__), "../skills/scrum-master/azdo_workitems.py")

def get_work_item(item_id):
    """Get work item details from Azure DevOps"""
    cmd = f"python3 {AZDO_SCRIPT} show --id {item_id}"
    result = subprocess.run(cmd, shell=True, capture_output=True, text=True)
    if result.returncode == 0:
        try:
            return json.loads(result.stdout)
        except:
            return None
    return None

def state_emoji(state):
    """Get emoji for state"""
    mapping = {
        "New": "⚪",
        "Active": "🔵",
        "Closed": "✅",
        "Removed": "❌",
        "Resolved": "🟢"
    }
    return mapping.get(state, "❓")

def print_feature(feature_id, feature_title, items):
    """Print a feature with its child items"""
    feature = get_work_item(feature_id)
    if not feature:
        return
    
    state = feature.get("state", "Unknown")
    emoji = state_emoji(state)
    
    print(f"\n{emoji} Feature #{feature_id}: {feature_title}")
    print(f"   State: {state}")
    print("   └─ Items:")
    
    for item_id, item_title, item_type in items:
        item = get_work_item(item_id)
        if item:
            item_state = item.get("state", "Unknown")
            item_emoji = state_emoji(item_state)
            indent = "      " if item_type == "Task" else "   "
            type_label = f"[{item_type}]".ljust(14)
            print(f"{indent}{item_emoji} #{item_id} {type_label} {item_state:10} - {item_title[:60]}")

def main():
    print("=" * 80)
    print("📊 AZURE DEVOPS BOARD STATUS - OpenLifter Blazor Migration")
    print("=" * 80)
    
    # Feature 426: Foundation & Infrastructure
    print_feature(426, "Foundation & Infrastructure", [
        (427, "As a developer, I can set up the solution", "User Story"),
        (428, "Create .NET Aspire Solution Structure", "Task"),
        (429, "Setup CI/CD Pipeline (DEFERRED)", "Task"),
        (430, "Provision Azure Infrastructure (DEFERRED)", "Task"),
        (431, "Configure Local Logging (Serilog + Seq)", "Task"),
    ])
    
    # Feature 432: Authentication & User Management
    print_feature(432, "Authentication & User Management", [
        (433, "As a user, I can register and login (Keycloak)", "User Story"),
        (434, "Integrate Keycloak for Authentication", "Task"),
        (435, "Implement Keycloak JWT Token Validation", "Task"),
        (436, "As a platform admin, I can control access", "User Story"),
        (437, "Configure Role-Based Authorization (Keycloak)", "Task"),
    ])
    
    # Feature 438: Database & Data Layer
    print_feature(438, "Database & Data Layer", [
        (439, "As a developer, I can persist data to PostgreSQL", "User Story"),
        (440, "Create EF Core Entity Models", "Task"),
        (441, "Configure ApplicationDbContext and Migrations", "Task"),
        (442, "Implement Repository Pattern", "Task"),
    ])
    
    print("\n" + "=" * 80)
    print("Board: https://dev.azure.com/phoenixcode/Factory/_boards/board/t/Factory%20Team/Stories")
    print("=" * 80)
    print("")

if __name__ == "__main__":
    main()
