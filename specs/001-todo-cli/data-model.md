# Data Model: TODO CLI Application

**Feature**: 001-todo-cli
**Date**: 2026-01-12
**Purpose**: Define entity schemas, validation rules, and state transitions

## Overview

The TODO CLI application has a single core entity: **TodoTask**. This document defines the complete data model including field specifications, validation rules, business logic constraints, and JSON serialization format.

---

## Entities

### TodoTask

Represents a single TODO task with title, optional metadata, and completion tracking.

#### Fields

| Field | Type | Required | Default | Constraints | Description |
|-------|------|----------|---------|-------------|-------------|
| `Id` | `Guid` | Yes (auto) | New GUID | Immutable, unique | Unique identifier for the task |
| `Title` | `string` | Yes | N/A | 1-200 characters, non-empty | Main description of the task |
| `Description` | `string?` | No | `null` | 0-1000 characters | Extended details about the task |
| `CreatedAt` | `DateTime` | Yes (auto) | UTC now | Immutable, UTC | When the task was created |
| `DueDate` | `DateOnly?` | No | `null` | Must be today or future | Target completion date |
| `IsCompleted` | `bool` | Yes (auto) | `false` | One-way: false → true only | Completion status |
| `CompletedAt` | `DateTime?` | No (auto) | `null` | Set when IsCompleted=true | When the task was completed |
| `Priority` | `Priority` | Yes | `Medium` | Low, Medium, or High | Importance level |
| `Tags` | `List<string>` | Yes (auto) | Empty list | 0-10 tags, each 1-20 chars | Categorization labels |

#### Field Details

**Id (Guid)**
- **Purpose**: Unique identifier for task across sessions
- **Generation**: Automatically assigned on task creation using `Guid.NewGuid()`
- **Immutability**: Cannot be changed after creation
- **Display**: First 8 characters shown in lists (e.g., "a3f2c8b1")
- **Matching**: Partial ID matching supported (minimum 4 characters)
- **Business Rule**: FR-002

**Title (string)**
- **Purpose**: Primary task description
- **Validation**:
  - Cannot be null, empty, or whitespace-only
  - Minimum length: 1 character
  - Maximum length: 200 characters
  - Trimmed of leading/trailing whitespace on save
- **Display**: Truncated to 28 chars in table view with "..." suffix
- **Business Rules**: FR-004, FR-005
- **Error Messages**:
  - Empty: "Task title cannot be empty"
  - Too long: "Task title cannot exceed 200 characters"

**Description (string?)**
- **Purpose**: Optional extended details for complex tasks
- **Validation**:
  - Nullable (optional field)
  - Maximum length: 1000 characters when provided
  - Trimmed of leading/trailing whitespace on save
- **Display**: Shown only in detail view (`todo show`)
- **Business Rule**: FR-019
- **Error Message**: "Task description cannot exceed 1000 characters"

**CreatedAt (DateTime)**
- **Purpose**: Timestamp of task creation for chronological tracking
- **Generation**: Automatically set to `DateTime.UtcNow` on creation
- **Immutability**: Cannot be changed after creation
- **Timezone**: Always stored in UTC
- **Display**: Converted to local timezone for user display
- **Business Rule**: FR-003
- **Use Cases**:
  - Default sort order for `todo list`
  - Calculate task age in `todo show`
  - Audit trail for task history

**DueDate (DateOnly?)**
- **Purpose**: Optional target completion date for planning
- **Validation**:
  - Nullable (optional field)
  - Must be today or a future date when set
  - Date-only (no time component)
  - Format: YYYY-MM-DD
- **Display**:
  - Red with `[!]` if overdue (< today)
  - Yellow if due today (= today)
  - Normal if future (> today)
  - "-" if not set
- **Business Rules**: FR-015, FR-016
- **Error Messages**:
  - Past date: "Due date must be today or in the future"
  - Invalid format: "Due date must be in YYYY-MM-DD format"

**IsCompleted (bool)**
- **Purpose**: Track whether task is done
- **Default**: `false` on creation
- **State Transition**: One-way operation (false → true only, no "uncomplete")
- **Business Rules**: FR-010 (cannot re-complete), Constitution: Task Completion
- **Display**:
  - `[ ]` for incomplete tasks
  - `[✓]` for completed tasks
  - Green color + strikethrough for completed in lists

