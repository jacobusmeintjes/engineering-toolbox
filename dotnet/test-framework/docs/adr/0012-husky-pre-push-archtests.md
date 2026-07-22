# ADR-0012: Husky.Net pre-push hook runs ArchTests before the full suite

**Status:** Accepted
**Date:** 2026-07-19

## Context
Architecture fitness function violations (layering, cycles, forbidden dependencies) are cheap to
detect and should be caught before a developer pushes, not first discovered in CI minutes later
alongside a full functional test run.

## Decision
Use Husky.Net to run only the `EnterpriseTestFramework.ArchTests` project as a pre-push git hook,
mirroring the same fast-first ordering used in the PR pipeline (ArchTests before UnitTests).

## Consequences
- Developers get structural feedback locally, before CI, shortening the feedback loop.
- `ArchTests` must remain fast and free of external infrastructure dependencies to stay viable as
  a pre-push hook — it must not require Aspire-hosted containers to run.
- Requires local Husky.Net installation/setup as part of onboarding.

## Alternatives Considered
- Running the full test suite pre-push: rejected — too slow for a hook that runs on every push,
  and full suite requires real infrastructure not guaranteed to be running locally.
