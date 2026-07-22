# ADR-0003: Use TngTech.ArchUnitNET.xUnit (not the deprecated ArchUnitNET.xUnit package)

**Status:** Accepted
**Date:** 2026-07-19

## Context
Two similarly-named NuGet packages exist for ArchUnitNET/xUnit integration; the bare
`ArchUnitNET.xUnit` package is deprecated in favor of the TNG-maintained fork.

## Decision
Depend on `TngTech.ArchUnitNET.xUnit` exclusively, pinned via Central Package Management.

## Consequences
- Avoids depending on an unmaintained package that will not receive fixes for newer .NET/xUnit
  versions.
- API details (e.g. `Architecture` as a `static readonly` field, `Slices()` from `ArchRuleDefinition`,
  `.Because(...)` for documenting rule intent) are specific to this package and must be followed
  exactly — verified against the TNG GitHub repository rather than assumed from memory.

## Alternatives Considered
- `ArchUnitNET.xUnit` (deprecated): rejected outright.
- NetArchTest: viable alternative fluent API, not chosen because ArchUnitNET's `Slices()` cycle
  detection and `IObjectProvider<T>` composability better fit this framework's layering rules.
