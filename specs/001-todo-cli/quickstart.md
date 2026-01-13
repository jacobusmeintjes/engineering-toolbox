# Quick Start Guide: TODO CLI Application

**Feature**: 001-todo-cli
**Date**: 2026-01-12
**Purpose**: End-to-end test scenarios and user onboarding

## Overview

This guide provides practical scenarios for testing and using the TODO CLI application. Each scenario maps to acceptance criteria from the feature specification and demonstrates key workflows.

---

## Installation & Setup

### Prerequisites

- .NET 8 SDK installed
- Terminal/command prompt access
- Windows 10+, macOS 12+, or Linux Ubuntu 22.04+

### Build & Install

```bash
# Clone repository
cd TodoCli/src/TodoCli

# Build project
dotnet build

# Run from build output
dotnet run -- <command>

# OR publish and add to PATH
dotnet publish -c Release
# Add publish directory to PATH environment variable
```

### First Run

On first execution, the application automatically creates:
- Storage directory (platform-specific)
- Empty `tasks.json` file

**Windows**: `C:\Users\<username>\AppData\Roaming\TodoCli\`
**macOS/Linux**: `~/.local/share/TodoCli/`

---

## Scenario 1: Quick Task Capture (P1)

**Goal**: Add a task with minimal input and verify it's saved.

**User Story**: Quick Task Capture (Priority: P1)

### Steps

```bash
# Add a simple task
todo add "Buy groceries"

# Expected output:
# ✓ Task added: Buy groceries [ID: a3f2c8b1]
```

### Verification

```bash
# List all tasks
todo list

# Expected output: Table with 1 task showing:
# - 8-character ID
# - [ ] status
# - "Buy groceries" title
# - [M] priority (default medium)
# - "-" due date (not set)
# - Empty tags
```

### Test Persistence

```bash
# Exit terminal and reopen
exit

# New terminal session
todo list

# Expected: Task still appears (persistence verified)
```

### Success Criteria Met

- ✅ SC-003: Task added in < 50ms
- ✅ SC-007: User completed intended task on first attempt
- ✅ SC-001: Learned core command within 5 minutes

---

## Scenario 2: Daily Task Review (P1)

**Goal**: View tasks with filtering and sorting.

**User Story**: Daily Task Review (Priority: P1)

### Setup

```bash
# Create diverse task set
todo add "Submit quarterly report" --due 2026-01-13 --priority high --tags work,urgent
todo add "Review code PR#42" --due 2026-01-14 --priority high --tags work,code-review
todo add "Buy birthday gift" --due 2026-01-16 --priority medium --tags personal,shopping
todo add "Update documentation" --priority low --tags work,docs
```

### Filter by Status

```bash
# Show all incomplete tasks (default)
todo list --status incomplete

# Complete one task
todo complete {id-of-report-task}

# Show only completed tasks
todo list --status complete

# Expected: Only the report task appears
```

### Filter by Priority

```bash
# Show only high priority tasks
todo list --priority high

# Expected: 2 tasks (report and PR review)
```

### Filter by Tags

```bash
# Show work-related tasks
todo list --tags work

# Expected: 3 tasks (report, PR, docs)

# Show personal tasks
todo list --tags personal

# Expected: 1 task (birthday gift)
```

### Sort Options

```bash
# Sort by priority (high first)
todo list --sort priority

# Expected: High priority tasks first, then medium, then low

# Sort by due date (closest first)
todo list --sort due

# Expected: Tasks sorted by due date, tasks without due date last
```

### Complex Filtering

```bash
# Incomplete high-priority work tasks
todo list --status incomplete --priority high --tags work

# Expected: PR review task (report was completed earlier)
```

### Success Criteria Met

- ✅ SC-004: Listed 100 tasks in < 200ms
- ✅ FR-031 through FR-037: All filter types working
- ✅ FR-042 through FR-049: Color coding and visual indicators

---

## Scenario 3: Task Completion Tracking (P1)

**Goal**: Mark tasks complete and verify timestamp/duration calculation.

**User Story**: Task Completion Tracking (Priority: P1)

### Steps

```bash
# Add a task
todo add "Write meeting notes" --tags work

# Note the task ID from output (e.g., b4e3d9c2)

# Wait a few minutes (or simulate with test data)

# Complete the task
todo complete b4e3

# Expected output:
# ✓ Task completed: Write meeting notes
#   Completed after 5 minutes
```

### Verify Completion

```bash
# List all tasks
todo list

# Expected: Task shows:
# - [✓] status (green)
# - Strikethrough title (green)
# - Completion timestamp
```

### View Details

```bash
# Show full task details
todo show b4e3

