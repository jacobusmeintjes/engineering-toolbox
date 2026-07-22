# ADR-0014: Phased fitness function rollout — Evaluate() report-only before Check() blocking

**Status:** Accepted
**Date:** 2026-07-20

## Context
Consuming services adopting the framework's baseline ArchUnitNET rules will typically have
pre-existing violations. Turning rules on as immediately build-breaking would either block all
work or pressure teams to weaken the rules; silently ignoring violations defeats the purpose.

## Decision
Roll out fitness functions per-service in phases: run baseline rules via ArchUnitNET's
`Evaluate()` (report-only) first, triage every violation into fix-now or an explicit
`[Waiver(ticket, expiryDate)]`, and only then promote rules one at a time to `Check()`
(build-breaking). Every fitness function is tagged with
`[FitnessFunction(category, cadence, owner, rationale)]` so the catalog is mechanically
discoverable.

## Consequences
- Adoption never blocks a team on day one, but every known violation is ticketed and dated
  rather than invisible.
- Expired waivers surface during a recurring review rather than living forever.
- Requires governance: someone must own rule-change approval and the waiver review cadence
  (documented per-service in Phase 3 of the adoption playbook).

## Alternatives Considered
- Blocking from day one: rejected — punishes existing services for history, incentivizes rule
  weakening.
- Permanent report-only mode: rejected — violations without consequences accumulate and the
  fitness function decays into noise.