**CompletedAt (DateTime?)**
- **Purpose**: Record when task was marked complete
- **Generation**: Automatically set to `DateTime.UtcNow` when `IsCompleted` set to `true`
- **Validation**:
  - Nullable (null for incomplete tasks)
  - Cannot be manually set (system-managed)
  - Must be after `CreatedAt`
- **Timezone**: Always stored in UTC
- **Display**: Converted to local timezone, shown with duration calculation
- **Business Rules**: FR-008, FR-009
- **Use Cases**:
  - Show "Completed after 3 hours" message
  - Calculate completion time in detail view
  - Filter completed tasks by date range (future enhancement)

**Priority (Priority enum)**
- **Purpose**: Indicate task importance for sorting and filtering
- **Values**: `Low`, `Medium`, `High`
- **Default**: `Medium` if not specified
- **Display**:
  - High: `[H]` in red
  - Medium: `[M]` in yellow
  - Low: `[L]` in grey
- **Business Rules**: FR-017, FR-018
- **Sorting**: High → Medium → Low

**Tags (List<string>)**
- **Purpose**: Flexible categorization for filtering
- **Initialization**: Empty list on creation (not null)
- **Constraints**:
  - Maximum 10 tags per task
  - Each tag: 1-20 characters
  - Allowed characters: letters, numbers, hyphens, underscores
  - Case-insensitive (stored as lowercase)
  - No duplicates within a task
  - Trimmed of whitespace
- **Validation**:
  - Reject invalid characters: `[^a-zA-Z0-9_-]`
  - Normalize to lowercase on save
  - Remove duplicates (case-insensitive comparison)
- **Display**:
  - Comma-separated in table view
  - Truncated to 13 chars with "..." if too long
  - Full list in detail view
- **Business Rules**: FR-020, FR-021, FR-022, FR-023, FR-026
- **Error Messages**:
  - Too many: "Maximum 10 tags allowed per task"
  - Too long: "Tag '{tag}' exceeds 20 character limit"
  - Invalid chars: "Tag '{tag}' contains invalid characters (use only letters, numbers, hyphens, underscores)"

---

## Enumerations

### Priority

Represents task importance levels for sorting and visual highlighting.

```csharp
public enum Priority
{
    Low = 0,
    Medium = 1,
    High = 2
}
```

**Usage**:
- Sorting: Higher numeric value = higher priority
- Filtering: Match exact priority level
- Display: Color-coded indicators

**JSON Serialization**: Stored as lowercase string ("low", "medium", "high") for readability

---

## Validation Rules

### Composite Validation Rules

Beyond individual field constraints, the following composite rules apply:

1. **Completion Consistency** (FR-010, FR-024)
   - If `IsCompleted = true`, then `CompletedAt` must not be null
   - If `IsCompleted = false`, then `CompletedAt` must be null
   - `CompletedAt` must be >= `CreatedAt` if set

2. **Update Constraints** (FR-024)
   - Cannot update `Id` (immutable)
   - Cannot update `CreatedAt` (immutable)
   - Cannot update `CompletedAt` directly (system-managed via completion)
   - Can update all other fields while task is incomplete or complete

3. **Tag Operations** (FR-026)
   - When adding tags: prevent duplicates (case-insensitive)
   - When removing tags: silently ignore non-existent tags
   - Total tag count must never exceed 10

4. **Date Validation**
   - `DueDate` if provided must be >= Today (no time component comparison)
   - `CompletedAt` if set must be >= `CreatedAt`

---

## State Transitions

### Task Lifecycle

```
┌─────────────────┐
│   Task Created  │
│  IsCompleted=   │
│      false      │
└────────┬────────┘
         │
         │ User executes: todo complete <id>
         │
         ▼
┌─────────────────┐
│ Task Completed  │
│  IsCompleted=   │
│      true       │
│  CompletedAt=   │
│    UTC Now      │
└─────────────────┘
         │
         │ One-way transition
         │ (no "uncomplete")
         │
         ▼
    (Permanent)
```

**Key Rules**:
- Incomplete → Complete: Allowed via `todo complete <id>`
- Complete → Incomplete: **Not allowed** (business rule from constitution)
- Deleted: Permanent removal from storage (no soft delete, no recovery)

