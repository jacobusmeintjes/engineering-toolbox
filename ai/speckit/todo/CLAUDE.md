# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

**SpecKit** is a feature specification and implementation workflow system built as a collection of Claude Code slash commands. It guides development from initial feature description through specification, planning, task breakdown, and implementation.

The workflow is:
1. `/speckit.specify` - Create feature specification from natural language
2. `/speckit.clarify` - Resolve ambiguities with targeted questions
3. `/speckit.plan` - Generate technical implementation plan
4. `/speckit.tasks` - Break down into actionable, dependency-ordered tasks
5. `/speckit.analyze` - Validate consistency across artifacts
6. `/speckit.implement` - Execute the implementation plan

## Project Structure

```
.specify/
├── memory/
│   └── constitution.md          # Project principles and constraints (template)
├── scripts/powershell/
│   ├── common.ps1              # Shared PowerShell functions
│   ├── create-new-feature.ps1  # Initialize feature branch & directory
│   ├── setup-plan.ps1          # Setup planning phase
│   ├── check-prerequisites.ps1 # Validate workflow state
│   └── update-agent-context.ps1 # Update AI agent context files
└── templates/
    ├── spec-template.md        # Feature specification template
    ├── plan-template.md        # Implementation plan template
    ├── tasks-template.md       # Task breakdown template
    ├── checklist-template.md   # Quality checklist template
    └── agent-file-template.md  # Agent context template

.claude/
└── commands/                    # Slash command definitions
    ├── speckit.specify.md
    ├── speckit.clarify.md
    ├── speckit.plan.md
    ├── speckit.tasks.md
    ├── speckit.analyze.md
    ├── speckit.implement.md
    ├── speckit.checklist.md
    ├── speckit.constitution.md
    └── speckit.taskstoissues.md

specs/                           # Generated per-feature directories
└── {NNN-feature-name}/         # Feature number + short name
    ├── spec.md                 # Feature specification
    ├── plan.md                 # Technical plan
    ├── tasks.md                # Task breakdown
    ├── research.md             # Research findings
    ├── data-model.md           # Entity definitions
    ├── quickstart.md           # Test scenarios
    ├── contracts/              # API contracts (OpenAPI/GraphQL)
    └── checklists/             # Quality validation checklists
```

## Feature Branch Workflow

SpecKit uses a numbered feature branch pattern:

- **Branch naming**: `NNN-short-name` (e.g., `001-user-auth`, `002-payment-flow`)
- **Feature directories**: `specs/NNN-short-name/` (matches branch name)
- Branches are auto-created by `/speckit.specify` based on feature description
- The system checks remote branches, local branches, and specs directories to find the next available number

## PowerShell Scripts

All automation scripts are in `.specify/scripts/powershell/`. They support both Git and non-Git repositories:

### Common Functions (`common.ps1`)
- `Get-RepoRoot` - Find repository root (git or script location fallback)
- `Get-CurrentBranch` - Determine active feature (env var, git, or latest specs dir)
- `Test-HasGit` - Check if repository has Git
- `Test-FeatureBranch` - Validate feature branch naming
- `Get-FeaturePathsEnv` - Build all feature paths as object

### Script Invocation Patterns

```powershell
# Standard JSON output mode for parsing
.specify/scripts/powershell/check-prerequisites.ps1 -Json

# With additional flags
.specify/scripts/powershell/check-prerequisites.ps1 -Json -RequireTasks -IncludeTasks

# Create new feature
.specify/scripts/powershell/create-new-feature.ps1 -Json -Number 5 -ShortName "user-auth" "Add user authentication"
```

**IMPORTANT**: Scripts return JSON output to stdout. Always parse the JSON to extract:
- `FEATURE_DIR` - Absolute path to feature directory
- `FEATURE_SPEC` - Absolute path to spec.md
- `IMPL_PLAN` - Absolute path to plan.md
- `TASKS` - Absolute path to tasks.md
- `BRANCH_NAME` - Current feature branch
- `AVAILABLE_DOCS` - List of existing documents

## Key Workflow Rules

### Specification Phase (`/speckit.specify`)

1. **Auto-generates short names** from feature descriptions (2-4 words)
2. **Checks for existing branches** across all sources before creating new feature
3. **Limits clarifications** to max 3 `[NEEDS CLARIFICATION]` markers
4. **Makes informed guesses** rather than asking for every detail
5. **Runs quality validation** after initial spec generation
6. **User stories MUST be prioritized** (P1, P2, P3) and independently testable
7. **Success criteria MUST be technology-agnostic** and measurable

### Planning Phase (`/speckit.plan`)

1. Reads spec.md and constitution.md for context
2. Phase 0: Research unknowns and resolve all `NEEDS CLARIFICATION` markers
3. Phase 1: Generate data-model.md, contracts/, quickstart.md
4. Updates agent context files after design artifacts are created
5. Validates against constitution principles

### Task Generation (`/speckit.tasks`)

**Critical formatting requirement**: All tasks MUST follow checklist format:

```markdown
- [ ] [TaskID] [P?] [Story?] Description with file path
```

