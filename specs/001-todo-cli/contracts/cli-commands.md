# CLI Command Contracts: TODO CLI Application

**Feature**: 001-todo-cli
**Date**: 2026-01-12
**Purpose**: Define command signatures, option schemas, validation rules, and output formats

## Overview

This document specifies the contract for all 6 CLI commands in the TODO application. Each command definition includes syntax, arguments, options, validation rules, success/error outputs, and exit codes.

---

## Command Structure

All commands follow the standard System.CommandLine structure:

```bash
todo <command> [arguments] [options]
```

### Global Options

Available on all commands:

| Option | Alias | Type | Description |
|--------|-------|------|-------------|
| `--help` | `-h` | flag | Display command help |
| `--version` | N/A | flag | Display application version |

### Exit Codes

| Code | Meaning | When Used |
|------|---------|-----------|
| `0` | Success | Command completed successfully |
| `1` | Error | Runtime error (validation failure, file I/O error, etc.) |
| `2` | Invalid Usage | Invalid command syntax or arguments |

---

## Commands

### 1. `add` - Add a New Task

Add a task to the TODO list with optional metadata.

#### Syntax

```bash
todo add <title> [options]
```

#### Arguments

| Argument | Type | Required | Description |
|----------|------|----------|-------------|
| `<title>` | string | Yes | Task title (1-200 characters) |

#### Options

| Option | Alias | Type | Default | Validation | Description |
|--------|-------|------|---------|------------|-------------|
| `--description` | `-d` | string | null | Max 1000 chars | Extended task details |
| `--due` | N/A | string | null | YYYY-MM-DD, today or future | Target completion date |
| `--priority` | `-p` | enum | medium | low\|medium\|high | Importance level |
| `--tags` | `-t` | string | [] | Comma-separated, max 10 | Categorization labels |

#### Validation Rules

1. **Title**:
   - Cannot be empty or whitespace-only
   - Max length: 200 characters
   - Trimmed of leading/trailing whitespace

2. **Description**:
   - Optional (can be omitted)
   - Max length: 1000 characters if provided

3. **Due Date**:
   - Optional (can be omitted)
   - Format: YYYY-MM-DD (strict parsing)
   - Must be today or future date
   - Invalid dates rejected: "Invalid due date format. Use YYYY-MM-DD"
   - Past dates rejected: "Due date must be today or in the future"

4. **Priority**:
   - Case-insensitive: "High", "HIGH", "high" all accepted
   - Invalid values rejected: "Invalid priority. Use low, medium, or high"

5. **Tags**:
   - Comma-separated list
   - Each tag: 1-20 characters
   - Allowed characters: letters, numbers, hyphens, underscores
   - Converted to lowercase automatically
   - Max 10 tags per task
   - Duplicates removed (case-insensitive)
   - Invalid tag format: "Tag '{tag}' contains invalid characters"
   - Too many tags: "Maximum 10 tags allowed per task"

#### Examples

```bash
# Minimal (title only)
todo add "Buy groceries"

# With due date
todo add "Submit report" --due 2026-01-20

# With priority and tags
todo add "Review PR #42" --priority high --tags work,urgent

# Full metadata
todo add "Update documentation" \
  --description "Add API reference for new endpoints" \
  --due 2026-01-18 \
  --priority medium \
  --tags work,documentation,api
```

#### Success Output

```
✓ Task added: {title} [ID: {first-8-chars-of-guid}]
```

**Example**:
```
✓ Task added: Buy groceries [ID: a3f2c8b1]
```

#### Error Outputs

| Scenario | Message | Exit Code |
|----------|---------|-----------|
| Empty title | `✗ Error: Task title cannot be empty` | 1 |
| Title too long | `✗ Error: Task title cannot exceed 200 characters` | 1 |
| Description too long | `✗ Error: Task description cannot exceed 1000 characters` | 1 |
| Invalid due date format | `✗ Error: Invalid due date format. Use YYYY-MM-DD` | 1 |
| Due date in past | `✗ Error: Due date must be today or in the future` | 1 |
| Invalid priority | `✗ Error: Invalid priority. Use low, medium, or high` | 1 |
| Invalid tag characters | `✗ Error: Tag 'tag@name' contains invalid characters (use only letters, numbers, hyphens, underscores)` | 1 |
| Too many tags | `✗ Error: Maximum 10 tags allowed per task` | 1 |
| File I/O error | `✗ Error: Failed to save task: {error-message}` | 1 |

