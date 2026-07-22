# ADR-0013: Correlation IDs are mandatory and threaded through TestExecutionMetadata and OTel spans

**Status:** Accepted
**Date:** 2026-07-19

## Context
Tests run against real, shared, non-isolated infrastructure (ADR-0006). Without a way to identify
which requests/messages belong to which test run, diagnosing failures and matching asynchronous
messages to their originating action is effectively impossible.

## Decision
Every `ITestChannel` execution carries a `CorrelationId` via `TestExecutionMetadata`, generated
per test action and propagated onto outgoing request headers and onto the OpenTelemetry `Activity`
span (via `TelemetryChannelDecorator`). `IMessageAwaiter` matching is keyed on this same
CorrelationId.

## Consequences
- Any trace, log, or dead-letter message can be traced back to the exact test that produced it,
  even in shared environments.
- All channel implementations and the message awaiter must consistently read/propagate this field;
  an implementation that drops it silently breaks traceability without an obvious symptom until
  someone tries to debug a flaky async test.

## Alternatives Considered
- Relying on timestamps/proximity to infer which message belongs to which test: rejected —
  unreliable under any real concurrency, which real infrastructure runs guarantee will occur.
