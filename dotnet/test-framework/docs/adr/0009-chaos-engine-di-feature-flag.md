# ADR-0009: Chaos engineering via DI decorator + feature flag, never inline in business code

**Status:** Accepted
**Date:** 2026-07-19

## Context
Chaos engineering (fault injection) needs to be exercisable in controlled test runs without ever
risking accidental activation in production, and without scattering chaos-specific conditionals
through business or test code.

## Decision
Implement chaos as `IChaosEngine`, registered at startup via dependency injection and gated by
`ChaosOptions.Enabled` (bound through `IOptionsMonitor<ChaosOptions>` for live reload). Production
and any non-whitelisted environment register `NoOpChaosEngine`, a pass-through no-op — a two-key
safeguard (explicit flag + environment gating) against accidental production activation.

## Consequences
- Business/test code depends only on `IChaosEngine`; chaos logic is fully removable by swapping
  the DI registration.
- Failure modes (`ThrowException`, `SlowThenThrow`, `IntermittentFlap`) are configurable per
  operation via JSON, enabling deterministic retry/circuit-breaker testing (e.g. flap every N
  calls).
- `Random.Shared` is used for thread safety under parallel test execution; structured logging uses
  a `[Chaos]` prefix so injected failures are distinguishable from real ones in logs.

## Alternatives Considered
- Network-layer fault injection only (Toxiproxy/service mesh): still valuable for infra-level
  chaos, but does not cover in-process, per-operation scenario control; kept as a complementary
  tool rather than a replacement.