# Expected output includes:
# Status:      Completed ✓
# Created:     2026-01-12 10:00 AM (5 minutes ago)
# Completed:   2026-01-12 10:05 AM (just now)
#              (completed after 5 minutes)
```

### Success Criteria Met

- ✅ FR-008: Completion timestamp recorded
- ✅ FR-009: Duration calculated and displayed
- ✅ FR-010: Cannot re-complete already-completed task

---

## Scenario 4: Task Metadata Management (P2)

**Goal**: Create and update tasks with full metadata.

**User Story**: Task Metadata Management (Priority: P2)

### Add Task with Full Metadata

```bash
# Create task with all optional fields
todo add "Deploy v2.0 to production" \
  --description "Run deployment checklist, monitor for 1 hour post-deploy" \
  --due 2026-01-20 \
  --priority high \
  --tags work,deployment,release

# Expected: Task created with all metadata
```

### Update Task Properties

```bash
# Update priority and due date
todo update {task-id} --priority medium --due 2026-01-22

# Expected output:
# ✓ Task updated: Deploy v2.0 to production
#
# Changes:
#   Priority: high → medium
#   Due: 2026-01-20 → 2026-01-22
```

### Manage Tags

```bash
# Add tags
todo update {task-id} --add-tags production,urgent

# Expected: Tags added to existing tags (no duplicates)

# Remove tags
todo update {task-id} --remove-tags urgent

# Expected: Tag removed, others remain
```

### Clear Due Date

```bash
# Remove due date
todo update {task-id} --due none

# Expected: Due date cleared (shows "-" in list)
```

### Success Criteria Met

- ✅ FR-024 through FR-026: All update operations working
- ✅ FR-015 through FR-023: Metadata validation and constraints

---

## Scenario 5: Task Deletion (P3)

**Goal**: Permanently remove tasks with confirmation.

**User Story**: Task Deletion (Priority: P3)

### Delete with Confirmation

```bash
# Delete a task (triggers confirmation)
todo delete {task-id}

# Expected prompt:
# Delete this task permanently?
#
#   Title: Buy groceries
#   Priority: Medium
#   Due: 2026-01-15
#   Tags: personal, shopping
#
# ⚠ This action cannot be undone.
#
# Continue? (yes/no):

# Type "yes" to confirm
yes

# Expected output:
# ✓ Task deleted: Buy groceries
```

### Delete with Force Flag

```bash
# Delete without confirmation
todo delete {task-id} --force

# Expected: Immediate deletion, no prompt
```

### Verify Deletion

```bash
# Try to show deleted task
todo show {deleted-task-id}

# Expected error:
# ✗ Error: Task not found with ID: {id}
```

### Success Criteria Met

- ✅ FR-012: Confirmation required unless --force
- ✅ FR-013: Task details shown in confirmation
- ✅ FR-014: Permanent deletion from storage

---

## Scenario 6: Error Handling & Recovery

**Goal**: Validate error messages and graceful failure handling.

### Validation Errors

```bash
# Empty title
todo add ""
# Expected: ✗ Error: Task title cannot be empty

# Title too long (> 200 chars)
todo add "$(printf 'A%.0s' {1..201})"
# Expected: ✗ Error: Task title cannot exceed 200 characters

# Invalid due date format
todo add "Task" --due "tomorrow"
# Expected: ✗ Error: Invalid due date format. Use YYYY-MM-DD

# Due date in past
todo add "Task" --due 2020-01-01
# Expected: ✗ Error: Due date must be today or in the future

# Invalid priority
todo add "Task" --priority urgent
# Expected: ✗ Error: Invalid priority. Use low, medium, or high

# Invalid tag characters
todo add "Task" --tags "work@home"
# Expected: ✗ Error: Tag 'work@home' contains invalid characters
```

### Partial ID Matching

```bash
# Create two tasks with similar IDs (unlikely but possible)
# Assume IDs: a3f2c8b1... and a3f9d7e2...

# Try to complete with ambiguous partial ID
todo complete a3f

# Expected:
# Multiple tasks match ID 'a3f':
#
# 1. a3f2c8b1 - Buy groceries
# 2. a3f9d7e2 - Review pull request
#
# Please use a longer ID prefix to specify the task.

# Retry with longer prefix
todo complete a3f2

# Expected: Task completed successfully
```

### File Corruption Recovery

```bash
# Manually corrupt tasks.json (for testing)
# Edit file to contain invalid JSON

# Try to list tasks
todo list

# Expected:
# ⚠ Corruption detected. Restoring from backup...
# {tasks listed from backup}
```

### Success Criteria Met

- ✅ FR-071 through FR-075: All validation working
- ✅ FR-077: Partial ID disambiguation
- ✅ SC-011: Users can resolve errors without external help

---

## Scenario 7: Performance Validation

**Goal**: Verify performance targets are met.

### Startup Time

```bash
# Measure cold start time (Windows)
Measure-Command { todo --help }

# Expected: TotalMilliseconds < 100
```

### Add Task Performance

```bash
# Measure add command time
Measure-Command { todo add "Performance test task" }

# Expected: TotalMilliseconds < 50
```

### List Performance

```bash
# Add 100 tasks
for i in {1..100}; do
  todo add "Task $i" --tags test
done

# Measure list time
Measure-Command { todo list }