#### Functional Requirements Coverage

FR-001, FR-002, FR-003, FR-004, FR-005, FR-006, FR-015, FR-016, FR-017, FR-018, FR-019, FR-020, FR-021, FR-022, FR-023

---

### 2. `list` - Display Tasks

Display tasks with optional filtering and sorting.

#### Syntax

```bash
todo list [options]
```

#### Arguments

None (all parameters are options)

#### Options

| Option | Alias | Type | Default | Validation | Description |
|--------|-------|------|---------|------------|-------------|
| `--status` | `-s` | enum | all | all\|complete\|incomplete | Filter by completion status |
| `--priority` | `-p` | enum | null | low\|medium\|high | Filter by priority level |
| `--tags` | `-t` | string | null | Comma-separated | Filter by tags (OR logic) |
| `--due-before` | N/A | string | null | YYYY-MM-DD | Show tasks due on or before date |
| `--sort` | N/A | enum | created | created\|due\|priority | Sort order |

#### Validation Rules

1. **Status**:
   - Case-insensitive
   - Invalid values rejected: "Invalid status. Use all, complete, or incomplete"

2. **Priority**:
   - Case-insensitive
   - Optional (no filter if omitted)
   - Invalid values rejected: "Invalid priority. Use low, medium, or high"

3. **Tags**:
   - Comma-separated list
   - Matches tasks with ANY of the specified tags (OR logic)
   - Case-insensitive matching
   - No validation on tag names (user may filter by non-existent tags → returns empty)

4. **Due Before**:
   - Optional date filter
   - Format: YYYY-MM-DD
   - Invalid format rejected: "Invalid due-before date format. Use YYYY-MM-DD"

5. **Sort**:
   - Case-insensitive
   - created: Sort by CreatedAt ascending (oldest first)
   - due: Sort by DueDate ascending (closest first), nulls last
   - priority: Sort by Priority descending (high first), then CreatedAt

#### Filter Combination

Multiple filters use AND logic:
```bash
# Returns tasks that are incomplete AND high priority AND tagged "work"
todo list --status incomplete --priority high --tags work
```

#### Examples

```bash
# List all tasks (default)
todo list

# Show only incomplete tasks
todo list --status incomplete

# Show high priority tasks
todo list --priority high

# Show tasks tagged "work" or "urgent"
todo list --tags work,urgent

# Show tasks due by end of week
todo list --due-before 2026-01-17

# Complex filter: incomplete high-priority work tasks, sorted by due date
todo list --status incomplete --priority high --tags work --sort due
```

#### Success Output

**Table Format** (Spectre.Console):

```
┌──────────┬────────┬───────────────────────┬──────────┬────────────┬────────────┐
│ ID       │ Status │ Title                 │ Priority │ Due        │ Tags       │
├──────────┼────────┼───────────────────────┼──────────┼────────────┼────────────┤
│ a3f2c8b1 │ [ ]    │ Buy groceries         │ [M]      │ 2026-01-15 │ personal   │
│ b4e3d9c2 │ [✓]    │ Review pull request   │ [H]      │ 2026-01-13 │ work       │
│ c5f4e0d3 │ [ ]    │ Update documentation  │ [L]      │ -          │ work,docs  │
└──────────┴────────┴───────────────────────┴──────────┴────────────┴────────────┘

3 tasks shown
```

**Color Coding**:
- Overdue tasks: Red with `[!]` indicator
- Today's tasks: Yellow
- Completed tasks: Green with strikethrough
- High priority: Red `[H]`
- Medium priority: Yellow `[M]`
- Low priority: Grey `[L]`

**Empty Result**:
```
No tasks match the specified filters.
```

