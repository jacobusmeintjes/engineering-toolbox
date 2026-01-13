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

## 7. **Analyze Implementation Plan** ⭐ NEW
```
/speckit.analyze
```
**What it does:**
Audits your implementation plan (`plan.md` and `tasks.md`) against your specification (`spec.md`) and constitution (`constitution.md`) to identify:
- Missing requirements that aren't covered by tasks
- Tasks that don't map to any requirement (scope creep)
- Conflicts between tasks and constitution principles
- Missing quality gates or testing tasks
- Dependencies or ordering issues
- Performance/security considerations not addressed

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
```
/speckit.implement Implement the MVP first strategy (Tasks: T001 - T045)