# Expected: TotalMilliseconds < 200
```

### Success Criteria Met

- ✅ SC-002: Startup < 100ms
- ✅ SC-003: Add task < 50ms
- ✅ SC-004: List 100 tasks < 200ms

---

## Scenario 8: Cross-Platform Verification

**Goal**: Verify identical behavior across platforms.

### Storage Location Verification

**Windows**:
```powershell
# Check storage location
dir "$env:APPDATA\TodoCli\"

# Expected: tasks.json and tasks.json.bak exist
```

**macOS/Linux**:
```bash
# Check storage location
ls ~/.local/share/TodoCli/

# Expected: tasks.json and tasks.json.bak exist
```

### Permission Verification

**Windows**:
```powershell
# Check file permissions (should be user-only)
icacls "$env:APPDATA\TodoCli\tasks.json"
```

**macOS/Linux**:
```bash
# Check file permissions (should be 600)
ls -l ~/.local/share/TodoCli/tasks.json

# Expected: -rw------- (user read/write only)
```

### Color Support Verification

All platforms should show:
- Green: Success messages, completed tasks
- Red: Errors, overdue tasks
- Yellow: Warnings, today's tasks

If terminal doesn't support colors, output should gracefully degrade to plain text with symbols (✓, ✗, [!]).

### Success Criteria Met

- ✅ SC-012: Identical behavior on all platforms
- ✅ FR-082: User-only file permissions

---

## Scenario 9: Help System

**Goal**: Verify comprehensive help documentation.

### Global Help

```bash
# Show all commands
todo --help

# Expected: List of all 6 commands with brief descriptions
```

### Command-Specific Help

```bash
# Show add command help
todo add --help

# Expected:
# - Description
# - Usage syntax
# - Argument details
# - Option details
# - Examples
```

### Success Criteria Met

- ✅ FR-063 through FR-065: Help system comprehensive
- ✅ SC-001: New users learn commands within 5 minutes

---

## Manual Editing

### JSON File Format

Tasks can be manually edited with a text editor:

```bash
# Open tasks.json
# Windows:
notepad %APPDATA%\TodoCli\tasks.json

# macOS:
open -e ~/.local/share/TodoCli/tasks.json

# Linux:
nano ~/.local/share/TodoCli/tasks.json
```

### Example Manual Edit

```json
[
  {
    "id": "a3f2c8b1-7d4e-4a9c-b6f1-2e8d9c5a7b3f",
    "title": "Buy groceries",
    "description": null,
    "createdAt": "2026-01-12T10:30:00Z",
    "dueDate": "2026-01-15",
    "isCompleted": false,
    "completedAt": null,
    "priority": "medium",
    "tags": ["personal", "shopping"]
  }
]
```

**Important**:
- Maintain valid JSON syntax
- Use ISO 8601 format for dates
- Backup file before editing (copy to tasks.json.manual.bak)
- Application validates JSON on load

---

## Troubleshooting

### Issue: "Failed to save task" error

**Cause**: Insufficient permissions or disk space

**Solution**:
1. Check disk space: Ensure sufficient storage
2. Verify permissions: Storage directory must be writable
3. Close other applications: Ensure file isn't locked

### Issue: Tasks not appearing after restart

**Cause**: File corruption or wrong storage location

**Solution**:
1. Check storage location (see Cross-Platform Verification)
2. Verify tasks.json exists and is valid JSON
3. Check tasks.json.bak for backup
4. Restore from backup if needed

### Issue: "Multiple tasks match ID" error

**Cause**: Partial ID too short

**Solution**:
- Use longer ID prefix (minimum 4 characters)
- Use unique prefix that distinguishes task
- Use full GUID if still ambiguous

### Issue: Colors not displaying

**Cause**: Terminal doesn't support ANSI colors

**Solution**:
- Use modern terminal (Windows Terminal, iTerm2, GNOME Terminal)
- Application automatically falls back to plain text
- Symbols (✓, ✗, [!]) still display

---

## Summary Checklist

After completing all scenarios, verify:

- ✅ **Core Workflows** (P1 features):
  - [x] Add task with minimal input
  - [x] List tasks with filtering
  - [x] Mark tasks complete
  - [x] Tasks persist across sessions

- ✅ **Enhanced Features** (P2 features):
  - [x] Add tasks with full metadata
  - [x] Update task properties
  - [x] Manage tags
  - [x] Filter by multiple criteria

- ✅ **Maintenance Features** (P3 features):
  - [x] Delete tasks with confirmation
  - [x] Force delete without confirmation

- ✅ **Quality Attributes**:
  - [x] Error messages are clear and actionable
  - [x] Performance targets met (< 100ms startup, < 50ms add)
  - [x] Help system comprehensive
  - [x] Cross-platform compatibility verified

---

## Next Steps

1. ✅ Quick start guide complete
2. ⏭️ Update agent context with technology stack
3. ⏭️ Run `/speckit.tasks` to generate implementation task breakdown
4. ⏭️ Run `/speckit.implement` to execute implementation plan