#### Column Specifications

| Column | Width | Content | Truncation |
|--------|-------|---------|------------|
| ID | 8 | First 8 chars of GUID | None |
| Status | 3 | `[ ]` or `[✓]` | None |
| Title | 30 | Task title | "..." at 28 chars |
| Priority | 3 | `[H]`, `[M]`, or `[L]` | None |
| Due | 10 | YYYY-MM-DD or "-" | None |
| Tags | 15 | Comma-separated | "..." at 13 chars |

**Total Width**: ~69 characters (fits in 80-column terminal with borders)

#### Error Outputs

| Scenario | Message | Exit Code |
|----------|---------|-----------|
| Invalid status | `✗ Error: Invalid status. Use all, complete, or incomplete` | 1 |
| Invalid priority | `✗ Error: Invalid priority. Use low, medium, or high` | 1 |
| Invalid due-before format | `✗ Error: Invalid due-before date format. Use YYYY-MM-DD` | 1 |
| Invalid sort option | `✗ Error: Invalid sort option. Use created, due, or priority` | 1 |
| File I/O error | `✗ Error: Failed to load tasks: {error-message}` | 1 |

#### Functional Requirements Coverage

FR-027, FR-028, FR-029, FR-030, FR-031, FR-032, FR-033, FR-034, FR-035, FR-036, FR-037, FR-042, FR-043, FR-044, FR-045, FR-046, FR-047, FR-048, FR-049

---

### 3. `complete` - Mark Task as Complete

Mark a task as complete with timestamp.

#### Syntax

```bash
todo complete <id>
```

#### Arguments

| Argument | Type | Required | Description |
|----------|------|----------|-------------|
| `<id>` | string | Yes | Full or partial task ID (min 4 chars) |

#### Validation Rules

1. **ID**:
   - Minimum 4 characters for partial match
   - Match is case-insensitive
   - Must match exactly one task
   - Zero matches: Error "Task not found with ID: {id}"
   - Multiple matches: Display disambiguation list

2. **Task State**:
   - Task must be incomplete
   - Already completed: Error "Task is already completed"

#### Disambiguation Flow

If partial ID matches multiple tasks:

```
Multiple tasks match ID 'a3f':

1. a3f2c8b1 - Buy groceries
2. a3f9d7e2 - Review pull request

Please use a longer ID prefix to specify the task.
```

User must re-run with longer prefix (e.g., `todo complete a3f2`)

#### Examples

```bash
# Complete by full ID
todo complete a3f2c8b1-7d4e-4a9c-b6f1-2e8d9c5a7b3f

# Complete by partial ID (recommended)
todo complete a3f2c8b1

# Complete by minimal partial ID
todo complete a3f2
```

#### Success Output

```
✓ Task completed: {title}
  Completed after {duration}
```

**Duration Calculation**:
- < 1 hour: "{minutes} minutes"
- < 1 day: "{hours} hours"
- >= 1 day: "{days} days"

**Examples**:
```
✓ Task completed: Buy groceries
  Completed after 3 hours

✓ Task completed: Review pull request
  Completed after 2 days
```

#### Error Outputs

| Scenario | Message | Exit Code |
|----------|---------|-----------|
| ID not provided | `✗ Error: Task ID is required` | 2 |
| Task not found | `✗ Error: Task not found with ID: {id}` | 1 |
| Multiple matches | `Multiple tasks match ID '{id}':\n\n{numbered-list}\n\nPlease use a longer ID prefix` | 1 |
| Already completed | `✗ Error: Task is already completed` | 1 |
| File I/O error | `✗ Error: Failed to save task: {error-message}` | 1 |

#### Functional Requirements Coverage

FR-007, FR-008, FR-009, FR-010, FR-077

---

### 4. `delete` - Remove Task Permanently

Permanently delete a task with optional confirmation.

#### Syntax

```bash
todo delete <id> [options]
```

#### Arguments

| Argument | Type | Required | Description |
|----------|------|----------|-------------|
| `<id>` | string | Yes | Full or partial task ID (min 4 chars) |

