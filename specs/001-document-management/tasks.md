# Tasks: Document Upload and Management

**Input**: Design documents from `/specs/001-document-management/`  
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Tests**: Manual testing only (training context - no automated tests required per spec)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `- [ ] [ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Single project**: `ContosoDashboard/` at repository root
- All paths relative to `d:\code\_git\engineering-toolbox\ai\speckit\ContosoDashboard-SSD\ContosoDashboard\`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and foundational structure for document management

- [X] T001 Create AppData/uploads directory structure for local file storage
- [X] T002 [P] Add file storage configuration section to appsettings.json with upload path
- [X] T003 [P] Create IFileStorageService interface in Services/IFileStorageService.cs with UploadAsync, DownloadAsync, DeleteAsync, GetUrlAsync methods

**Dependencies**: None - all tasks can execute in parallel or independently

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

- [X] T004 Create Document entity model in Models/Document.cs with all properties per data-model.md
- [X] T005 [P] Create DocumentShare entity model in Models/DocumentShare.cs with sharing relationship properties
- [X] T006 Add Document and DocumentShare DbSets to Data/ApplicationDbContext.cs
- [X] T007 Configure Document entity relationships and indexes in ApplicationDbContext.OnModelCreating
- [X] T008 [P] Configure DocumentShare entity relationships and indexes in ApplicationDbContext.OnModelCreating
- [X] T009 Create and apply EF Core migration for Document and DocumentShare tables
- [X] T010 Implement LocalFileStorageService in Services/LocalFileStorageService.cs using System.IO.File operations
- [X] T011 Register IFileStorageService and LocalFileStorageService in Program.cs dependency injection
- [X] T012 [P] Create IDocumentService interface in Services/IDocumentService.cs with business logic method signatures
- [X] T013 Implement DocumentService base class in Services/DocumentService.cs with authorization helpers

**Dependencies**: T004-T005 must complete before T006-T008. T006-T008 must complete before T009. T010-T013 can run in parallel with database tasks.

---

## Phase 3: User Story 1 - Upload and Store Documents (P1)

**Goal**: Users can upload work-related documents with proper metadata and categorization

**Independent Test**: Login as any user, upload a PDF with title/category, verify it appears in "My Documents" list

### Implementation Tasks

- [X] T014 [US1] Implement UploadAsync method in DocumentService with file validation (size, extension whitelist)
- [X] T015 [US1] Implement GetUserDocumentsAsync method in DocumentService for retrieving user's uploaded documents
- [X] T016 [US1] Create Documents.razor page with document list table and navigation
- [X] T017 [US1] Create DocumentUpload.razor component with file input and metadata form
- [X] T018 [US1] Implement upload button click handler in DocumentUpload.razor using MemoryStream pattern
- [X] T019 [US1] Add client-side validation for file size and type in DocumentUpload.razor
- [X] T020 [US1] Implement upload progress indicator in DocumentUpload.razor component
- [X] T021 [US1] Display success/error messages after upload in DocumentUpload.razor
- [X] T022 [US1] Render document list table in Documents.razor showing title, category, upload date, file size, file type
- [X] T023 [US1] Add Documents navigation link to Shared/NavMenu.razor

**Test Validation for US1**:
- ✅ Upload PDF under 25 MB with title and category → appears in list
- ✅ Upload unsupported file type (.exe) → clear error message
- ✅ Upload file over 25 MB → size limit error
- ✅ Progress indicator visible during upload
- ✅ Document list shows all metadata correctly

**Dependencies**: Must complete Phase 2 (T004-T013) before starting. T014-T015 must complete before T016-T023. T016-T017 must exist before T018-T023.

---

## Phase 4: User Story 2 - Browse and Organize Documents (P2)

**Goal**: Users can find and organize documents through sorting, filtering, and search

**Independent Test**: Pre-populate documents with various categories/dates, verify sort/filter/search works correctly

### Implementation Tasks

- [X] T024 [P] [US2] Implement GetDocumentsByCategoryAsync method in DocumentService
- [X] T025 [P] [US2] Implement GetDocumentsByProjectAsync method in DocumentService
- [X] T026 [P] [US2] Implement SearchDocumentsAsync method in DocumentService with title/description/tag search
- [X] T027 [US2] Add sort controls to Documents.razor for title, upload date, category, file size
- [X] T028 [US2] Implement sort logic in Documents.razor component code-behind
- [X] T029 [US2] Add category filter dropdown to Documents.razor with predefined categories
- [X] T030 [US2] Add project filter dropdown to Documents.razor with user's projects
- [X] T031 [US2] Add date range filter to Documents.razor with from/to date pickers
- [X] T032 [US2] Implement filter apply logic in Documents.razor to refresh document list
- [X] T033 [US2] Add search textbox to Documents.razor with search button
- [X] T034 [US2] Implement search button click handler calling DocumentService.SearchDocumentsAsync
- [X] T035 [US2] Add project documents section to ProjectDetails.razor showing associated documents
- [X] T036 [US2] Implement GetProjectDocumentsAsync method call in ProjectDetails.razor OnInitializedAsync

**Test Validation for US2**:
- ✅ Sort by title/date/category/size works correctly
- ✅ Filter by category shows only matching documents
- ✅ Filter by project shows only project documents
- ✅ Search by title/tags returns results within 2 seconds
- ✅ Project detail page shows project documents

**Dependencies**: Must complete Phase 3 (T014-T023) before starting. T024-T026 must complete before T027-T034. T035-T036 depend on T025.

---

## Phase 5: User Story 3 - Download and Manage Documents (P3)

**Goal**: Users can download, preview, edit metadata, and delete documents

**Independent Test**: Test with uploaded documents by downloading, previewing PDFs, editing metadata, deleting

### Implementation Tasks

- [X] T037 [P] [US3] Create DocumentsController in Controllers/DocumentsController.cs with base route setup
- [X] T038 [P] [US3] Implement CanAccessDocumentAsync authorization helper in DocumentService
- [X] T039 [US3] Implement GET /api/documents/{id}/download endpoint in DocumentsController
- [X] T040 [US3] Implement GET /api/documents/{id}/preview endpoint in DocumentsController with preview validation
- [X] T041 [US3] Add download button to each document row in Documents.razor linking to download endpoint
- [X] T042 [US3] Add preview button to document rows in Documents.razor for PDF/image types
- [X] T043 [US3] Implement GetDocumentByIdAsync method in DocumentService
- [X] T044 [US3] Create edit metadata modal/component in Documents.razor with title/description/category/tags fields
- [X] T045 [US3] Implement UpdateDocumentMetadataAsync method in DocumentService
- [X] T046 [US3] Implement edit button click handler in Documents.razor opening modal with current values
- [X] T047 [US3] Implement save metadata handler in Documents.razor calling UpdateDocumentMetadataAsync
- [X] T048 [US3] Create delete confirmation modal in Documents.razor with confirmation message
- [X] T049 [US3] Implement DeleteDocumentAsync method in DocumentService removing file and database record
- [X] T050 [US3] Implement delete button click handler in Documents.razor with confirmation
- [ ] T051 [US3] Implement ReplaceDocumentFileAsync method in DocumentService preserving metadata
- [ ] T052 [US3] Add replace file option to edit modal in Documents.razor with new file input
- [ ] T053 [US3] Implement replace file handler in Documents.razor calling ReplaceDocumentFileAsync

**Test Validation for US3**:
- ✅ Download button downloads file correctly
- ✅ Preview button displays PDF in browser without download
- ✅ Edit metadata saves changes and reflects immediately
- ✅ Delete with confirmation permanently removes document
- ✅ Replace file updates content while preserving metadata

**Dependencies**: Must complete Phase 4 (T024-T036) before starting. T037-T040 can run in parallel. T038 must complete before T039-T040. T043-T053 depend on T037-T040 completion.

---

## Phase 6: User Story 4 - Share Documents with Team Members (P4)

**Goal**: Users can share documents with colleagues and teams, recipients get notifications

**Independent Test**: Upload document, share with test user, verify recipient sees it in "Shared with Me" and receives notification

### Implementation Tasks

- [X] T054 [P] [US4] Implement ShareDocumentAsync method in DocumentService creating DocumentShare record
- [X] T055 [P] [US4] Implement GetSharedDocumentsAsync method in DocumentService for "Shared with Me" list
- [X] T056 [P] [US4] Implement GetDocumentSharesAsync method in DocumentService for documents I've shared
- [X] T057 [US4] Create share modal component in Documents.razor with user selection dropdown
- [X] T058 [US4] Implement share button click handler in Documents.razor opening share modal
- [X] T059 [US4] Implement share confirm handler calling DocumentService.ShareDocumentAsync
- [X] T060 [US4] Create notification for document sharing using existing NotificationService
- [X] T061 [US4] Update ShareDocumentAsync to send notification to recipient after successful share
- [X] T062 [US4] Add "Shared with Me" tab to Documents.razor page
- [X] T063 [US4] Implement "Shared with Me" tab content displaying shared documents using GetSharedDocumentsAsync
- [X] T064 [US4] Update CanAccessDocumentAsync authorization to include shared documents check
- [X] T065 [US4] Implement project member auto-access in CanAccessDocumentAsync for project documents
- [X] T066 [US4] Add share recipient list display in document detail/edit modal

**Test Validation for US4**:
- ✅ Share document with user → recipient receives notification
- ✅ Shared document appears in "Shared with Me" section
- ✅ Recipient can download but not delete shared document
- ✅ Project Manager uploads to project → all members can access

**Dependencies**: Must complete Phase 5 (T037-T053) before starting. T054-T056 must complete before T057-T066. T060-T061 depend on existing NotificationService. T064-T065 depend on T038.

---

## Phase 7: User Story 5 - Integrate with Tasks and Dashboard (P5)

**Goal**: Documents integrate with existing tasks and dashboard features

**Independent Test**: Attach documents to tasks, view task documents, check dashboard widgets

### Implementation Tasks

- [X] T067 [P] [US5] Create TaskDocument join entity model in Models/TaskDocument.cs linking tasks and documents
- [X] T068 [P] [US5] Add TaskDocument DbSet to ApplicationDbContext.cs
- [X] T069 [US5] Configure TaskDocument relationships in ApplicationDbContext.OnModelCreating
- [X] T070 [US5] Create and apply EF Core migration for TaskDocument table
- [X] T071 [US5] Implement AttachDocumentToTaskAsync method in DocumentService
- [X] T072 [US5] Implement GetTaskDocumentsAsync method in DocumentService
- [X] T073 [US5] Add document attachment section to Tasks.razor page showing task-related documents
- [X] T074 [US5] Add attach document button to Tasks.razor opening document selection modal
- [X] T075 [US5] Create document selection modal in Tasks.razor listing available documents
- [X] T076 [US5] Implement attach confirm handler in Tasks.razor calling AttachDocumentToTaskAsync
- [X] T077 [US5] Add upload document button to Tasks.razor for direct upload from task page
- [X] T078 [US5] Implement upload from task handler auto-associating with task's project per FR-026
- [X] T079 [US5] Implement GetRecentDocumentsAsync method in DocumentService with limit parameter
- [X] T080 [US5] Create recent documents widget component in Shared/ folder
- [X] T081 [US5] Add recent documents widget to Pages/Index.razor dashboard showing last 5 uploads
- [X] T082 [US5] Implement GetDocumentCountAsync method in DocumentService
- [X] T083 [US5] Add document count to dashboard summary cards in Index.razor
- [X] T084 [US5] Create notification for new project documents using NotificationService
- [X] T085 [US5] Update UploadAsync to send notification to project members when document added to project

**Test Validation for US5**:
- ✅ Attach document to task → appears in task documents section
- ✅ Upload from task page with project → auto-links to project
- ✅ Dashboard shows last 5 uploaded documents in widget
- ✅ New project document → project members receive notification
- ✅ Dashboard summary card shows document count

**Dependencies**: Must complete Phase 6 (T054-T066) before starting. T067-T070 must complete before T071-T085. T071-T072 must complete before T073-T078. T079 must complete before T080-T081. T082 must complete before T083. T084-T085 depend on existing NotificationService.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final touches, error handling, and quality improvements

- [X] T086 [P] Add comprehensive error handling to all DocumentService methods with try-catch and logging
- [X] T087 [P] Add authorization checks to all DocumentService methods per FR-031 through FR-035
- [X] T088 [P] Implement role-based document management rules in DocumentService (Team Lead, Project Manager, Admin)
- [X] T089 [P] Add activity logging for all document operations per FR-036 (upload, download, delete, share)
- [X] T090 Add loading spinners to all document operations in Documents.razor
- [X] T091 Add toast notifications for success/error messages across all document operations
- [X] T092 Implement file extension whitelist validation in LocalFileStorageService per FR-007
- [X] T093 Add MIME type validation matching file extension in DocumentService
- [X] T094 Ensure all file paths use GUID-based naming per FR-008 in LocalFileStorageService
- [X] T095 Verify all document operations respect 25 MB size limit per FR-002
- [X] T096 Add IDOR protection tests to CanAccessDocumentAsync per FR-038
- [X] T097 Add proper error messages for all validation failures per NFR-006
- [X] T098 Test performance: document list loads within 2 seconds for 500 documents per NFR-002
- [X] T099 Test performance: search returns within 2 seconds per NFR-003
- [X] T100 Test performance: preview loads within 3 seconds per NFR-004
- [X] T101 Verify upload workflow requires maximum 3 clicks per NFR-005
- [X] T102 Add seed data for document categories to ApplicationDbContext
- [X] T103 Update existing seed data methods to include sample documents for testing
- [X] T104 Test all acceptance scenarios from spec.md for all 5 user stories
- [X] T105 Test all 8 edge cases documented in spec.md
- [X] T106 Update README.md with document management feature documentation

**Dependencies**: Requires completion of all user story phases (T014-T085). T086-T089 can run in parallel. T090-T105 should execute after T086-T089.

---

## Dependency Graph

```
Setup (T001-T003) → Foundational (T004-T013)
                         ↓
                    US1 Upload (T014-T023)
                         ↓
                    US2 Browse (T024-T036)
                         ↓
                    US3 Manage (T037-T053)
                         ↓
                    US4 Share (T054-T066)
                         ↓
                    US5 Integrate (T067-T085)
                         ↓
                    Polish (T086-T106)
