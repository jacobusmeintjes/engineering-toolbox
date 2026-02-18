#!/usr/bin/env python3
"""
Create proper Azure DevOps work item hierarchy:
Epic → Feature → User Story → Task
"""

import subprocess
import json
import sys
import time

PARENT_EPIC_ID = 425  # OpenLifter Blazor Migration

def run_cmd(cmd):
    """Execute Azure DevOps command and return parsed JSON."""
    try:
        result = subprocess.run(cmd, capture_output=True, text=True, check=True)
        return json.loads(result.stdout)
    except subprocess.CalledProcessError as e:
        print(f"❌ Command failed: {' '.join(cmd)}")
        print(f"   Error: {e.stderr}")
        return None
    except json.JSONDecodeError:
        print(f"❌ Failed to parse JSON response")
        return None

def create_feature(title, description):
    """Create a Feature under the parent Epic."""
    cmd = [
        "python3", ".github/skills/scrum-master/azdo_workitems.py", "create",
        "--title", title,
        "--type", "Feature",
        "--description", description,
        "--priority", "1",
        "--parent-id", str(PARENT_EPIC_ID)
    ]
    result = run_cmd(cmd)
    if result:
        print(f"✅ Feature: {result['title']} (ID: {result['id']})")
        return result['id']
    return None

def create_user_story(title, description, feature_id):
    """Create a User Story under a Feature."""
    cmd = [
        "python3", ".github/skills/scrum-master/azdo_workitems.py", "create",
        "--title", title,
        "--type", "User Story",
        "--description", description,
        "--priority", "1",
        "--parent-id", str(feature_id)
    ]
    result = run_cmd(cmd)
    if result:
        print(f"  ✅ User Story: {result['title']} (ID: {result['id']})")
        return result['id']
    return None

def create_task(title, description, effort, user_story_id):
    """Create a Task under a User Story."""
    cmd = [
        "python3", ".github/skills/scrum-master/azdo_workitems.py", "create",
        "--title", title,
        "--type", "Task",
        "--description", description,
        "--effort", str(effort),
        "--priority", "1",
        "--parent-id", str(user_story_id)
    ]
    result = run_cmd(cmd)
    if result:
        print(f"    ✅ Task: {result['title']} (ID: {result['id']})")
        return result['id']
    return None