#### Options

| Option | Alias | Type | Default | Description |
|--------|-------|------|---------|-------------|
| `--force` | `-f` | flag | false | Skip confirmation prompt |

#### Validation Rules

1. **ID**:
   - Same validation as `complete` command
   - Minimum 4 characters for partial match
   - Must match exactly one task

2. **Confirmation**:
   - Required unless `--force` flag is used
   - User must type "yes" or "y" (case-insensitive)
   - Any other input cancels deletion

#### Confirmation Flow

Without `--force`:

```bash
$ todo delete a3f2
Delete this task permanently?

  Title: Buy groceries
  Priority: Medium
  Due: 2026-01-15
  Tags: personal, shopping

⚠ This action cannot be undone.

Continue? (yes/no):
```

If user types "yes" or "y": Delete task
If user types anything else: Cancel deletion

With `--force`:

```bash
$ todo delete a3f2 --force
✓ Task deleted: Buy groceries
```

#### Examples

```bash
# Delete with confirmation prompt
todo delete a3f2

# Delete without confirmation (force)
todo delete a3f2 --force

# Delete by full ID
todo delete a3f2c8b1-7d4e-4a9c-b6f1-2e8d9c5a7b3f --force
```

#### Success Output

```
✓ Task deleted: {title}
```

**Example**:
```
✓ Task deleted: Buy groceries
```

#### Cancellation Output

```
Task deletion cancelled.
```

#### Error Outputs

| Scenario | Message | Exit Code |
|----------|---------|-----------|
| ID not provided | `✗ Error: Task ID is required` | 2 |
| Task not found | `✗ Error: Task not found with ID: {id}` | 1 |
| Multiple matches | `Multiple tasks match ID '{id}':\n\n{numbered-list}\n\nPlease use a longer ID prefix` | 1 |
| File I/O error | `✗ Error: Failed to save tasks: {error-message}` | 1 |

#### Functional Requirements Coverage

FR-011, FR-012, FR-013, FR-014, FR-077

---

### 5. `update` - Modify Task Properties

Update one or more properties of an existing task.

#### Syntax

```bash
todo update <id> [options]
```

#### Arguments

| Argument | Type | Required | Description |
|----------|------|----------|-------------|
| `<id>` | string | Yes | Full or partial task ID (min 4 chars) |

#### Options

| Option | Alias | Type | Default | Validation | Description |
|--------|-------|------|---------|------------|-------------|
| `--title` | N/A | string | (no change) | 1-200 chars | Update task title |
| `--description` | `-d` | string | (no change) | Max 1000 chars | Update task description |
| `--due` | N/A | string | (no change) | YYYY-MM-DD or "none" | Update or clear due date |
| `--priority` | `-p` | enum | (no change) | low\|medium\|high | Update priority |
| `--add-tags` | N/A | string | (no change) | Comma-separated | Add tags (no duplicates) |
| `--remove-tags` | N/A | string | (no change) | Comma-separated | Remove tags (silent if not exist) |

#### Validation Rules

1. **ID**:
   - Same validation as `complete` and `delete`
   - Must match exactly one task

2. **Partial Updates**:
   - At least one option must be provided
   - Only specified fields are changed
   - Unchanged fields retain original values

3. **Title, Description, Priority, Due**:
   - Same validation as `add` command

4. **Due Date Clearing**:
   - Special value "none" or "null" clears due date
   - Example: `--due none`

5. **Tag Operations**:
   - `--add-tags`: Add to existing tags, prevent duplicates (case-insensitive)
   - `--remove-tags`: Remove from existing tags, silently ignore non-existent
   - Cannot use both `--add-tags` and `--remove-tags` in same command
   - Total tag count after addition must not exceed 10

6. **Immutable Fields**:
   - Cannot update: Id, CreatedAt, CompletedAt (system-managed)
   - Attempting to update these fields: N/A (not exposed as options)

#### Examples

