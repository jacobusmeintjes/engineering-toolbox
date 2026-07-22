# ADR-0011: Use NBomber (not k6) for load and performance testing

**Status:** Accepted
**Date:** 2026-07-20

## Context
k6 is a strong, widely-used load testing tool, but its scripts are written in JavaScript/TypeScript.
This framework and its consuming teams are entirely C#/.NET. NBomber is a load-testing framework
written in F#/C# with a native .NET API and OpenTelemetry metrics export.

## Decision
Use NBomber for load/performance testing, running as a standalone console entry point
(`EnterpriseTestFramework.LoadTests`) rather than an xUnit test project.

## Consequences
- Load test scenarios are written in C#, reusing the same `HttpClient`/correlation-id conventions
  as the rest of the framework, instead of maintaining a separate JavaScript toolchain.
- OTel export uses the official `NBomber.Sinks.OpenTelemetry` reporting sink (introduced in
  NBomber v6.2.0), configured via `.WithReportingSinks(new OpenTelemetrySink(...))` against an
  OTLP endpoint or Collector — letting load metrics flow into the same Prometheus/Grafana/Tempo
  stack as regular application telemetry (ADR-0010). Note: this is a reporting sink, not a .NET
  Meter — there is no NBomber Meter to subscribe to via the OTel Metrics SDK directly.
- Load tests are long-running and resource-intensive, so they run on the nightly pipeline rather
  than as an xUnit `[Fact]` gating every PR (see ADR-0004's cadence distinction between continuous
  and triggered fitness functions).

## Alternatives Considered
- k6: rejected — introduces a JavaScript toolchain into an otherwise all-C# framework.
- JMeter: rejected — GUI-based and heavier-feeling; poor fit for CI-as-code conventions already
  established (YAML pipelines, C# everywhere else).
