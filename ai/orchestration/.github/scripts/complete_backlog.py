#!/usr/bin/env python3
"""
Complete Azure DevOps backlog with all user stories and tasks.
Adds remaining items to existing Features.
"""

import subprocess
import json
import time

# Existing Feature IDs from previous creation
FEATURES = {
    "foundation": 426,
    "auth": 432,
    "database": 438,
    "meet_mgmt": 443,
    "registration": 448,
    "live_scoring": 452,
    "results": 457,
    "public_site": 459,
    "import": 461,
    "testing": 463
}

def run_cmd(cmd):
    """Execute Azure DevOps command and return parsed JSON."""
    try:
        result = subprocess.run(cmd, capture_output=True, text=True, check=True)
        return json.loads(result.stdout)
    except (subprocess.CalledProcessError, json.JSONDecodeError) as e:
        print(f"❌ Error: {e}")
        return None

def create_user_story(title, description, feature_id):
    """Create a User Story under a Feature."""
    cmd = [
        "python3", ".github/skills/scrum-master/azdo_workitems.py", "create",
        "--title", title, "--type", "User Story",
        "--description", description, "--priority", "1",
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
        "--title", title, "--type", "Task",
        "--description", description, "--effort", str(effort),
        "--priority", "1", "--parent-id", str(user_story_id)
    ]
    result = run_cmd(cmd)
    if result:
        print(f"    ✅ Task: {result['title']} (ID: {result['id']})")
        return result['id']
    return None

print("=" * 80)
print("Completing Azure DevOps Backlog")
print("=" * 80)

# FEATURE 4: Meet Management - Add more user stories
print("\n[FEATURE 4] Meet Management - Adding user stories...")
us_meet_2 = create_user_story(
    "As a meet director, I can manage meet status lifecycle",
    "Manage meet through statuses: Draft, Published, Registration Open/Closed, In Progress, Completed, Cancelled",
    FEATURES["meet_mgmt"]
)
if us_meet_2:
    create_task("Implement Meet Status Workflow API", 
               "State transitions with validation and business rules", 5, us_meet_2)
    create_task("Build Meet Status Management UI",
               "Status badge, transition buttons, confirmation dialogs", 4, us_meet_2)

time.sleep(1)

us_meet_3 = create_user_story(
    "As a meet director, I can configure divisions and weight classes",
    "Set up divisions (Open, Masters, Junior) and weight class boundaries for the meet",
    FEATURES["meet_mgmt"]
)
if us_meet_3:
    create_task("Build Division/Weight Class API Endpoints",
               "CRUD for divisions and weight classes with validation", 6, us_meet_3)
    create_task("Build Division/Weight Class Configuration UI",
               "Multi-select, custom weight boundaries, preview", 6, us_meet_3)

time.sleep(1)

# FEATURE 5: Registration - Add more user stories and tasks
print("\n[FEATURE 5] Registration Management - Adding user stories...")
us_reg_2 = create_user_story(
    "As a meet director, I can bulk import athlete registrations",
    "Import registrations from CSV file with validation and preview",
    FEATURES["registration"]
)
if us_reg_2:
    create_task("Build CSV Import API Endpoint",
               "Parse CSV, validate data, return preview/errors", 6, us_reg_2)
    create_task("Build CSV Import UI with Preview",
               "File upload, column mapping, validation errors, confirm import", 6, us_reg_2)

time.sleep(1)

us_reg_3 = create_user_story(
    "As a meet director, I can record weigh-ins",
    "Record bodyweight for each athlete during weigh-in period",
    FEATURES["registration"]
)
if us_reg_3:
    create_task("Build Weigh-In API Endpoints",
               "POST weight with validation (within weight class)", 4, us_reg_3)
    create_task("Build Weigh-In Recording UI",
               "Quick entry interface, QR code scanner, weight validation", 6, us_reg_3)

time.sleep(1)

us_reg_4 = create_user_story(
    "As a meet director, I can assign athletes to flights",
    "Organize athletes into flights for efficient meet flow",
    FEATURES["registration"]
)
if us_reg_4:
    create_task("Build Flight Assignment Algorithm",
               "Auto-assign based on weight class, division, platform", 5, us_reg_4)
    create_task("Build Flight Management UI",
               "Drag-drop athlete assignment, flight schedule", 6, us_reg_4)

time.sleep(1)

# FEATURE 6: Live Scoring - Add more user stories and tasks
print("\n[FEATURE 6] Live Scoring - Adding user stories...")
us_score_2 = create_user_story(
    "As a referee, I can submit lift decisions",
    "White light/red light decisions for each attempt",
    FEATURES["live_scoring"]
)
if us_score_2:
    create_task("Build Referee Decision API",
               "Record 3 referee decisions, calculate majority", 4, us_score_2)
    create_task("Build Referee Decision UI",
               "Large buttons for white/red light, referee identification", 4, us_score_2)

time.sleep(1)

