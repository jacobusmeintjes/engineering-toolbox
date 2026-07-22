# ADR-0004: Phased project structure — start with two projects, split on a real trigger

**Status:** Accepted
**Date:** 2026-07-19

## Context
Two structures were evaluated: a minimal two-project shape (Core class library + Tests project)
versus a granular five-project shape (Core, Rest, Messaging, Telemetry, ArchTests as separate
assemblies). Committing to five assemblies before there is a second consumer or a messaging
requirement risks premature structural complexity; committing to two forever risks unclear
boundaries once messaging and a second consuming team arrive.

## Decision
Start lean with a small number of projects, but enforce OpenTelemetry instrumentation and
ArchUnitNET rules from day zero using folder-level namespace conventions rather than project-level
assembly boundaries. Split into separate assemblies only when one of these triggers occurs:
messaging work begins, a second team consumes the framework, or package-level (NuGet) versioning
independent of Core is needed.

## Consequences
- Avoids paying the coordination cost of multiple assemblies before there's a concrete reason.
- ArchUnitNET rules written against namespaces (not assembly references) continue to work
  unchanged across the split, since they were designed with this migration in mind.
- Requires discipline to actually execute the split when a trigger fires, rather than let folder
  conventions calcify into permanent structure by default.

## Alternatives Considered
- Five assemblies from day one: rejected as premature for a framework with a single initial
  consumer and no messaging requirement yet.
- Two assemblies permanently: rejected — REST/messaging/telemetry have different release cadences
  once a second consumer exists.
