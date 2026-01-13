# Tasks: TODO CLI Application

**Input**: Design documents from `/specs/001-todo-cli/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/cli-commands.md

**Tests**: BDD tests with SpecFlow are REQUIRED by the constitution. All user stories include SpecFlow feature files and step definitions.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `- [ ] [ID] [P?] [Story?] Description`

- **- [ ]**: Markdown checkbox (required for all tasks)
- **[ID]**: Sequential task number (T001, T002, T003...)
- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: User story label (US1, US2, etc.) - required for user story phase tasks
- Include exact file paths in descriptions

## Path Conventions

Based on plan.md, this is a single .NET console application project:

```
src/TodoCli/                  # Main application
src/TodoCli.Specs/            # BDD acceptance tests
src/TodoCli.UnitTests/        # Unit tests
tests/TestUtilities/          # Shared test helpers
docs/                         # Documentation
```

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure per plan.md

- [X] T001 Create .NET solution file TodoCli.sln at repository root
- [X] T002 Create TodoCli console application project in src/TodoCli/
- [X] T003 [P] Create TodoCli.Specs SpecFlow test project in src/TodoCli.Specs/
- [X] T004 [P] Create TodoCli.UnitTests xUnit test project in src/TodoCli.UnitTests/
- [X] T005 [P] Create TestUtilities shared test library in tests/TestUtilities/
- [X] T006 Add NuGet package System.CommandLine 2.0.0-beta4 to src/TodoCli/
- [X] T007 [P] Add NuGet package Spectre.Console 0.48.0 to src/TodoCli/
- [X] T008 [P] Add NuGet package System.Text.Json 9.0.1 to src/TodoCli/ (upgraded from 8.0.0 for security)
- [X] T009 [P] Add NuGet packages SpecFlow 3.9.74 and SpecFlow.xUnit 3.9.74 to src/TodoCli.Specs/
- [X] T010 [P] Add NuGet packages xUnit 2.6.2 and FluentAssertions 6.12.0 to src/TodoCli.Specs/
- [X] T011 [P] Add NuGet packages xUnit 2.6.2, FluentAssertions 6.12.0, Moq 4.20.70 to src/TodoCli.UnitTests/
- [X] T012 Create directory structure per plan.md in src/TodoCli/ (Models, Services, Infrastructure, Commands, Output)
- [X] T013 [P] Create directory structure for SpecFlow in src/TodoCli.Specs/ (Features, StepDefinitions, Drivers, Hooks, Support)
- [X] T014 [P] Create README.md with installation and usage instructions in docs/
- [X] T015 [P] Create .gitignore for .NET projects at repository root

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core domain model and infrastructure that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Domain Layer (Zero Dependencies)

- [X] T016 [P] Create Priority enum (Low, Medium, High) in src/TodoCli/Models/Priority.cs
- [X] T017 Create TodoTask entity with all fields and validation in src/TodoCli/Models/TodoTask.cs
- [X] T018 [P] Add TodoTask field validations (title 1-200 chars, description 0-1000 chars, tags) in src/TodoCli/Models/TodoTask.cs
- [X] T019 [P] Add TodoTask business rules (completion one-way transition, due date validation) in src/TodoCli/Models/TodoTask.cs

### Infrastructure Layer (Storage & File I/O)

- [X] T020 [P] Create IFileStorage interface for atomic file operations in src/TodoCli/Infrastructure/Storage/IFileStorage.cs
- [X] T021 Implement FileStorage with atomic write pattern (temp + rename) in src/TodoCli/Infrastructure/Storage/FileStorage.cs
- [X] T022 [P] Add file corruption detection and backup restoration to FileStorage in src/TodoCli/Infrastructure/Storage/FileStorage.cs
- [X] T023 [P] Create StoragePathProvider for platform-specific paths in src/TodoCli/Infrastructure/Configuration/StoragePathProvider.cs
- [X] T024 [P] Implement permission setting (user-only chmod 600 equivalent) in StoragePathProvider in src/TodoCli/Infrastructure/Configuration/StoragePathProvider.cs
- [X] T025 [P] Create TodoTaskJsonContext with source generators for serialization in src/TodoCli/Models/TodoTaskJsonContext.cs
- [X] T026 Create ITaskRepository interface for task persistence in src/TodoCli/Services/ITaskRepository.cs
- [X] T027 Implement JsonTaskRepository using TodoTaskJsonContext in src/TodoCli/Infrastructure/Storage/JsonTaskRepository.cs

### Application Layer (Business Logic)

- [X] T028 Create ITaskService interface for task operations in src/TodoCli/Services/ITaskService.cs
- [X] T029 Create TaskFilter class for filtering and sorting logic in src/TodoCli/Services/TaskFilter.cs
- [X] T030 Implement TaskService with all CRUD operations in src/TodoCli/Services/TaskService.cs

### Presentation Layer (CLI Output Formatting)

- [X] T031 [P] Create IConsoleWriter interface for testable output in src/TodoCli/Output/IConsoleWriter.cs
- [X] T032 [P] Create ColorProvider for ANSI color codes and fallback in src/TodoCli/Output/ColorProvider.cs
- [X] T033 Implement TableFormatter using Spectre.Console for task lists in src/TodoCli/Output/TableFormatter.cs
- [X] T034 [P] Implement ConsoleWriter with color support detection in src/TodoCli/Output/ConsoleWriter.cs

### Test Infrastructure

- [X] T035 [P] Create TestHooks for SpecFlow setup/teardown in src/TodoCli.Specs/Hooks/TestHooks.cs
- [X] T036 [P] Create TodoCliDriver for test automation in src/TodoCli.Specs/Drivers/TodoCliDriver.cs
- [X] T037 [P] Create TaskStorageDriver for test data management in src/TodoCli.Specs/Drivers/TaskStorageDriver.cs
- [X] T038 [P] Create TestDataBuilder for fluent task creation in tests/TestUtilities/TestDataBuilder.cs
- [X] T039 [P] Create TestFileSystem for temp directory management in tests/TestUtilities/TestFileSystem.cs
- [X] T040 [P] Create FakeSystemClock for time-based testing in tests/TestUtilities/FakeSystemClock.cs

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Quick Task Capture (Priority: P1) 🎯 MVP

**Goal**: Enable frictionless task capture with minimal input (just title). User can add task via `todo add "title"` and see confirmation with ID within 50ms.

**Independent Test**: Run `todo add "Buy groceries"` and verify task appears in tasks.json with GUID, timestamp, and default values. Deliverable as standalone task capture tool.

### BDD Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T041 [P] [US1] Create AddTask.feature with all acceptance scenarios in src/TodoCli.Specs/Features/AddTask.feature
- [X] T042 [P] [US1] Create AddTaskSteps step definitions for add command in src/TodoCli.Specs/StepDefinitions/AddTaskSteps.cs
- [X] T043 [US1] Run SpecFlow tests for AddTask to verify they FAIL (no implementation yet)

### Implementation for User Story 1

- [X] T044 [US1] Create AddCommand class with System.CommandLine setup in src/TodoCli/Commands/AddCommand.cs
- [X] T045 [US1] Implement AddCommand.ExecuteAsync with title-only validation in src/TodoCli/Commands/AddCommand.cs
- [X] T046 [US1] Add optional metadata support (--description, --due, --priority, --tags) to AddCommand in src/TodoCli/Commands/AddCommand.cs
- [X] T047 [US1] Add input validation and error messages to AddCommand in src/TodoCli/Commands/AddCommand.cs
- [X] T048 [US1] Implement success output with task ID display in AddCommand in src/TodoCli/Commands/AddCommand.cs
- [X] T049 [US1] Register AddCommand in dependency injection container in src/TodoCli/Program.cs
- [X] T050 [US1] Wire AddCommand to root command in System.CommandLine in src/TodoCli/Program.cs
- [X] T051 [US1] Run SpecFlow tests for AddTask to verify they PASS

### Unit Tests for User Story 1

- [X] T052 [P] [US1] Create TodoTaskTests for entity validation rules in src/TodoCli.UnitTests/Models/TodoTaskTests.cs
- [X] T053 [P] [US1] Create JsonTaskRepositoryTests for persistence logic in src/TodoCli.UnitTests/Infrastructure/JsonTaskRepositoryTests.cs
- [X] T054 [P] [US1] Create FileStorageTests for atomic write operations in src/TodoCli.UnitTests/Infrastructure/FileStorageTests.cs

**Checkpoint**: ✅ User Story 1 COMPLETE! Users can add tasks with all metadata and see them persisted in JSON file. 43 unit tests passing.

---

## Phase 4: User Story 2 - Daily Task Review (Priority: P1)

**Goal**: Enable users to view tasks in formatted, color-coded table with sorting and filtering. User runs `todo list` to see all tasks, with overdue tasks highlighted in red, tasks due today in yellow.

**Independent Test**: Add 5 tasks with different priorities/due dates, run `todo list`, verify table formatting, colors, and sorting options work correctly.

### BDD Tests for User Story 2

- [X] T055 [P] [US2] Create ListTasks.feature with all filter/sort scenarios in src/TodoCli.Specs/Features/ListTasks.feature
- [X] T056 [P] [US2] Create ListTasksSteps step definitions for list command in src/TodoCli.Specs/StepDefinitions/ListTasksSteps.cs
- [X] T057 [US2] Run SpecFlow tests for ListTasks to verify they FAIL

### Implementation for User Story 2

- [X] T058 [US2] Create ListCommand class with filter/sort options in src/TodoCli/Commands/ListCommand.cs
- [X] T059 [US2] Implement status filtering (all, complete, incomplete) in ListCommand in src/TodoCli/Commands/ListCommand.cs
- [X] T060 [P] [US2] Implement priority filtering in ListCommand in src/TodoCli/Commands/ListCommand.cs
- [X] T061 [P] [US2] Implement tag filtering (OR logic for multiple tags) in ListCommand in src/TodoCli/Commands/ListCommand.cs
- [X] T062 [P] [US2] Implement due date filtering in ListCommand in src/TodoCli/Commands/ListCommand.cs
- [X] T063 [US2] Implement sorting (by created, due, priority) in ListCommand in src/TodoCli/Commands/ListCommand.cs
- [X] T064 [US2] Add color coding (red for overdue, yellow for today, green for complete) in TableFormatter in src/TodoCli/Output/TableFormatter.cs
- [X] T065 [US2] Add visual indicators ([!] overdue, [✓] complete, [H][M][L] priority) in TableFormatter in src/TodoCli/Output/TableFormatter.cs
- [X] T066 [US2] Implement 80-column terminal compatibility with truncation in TableFormatter in src/TodoCli/Output/TableFormatter.cs
- [X] T067 [US2] Register ListCommand in DI container in src/TodoCli/Program.cs
- [X] T068 [US2] Wire ListCommand to root command in src/TodoCli/Program.cs
- [X] T069 [US2] Run SpecFlow tests for ListTasks to verify they PASS

### Unit Tests for User Story 2

- [X] T070 [P] [US2] Create TaskFilterTests for filtering logic in src/TodoCli.UnitTests/Services/TaskFilterTests.cs
- [X] T071 [P] [US2] Create TableFormatterTests for output rendering in src/TodoCli.UnitTests/Output/TableFormatterTests.cs

**Checkpoint**: Users can now add tasks (US1) and view them in formatted lists (US2). These two stories provide MVP value: capture + review.

---

## Phase 5: User Story 3 - Task Completion Tracking (Priority: P1)

**Goal**: Enable users to mark tasks complete with timestamp tracking. User runs `todo complete <id>` to mark task done, seeing confirmation with duration calculation.

**Independent Test**: Add task, wait briefly, mark complete, verify completion timestamp set, duration displayed, and status changes to complete in list view.

### BDD Tests for User Story 3

- [X] T072 [P] [US3] Create CompleteTask.feature with completion scenarios in src/TodoCli.Specs/Features/CompleteTask.feature
- [X] T073 [P] [US3] Create CompleteTaskSteps step definitions in src/TodoCli.Specs/StepDefinitions/CompleteTaskSteps.cs
- [X] T074 [US3] Run SpecFlow tests for CompleteTask to verify they FAIL

### Implementation for User Story 3

- [X] T075 [US3] Create CompleteCommand class with ID argument in src/TodoCli/Commands/CompleteCommand.cs
- [X] T076 [US3] Implement partial ID matching (minimum 4 characters) in CompleteCommand in src/TodoCli/Commands/CompleteCommand.cs
- [X] T077 [US3] Implement ID disambiguation for multiple matches in CompleteCommand in src/TodoCli/Commands/CompleteCommand.cs
- [X] T078 [US3] Add task completion logic with timestamp in TaskService in src/TodoCli/Services/TaskService.cs
- [X] T079 [US3] Implement duration calculation (CreatedAt to CompletedAt) in CompleteCommand in src/TodoCli/Commands/CompleteCommand.cs
- [X] T080 [US3] Add validation (task must be incomplete) in CompleteCommand in src/TodoCli/Commands/CompleteCommand.cs
- [X] T081 [US3] Implement success output with duration display in CompleteCommand in src/TodoCli/Commands/CompleteCommand.cs
- [X] T082 [US3] Register CompleteCommand in DI container in src/TodoCli/Program.cs
- [X] T083 [US3] Wire CompleteCommand to root command in src/TodoCli/Program.cs
- [X] T084 [US3] Run SpecFlow tests for CompleteTask to verify they PASS

### Unit Tests for User Story 3

- [X] T085 [P] [US3] Create TaskServiceTests for completion logic in src/TodoCli.UnitTests/Services/TaskServiceTests.cs

**Checkpoint**: Core workflow complete (capture → review → complete). Users have full MVP: add, list, complete tasks.

---

## Phase 6: User Story 4 - Task Metadata Management (Priority: P2)

**Goal**: Enable users to update task properties and view detailed task information. User can run `todo update <id>` to change title, priority, due date, tags. User can run `todo show <id>` to see all task details with calculated fields.

**Independent Test**: Add task with metadata, update multiple fields, verify only specified fields change. Show task detail, verify all fields displayed with human-friendly formatting.

### BDD Tests for User Story 4

- [ ] T086 [P] [US4] Create UpdateTask.feature with update scenarios in src/TodoCli.Specs/Features/UpdateTask.feature
- [ ] T087 [P] [US4] Create ShowTask.feature with detail view scenarios in src/TodoCli.Specs/Features/ShowTask.feature
- [ ] T088 [P] [US4] Create UpdateTaskSteps step definitions in src/TodoCli.Specs/StepDefinitions/UpdateTaskSteps.cs
- [ ] T089 [P] [US4] Create ShowTaskSteps step definitions in src/TodoCli.Specs/StepDefinitions/ShowTaskSteps.cs
- [ ] T090 [US4] Run SpecFlow tests for UpdateTask and ShowTask to verify they FAIL

### Implementation for User Story 4 (Update Command)

- [X] T091 [US4] Create UpdateCommand class with all update options in src/TodoCli/Commands/UpdateCommand.cs
- [X] T092 [US4] Implement partial update logic (only specified fields change) in UpdateCommand in src/TodoCli/Commands/UpdateCommand.cs
- [X] T093 [P] [US4] Add tag operations (--add-tags, --remove-tags) to UpdateCommand in src/TodoCli/Commands/UpdateCommand.cs
- [X] T094 [P] [US4] Add due date clearing (--due none) to UpdateCommand in src/TodoCli/Commands/UpdateCommand.cs
- [X] T095 [US4] Implement validation for update operations in UpdateCommand in src/TodoCli/Commands/UpdateCommand.cs
- [X] T096 [US4] Add before/after change display in UpdateCommand in src/TodoCli/Commands/UpdateCommand.cs
- [X] T097 [US4] Register UpdateCommand in DI container in src/TodoCli/Program.cs
- [X] T098 [US4] Wire UpdateCommand to root command in src/TodoCli/Program.cs

### Implementation for User Story 4 (Show Command)

- [X] T099 [US4] Create ShowCommand class with detail formatting in src/TodoCli/Commands/ShowCommand.cs
- [X] T100 [US4] Implement calculated fields (task age, time until due, time since completion) in ShowCommand in src/TodoCli/Commands/ShowCommand.cs
- [X] T101 [US4] Add formatted detail output with all task properties in ShowCommand in src/TodoCli/Commands/ShowCommand.cs
- [X] T102 [US4] Register ShowCommand in DI container in src/TodoCli/Program.cs
- [X] T103 [US4] Wire ShowCommand to root command in src/TodoCli/Program.cs
- [ ] T104 [US4] Run SpecFlow tests for UpdateTask and ShowTask to verify they PASS

**Checkpoint**: Users can now fully manage task metadata - create, view, update, complete tasks with all properties.

---

## Phase 7: User Story 5 - Task Filtering and Searching (Priority: P2)

**Goal**: Already implemented in User Story 2! Filtering and sorting are part of the list command.

**Status**: ✅ Complete (implemented in Phase 4)

**Note**: This user story maps to the filtering functionality added in ListCommand (tasks T059-T063). No additional implementation needed.

---

## Phase 8: User Story 6 - Task Deletion (Priority: P3)

**Goal**: Enable permanent task deletion with confirmation. User runs `todo delete <id>` to remove task with confirmation prompt. User can use `--force` to skip confirmation.

**Independent Test**: Add task, delete with confirmation, verify removed from storage. Add task, delete with --force, verify immediate deletion without prompt.

### BDD Tests for User Story 6

- [ ] T105 [P] [US6] Create DeleteTask.feature with deletion scenarios in src/TodoCli.Specs/Features/DeleteTask.feature
- [ ] T106 [P] [US6] Create DeleteTaskSteps step definitions in src/TodoCli.Specs/StepDefinitions/DeleteTaskSteps.cs
- [ ] T107 [US6] Run SpecFlow tests for DeleteTask to verify they FAIL

### Implementation for User Story 6

- [X] T108 [US6] Create DeleteCommand class with confirmation flow in src/TodoCli/Commands/DeleteCommand.cs
- [X] T109 [US6] Implement confirmation prompt with task details display in DeleteCommand in src/TodoCli/Commands/DeleteCommand.cs
- [X] T110 [US6] Add --force flag to skip confirmation in DeleteCommand in src/TodoCli/Commands/DeleteCommand.cs
- [X] T111 [US6] Implement permanent deletion logic in TaskService in src/TodoCli/Services/TaskService.cs
- [X] T112 [US6] Add cancellation handling in DeleteCommand in src/TodoCli/Commands/DeleteCommand.cs
- [X] T113 [US6] Register DeleteCommand in DI container in src/TodoCli/Program.cs
- [X] T114 [US6] Wire DeleteCommand to root command in src/TodoCli/Program.cs
- [ ] T115 [US6] Run SpecFlow tests for DeleteTask to verify they PASS

**Checkpoint**: All 6 user stories complete. Full feature set implemented: add, list, complete, update, show, delete.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories and overall quality

### Help System & Error Handling

- [ ] T116 [P] Add comprehensive --help text for all commands in Program.cs
- [ ] T117 [P] Implement command suggestion for typos ("Did you mean...?") in Program.cs
- [ ] T118 [P] Add global exception handler with user-friendly messages in Program.cs
- [ ] T119 [P] Ensure all error messages follow consistent format in all Command classes

### Performance Optimization

- [ ] T120 [P] Verify startup time < 100ms with performance profiling
- [ ] T121 [P] Verify add command < 50ms with performance profiling
- [ ] T122 [P] Verify list 100 tasks < 200ms with performance profiling
- [ ] T123 [P] Optimize JSON serialization with source generators (verify T025 implementation)

### Documentation

- [ ] T124 [P] Update README.md with all commands and examples in docs/README.md
- [ ] T125 [P] Add CONTRIBUTING.md with development guide in docs/CONTRIBUTING.md
- [ ] T126 [P] Create command reference documentation in docs/
- [ ] T127 [P] Add architecture diagram to documentation in docs/

### Quality Assurance

- [ ] T128 [P] Run all SpecFlow scenarios and verify 100% pass rate
- [ ] T129 [P] Verify code coverage meets 80% threshold (100% for critical paths)
- [ ] T130 [P] Run static analysis (Roslyn analyzers) and fix all warnings
- [ ] T131 [P] Run quickstart.md validation scenarios end-to-end
- [ ] T132 [P] Test on Windows, macOS, and Linux platforms
- [ ] T133 [P] Verify color support graceful degradation on legacy terminals

### Packaging & Distribution

- [ ] T134 Publish release build for cross-platform deployment
- [ ] T135 [P] Create installation guide for all platforms in docs/
- [ ] T136 [P] Add version command (--version) in Program.cs
- [ ] T137 [P] Add tab completion script generation for PowerShell/Bash in docs/

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - start immediately
  - All T001-T015 can begin in parallel

- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
  - Starts after T015 complete
  - Domain, Infrastructure, Application, Presentation layers can proceed somewhat in parallel
  - Test infrastructure can proceed in parallel

- **User Stories (Phases 3-8)**: All depend on Foundational phase (T040 complete)
  - **Critical Path**: T040 → US1 (T041) → US2 (T055) → US3 (T072)
  - User stories CAN proceed in parallel if team capacity allows
  - Recommended: Sequential by priority (P1 stories first, then P2, then P3)

- **Polish (Phase 9)**: Depends on all user stories complete
  - Can start after T115 complete
  - All T116-T137 can proceed in parallel

### User Story Dependencies

- **User Story 1 (P1 - T041-T054)**: Depends on T040 (Foundational complete) - No story dependencies
- **User Story 2 (P1 - T055-T071)**: Depends on T040 (Foundational complete) - No story dependencies
- **User Story 3 (P1 - T072-T085)**: Depends on T040 (Foundational complete) - No story dependencies
- **User Story 4 (P2 - T086-T104)**: Depends on T040 (Foundational complete) - No story dependencies
- **User Story 5 (P2)**: No tasks (already in US2)
- **User Story 6 (P3 - T105-T115)**: Depends on T040 (Foundational complete) - No story dependencies

All user stories are **independently testable** and can be implemented in any order after Foundation.

### Within Each User Story

1. **BDD Tests FIRST**: Write SpecFlow features and steps, verify they FAIL
2. **Implementation**: Implement command, service logic, validation
3. **Verification**: Run SpecFlow tests, verify they PASS
4. **Unit Tests**: Add unit tests for complex logic
5. **Checkpoint**: Verify story works independently before proceeding

### Parallel Opportunities

#### Phase 1: Setup (All parallel)
- T003, T004, T005 (project creation)
- T006, T007, T008 (main project packages)
- T009, T010, T011 (test project packages)
- T012, T013 (directory structures)
- T014, T015 (documentation, git)

#### Phase 2: Foundational (Partial parallelism)
- T016, T017, T018, T019 (Domain layer - sequential dependency)
- T020, T021, T022, T023, T024, T025 (Infrastructure - some parallelism)
- T035-T040 (Test infrastructure - all parallel)

#### Across User Stories (Team parallelism)
- Developer A: User Story 1 (T041-T054)
- Developer B: User Story 2 (T055-T071)
- Developer C: User Story 3 (T072-T085)
- All start after T040 complete

#### Phase 9: Polish (All parallel)
- T116-T137 can all proceed in parallel

---

## Parallel Example: User Story 1

```bash
# Write all BDD tests for User Story 1 in parallel:
Task: "Create AddTask.feature with all acceptance scenarios"
Task: "Create AddTaskSteps step definitions"

