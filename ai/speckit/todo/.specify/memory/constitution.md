# Project Constitution: TODO CLI Application

## Project Overview

### Application Type
A simple command-line interface (CLI) application for managing personal TODO tasks, built with .NET 8 and C# 12.

### Purpose
- Learn and demonstrate CLI application development in C#
- Practice BDD (Behavior-Driven Development) with automated testing
- Create a maintainable, testable, and extensible task management tool

### Target Users
- Individual developers and power users comfortable with command-line interfaces
- Users who prefer keyboard-driven task management over GUI applications

---

## Technical Standards

### Language & Framework
- **Language**: C# 12
- **Framework**: .NET 8 SDK
- **Project Type**: Console Application (.NET CLI template)
- **Target**: Cross-platform (Windows, macOS, Linux)

### Architecture Principles
1. **Clean Architecture**: Separate concerns into distinct layers
   - Domain layer: Core entities and business logic
   - Application layer: Use cases and commands
   - Infrastructure layer: Data persistence and external dependencies
   - Presentation layer: CLI interface and command parsing

2. **Command Pattern**: Each TODO operation is a discrete command
   - `add` - Add a new task
   - `list` - Display all tasks
   - `complete` - Mark task as complete
   - `delete` - Remove a task
   - `update` - Modify task details

3. **Dependency Injection**: Use built-in .NET DI container
   - Register services in `Program.cs`
   - Constructor injection throughout
   - Interface-based abstractions

### Project Structure
```
TodoCli/
├── src/
│   ├── TodoCli/                    (Main CLI project)
│   │   ├── Commands/               (Command handlers)
│   │   ├── Services/               (Application services)
│   │   ├── Infrastructure/         (Data access, file I/O)
│   │   ├── Models/                 (Domain entities)
│   │   └── Program.cs              (Entry point, DI setup)
│   └── TodoCli.Specs/              (BDD specifications)
│       ├── Features/               (Gherkin .feature files)
│       ├── StepDefinitions/        (Step implementation)
│       ├── Drivers/                (Test automation helpers)
│       └── Hooks/                  (Test setup/teardown)
└── tests/
    └── TodoCli.UnitTests/          (Unit tests for complex logic)
```

