# TODO CLI Application

A cross-platform command-line task manager built with .NET 8 and C# 12.

## Features

- Quick task capture with minimal input
- Rich terminal output with color-coded priorities
- Filtering and sorting capabilities
- Task completion tracking with timestamps
- Cross-platform support (Windows, macOS, Linux)

## Installation

### Prerequisites

- .NET 8 SDK or later
- Terminal with ANSI color support (Windows Terminal, iTerm2, GNOME Terminal)

### Build from Source

```bash
# Clone repository and navigate to project
cd ai/speckit/todo

# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run application
dotnet run --project src/TodoCli/TodoCli.csproj -- <command>
```

### Publish for Deployment

```bash
# Publish as self-contained executable
dotnet publish src/TodoCli/TodoCli.csproj -c Release -r win-x64 --self-contained
dotnet publish src/TodoCli/TodoCli.csproj -c Release -r osx-x64 --self-contained
dotnet publish src/TodoCli/TodoCli.csproj -c Release -r linux-x64 --self-contained
```

## Quick Start

```bash
# Add a task
todo add "Buy groceries"

# Add task with metadata
todo add "Deploy v2.0" --due 2026-01-20 --priority high --tags work,deployment

# List all tasks
todo list

# List incomplete high-priority tasks
todo list --status incomplete --priority high

# Complete a task
todo complete a3f2

# Update a task
todo update a3f2 --priority medium --due 2026-01-25

# Show task details
todo show a3f2

# Delete a task
todo delete a3f2
```

## Commands

### add - Add New Task

```bash
# Minimal task (title only)
todo add "Buy groceries"

# With all metadata
todo add "Deploy v2.0" \
  --description "Deploy to production environment" \
  --due 2026-01-20 \
  --priority High \
  --tags work,deployment,urgent

# Options:
#   -d, --description <text>     Task description (0-1000 chars)
#   --due <yyyy-mm-dd>           Due date
#   -p, --priority <High|Medium|Low>  Priority level (default: Medium)
#   --tags <tag1,tag2>           Comma-separated tags
```

### list - Display Tasks

```bash
# List all incomplete tasks (default)
todo list

# Filter by status
todo list --status all
todo list --status complete
todo list --status incomplete

# Filter by priority
todo list --priority High

# Filter by tags (OR logic)
todo list --tags work,urgent

# Filter by due date
todo list --due overdue
todo list --due today
todo list --due week
todo list --due month

# Sort results
todo list --sort created   # Default
todo list --sort due
todo list --sort priority

# Combine filters
todo list --status incomplete --priority High --due week --sort due

# Options:
#   -s, --status <all|complete|incomplete>
#   -p, --priority <High|Medium|Low>
#   -t, --tags <tag1,tag2>
#   -d, --due <overdue|today|week|month>
#   --sort <created|due|priority>
```

### complete - Mark Task Complete

```bash
# Complete with partial ID (minimum 4 characters)
todo complete a3f2

# Complete with full ID
todo complete a3f2b8c1-4d5e-6f7a-8b9c-0d1e2f3a4b5c

# Shows completion duration
# Example output: ✓ Task completed: Deploy v2.0 (took 2 days)
```

### update - Modify Task

```bash
# Update title
todo update a3f2 --title "New title"

# Update multiple fields
todo update a3f2 --priority High --due 2026-01-25

# Add tags
todo update a3f2 --add-tags urgent,critical

# Remove tags
todo update a3f2 --remove-tags old-tag

# Clear due date
todo update a3f2 --due none

# Options:
#   -t, --title <text>           New title
#   -d, --description <text>     New description
#   -p, --priority <High|Medium|Low>  New priority
#   --due <yyyy-mm-dd|none>      New due date or 'none' to clear
#   --add-tags <tag1,tag2>       Tags to add
#   --remove-tags <tag1,tag2>    Tags to remove
```

### show - Display Full Details

```bash
# Show all task fields
todo show a3f2

# Displays:
#   - Title, description, status
#   - Priority (color-coded)
#   - Due date with countdown
#   - Tags
#   - Created timestamp with age
#   - Completed timestamp (if applicable) with duration
```

### delete - Remove Task

```bash
# Delete with confirmation
todo delete a3f2

# Delete without confirmation
todo delete a3f2 --force

# Options:
#   -f, --force    Skip confirmation prompt
```

## Global Options

```bash
# Show version
todo --version

# Show help
todo --help
todo <command> --help
```

Use `todo <command> --help` for detailed command usage.

## Data Storage

Tasks are stored in JSON format at:
- **Windows**: `C:\Users\<username>\AppData\Roaming\TodoCli\tasks.json`
- **macOS/Linux**: `~/.local/share/TodoCli/tasks.json`

Automatic backups are created before each write operation.

## Development

See [CONTRIBUTING.md](CONTRIBUTING.md) for development setup and testing information.

## License

This project is part of the engineering-toolbox repository.
