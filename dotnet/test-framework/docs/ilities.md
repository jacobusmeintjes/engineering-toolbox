# Quality Attributes ("-ilities") — Enterprise Test Framework

This document catalogs the non-functional requirements ("-ilities") relevant to an enterprise
.NET test framework, and states concretely how this solution addresses each one, with a pointer
to the mechanism and the ADR that decided it. Each -ility is also treated as a candidate fitness
function category — several are directly enforced in CI, not just aspired to.

| -ility | Concern | How it's addressed in this framework |
|---|---|---|
| **Testability** | Can the framework's own correctness and the systems it tests be verified cheaply and repeatably? | `ITestChannel<TRequest,TResponse>` gives one uniform seam for assertions across REST and messaging. `IInternalStateProbe` is kept structurally separate so implementation-testing shortcuts don't quietly become the norm (ADR-0007). ArchTests are themselves tests of the framework's testability guarantees. Stryker.NET mutation testing (nightly) verifies the test suite actually kills mutants — gating on mutation score, not line coverage. |
| **Maintainability** | Can the codebase be understood and changed safely as it grows? | Central Package Management (`Directory.Packages.props`, ADR-0005) eliminates version drift. A PR size gate (ADR-0016) keeps changes reviewable — large PRs correlate with shallow reviews. `Directory.Build.props` enforces nullable + warnings-as-errors + analyzers solution-wide. ADRs capture *why*, not just *what*, so future contributors don't need tribal knowledge. |
| **Extensibility** | Can new transports, assertions, or channels be added without reworking existing ones? | New transports implement `ITestChannel` and get telemetry, chaos, and correlation-ID support for free via decorators — no changes to existing channels required. The phased project structure (ADR-0004) defers premature assembly splitting until a real extension trigger (e.g. messaging) arrives. |
| **Observability** | Can behavior, performance, and failures be seen without attaching a debugger? | `TelemetryChannelDecorator` emits an OpenTelemetry `Activity` span per channel execution, tagged with `CorrelationId`, duration, and success/failure. Self-hosted Prometheus/Grafana/Tempo (ADR-0010) receives these, plus NBomber load metrics and even coverage percentages, so fitness functions become live dashboard signals rather than one-off CI gates. |
| **Traceability / Auditability** | Can a specific trace, log line, or dead-lettered message be tied back to the exact test run that produced it? | Mandatory `CorrelationId` propagation through `TestExecutionMetadata`, request headers, OTel spans, and `IMessageAwaiter` matching (ADR-0013) — essential because tests run against shared, real, non-isolated infrastructure (ADR-0006). |
| **Reliability / Resiliency** | Does the framework (and the systems it tests) behave correctly under partial failure? | `IChaosEngine` / `NoOpChaosEngine` (ADR-0009) inject configurable latency and failure modes (`ThrowException`, `SlowThenThrow`, `IntermittentFlap`) via DI + feature flag, letting retry/circuit-breaker behavior be asserted deterministically. `IMessageAwaiter.AwaitDeadLetterAsync` treats DLQ delivery as a first-class assertion rather than an inferred timeout (ADR-0008). |
| **Performance / Efficiency** | Does the system under test meet latency/throughput budgets, and does the test suite itself run in reasonable time? | NBomber load tests (ADR-0011) assert p95/p99 latency and throughput budgets against real infrastructure, exported via OTel. `ChannelExecutionResult.Duration` makes per-call timing available to any test for latency-budget assertions without a separate instrumentation pass. Fast ArchTests run pre-push and first-in-PR (ADR-0012) to keep the cheap check cheap. |
| **Scalability** | Does the framework and the system under test hold up as load, team size, or scope grows? | NBomber ramping/steady-state simulations validate the system under test scales. The phased project structure (ADR-0004) is the framework's own scalability answer for *team* growth — splitting into assemblies only when a second consuming team or messaging scope actually arrives, not speculatively. |
| **Security** | Are secrets, dependencies, and access patterns safe by default? | `nuget-license` compliance gate in the PR pipeline blocks disallowed licenses. Dependency vulnerability scanning (`dotnet list package --vulnerable --include-transitive`) and secrets detection (gitleaks) run as blocking steps in the PR pipeline. Chaos and load test entry points are excluded from production DI registration by construction (`NoOpChaosEngine` default). |
| **Portability** | Can the framework run across environments (local, CI, cloud) without rework? | Real infrastructure runs via .NET Aspire container resources (ADR-0006), so the same broker/API topology used in CI can be run locally. No hard-coded endpoints — configuration is externalized via `Microsoft.Extensions.Configuration` (JSON + environment variables). |
| **Reusability** | Can other teams consume the framework without forking it? | The framework is intended for distribution via an internal NuGet feed (Azure Artifacts). Abstractions (`ITestChannel`, `IChaosEngine`) are transport- and team-agnostic by design; `ArchTests` conventions were built with a future multi-consumer split in mind (ADR-0004). |
| **Configurability** | Can behavior be changed without a code change/redeploy? | `ChaosOptions` bound via `IOptionsMonitor` supports live config reload. Load test target (`TARGET_BASE_URL`) and pipeline schedule are externalized as pipeline variables, not hard-coded. |
| **Usability (Developer Experience)** | How easy is it for a new contributor to write a correct test? | Uniform `ITestChannel` shape across transports means learning one abstraction covers REST and messaging alike. `.Because(...)` annotations on every ArchUnitNET rule document *why* a rule exists directly in the failure output, not just what failed. |
| **Compatibility** | Does the framework work correctly across supported runtime/tooling versions? | `net9.0` targeted uniformly via `Directory.Build.props`; Central Package Management prevents cross-project version skew that would otherwise surface as compatibility bugs only under specific combinations. |
| **Governability / Evolvability** | Can the architecture change deliberately over time without silent regression? | ArchUnitNET rules are structural fitness functions in the evolutionary-architecture sense — atomic checks (single rule) and holistic checks (whole ArchTests suite as a gate), run continuously (every PR) for cheap structural rules and on a triggered cadence (nightly) for expensive ones (load, chaos-enabled resiliency). |

## Cadence model (continuous vs. triggered)

Not every fitness function belongs on every PR. This framework follows the evolutionary
architecture distinction:

- **Continuous** (every PR, via `azure-pipelines-pr.yml`): ArchTests (structural), unit tests,
  license compliance. Cheap, fast, blocking.
- **Triggered** (nightly, via `azure-pipelines-nightly.yml`): full functional regression against
  real infrastructure, NBomber load tests, chaos-enabled resiliency pass. Expensive, informative,
  not merge-blocking on every change.

## Not yet wired (flagged, not fabricated)

The following were discussed as recommended additions but are not yet represented as concrete
files in this scaffold — called out explicitly rather than claimed as implemented:

- Authorization-coverage and health-check-presence fitness functions as ArchUnitNET/custom rules.
- Coverage-as-OTel-metrics exporter (parsing Cobertura XML and emitting gauges) — the pattern was
  designed in conversation but the parsing/emitting utility is not yet a project in this solution.

Previously listed here and now wired: dependency vulnerability scanning
(`dotnet list package --vulnerable --include-transitive`) and secrets detection (gitleaks) both
run as blocking steps in `azure-pipelines-pr.yml`.