```bash
# Update title only
todo update a3f2 --title "Buy groceries and supplies"

# Update priority and due date
todo update a3f2 --priority high --due 2026-01-14

# Clear due date
todo update a3f2 --due none

# Add tags
todo update a3f2 --add-tags urgent,important

# Remove tags
todo update a3f2 --remove-tags personal

# Update multiple fields
todo update a3f2 \
  --title "Complete grocery shopping" \
  --description "Include items for dinner party" \
  --priority high \
  --add-tags event
```

#### Success Output

```
✓ Task updated: {title}

Changes:
  {field-1}: {old-value} → {new-value}
  {field-2}: {old-value} → {new-value}
  ...
```

**Example**:
```
✓ Task updated: Buy groceries

Changes:
  Priority: medium → high
  Due: 2026-01-15 → 2026-01-14
  Tags: [personal] → [personal, urgent]
```

**No Changes**:
```
No changes made (all values identical to existing).
```

#### Error Outputs

| Scenario | Message | Exit Code |
|----------|---------|-----------|
| ID not provided | `✗ Error: Task ID is required` | 2 |
| No options provided | `✗ Error: At least one field must be specified for update` | 2 |
| Task not found | `✗ Error: Task not found with ID: {id}` | 1 |
| Multiple matches | `Multiple tasks match ID '{id}':\n\n{numbered-list}\n\nPlease use a longer ID prefix` | 1 |
| Title too long | `✗ Error: Task title cannot exceed 200 characters` | 1 |
| Description too long | `✗ Error: Task description cannot exceed 1000 characters` | 1 |
| Invalid due date | `✗ Error: Invalid due date format. Use YYYY-MM-DD or 'none' to clear` | 1 |
| Due date in past | `✗ Error: Due date must be today or in the future` | 1 |
| Invalid priority | `✗ Error: Invalid priority. Use low, medium, or high` | 1 |
| Too many tags | `✗ Error: Cannot add tags - would exceed maximum of 10 tags per task` | 1 |
| Invalid tag characters | `✗ Error: Tag '{tag}' contains invalid characters` | 1 |
| File I/O error | `✗ Error: Failed to save task: {error-message}` | 1 |

#### Functional Requirements Coverage

FR-024, FR-025, FR-026, FR-077

---

### 6. `show` - Display Task Details

Display complete details for a single task including calculated fields.

#### Syntax

```bash
todo show <id>
```

#### Arguments

| Argument | Type | Required | Description |
|----------|------|----------|-------------|
| `<id>` | string | Yes | Full or partial task ID (min 4 chars) |

#### Validation Rules

1. **ID**:
   - Same validation as other commands
   - Must match exactly one task

#### Examples

```bash
# Show task details by partial ID
todo show a3f2

# Show by full ID
todo show a3f2c8b1-7d4e-4a9c-b6f1-2e8d9c5a7b3f
```

#### Success Output

```
─────────────────────────────────────────
Task Details
─────────────────────────────────────────

ID:          {full-guid}
Title:       {title}
Description: {description or "-"}

Status:      {Incomplete or Completed}
Priority:    {Low, Medium, or High}
Due Date:    {YYYY-MM-DD or "-"}

Created:     {local-datetime} ({age})
Completed:   {local-datetime or "-"} {(duration) if completed}

Tags:        {comma-separated or "-"}

─────────────────────────────────────────
```

**Example (Incomplete Task)**:
```
─────────────────────────────────────────
Task Details
─────────────────────────────────────────

ID:          a3f2c8b1-7d4e-4a9c-b6f1-2e8d9c5a7b3f
Title:       Buy groceries
Description: Need milk, eggs, bread, and coffee

Status:      Incomplete
Priority:    Medium
Due Date:    2026-01-15 (in 3 days)

Created:     2026-01-12 10:30 AM (2 hours ago)
Completed:   -

Tags:        personal, shopping

─────────────────────────────────────────
```