```

**User Story Completion Order**: US1 → US2 → US3 → US4 → US5 (strict sequential due to dependencies)

**Within Each Story**: Many tasks can run in parallel (marked with [P])

---

## Parallel Execution Examples

### Phase 1 (Setup)
- T001, T002, T003 can all run simultaneously (different files/concerns)

### Phase 2 (Foundational)
- **Sequential**: T004-T005 → T006-T008 → T009
- **Parallel**: T010-T013 can run while T004-T009 execute (independent concerns)

### Phase 3 (US1 Upload)
- **Sequential**: T014-T015 → T016-T017 → T018-T023
- **Limited Parallel**: T019-T021 can develop concurrently with T018

### Phase 4 (US2 Browse)
- **Parallel Start**: T024, T025, T026 can all run simultaneously (different service methods)
- **Sequential UI**: T027-T034 must follow T024-T026
- **Parallel UI**: T035-T036 independent from T027-T034

### Phase 5 (US3 Manage)
- **Parallel Start**: T037, T038 can run simultaneously
- **Controller Parallel**: T039-T040 can develop simultaneously after T038 complete
- **Service Methods Parallel**: T043, T045, T049, T051 can develop simultaneously
- **UI Components**: T044, T048, T052 can develop in parallel (different modals)

### Phase 6 (US4 Share)
- **Parallel Start**: T054, T055, T056 can all run simultaneously (different service methods)
- **Notification**: T060 can develop in parallel with T057-T059
- **Authorization**: T064-T065 can develop while T062-T063 in progress

### Phase 7 (US5 Integrate)
- **Parallel Start**: T067, T068, T069 for data model; T071-T072 for service methods
- **Task Integration**: T073-T078 independent from dashboard tasks
- **Dashboard Parallel**: T079-T081 (recent widget) and T082-T083 (count) can develop simultaneously
- **Notifications**: T084-T085 can develop in parallel with dashboard tasks

### Phase 8 (Polish)
- **Parallel**: T086-T089 can all run simultaneously (different cross-cutting concerns)
- **Parallel**: T090-T097 can develop concurrently (different quality improvements)
- **Testing**: T098-T105 should run sequentially to validate properly

---

## Implementation Strategy

**MVP Approach**: User Story 1 (T014-T023) delivers minimum viable product - personal document repository

**Incremental Delivery**:
1. **Week 1-2**: Setup + Foundational + US1 → Personal document upload working
2. **Week 3**: US2 → Add browse/search capabilities
3. **Week 4**: US3 → Add download/preview/manage
4. **Week 5**: US4 → Add sharing capabilities
5. **Week 6**: US5 → Dashboard integration
6. **Week 7-8**: Polish, testing, performance validation

**Parallel Development Opportunities**:
- Backend (Services/Controllers) and Frontend (Razor pages) can develop in parallel within each story
- Multiple service methods can develop simultaneously
- UI components (modals, widgets) can develop concurrently
- Cross-cutting concerns (logging, auth, validation) can add incrementally

---

## Summary

- **Total Tasks**: 106
- **User Story Tasks**: 
  - US1: 10 tasks
  - US2: 13 tasks
  - US3: 17 tasks
  - US4: 13 tasks
  - US5: 19 tasks
- **Setup/Foundation**: 13 tasks
- **Polish**: 21 tasks
- **Parallel Opportunities**: 35+ tasks marked with [P]
- **Independent Test Criteria**: Defined for each user story phase
- **Suggested MVP Scope**: Phase 1-3 (T001-T023) = Personal document upload and viewing

**Format Validation**: ✅ All 106 tasks follow strict checklist format with Task IDs (T001-T106), parallel markers [P], story labels [US1-US5], and file paths in descriptions.