# Write all unit tests for User Story 1 in parallel:
Task: "Create TodoTaskTests for entity validation"
Task: "Create JsonTaskRepositoryTests for persistence"
Task: "Create FileStorageTests for atomic writes"
```

---

## Implementation Strategy

### MVP First (User Stories 1, 2, 3 Only - Core P1)

1. Complete **Phase 1: Setup** (T001-T015)
2. Complete **Phase 2: Foundational** (T016-T040) - CRITICAL, blocks all stories
3. Complete **Phase 3: User Story 1** (T041-T054) - Quick Task Capture
4. Complete **Phase 4: User Story 2** (T055-T071) - Daily Task Review
5. Complete **Phase 5: User Story 3** (T072-T085) - Task Completion
6. **STOP and VALIDATE**: Test core workflow independently (add → list → complete)
7. **MVP Ready**: Deploy/demo with P1 user stories

**MVP Deliverable**: Users can capture, review, and complete tasks - full core value loop.

### Incremental Delivery (Add P2 and P3 Features)

8. Complete **Phase 6: User Story 4** (T086-T104) - Metadata Management
9. **Deploy/Demo**: Enhanced version with update and show commands
10. Complete **Phase 8: User Story 6** (T105-T115) - Task Deletion
11. **Deploy/Demo**: Complete feature set
12. Complete **Phase 9: Polish** (T116-T137) - Quality and documentation

### Parallel Team Strategy

With 3 developers after Foundation complete:

1. **Team completes**: Setup (Phase 1) + Foundational (Phase 2) together
2. **Once T040 complete**:
   - Developer A: User Story 1 (Quick Task Capture)
   - Developer B: User Story 2 (Daily Task Review)
   - Developer C: User Story 3 (Task Completion)
3. Each developer completes their story independently
4. Integrate and validate all P1 stories together
5. Repeat for P2 stories (US4) and P3 stories (US6)

---

## Notes

### Task Format Compliance

✅ **All tasks follow required format**:
- `- [ ]` checkbox (markdown format)
- `[TaskID]` sequential number (T001-T137)
- `[P]` marker for parallelizable tasks
- `[Story]` label for user story tasks (US1-US6)
- Description with exact file path

### Critical Success Factors

- **BDD First**: Write SpecFlow tests before implementation, verify FAIL, then implement, verify PASS
- **Independent Stories**: Each user story delivers value independently
- **Foundation First**: T040 MUST complete before any user story begins
- **Performance Targets**: Track startup (< 100ms), add (< 50ms), list (< 200ms)
- **Test Coverage**: Maintain 80% overall, 100% for persistence and command parsing
- **Constitution Compliance**: All architecture, testing, and quality gates from constitution must be met

### Verification at Each Checkpoint

After each user story phase:
1. Run all SpecFlow scenarios for that story - MUST all pass
2. Verify story works independently (can use without other stories)
3. Check performance targets are met
4. Ensure code coverage threshold maintained
5. Commit changes with descriptive message

### Avoid

- ❌ Implementing before writing BDD tests
- ❌ Cross-story dependencies that break independence
- ❌ Tasks without file paths
- ❌ Missing [Story] labels in user story phases
- ❌ Vague task descriptions