**Example (Completed Task)**:
```
─────────────────────────────────────────
Task Details
─────────────────────────────────────────

ID:          b4e3d9c2-8e5f-5b0d-c7g2-3f9e0d6b8c4g
Title:       Review pull request #42
Description: -

Status:      Completed ✓
Priority:    High
Due Date:    2026-01-13 (was yesterday)

Created:     2026-01-12 11:00 AM (3 hours ago)
Completed:   2026-01-12 2:30 PM (30 minutes ago)
             (completed after 3 hours)

Tags:        work, code-review

─────────────────────────────────────────
```

#### Calculated Fields

| Field | Calculation | Format |
|-------|-------------|--------|
| Task age | Now - CreatedAt | "{n} minutes/hours/days ago" |
| Time until due | DueDate - Today | "in {n} days" or "was {n} days ago" |
| Time since completion | Now - CompletedAt | "{n} minutes/hours/days ago" |
| Completion duration | CompletedAt - CreatedAt | "completed after {n} minutes/hours/days" |

#### Error Outputs

| Scenario | Message | Exit Code |
|----------|---------|-----------|
| ID not provided | `✗ Error: Task ID is required` | 2 |
| Task not found | `✗ Error: Task not found with ID: {id}` | 1 |
| Multiple matches | `Multiple tasks match ID '{id}':\n\n{numbered-list}\n\nPlease use a longer ID prefix` | 1 |
| File I/O error | `✗ Error: Failed to load tasks: {error-message}` | 1 |

#### Functional Requirements Coverage

FR-038, FR-039, FR-040, FR-041, FR-077

---

## Cross-Cutting Concerns

### Help System

All commands support `--help`:

```bash
todo --help            # Show all commands
todo add --help        # Show add command help
todo list --help       # Show list command help
# ... etc
```

**Help Output Format**:

```
Description:
  {command-description}

Usage:
  todo {command} {usage-pattern}

Arguments:
  {argument-list}

Options:
  {option-list}

Examples:
  {examples}
```

**Business Rule**: FR-063, FR-064, FR-065

### Tab Completion

Shell completion scripts can be generated for:
- Bash
- Zsh
- PowerShell
- Fish

**Commands complete**: add, list, complete, delete, update, show
**Options complete**: All long-form options (--priority, --tags, etc.)

**Business Rule**: FR-067

### Error Message Format

All error messages follow consistent pattern:

```
✗ Error: {error-description}

{optional-suggestion}

Try 'todo {command} --help' for more information.
```

**Example**:
```
✗ Error: Unknown command 'ad'. Did you mean 'add'?

Try 'todo --help' for more information.
```

**Business Rule**: FR-068, FR-069

### Output Colors

| Type | Color | Symbol | Usage |
|------|-------|--------|-------|
| Success | Green | ✓ | Confirmations, completed tasks |
| Error | Red | ✗ | Error messages, overdue tasks |
| Warning | Yellow | ⚠ | Warnings, tasks due today |
| Info | Cyan | ℹ | Informational messages |
| Normal | Default | - | Regular output |

**Graceful Degradation**: If terminal doesn't support colors, symbols remain but colors are omitted.

**Business Rule**: FR-049

---

## Performance Contracts

| Command | Target | Measurement Point |
|---------|--------|------------------|
| `add` | < 50ms | Command invocation to confirmation displayed |
| `list` (100 tasks) | < 200ms | Command invocation to table rendered |
| `complete` | < 50ms | Command invocation to confirmation displayed |
| `delete` | < 50ms | Command invocation to confirmation displayed (excluding user input wait) |
| `update` | < 50ms | Command invocation to confirmation displayed |
| `show` | < 50ms | Command invocation to details displayed |

**Business Rules**: PR-2, PR-3, FR-079

---

## Summary

All 6 commands are fully specified with:
- ✅ Clear syntax and usage patterns
- ✅ Comprehensive validation rules
- ✅ Success and error output formats
- ✅ Exit code conventions
- ✅ Functional requirements traceability
- ✅ Performance targets

**Next Steps**:
1. ✅ CLI command contracts defined
2. ⏭️ Generate quick start guide (`quickstart.md`)
3. ⏭️ Update agent context with technology stack
4. ⏭️ Proceed to Phase 2 task breakdown (`/speckit.tasks`)
