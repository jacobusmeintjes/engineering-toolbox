#!/usr/bin/env python3
"""
Update Azure DevOps tasks to reflect local-first development approach.

Changes:
- Remove/defer CI/CD and cloud infrastructure tasks
- Update auth to use Keycloak instead of ASP.NET Core Identity
- Focus on local Aspire development
"""

import subprocess
import sys

def run_cmd(cmd):
    """Run command and return output"""
    result = subprocess.run(cmd, shell=True, capture_output=True, text=True)
    if result.returncode != 0:
        print(f"ERROR: {result.stderr}", file=sys.stderr)
        return False
    print(result.stdout)
    return True

# Task 428: Update to emphasize local Aspire development
print("📝 Updating Task 428: Create .NET Aspire Solution Structure (LOCAL)")
run_cmd("""python3 .github/skills/scrum-master/azdo_workitems.py update \
    --id 428 \
    --title "Create .NET Aspire Solution Structure (Local Development)" \
    --description "Create .NET 10 Aspire solution for LOCAL development. All services run in Aspire AppHost with Docker containers (PostgreSQL, Redis, Keycloak). NO cloud resources." \
    --tags "local-dev,aspire,foundation"
""")

# Task 429: Mark as Removed - no CI/CD until MVP ready
print("\n❌ Removing Task 429: CI/CD Pipeline (deferred until MVP)")
run_cmd("""python3 .github/skills/scrum-master/azdo_workitems.py move \
    --id 429 \
    --state Removed
""")

# Task 430: Mark as Removed - no Azure infrastructure for now
print("\n❌ Removing Task 430: Azure Infrastructure (deferred until production)")
run_cmd("""python3 .github/skills/scrum-master/azdo_workitems.py move \
    --id 430 \
    --state Removed
""")

# Task 431: Update to focus on local logging only
print("\n📝 Updating Task 431: Configure Local Logging")
run_cmd("""python3 .github/skills/scrum-master/azdo_workitems.py update \
    --id 431 \
    --title "Configure Local Logging (Serilog + Seq)" \
    --description "Setup Serilog with Seq for local development. NO Application Insights or cloud monitoring. Seq runs in Docker via Aspire." \
    --tags "local-dev,logging,serilog,seq"
""")

# Task 434: Change to Keycloak integration
print("\n📝 Updating Task 434: Integrate Keycloak for Authentication")
run_cmd("""python3 .github/skills/scrum-master/azdo_workitems.py update \
    --id 434 \
    --title "Integrate Keycloak for Authentication" \
    --description "Integrate Keycloak (running in Docker via Aspire) for authentication and authorization. Configure realms, clients, and roles. NO ASP.NET Core Identity until production." \
    --tags "local-dev,keycloak,authentication"
""")

# Task 435: Update to Keycloak JWT
print("\n📝 Updating Task 435: Implement Keycloak JWT Authentication")
run_cmd("""python3 .github/skills/scrum-master/azdo_workitems.py update \
    --id 435 \
    --title "Implement Keycloak JWT Token Validation" \
    --description "Configure API to validate JWT tokens issued by Keycloak. Setup role-based authorization using Keycloak roles." \
    --tags "local-dev,keycloak,jwt,authorization"
""")

print("\n✅ All tasks updated for local-first development approach")
print("\nSummary:")
print("- Task 428: Updated for local Aspire development")
print("- Task 429: Removed (CI/CD deferred)")
print("- Task 430: Removed (Azure infra deferred)")
print("- Task 431: Updated for local logging with Seq")
print("- Task 434: Changed to Keycloak integration")
print("- Task 435: Updated to Keycloak JWT")
