# ADR-0016: PR size gate enforced as a pipeline step

**Status:** Accepted
**Date:** 2026-07-20

## Context
Large pull requests correlate strongly with shallow reviews — beyond a few hundred changed
lines, reviewer defect-detection drops sharply, and for a framework consumed by multiple teams,
review quality is a first-order concern. Azure DevOps branch policies have no native PR size
limit, so if a limit is wanted, it must be enforced in the build validation pipeline itself.

## Decision
Add a PR size gate as the first step of `azure-pipelines-pr.yml`, failing the build if a PR's
total changed lines (additions + deletions, diffed against the target branch) exceed the
`maxPrLines` variable (default 400). Generated and lock files (`*.lock`, `*.Designer.cs`,
`PublicAPI.*.txt`) are excluded so the limit measures reviewable change rather than churn.
The variable can be overridden at queue time for justified exceptions (e.g. a large mechanical
rename), making overrides visible in the run rather than silent.

## Consequences
- Runs before build/restore, so oversized PRs fail in seconds.
- Requires `fetchDepth: 0` on checkout so the target branch is available to diff against.
- The 400-line default is a starting point, not a researched constant for this team — tune it
  against real review behavior. A limit that's constantly overridden is worse than a slightly
  higher limit that's respected.
- Purely mechanical large changes (formatting sweeps, dependency bumps) will need either the
  queue-time override or additional path exclusions as they're identified.

## Alternatives Considered
- Azure DevOps native branch policy: not available for PR size.
- A PR-decorating extension/status API service: heavier to operate than a pipeline step, and the
  pipeline step keeps the rule versioned alongside the code it governs.
- Warning-only (non-blocking): rejected as the default — advisory limits decay into noise; the
  queue-time override provides the escape hatch instead.
