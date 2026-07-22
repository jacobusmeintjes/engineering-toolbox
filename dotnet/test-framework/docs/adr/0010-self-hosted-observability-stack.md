# ADR-0010: Self-hosted Prometheus/Grafana/Tempo as Aspire container resources

**Status:** Accepted
**Date:** 2026-07-19

## Context
The framework needs an observability backend for OpenTelemetry traces and metrics. An existing
Prometheus/Grafana/Tempo stack is already used by the "Trade" system this framework's patterns
are benchmarked against.

## Decision
Run Prometheus, Grafana, and Tempo as .NET Aspire container resources for local/CI development,
matching the existing production observability stack rather than adopting a cloud-managed
alternative.

## Consequences
- Consistency with the existing Trade stack reduces the number of distinct observability tools
  the team must operate.
- Test coverage percentages and NBomber load metrics can be pushed through the same OTel pipeline
  as regular traces, turning fitness functions into continuously observable dashboard data rather
  than one-off CI pass/fail gates.
- Self-hosting requires the team to maintain the Prometheus/Tempo configuration (retention,
  storage) rather than delegating that to a managed vendor.

## Alternatives Considered
- Cloud-managed observability (e.g. a SaaS APM): rejected to stay consistent with the existing
  Trade stack and avoid operating two different observability approaches.
