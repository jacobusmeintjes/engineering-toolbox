# ADR-0008: IMessageAwaiter is an independent subscriber, not a channel decorator

**Status:** Accepted
**Date:** 2026-07-19

## Context
Asynchronous messaging verification differs fundamentally from synchronous REST verification:
the message confirming an action's effect typically arrives on a different topic/queue than the
one the triggering action published to, against a real broker topology (not an in-process mock).

## Decision
Model `IMessageAwaiter<TMessage>` as an independent, polling-based subscriber component with
timeout, backoff, and correlation ID matching — not as a decorator wrapping the publishing
`ITestChannel`.

## Consequences
- Correctly reflects that the awaiter subscribes to a different destination than the publisher
  writes to.
- Dead-letter-queue verification (`AwaitDeadLetterAsync`) is first-class and explicit, rather than
  inferred from a happy-path timeout — a timeout on the primary awaiter says only "it didn't
  arrive," not "it was dead-lettered."
- Correlation ID propagation through `TestExecutionMetadata` and OpenTelemetry `Activity` spans is
  mandatory, since shared/non-isolated test environments make it otherwise impossible to
  distinguish which message belongs to which test run.

## Alternatives Considered
- Decorating the publishing channel to also await the response: rejected — conflates two distinct
  broker interactions (publish vs. subscribe) into one component and would not generalize to
  fan-out topologies.