def main():
    print("=" * 80)
    print("Creating Azure DevOps Work Item Hierarchy")
    print("Epic → Feature → User Story → Task")
    print("=" * 80)
    print(f"\nParent Epic ID: {PARENT_EPIC_ID}\n")
    
    # Feature 1: Foundation & Infrastructure
    print("\n[1/10] Creating Feature: Foundation & Infrastructure...")
    feature1 = create_feature(
        "Foundation & Infrastructure",
        "Establish project foundation with .NET Aspire, CI/CD, Azure infrastructure, and monitoring"
    )
    
    if feature1:
        # User Story 1.1
        us1 = create_user_story(
            "As a developer, I can set up the solution so that the team can start building features",
            "Set up .NET 10 Aspire solution with all projects, CI/CD pipeline, and Azure infrastructure",
            feature1
        )
        if us1:
            create_task("Create .NET Aspire Solution Structure", 
                       "Create AppHost, API, AdminWeb, PublicWeb, Core, Infrastructure projects", 4, us1)
            create_task("Setup CI/CD Pipeline with GitHub Actions",
                       "Configure build, test, and deployment workflows", 6, us1)
            create_task("Provision Azure Infrastructure with Bicep",
                       "Create IaC for PostgreSQL, App Services, Redis, SignalR, etc.", 8, us1)
            create_task("Configure Logging and Monitoring",
                       "Setup Serilog and Application Insights", 4, us1)
    
    time.sleep(1)  # Rate limiting
    
    # Feature 2: Authentication & User Management
    print("\n[2/10] Creating Feature: Authentication & User Management...")
    feature2 = create_feature(
        "Authentication & User Management",
        "Implement ASP.NET Core Identity, JWT authentication, and role-based authorization for 5 user roles"
    )
    
    if feature2:
        # User Story 2.1
        us1 = create_user_story(
            "As a user, I can register and login so that I can access the platform",
            "User registration with email confirmation and login with JWT tokens",
            feature2
        )
        if us1:
            create_task("Implement ASP.NET Core Identity",
                       "Configure ApplicationUser, password requirements, email confirmation", 6, us1)
            create_task("Implement JWT Authentication",
                       "JWT token generation, refresh tokens, validation middleware", 6, us1)
        
        # User Story 2.2
        us2 = create_user_story(
            "As a platform admin, I can control access based on user roles",
            "Role-based authorization with 5 roles: PlatformAdmin, FederationAdmin, MeetDirector, Athlete, Spectator",
            feature2
        )
        if us2:
            create_task("Configure Role-Based Authorization",
                       "Setup authorization policies for all 5 roles", 5, us2)
    
    time.sleep(1)
    
    # Feature 3: Database & Data Layer
    print("\n[3/10] Creating Feature: Database & Data Layer...")
    feature3 = create_feature(
        "Database & Data Layer",
        "Build complete data layer with EF Core 10, PostgreSQL entities, migrations, and repository pattern"
    )
    
    if feature3:
        # User Story 3.1
        us1 = create_user_story(
            "As a developer, I can persist data to PostgreSQL using EF Core",
            "Complete entity model with User, Federation, Meet, Entry, Attempt, Platform, Flight",
            feature3
        )
        if us1:
            create_task("Create EF Core Entity Models",
                       "Build all domain entities with base interfaces and navigation properties", 8, us1)
            create_task("Configure ApplicationDbContext and Migrations",
                       "Setup DbContext with configurations and initial migration", 6, us1)
            create_task("Implement Repository Pattern",
                       "Generic repository and UnitOfWork pattern", 5, us1)
    
    time.sleep(1)
    
    # Feature 4: Meet Management
    print("\n[4/10] Creating Feature: Meet Management...")
    feature4 = create_feature(
        "Meet Management (Admin)",
        "Comprehensive meet management with creation wizard, configuration, and lifecycle management"
    )
    
    if feature4:
        # User Story 4.1
        us1 = create_user_story(
            "As a meet director, I can create and configure a meet",
            "Multi-step wizard for meet creation with basic info, dates, rules, divisions, weight classes, equipment",
            feature4
        )
        if us1:
            create_task("Build Meet CRUD API Endpoints",
                       "POST/GET/PUT/DELETE /api/v1/meets with MediatR and FluentValidation", 8, us1)
            create_task("Build Meet Creation Wizard UI",
                       "7-step Blazor wizard with auto-save and progress indicator", 8, us1)
            create_task("Style Meet Management Pages",
                       "CSS for wizard, forms, validation states", 4, us1)
    
    time.sleep(1)
    
    # Feature 5: Registration Management
    print("\n[5/10] Creating Feature: Registration Management...")
    feature5 = create_feature(
        "Registration Management",
        "Athlete registration system with online forms, bulk import, weigh-ins, and flight assignments"
    )
    
    if feature5:
        us1 = create_user_story(
            "As an athlete, I can register for a meet online",
            "Online registration form with athlete info, division/weight class selection, and payment",
            feature5
        )
        if us1:
            create_task("Build Registration API Endpoints",
                       "POST /api/v1/meets/{id}/register with validation", 6, us1)
            create_task("Build Registration Form UI",
                       "Blazor registration form with validation", 6, us1)
    
    time.sleep(1)
    
    # Feature 6: Live Scoring System
    print("\n[6/10] Creating Feature: Live Scoring System...")
    feature6 = create_feature(
        "Live Scoring System",
        "Real-time scoring with SignalR, attempt recording, lifting order calculation, and live standings"
    )
    
    if feature6:
        us1 = create_user_story(
            "As a scorekeeper, I can record attempts in real-time",
            "Attempt entry with good/no-good calls, weight, and automatic lifting order updates",
            feature6
        )
        if us1:
            create_task("Implement SignalR Hub for Live Scoring",
                       "LiveScoringHub with attempt recording and broadcast", 8, us1)
            create_task("Build Scoring API Endpoints",
                       "POST /api/v1/attempts with lifting order calculation", 8, us1)
            create_task("Build Live Scoring Admin UI",
                       "Blazor scoring interface with SignalR integration", 8, us1)
    
    time.sleep(1)
    
    # Feature 7: Results & Export
    print("\n[7/10] Creating Feature: Results & Export...")
    feature7 = create_feature(
        "Results & Export",
        "Placement calculation, results display, and federation-specific export formats"
    )
    
    if feature7:
        us1 = create_user_story(
            "As a meet director, I can view and export meet results",
            "Results with placement calculation, formula scores, and exports",
            feature7
        )
    
    time.sleep(1)
    
    # Feature 8: Public Site & Calendar
    print("\n[8/10] Creating Feature: Public Site & Calendar...")
    feature8 = create_feature(
        "Public Site & Calendar",
        "Public-facing meet calendar and live tracking for spectators"
    )
    
    if feature8:
        us1 = create_user_story(
            "As a spectator, I can view upcoming meets and track live scoring",
            "Meet calendar with filtering and live tracking page with real-time updates",
            feature8
        )
    
    time.sleep(1)
    
    # Feature 9: Data Import
    print("\n[9/10] Creating Feature: Data Import from Desktop...")
    feature9 = create_feature(
        "Data Import from Desktop OpenLifter",
        "JSON import from desktop OpenLifter with validation and mapping"
    )
    
    if feature9:
        us1 = create_user_story(
            "As a meet director, I can import existing meet data from desktop OpenLifter",
            "JSON file upload with validation, preview, and import",
            feature9
        )
    
    time.sleep(1)
    
    # Feature 10: Testing & QA
    print("\n[10/10] Creating Feature: Testing & QA...")
    feature10 = create_feature(
        "Testing & Quality Assurance",
        "Comprehensive testing with unit tests, API tests (HAR recording), and UI tests (video/trace recording)"
    )
    
    if feature10:
        us1 = create_user_story(
            "As a developer, I can verify code quality through automated tests",
            "Complete test suite with 80%+ coverage, API recording, and UI recording",
            feature10
        )
    
    print("\n" + "=" * 80)
    print("✅ Work item hierarchy created successfully!")
    print("=" * 80)
    print("\nHierarchy:")
    print(f"  Epic {PARENT_EPIC_ID}: OpenLifter Blazor Migration")
    print("    ├── Feature: Foundation & Infrastructure")
    print("    │     └── User Story: Setup solution")
    print("    │           ├── Task: Solution structure")
    print("    │           ├── Task: CI/CD pipeline")
    print("    │           ├── Task: Azure infrastructure")
    print("    │           └── Task: Logging")
    print("    ├── Feature: Authentication & User Management")
    print("    │     ├── User Story: Register and login")
    print("    │     └── User Story: Role-based access")
    print("    ├── Feature: Database & Data Layer")
    print("    ├── Feature: Meet Management")
    print("    ├── Feature: Registration Management")
    print("    ├── Feature: Live Scoring System")
    print("    ├── Feature: Results & Export")
    print("    ├── Feature: Public Site & Calendar")
    print("    ├── Feature: Data Import")
    print("    └── Feature: Testing & QA")
    print(f"\nView Epic: https://dev.azure.com/phoenixcode/cf79c609-44af-4b35-aeed-80cab26b6c41/_workitems/edit/{PARENT_EPIC_ID}")

if __name__ == "__main__":
    sys.exit(main())
