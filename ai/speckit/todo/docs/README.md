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

- `add` - Add new task
- `list` - Display tasks with filtering and sorting
- `complete` - Mark task as complete
- `update` - Modify task properties
- `show` - Display full task details
- `delete` - Remove task permanently

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