### CLI Framework
- **Library**: System.CommandLine (Microsoft's official CLI library)
- **Features**:
  - Strongly-typed command options
  - Built-in help generation
  - Tab completion support
  - Input validation

### Data Persistence
- **Storage**: JSON file-based persistence (simple and human-readable)
- **Location**: User's app data directory
  - Windows: `%APPDATA%\TodoCli\tasks.json`
  - macOS/Linux: `~/.local/share/TodoCli/tasks.json`
- **Format**: JSON array of task objects
- **Backup**: Create backup before writes (`tasks.json.bak`)

---

## BDD Testing Standards

### BDD Framework
- **Primary Framework**: SpecFlow 3.9+ (Gherkin-based BDD for .NET)
- **Test Runner**: xUnit (integrates well with SpecFlow)
- **Assertion Library**: FluentAssertions (readable, expressive assertions)

### Feature File Organization
```gherkin
# Example: Features/AddTask.feature
Feature: Add Task
  As a user
  I want to add tasks to my TODO list
  So that I can track things I need to do

  Scenario: Add a simple task
    Given the TODO list is empty
    When I add a task "Buy groceries"
    Then the task list should contain 1 task
    And the task "Buy groceries" should be marked as incomplete

  Scenario: Add task with due date
    Given the TODO list is empty
    When I add a task "Submit report" with due date "2025-01-20"
    Then the task "Submit report" should have due date "2025-01-20"
```

### BDD Requirements
1. **Feature Coverage**: Every user-facing command must have feature files
2. **Scenario Format**: Use Given-When-Then structure consistently
3. **Step Reusability**: Create reusable step definitions
4. **Test Isolation**: Each scenario runs with clean state
5. **Living Documentation**: Feature files serve as project documentation

### Test Categories
- **Acceptance Tests (SpecFlow)**: End-to-end CLI command execution
- **Unit Tests (xUnit)**: Complex business logic, edge cases
- **Integration Tests**: File I/O, data persistence validation

### Coverage Requirements
- **Minimum Coverage**: 80% code coverage across all projects
- **Critical Paths**: 100% coverage for data persistence and command parsing
- **BDD Scenarios**: All acceptance criteria from specs must have scenarios

---

## Domain Model

### Core Entity: TodoTask
```csharp
public class TodoTask
{
    public Guid Id { get; init; }           // Unique identifier
    public string Title { get; set; }       // Task description (required, max 200 chars)
    public string? Description { get; set; } // Optional details (max 1000 chars)
    public DateTime CreatedAt { get; init; } // Creation timestamp
    public DateTime? DueDate { get; set; }   // Optional due date
    public bool IsCompleted { get; set; }    // Completion status
    public DateTime? CompletedAt { get; set; } // Completion timestamp
    public Priority Priority { get; set; }   // Low, Medium, High
    public List<string> Tags { get; init; }  // Categorization tags
}

public enum Priority
{
    Low,
    Medium,
    High
}
```

### Business Rules
1. **Task Creation**:
   - Title is required and cannot be empty
   - Title maximum length: 200 characters
   - Description maximum length: 1000 characters
   - CreatedAt automatically set to current UTC time
   - Id automatically generated as GUID

2. **Task Completion**:
   - Can only complete incomplete tasks
   - CompletedAt automatically set when marked complete
   - Cannot "uncomplete" a task (set IsCompleted to false)

3. **Task Deletion**:
   - Deleted tasks are permanently removed (no soft delete)
   - Confirmation required for deletion

4. **Due Dates**:
   - Must be in the future when set
   - Optional field (null allowed)
   - Date-only (no time component)

5. **Tags**:
   - Case-insensitive (stored lowercase)
   - Alphanumeric only (hyphens and underscores allowed)
   - Maximum 10 tags per task
   - Each tag max 20 characters

---

## Command Specifications

### Command: `add`
```bash
todo add "Buy groceries" --due 2025-01-20 --priority high --tags shopping,personal
```
**Options**:
- `<title>`: Task title (required, positional)
- `--description, -d`: Optional description
- `--due`: Optional due date (YYYY-MM-DD format)
- `--priority, -p`: Priority level (low|medium|high), default: medium
- `--tags, -t`: Comma-separated tags

### Command: `list`
```bash
todo list --status incomplete --priority high --tags work
```
**Options**:
- `--status, -s`: Filter by status (all|complete|incomplete), default: all
- `--priority, -p`: Filter by priority
- `--tags, -t`: Filter by tags (match any)
- `--due`: Show only tasks due before specified date
- `--sort`: Sort by (created|due|priority), default: created

### Command: `complete`
```bash
todo complete <id>
```
**Arguments**:
- `<id>`: Task ID (required, accepts partial GUID match)

### Command: `delete`
```bash
todo delete <id> --force
```
**Arguments**:
- `<id>`: Task ID (required)
**Options**:
- `--force, -f`: Skip confirmation prompt

### Command: `update`
```bash
todo update <id> --title "New title" --due 2025-02-01
```
**Arguments**:
- `<id>`: Task ID (required)
**Options**:
- `--title`: Update title
- `--description, -d`: Update description
- `--due`: Update due date
- `--priority, -p`: Update priority
- `--add-tags`: Add tags (comma-separated)
- `--remove-tags`: Remove tags (comma-separated)

### Command: `show`
```bash
todo show <id>
```
**Arguments**:
- `<id>`: Task ID (required)
**Output**: Display full task details

---

## Code Quality Standards

### C# Coding Conventions
1. **Naming**:
   - PascalCase for classes, methods, properties, constants
   - camelCase for local variables and parameters
   - Prefix interfaces with `I` (e.g., `ITaskRepository`)
   - Async methods end with `Async` suffix

2. **Null Safety**:
   - Enable nullable reference types (`<Nullable>enable</Nullable>`)
   - Use null-forgiving operator (`!`) sparingly
   - Prefer pattern matching for null checks

3. **Modern C# Features**:
   - Use records for immutable DTOs
   - Use file-scoped namespaces
   - Use top-level statements in Program.cs
   - Use pattern matching and switch expressions
   - Use init-only properties where appropriate

4. **Error Handling**:
   - Use specific exception types (ArgumentException, InvalidOperationException)
   - Provide meaningful error messages
   - Catch exceptions at command level
   - Display user-friendly error messages in CLI

### Dependency Rules
1. **No circular dependencies** between layers
2. **Domain layer** has zero external dependencies
3. **Infrastructure depends on** Domain and Application
4. **Presentation depends on** all layers (composition root)

### Performance Considerations
- Task list limited to 10,000 tasks (reasonable for CLI use)
- List command output limited to 100 tasks (use pagination if needed)
- File I/O should be async
- JSON serialization with source generators (System.Text.Json)

---

## Testing Principles

### BDD Scenario Guidelines
1. **Focus on Behavior**: Scenarios describe user-visible behavior, not implementation
2. **Declarative Style**: Focus on "what" not "how"
   - ✅ Good: `When I add a task "Buy milk"`
   - ❌ Bad: `When I call the AddTask method with parameter "Buy milk"`
3. **Business Language**: Use domain terminology, not technical jargon
4. **Independent Scenarios**: Each scenario should be runnable in isolation
5. **Single Responsibility**: Each scenario tests one specific behavior

### Test Data Strategy
- **Test Files**: Use temporary directory for test data (`Path.GetTempPath()`)
- **Cleanup**: Always delete test files in teardown hooks
- **Fixtures**: Use SpecFlow hooks for setup/teardown
- **Time**: Use `ISystemClock` abstraction for testable time-based logic

### Unit Test Guidelines
- **AAA Pattern**: Arrange, Act, Assert structure
- **One Assertion**: Prefer single logical assertion per test (exceptions for complex validation)
- **Test Naming**: `MethodName_Scenario_ExpectedBehavior` format
- **FluentAssertions**: Use `.Should()` syntax for readable assertions

---

## User Experience Principles

### CLI Output
1. **Clarity**: Clear, concise messages
2. **Colors**: Use ANSI colors for better readability
   - Red: Errors
   - Yellow: Warnings
   - Green: Success messages
   - Cyan: Information
3. **Tables**: Use formatted tables for list output (Spectre.Console library)
4. **Progress**: Show progress indicators for long operations
5. **Help**: Comprehensive `--help` for all commands

### Error Handling
- **Graceful Failures**: Never crash with unhandled exceptions
- **Clear Messages**: Explain what went wrong and how to fix it
- **Exit Codes**: Use standard exit codes (0 = success, 1 = error)

---

## Development Workflow

### Version Control
- **Branching**: Feature branches from `main`
- **Commits**: Conventional Commits format (feat:, fix:, test:, docs:)
- **Pull Requests**: Required for all changes, include BDD scenarios

### CI/CD Requirements
1. **Build**: All projects must compile without warnings
2. **Tests**: All SpecFlow and xUnit tests must pass
3. **Coverage**: Minimum 80% code coverage
4. **Static Analysis**: Pass Roslyn analyzers with no warnings

### Definition of Done
A feature is complete when:
- ✅ BDD scenarios written and passing
- ✅ Unit tests for complex logic
- ✅ Code coverage meets threshold
- ✅ Help text updated
- ✅ README documentation updated
- ✅ Manual smoke testing completed

---

## Non-Functional Requirements

### Performance
- Startup time: < 100ms
- Add task: < 50ms
- List 100 tasks: < 200ms
- File operations: < 100ms (async)

### Reliability
- Data integrity: Never corrupt tasks.json
- Atomic writes: Use temp file + rename pattern
- Validation: All user input validated before processing

### Usability
- Commands intuitive and memorable
- Error messages actionable
- Help text comprehensive
- Tab completion for commands

### Maintainability
- Clear separation of concerns
- Interface-based abstractions
- Comprehensive test coverage
- Living documentation via BDD scenarios

---

## Out of Scope (Future Enhancements)

### Not Included in Initial Version
- ❌ Multi-user support / authentication
- ❌ Cloud synchronization
- ❌ Recurring tasks
- ❌ Task dependencies / subtasks
- ❌ Reminders / notifications
- ❌ GUI or web interface
- ❌ Import/export to other formats
- ❌ Task history / audit log
- ❌ Attachments or file links
- ❌ Natural language processing for input

---

## Success Criteria

### Functional Success
- Users can perform all CRUD operations on tasks
- Data persists correctly across sessions
- Commands work identically on Windows, macOS, and Linux
- Error handling prevents data corruption

### Technical Success
- 80%+ code coverage
- All BDD scenarios passing
- Zero compiler warnings
- Passes static analysis
- Startup time under 100ms

### User Success
- Intuitive command structure (minimal learning curve)
- Fast and responsive CLI experience
- Helpful error messages guide users to solutions
- Comprehensive help documentation

---

## Dependencies & Libraries

### Required Packages
```xml
<!-- TodoCli.csproj -->
<PackageReference Include="System.CommandLine" Version="2.0.0-beta4.22272.1" />
<PackageReference Include="Spectre.Console" Version="0.48.0" />
<PackageReference Include="System.Text.Json" Version="8.0.0" />

<!-- TodoCli.Specs.csproj -->
<PackageReference Include="SpecFlow" Version="3.9.74" />
<PackageReference Include="SpecFlow.xUnit" Version="3.9.74" />
<PackageReference Include="xunit" Version="2.6.2" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />

<!-- TodoCli.UnitTests.csproj -->
<PackageReference Include="xunit" Version="2.6.2" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Moq" Version="4.20.70" />
```

### Justification
- **System.CommandLine**: Official Microsoft library for building CLI apps
- **Spectre.Console**: Rich CLI output formatting and tables
- **System.Text.Json**: Built-in, high-performance JSON serialization
- **SpecFlow**: Industry-standard BDD framework for .NET
- **FluentAssertions**: Makes test assertions more readable
- **Moq**: Mocking framework for unit tests

---

## Notes

### Design Philosophy
This constitution prioritizes:
1. **Simplicity**: Single-user, local storage, straightforward commands
2. **Testability**: BDD-first approach with comprehensive scenarios
3. **Maintainability**: Clean architecture enables future enhancements
4. **User Experience**: Fast, intuitive CLI with helpful feedback

### Learning Objectives
This project demonstrates:
- CLI application development in C#
- BDD with SpecFlow
- Clean architecture principles
- System.CommandLine usage
- File-based data persistence
- Cross-platform .NET development