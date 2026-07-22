# ADR-0007: IInternalStateProbe is a distinct interface, deliberately not part of ITestChannel

**Status:** Accepted
**Date:** 2026-07-19

## Context
Verification should default to going through the application under test (its REST/messaging API),
not directly against a database or cache — direct storage assertions test implementation rather
than contract, and can produce false results against intentionally eventually-consistent read
models. However, narrow legitimate exceptions exist: fixture seeding, inspecting internal side
effects with no API surface (outbox tables, audit logs, DLQ internals), and post-failure diagnosis.

## Decision
Introduce `IInternalStateProbe<TState>` as a interface that does not implement
`ITestChannel<TRequest, TResponse>`, and add an ArchUnitNET rule (`InternalStateProbe_MustNotImplementITestChannel`)
forbidding any class from implementing both.

## Consequences
- Direct storage access remains possible for the narrow legitimate cases, but is structurally
  prevented from being composed into ordinary black-box assertion chains.
- Every use of an internal probe is a conscious, auditable decision rather than an accidental
  convenience.
- A generic "database channel" peer to REST/Messaging was explicitly rejected to preserve this
  separation.

## Alternatives Considered
- A generic `IDatabaseChannel` implementing `ITestChannel`: rejected — would normalize
  implementation-testing as equivalent to contract-testing.