us_score_3 = create_user_story(
    "As a scorekeeper, I can see automatic bar loading calculations",
    "Display plates needed for next attempt based on available equipment",
    FEATURES["live_scoring"]
)
if us_score_3:
    create_task("Implement Bar Loading Algorithm",
               "Calculate optimal plate combination from available plates", 6, us_score_3)
    create_task("Build Bar Loading Display UI",
               "Visual plate diagram, collar position, total weight", 5, us_score_3)

time.sleep(1)

us_score_4 = create_user_story(
    "As a spectator, I can view live standings",
    "Real-time leaderboard updated after each attempt",
    FEATURES["live_scoring"]
)
if us_score_4:
    create_task("Implement Standings Calculation",
               "Formula calculations (Wilks, Dots, etc.), placement by division", 6, us_score_4)
    create_task("Build Live Standings Public UI",
               "Auto-refresh leaderboard with SignalR", 5, us_score_4)

time.sleep(1)

# FEATURE 7: Results & Export - Add tasks
print("\n[FEATURE 7] Results & Export - Adding tasks...")
us_results_1 = 458  # Existing user story
create_task("Implement Results Calculation Engine",
           "Final placements, formula scores, records", 6, us_results_1)
create_task("Build Results Display Page",
           "Filterable results table by division, weight class", 5, us_results_1)
create_task("Implement Federation Export Formats",
           "USAPL, IPF, USPA CSV/JSON export formats", 6, us_results_1)
create_task("Build Export UI",
           "Format selection, download button, preview", 3, us_results_1)

time.sleep(1)

# FEATURE 8: Public Site - Add tasks
print("\n[FEATURE 8] Public Site & Calendar - Adding tasks...")
us_public_1 = 460  # Existing user story
create_task("Build Meet Calendar API Endpoint",
           "GET /api/v1/calendar with date/location/federation filters", 5, us_public_1)
create_task("Build Meet Calendar Page",
           "Calendar grid view, list view, filters, search", 8, us_public_1)
create_task("Build Live Tracking Spectator Page",
           "Real-time attempt updates via SignalR for public viewers", 6, us_public_1)
create_task("Style Public Site",
           "Responsive design, SEO-friendly, accessibility", 6, us_public_1)

time.sleep(1)

us_public_2 = create_user_story(
    "As an athlete, I can view my competition history",
    "Personal dashboard with past meets, PRs, and statistics",
    FEATURES["public_site"]
)
if us_public_2:
    create_task("Build Athlete Profile API",
               "GET /api/v1/athletes/{id} with meet history", 5, us_public_2)
    create_task("Build Athlete Profile Page",
               "Meet history, PRs, personal records, graph", 6, us_public_2)

time.sleep(1)

# FEATURE 9: Data Import - Add tasks
print("\n[FEATURE 9] Data Import - Adding tasks...")
us_import_1 = 462  # Existing user story
create_task("Build JSON Import Parser",
           "Parse desktop OpenLifter JSON format", 5, us_import_1)
create_task("Implement Data Validation for Import",
           "Validate athletes, divisions, weight classes, attempts", 6, us_import_1)
create_task("Build Import Preview UI",
           "Show what will be imported, conflicts, warnings", 6, us_import_1)
create_task("Implement Import Execution",
           "Transaction handling, rollback on error", 5, us_import_1)

time.sleep(1)

# FEATURE 10: Testing - Add tasks
print("\n[FEATURE 10] Testing & QA - Adding tasks...")
us_testing_1 = 464  # Existing user story

# Unit Tests
create_task("Write Unit Tests for Authentication",
           "Test Identity, JWT, authorization policies with NSubstitute", 8, us_testing_1)
create_task("Write Unit Tests for Meet Management",
           "Test meet CRUD, validation, status transitions", 8, us_testing_1)
create_task("Write Unit Tests for Scoring Logic",
           "Test lifting order, bar loading, standings calculation", 8, us_testing_1)
create_task("Write Unit Tests for Results",
           "Test placement calculation, formula calculations", 6, us_testing_1)

time.sleep(1)

# API Tests
create_task("Write API Tests for Authentication [HAR]",
           "Test login, register, refresh token with HAR recording", 6, us_testing_1)
create_task("Write API Tests for Meet Management [HAR]",
           "Test meet CRUD endpoints with HAR recording", 6, us_testing_1)
create_task("Write API Tests for Scoring [HAR]",
           "Test attempt recording, lifting order with HAR recording", 6, us_testing_1)

time.sleep(1)

# UI Tests
create_task("Write UI Tests for Meet Creation Wizard [VIDEO]",
           "Test 7-step wizard with video and trace recording", 8, us_testing_1)
create_task("Write UI Tests for Live Scoring [VIDEO]",
           "Test scoring interface with SignalR updates and recording", 8, us_testing_1)
create_task("Write UI Tests for Public Pages [VIDEO]",
           "Test calendar, live tracking, athlete profile with recording", 6, us_testing_1)

print("\n" + "=" * 80)
print("✅ Complete backlog created!")
print("=" * 80)
print("\nTotal Structure:")
print("  1 Epic: OpenLifter Blazor Migration")
print("  10 Features")
print("  ~25 User Stories")
print("  ~60 Tasks")
print("\nAll work items ready for implementation!")
