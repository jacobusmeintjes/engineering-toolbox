# ADR-0006: Run against real infrastructure (Aspire + containerized brokers), not mocks

**Status:** Accepted
**Date:** 2026-07-19

## Context
The framework verifies both REST and asynchronous messaging behavior. Mocking the broker/API layer
would make tests faster but would not catch real serialization, timing, or broker-specific
behavior (visibility timeouts, DLQ redelivery counts, etc.).

## Decision
Run tests against real infrastructure — REST APIs and message brokers hosted as .NET Aspire
container resources — rather than mocked substitutes.

## Consequences
- Tests catch real integration issues (serialization mismatches, broker timing) that mocks would
  hide.
- Async messaging verification requires a genuine polling primitive (`IMessageAwaiter<TMessage>`)
  with timeout/backoff rather than a synchronous mock return.
- Test run time and CI infrastructure cost increase versus a fully mocked suite; mitigated by
  running the heavier suite on a nightly cadence rather than blocking every PR.

## Alternatives Considered
- Mocked REST/messaging layers: rejected — insufficient fidelity for an enterprise framework
  whose purpose is confidence in real integration behavior.
