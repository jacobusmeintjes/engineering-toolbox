<!--
Sync Impact Report - Constitution Update
=========================================
Version Change: (unversioned template) → 1.0.0
Change Type: MAJOR - Initial constitution ratification
Modified Principles: All (initial definition from template)
Added Sections:
  - Core Principles (5 principles defined)
  - Development Quality Standards
  - Governance
Removed Sections: [SECTION_2_NAME], [SECTION_3_NAME] (template placeholders)
Templates Requiring Updates:
  ✅ spec-template.md - Aligned (prioritized user stories, mandatory sections)
  ✅ plan-template.md - Aligned (constitution check gate, NEEDS CLARIFICATION pattern)
  ✅ tasks-template.md - Aligned (user story organization, parallel markers, checklist format)
  ✅ All agent files - Aligned (MUST/SHOULD language, constitution authority)
Follow-up TODOs: None - all placeholders resolved
-->

# ContosoDashboard Spec Kit Constitution

## Core Principles

### I. Specification-Driven Development (NON-NEGOTIABLE)

Every feature MUST begin with a complete specification before implementation:
- Specifications MUST include prioritized user stories with independent test criteria
- Each user story MUST be independently testable and deliverable as an MVP increment
- Specifications MUST define functional requirements, acceptance scenarios, and edge cases
- Implementation MUST NOT begin until specification is approved and clarified

**Rationale**: Spec-driven development ensures shared understanding, reduces rework, enables parallel development, and provides clear success criteria before code is written.

### II. Progressive Clarity Over Premature Precision

Ambiguity MUST be explicitly marked and resolved through dedicated clarification phases:
- Use `[NEEDS CLARIFICATION: specific question]` markers for genuine ambiguities
- Limit clarification markers to maximum 3 per specification (prioritize by impact)
- Clarifications MUST be resolved before technical planning begins
- Make informed assumptions for unspecified details and document them explicitly

**Rationale**: This balances speed with rigor - avoiding analysis paralysis while ensuring critical decisions are surfaced and addressed before they become expensive to change.

### III. Independent User Story Implementation

User stories MUST be designed for independent development, testing, and deployment:
- Each user story MUST have explicit priority ranking (P1, P2, P3, etc.)
- Each user story MUST include "Independent Test" description showing standalone value
- Tasks MUST be organized by user story to enable phased delivery
- Any user story MUST be implementable without requiring other stories to be complete

**Rationale**: Independent stories enable incremental delivery, parallel development, early user feedback, and the ability to stop development at any point with a working product.

### IV. Constitution Authority

The constitution is NON-NEGOTIABLE within implementation scope:
- Constitution violations detected during analysis are automatically CRITICAL severity
- Specs, plans, and tasks MUST be adjusted to comply - not the constitution
- Constitution changes require explicit amendment process outside feature workflows
- All agents MUST validate constitutional compliance before proceeding to next phase

**Rationale**: Consistent principles prevent drift, enable automation, ensure quality standards are maintained, and provide objective criteria for gate decisions.

### V. Token Efficiency and Progressive Disclosure

All workflows MUST optimize for minimal context loading and maximal signal:
- Load only necessary content from artifacts (not entire files)
- Build semantic models internally - do not dump raw content in outputs
- Use progressive disclosure - fetch additional context only when needed
- Limit output sizes (e.g., 50 findings max in analysis reports, summarize overflow)
- Parallelize independent read operations when possible

**Rationale**: Token efficiency reduces cost, improves response time, maintains focus on high-signal information, and enables scaling to larger projects.

## Development Quality Standards

### Mandatory Sections in Specifications

All feature specifications MUST include:
- User Scenarios & Testing with prioritized user stories
- Functional Requirements (testable, with FR-### identifiers)
- Success Criteria (measurable, technology-agnostic)
- Edge Cases
- Assumptions (when reasonable defaults are used)

### Technical Planning Requirements

Implementation plans MUST include:
- Technical Context with specific language versions and dependencies
- Constitution Check section validating compliance with all principles
- Project structure for both documentation and source code
- Research phase (Phase 0) to resolve all `[NEEDS CLARIFICATION]` markers
- Design artifacts phase (Phase 1) including data models, contracts, and quickstart guides

### Task Generation Standards

Task lists MUST follow strict format requirements:
- Checklist format: `- [ ] T### [P] [Story] Description with exact file paths`
- `[P]` marker for parallelizable tasks (different files, no dependencies)
- `[Story]` label (US1, US2, etc.) for all user story implementation tasks
- Phase organization: Setup → Foundational → User Stories (by priority) → Polish
- Each user story phase MUST include independent test criteria

### Quality Gates

Before proceeding to implementation, projects MUST:
- Pass constitution check (all MUST requirements satisfied)
- Resolve all `[NEEDS CLARIFICATION]` markers in specification
- Complete consistency analysis with zero CRITICAL issues
- Validate all checklists are complete (if checklist workflow used)

## Governance

This constitution supersedes all other development practices for the ContosoDashboard Spec Kit framework.

**Amendment Process**:
- Constitution changes MUST be made via explicit `/speckit.constitution` command
- Version MUST increment following semantic versioning (MAJOR.MINOR.PATCH)
- Amendments MUST include Sync Impact Report documenting affected templates and agents
- All dependent artifacts MUST be updated to maintain consistency

**Compliance Verification**:
- The `/speckit.analyze` command validates constitutional compliance
- All agent workflows include constitution check gates
- Constitution violations automatically receive CRITICAL severity
- Complexity or deviations MUST be explicitly justified in writing

**Version History**:
- MAJOR increment: Backward incompatible changes, principle removals or redefinitions
- MINOR increment: New principle/section added or materially expanded guidance
- PATCH increment: Clarifications, wording fixes, non-semantic refinements

**Version**: 1.0.0 | **Ratified**: 2026-01-12 | **Last Amended**: 2026-01-12
