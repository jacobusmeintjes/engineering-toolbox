specify init --here --ai copilot --script ps
```
**Files created - verify these exist:**
- `.github/agents/` - Agent workflow files
- `.github/prompts/` - Prompt instruction files
- `.specify/memory/` - Constitution storage
- `.specify/templates/` - Template files (spec-template.md, plan-template.md, etc.)
- `.specify/scripts/powershell/` - Automation scripts
- `.vscode/settings.json` - Updated/created

---

## 2. **Generate Constitution**
**What it does:** Analyzes your project (or creates from scratch) to establish governing principles, technical standards, security requirements, and development constraints that all features must follow. Think of it as your project's "rulebook" that ensures consistency.
```
/speckit.constitution
```
**Files to review:**
- `.specify/memory/constitution.md` - Updated with project principles
  - ✅ Verify: Security, performance, quality, technical standards are covered
  - ✅ Verify: Principles are specific and actionable
  - ✅ Verify: No contradictions between principles

---

## 3. **Create Feature Specification**
**What it does:** Transforms stakeholder requirements into a detailed specification document that defines WHAT to build (not HOW). Includes user scenarios, acceptance criteria, requirements, and success metrics from a business/user perspective.
```
/speckit.specify --file StakeholderDocs/document-upload-and-management-feature.md
```
**Files to review:**
- `specs/spec.md` - Feature specification
  - ✅ Verify: User Scenarios & Testing section
  - ✅ Verify: Requirements section (file size limits, supported types, performance targets)
  - ✅ Verify: Success Criteria section
  - ✅ Verify: Acceptance scenarios use Given-When-Then format
- `checklists/requirements.md` - Validation checklist
  - ✅ Verify: All checklist items passed successfully

**Note:** This creates a new branch - verify branch was published to GitHub

---

## 4. **Clarify Requirements**
**What it does:** Analyzes your specification to identify ambiguities, gaps, and underspecified areas. Asks targeted questions to ensure all requirements are clear and complete before moving to technical planning. Prevents building the wrong thing.
```
/speckit.clarify
```
**Files to review:**
- `specs/spec.md` - Updated with clarifications
  - ✅ Verify: Your answers are accurately reflected
  - ✅ Verify: Previously ambiguous areas now have clear requirements
  - ✅ Verify: New acceptance criteria added based on clarifications

---

## 5. **Generate Technical Plan**
**What it does:** Creates a comprehensive technical implementation plan that defines HOW to build the feature. Includes architecture decisions, technology choices, data models, API designs, and implementation approach while respecting constitution constraints.
```
/speckit.plan
```
**Files to review:**
- `specs/plan.md` - Technical implementation plan
  - ✅ Verify: Architecture decisions
  - ✅ Verify: Technology choices
  - ✅ Verify: Implementation strategy
- `specs/research.md` - Research findings and technology decisions
- `specs/quickstart.md` - Setup instructions
- `specs/data-model.md` - Data entities, properties, relationships
- `contracts/` folder (optional) - May contain API contract files

---

## 6. **Generate Tasks**
**What it does:** Breaks down the technical plan into specific, actionable implementation tasks. Each task is sized for completion in a few hours to a day, organized by phase/user story, with clear acceptance criteria. Creates your development roadmap.
```
/speckit.tasks
```
**Files to review:**
- `specs/tasks.md` - Actionable implementation tasks
  - ✅ Verify: Tasks organized by phase and user story
  - ✅ Verify: Each requirement maps to one or more tasks
  - ✅ Verify: Tasks are ordered logically (foundation → backend → frontend → testing → deployment)
  - ✅ Verify: Each task is specific and actionable
  - ✅ Verify: Tasks have reasonable scope (few hours to a day)
  - ✅ Verify: Implementation Strategy section defines MVP approach

---

## 7. **Analyze Implementation Plan** ⭐ 
**What it does:** Audits your implementation plan against your specification and constitution to catch issues before coding. Identifies missing requirements, scope creep, conflicts with principles, missing tests, dependency problems, and unaddressed performance/security concerns. Your quality gate before implementation.
```
/speckit.analyze
```
**Files to review:**
- `specs/analysis.md` or audit report in Chat view
  - ✅ Verify: All spec requirements have corresponding tasks
  - ✅ Verify: No critical gaps identified
  - ✅ Verify: Constitution principles are addressed in the plan
  - ✅ Verify: Testing coverage is adequate
  - ✅ Review: Any warnings or recommendations

**Action items:**
- Address any critical gaps by updating `plan.md` or `tasks.md`
- Re-run `/speckit.tasks` if significant changes are needed
- Get stakeholder sign-off that the plan is complete

---

## 8. **Implement MVP**
**What it does:** Executes the implementation plan by generating code for your specified task range. Works incrementally, task by task, following the order in tasks.md. Requests manual testing and assistance as needed. Implements the actual feature with AI-generated code that follows your spec, plan, and constitution.
```
/speckit.implement Implement the MVP first strategy (Tasks: T001 - T045)
```
**Files to review (application code):**
- `ContosoDashboard/Models/` - New entity classes (e.g., Document.cs)
- `ContosoDashboard/Data/ApplicationDbContext.cs` - Updated with new DbSet
- `ContosoDashboard/Services/` - New service classes (e.g., DocumentService.cs)
- `ContosoDashboard/Pages/` - New pages (e.g., Documents.razor, MyDocuments.razor)
- `ContosoDashboard/Shared/` - Updated navigation (NavMenu.razor)
- Database migration files (if applicable)

**Manual testing verification:**
- ✅ Run application: `dotnet run` from ContosoDashboard directory
- ✅ Navigate to http://localhost:5000
- ✅ Login as test user (Ni Kang)
- ✅ Test acceptance scenarios from `specs/spec.md` (User Story 1)
- ✅ Verify upload functionality works
- ✅ Verify documents appear in list
- ✅ Report results back to GitHub Copilot

---

## Bonus Commands

### **Validate Specification Completeness**
**What it does:** Checks your specification against a comprehensive checklist to ensure all necessary sections are complete and nothing important is missing.
```
/speckit.checklist
```

### **Convert Tasks to GitHub Issues**
**What it does:** Automatically creates GitHub issues from your tasks.md file, making it easy to track implementation progress in your repository's issue tracker.
```
/speckit.taskstoissues



git add .
git commit -m "Descriptive message"
git push
```
**Verify on GitHub:** Check your repository to confirm files were pushed successfully

---

## Quick Reference: Command Flow
```
specify init          → Set up infrastructure
/speckit.constitution → Define project rules
/speckit.specify      → What to build (business view)
/speckit.clarify      → Remove ambiguities
/speckit.plan         → How to build (technical view)
/speckit.tasks        → Break into actionable steps
/speckit.analyze      → Quality gate - catch issues
/speckit.implement    → Generate code