### Field Mutability Matrix

| Field | On Creation | While Incomplete | While Complete | Notes |
|-------|-------------|------------------|----------------|-------|
| `Id` | Auto-generated | Immutable | Immutable | Never changes |
| `Title` | User input | Mutable | Mutable | Can always update |
| `Description` | Optional input | Mutable | Mutable | Can always update |
| `CreatedAt` | Auto-set UTC | Immutable | Immutable | Never changes |
| `DueDate` | Optional input | Mutable | Mutable | Can always update |
| `IsCompleted` | Auto = false | Mutable (→true) | Immutable | One-way transition |
| `CompletedAt` | Auto = null | Auto-set on complete | Immutable | System-managed |
| `Priority` | User input or default | Mutable | Mutable | Can always update |
| `Tags` | Optional input | Mutable | Mutable | Can always update |

---

## JSON Serialization Format

### Schema

Tasks are stored as a JSON array in `tasks.json`. Each task is serialized with camelCase property names for JSON conventions.

#### Single Task Example

```json
{
  "id": "a3f2c8b1-7d4e-4a9c-b6f1-2e8d9c5a7b3f",
  "title": "Buy groceries",
  "description": "Need milk, eggs, bread, and coffee",
  "createdAt": "2026-01-12T10:30:00Z",
  "dueDate": "2026-01-15",
  "isCompleted": false,
  "completedAt": null,
  "priority": "medium",
  "tags": ["personal", "shopping"]
}
```

#### Multiple Tasks (File Format)

```json
[
  {
    "id": "a3f2c8b1-7d4e-4a9c-b6f1-2e8d9c5a7b3f",
    "title": "Buy groceries",
    "description": "Need milk, eggs, bread, and coffee",
    "createdAt": "2026-01-12T10:30:00Z",
    "dueDate": "2026-01-15",
    "isCompleted": false,
    "completedAt": null,
    "priority": "medium",
    "tags": ["personal", "shopping"]
  },
  {
    "id": "b4e3d9c2-8e5f-5b0d-c7g2-3f9e0d6b8c4g",
    "title": "Review pull request #42",
    "description": null,
    "createdAt": "2026-01-12T11:00:00Z",
    "dueDate": "2026-01-13",
    "isCompleted": true,
    "completedAt": "2026-01-12T14:30:00Z",
    "priority": "high",
    "tags": ["work", "code-review"]
  },
  {
    "id": "c5f4e0d3-9f6g-6c1e-d8h3-4g0f1e7c9d5h",
    "title": "Update documentation",
    "description": null,
    "createdAt": "2026-01-12T12:00:00Z",
    "dueDate": null,
    "isCompleted": false,
    "completedAt": null,
    "priority": "low",
    "tags": []
  }
]
```

### Serialization Rules

1. **Property Naming**: camelCase (JSON convention)
   - C# `IsCompleted` → JSON `"isCompleted"`
   - C# `CreatedAt` → JSON `"createdAt"`

2. **Null Handling**: Omit null values for optional fields
   - `"description": null` can be omitted
   - `"dueDate": null` can be omitted
   - `"completedAt": null` can be omitted

3. **Date Formats**:
   - `DateTime` (CreatedAt, CompletedAt): ISO 8601 with UTC indicator (`"2026-01-12T10:30:00Z"`)
   - `DateOnly` (DueDate): ISO 8601 date-only (`"2026-01-15"`)

4. **Enum Serialization**: Lowercase strings
   - `Priority.High` → `"high"`
   - `Priority.Medium` → `"medium"`
   - `Priority.Low` → `"low"`

5. **Collections**: Always serialize as arrays (never null)
   - Empty tags: `"tags": []`
   - With tags: `"tags": ["work", "urgent"]`

6. **Formatting**: Pretty-printed with 2-space indentation for human readability (constitution requirement)

### File Structure

```
# Windows
C:\Users\<username>\AppData\Roaming\TodoCli\tasks.json
C:\Users\<username>\AppData\Roaming\TodoCli\tasks.json.bak

# macOS / Linux
~/.local/share/TodoCli/tasks.json
~/.local/share/TodoCli/tasks.json.bak
```