Where:
- `- [ ]` - Markdown checkbox (required)
- `[T001]` - Sequential task ID (required)
- `[P]` - Parallel execution marker (optional, only if truly parallelizable)
- `[US1]` - User story label (required for story phase tasks)
- Description must include exact file path

**Task organization by user story**:
- Each user story from spec.md gets its own phase
- Tasks are grouped by story to enable independent implementation
- Each story phase should be independently testable
- Dependencies between stories are explicitly noted

**Phase structure**:
- Phase 1: Setup (project initialization)
- Phase 2: Foundational (blocking prerequisites)
- Phase 3+: User Stories in priority order (P1, P2, P3...)
- Final Phase: Polish & cross-cutting concerns

**Tests are OPTIONAL**: Only generate test tasks if explicitly requested in spec or user asks for TDD.

### Implementation Phase (`/speckit.implement`)

1. **Checks checklists first** - validates all checklist items before proceeding
2. **Creates/verifies ignore files** based on detected technology stack
3. Executes tasks phase-by-phase in dependency order
4. **Marks completed tasks** in tasks.md with `[X]`
5. Respects `[P]` markers for parallel execution
6. **Follows TDD** if tests are included in task breakdown

## Constitution System

The `.specify/memory/constitution.md` is a **template** for defining project-specific principles and constraints. Key characteristics:

- **Non-negotiable during analysis** - violations are CRITICAL issues
- Used by `/speckit.plan` to validate architecture decisions
- Used by `/speckit.analyze` to check consistency
- Can define: architectural patterns, testing requirements, quality gates, technology constraints

## Technology Detection & Ignore Files

The system detects technology stack from `plan.md` and automatically creates appropriate ignore files:

**Languages detected**:
- Node.js/JS/TS → `.gitignore`, `.eslintignore`, `.prettierignore`, `.npmignore`
- Python → `__pycache__/`, `.venv/`, `*.pyc`
- Java → `target/`, `*.class`, `.gradle/`
- C#/.NET → `bin/`, `obj/`, `packages/`
- Go, Ruby, PHP, Rust, Kotlin, C++, C, Swift, R

**Tools detected**:
- Docker (Dockerfile*) → `.dockerignore`
- Terraform (*.tf) → `.terraformignore`
- Kubernetes (helm charts) → `.helmignore`

## Clarification System (`/speckit.clarify`)

- **Maximum 5 questions** per clarification session
- Questions must be answerable via multiple choice or ≤5 words
- **Provides recommendations** for each question based on best practices
- Users can accept recommendations by saying "yes", "recommended", or "suggested"
- **Incrementally updates spec.md** after each answer
- Adds `## Clarifications` section with session date and Q&A pairs
- Integrates answers into appropriate spec sections immediately

## Analysis System (`/speckit.analyze`)

**Strictly read-only** - produces analysis reports without modifying files.

Checks for:
- **Duplication**: Near-duplicate requirements
- **Ambiguity**: Vague adjectives lacking metrics, unresolved placeholders
- **Underspecification**: Requirements missing measurable outcomes
- **Constitution violations**: Conflicts with MUST principles (always CRITICAL)
- **Coverage gaps**: Requirements with no tasks, tasks with no requirements
- **Inconsistency**: Terminology drift, conflicting requirements, ordering contradictions

**Severity levels**:
- CRITICAL: Constitution violations, missing core artifacts, zero-coverage requirements
- HIGH: Duplicate/conflicting requirements, ambiguous security/performance
- MEDIUM: Terminology drift, missing non-functional coverage
- LOW: Style improvements, minor redundancy

## Handoffs Between Commands

Slash commands declare handoffs to guide workflow progression:

```yaml
handoffs:
  - label: Build Technical Plan
    agent: speckit.plan
    prompt: Create a plan for the spec. I am building with...
    send: true  # Auto-transition vs. suggestion
```

The `send: true` flag indicates automatic progression vs. manual trigger.

## Best Practices for Working with SpecKit

1. **Always start with `/speckit.specify`** - don't manually create spec files
2. **Let the system number features** - it checks all sources to avoid conflicts
3. **Trust the defaults** - the system makes informed guesses to reduce clarifications
4. **Use absolute paths** - all scripts return absolute paths in JSON output
5. **Parse JSON outputs** - don't rely on console messages for paths
6. **Validate before implementing** - run `/speckit.analyze` after task generation
7. **Check checklists** - `/speckit.implement` enforces checklist completion
8. **One task format** - strictly follow the `- [ ] [TaskID] [P?] [Story?] Description` pattern
9. **Escape single quotes in PowerShell** - use `'I'\''m Groot'` or double quotes `"I'm Groot"`

## Non-Git Repository Support

SpecKit works in both Git and non-Git environments:

- **Git repos**: Uses branches, validates branch names
- **Non-Git repos**: Uses `specs/NNN-feature-name/` directories, skips branch validation
- **Environment override**: Set `SPECIFY_FEATURE=NNN-feature-name` to force specific feature

## Windows/PowerShell Focus

This implementation uses PowerShell scripts (`.ps1`) rather than Bash. All path handling uses PowerShell conventions and `Join-Path` for cross-platform compatibility.
