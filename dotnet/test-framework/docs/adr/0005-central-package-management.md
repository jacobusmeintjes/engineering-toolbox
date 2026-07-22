# ADR-0005: Central Package Management via Directory.Packages.props

**Status:** Accepted
**Date:** 2026-07-19

## Context
Multiple projects (Core, Rest, Messaging, ArchTests, UnitTests, LoadTests) must reference shared
packages (xunit, OpenTelemetry, analyzers). Per-project version pinning risks silent drift — e.g.
`.Tests` and `.ArchTests` landing on different `xunit` versions and breaking the Husky pre-push hook
without an obvious cause.

## Decision
Adopt .NET Central Package Management: a single `Directory.Packages.props` at the solution root
holds every `PackageVersion`; individual `.csproj` files reference packages without version
attributes.

## Consequences
- One place to bump a dependency version across the whole solution.
- `Directory.Build.props` (shared compiler settings: nullable, warnings-as-errors,
  PublicApiAnalyzers) complements this for non-package settings.
- Adding a new package version requires editing the root file even for a single-project need,
  which is a minor friction accepted in exchange for consistency.

## Alternatives Considered
- Per-project `Version="..."` attributes: rejected due to drift risk across six+ projects.