**Backup Strategy**:
- `tasks.json.bak` created before each write
- Contains previous state for recovery
- Only one backup retained (latest)

---

## Edge Cases and Handling

### Partial ID Matching

**Scenario**: User provides partial ID that matches multiple tasks

**Rule**: Minimum 4 characters required for partial match

**Examples**:
- `a3f2c8b1...` + user input `a3f2` → Single match ✅
- `a3f2c8b1...` + `a3f9d7e2...` + user input `a3f` → Ambiguous match ❌

**Handling**:
1. If partial ID matches zero tasks: Error "Task not found with ID: {input}"
2. If partial ID matches one task: Proceed with operation
3. If partial ID matches multiple tasks: Display numbered list of matches and prompt for selection

**Business Rule**: FR-077

### Tag Normalization

**Scenario**: User provides tags with mixed case or spaces

**Input**: `--tags "Work, URGENT, Code-Review "`

**Processing**:
1. Split by comma: `["Work", " URGENT", " Code-Review "]`
2. Trim whitespace: `["Work", "URGENT", "Code-Review"]`
3. Convert to lowercase: `["work", "urgent", "code-review"]`
4. Validate characters: All pass (letters and hyphens only)
5. Check for duplicates: None
6. Store: `["work", "urgent", "code-review"]`

**Business Rules**: FR-022, FR-023, FR-026

### Concurrent Access

**Scenario**: Two terminal sessions modify tasks simultaneously

**Behavior**: Last write wins (no conflict detection)

**Justification**: Single-user tool, out of scope for MVP per constitution

**Risk**: Low (single user unlikely to have multiple sessions)

**Future Enhancement**: File locking or modification timestamp check

### File Corruption Recovery

**Scenario**: `tasks.json` contains invalid JSON

**Detection**: JSON parsing fails on load

**Recovery Steps**:
1. Display warning: "⚠ Corruption detected. Restoring from backup..."
2. Copy `tasks.json.bak` to `tasks.json`
3. Retry load from restored file
4. If backup also corrupt: Error with manual resolution instructions

**Business Rule**: FR-057

---

## Data Integrity Constraints

### Atomic Operations

All file writes use atomic write pattern (research.md finding #3):
1. Write to `tasks.json.tmp`
2. Validate JSON structure
3. Rename `tasks.json.tmp` → `tasks.json` (atomic operation)

**Guarantee**: File is never partially written (all-or-nothing)

**Business Rule**: FR-054

### Validation Checkpoints

1. **Pre-Save Validation**: Before serializing to JSON
   - All field constraints enforced
   - Composite rules validated
   - Invalid tasks rejected with clear error message

2. **Post-Load Validation**: After deserializing from JSON
   - JSON structure validation (schema check)
   - Graceful handling of unknown properties (ignored)
   - Migration support for future schema changes

---

## Performance Considerations

### In-Memory Operations

- **Load**: Deserialize entire task list into `List<TodoTask>` on startup
- **Query**: Use LINQ for filtering, sorting (no database indexing)
- **Save**: Serialize entire list on every modification

**Scalability**: Acceptable for ≤10,000 tasks per performance analysis

### Filter Optimization

- **Status filter**: Single boolean check (O(1) per task)
- **Priority filter**: Enum comparison (O(1) per task)
- **Tag filter**: HashSet intersection (O(n) per task where n = tag count)
- **Date filter**: DateTime comparison (O(1) per task)

**Overall**: O(n) complexity where n = total task count (acceptable for ≤10,000)

---

## Summary

The TodoTask entity is intentionally simple with minimal fields and clear business rules. All validation is enforced at the model level, ensuring data integrity regardless of entry point (CLI commands or direct JSON editing).

**Key Principles**:
- **Immutability**: ID and CreatedAt never change
- **One-way completion**: Tasks can be marked complete but not "uncompleted"
- **Flexibility**: Title is only required field, all metadata optional
- **Human-readable**: JSON format allows manual editing with text editor
- **Atomic integrity**: File operations guarantee no partial writes

**Next Steps**:
1. ✅ Data model defined and documented
2. ⏭️ Generate CLI command contracts (`contracts/cli-commands.md`)
3. ⏭️ Generate quick start guide (`quickstart.md`)
4. ⏭️ Update agent context with technology stack
