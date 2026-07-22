# ADR-0015: Mutation testing with Stryker.NET, gating on mutation score not line coverage

**Status:** Accepted
**Date:** 2026-07-20

## Context
Line coverage is a weak fitness function for a test framework: 100% line coverage with a zero
mutation kill rate can still pass as "healthy" — the tests execute the code without actually
verifying its behavior. For a framework other teams depend on, the quality signal that matters
is whether the test suite detects behavioral changes, which is what mutation testing measures.

## Decision
Use Stryker.NET (`dotnet-stryker`, installed via the local tool manifest for CI reproducibility)
to mutation-test Core (extending to Rest once it has concrete channel implementations), running
on the nightly pipeline. Configuration lives in `stryker-config.json` with thresholds
high=80 / low=60 / break=0; `break` starts at 0 (informational) and is raised once a baseline
mutation score is established, at which point a score regression fails the nightly build.

## Consequences
- The gate is mutation score, not line coverage — surviving mutants point at exactly which lines
  tests execute but do not verify.
- Nightly cadence (a triggered fitness function) because full mutation runs are slow; incremental
  PR-time runs remain available via `dotnet stryker --since main`, which mutates only code changed
  relative to the baseline branch.
- HTML mutation reports are published as pipeline artifacts for triaging surviving mutants.
- Adds meaningful nightly pipeline duration; accepted in exchange for a genuine test-quality
  signal that coverage percentages cannot provide.

## Alternatives Considered
- Line/branch coverage thresholds alone: rejected as the primary gate — measures execution, not
  verification. Coverage remains published for visibility but does not gate.
- Running full mutation tests on every PR: rejected — too slow; `--since` incremental mode is the
  PR-time option if faster feedback is later wanted.
