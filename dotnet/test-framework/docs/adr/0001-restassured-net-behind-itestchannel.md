# ADR-0001: Wrap RestAssured.Net behind ITestChannel rather than exposing it as the public DSL

**Status:** Accepted
**Date:** 2026-07-19

## Context
The framework needs an HTTP execution engine for REST verification. RestAssured.Net (basdijkstra/rest-assured-net)
is a mature fluent DSL, but adopting its fluent surface directly as the framework's public API would
couple every consuming test to that specific library's API shape and its synchronous execution model.

## Decision
Use RestAssured.Net v4.10.0 purely as the internal HTTP execution engine inside REST channel
implementations. The public surface consuming teams interact with is `ITestChannel<TRequest, TResponse>`.

## Consequences
- Consuming tests are insulated from a RestAssured.Net major version change or a future swap to a
  different HTTP execution library.
- The same `ITestChannel` shape is shared with messaging channels, letting cross-cutting concerns
  (telemetry, chaos, correlation IDs) be implemented once via decorators instead of per-transport.
- Adds one layer of indirection; teams already fluent in RestAssured.Net syntax must instead learn
  the framework's channel abstraction.

## Alternatives Considered
- Exposing RestAssured.Net's fluent API directly: rejected — locks the framework's public contract
  to a third-party library's API surface.
