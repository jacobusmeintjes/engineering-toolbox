# ADR-0002: Use xUnit as the test runner

**Status:** Accepted
**Date:** 2026-07-19

## Context
A test runner is needed for both functional tests and architecture fitness function tests
(ArchUnitNET). TngTech's ArchUnitNET has first-class xUnit integration via `TngTech.ArchUnitNET.xUnit`.

## Decision
Standardize on xUnit across UnitTests and ArchTests projects.

## Consequences
- Direct compatibility with `TngTech.ArchUnitNET.xUnit`, avoiding an adapter layer.
- Consistent `dotnet test` invocation and TRX/coverage output across all test projects in CI.
- Team must standardize on xUnit conventions (`[Fact]`/`[Theory]`) rather than NUnit/MSTest idioms.

## Alternatives Considered
- NUnit: viable, but would require a separate ArchUnitNET adapter and loses the direct xUnit
  integration package.
