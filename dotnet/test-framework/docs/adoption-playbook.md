# Fitness Function Adoption Playbook

Per-service checklist for rolling the framework's fitness functions out to consuming services.
The critical mechanism is ArchUnitNET's two evaluation modes: `Evaluate()` reports violations
without failing (report-only), while `Check()` throws and breaks the build (blocking). Rules are
introduced in report-only mode first, triaged, then promoted to blocking one at a time.

## Phase 1 — Framework onboarding

- [ ] Reference the framework from the internal NuGet feed (Azure Artifacts)
- [ ] Wire `TelemetryChannelDecorator` around the service's channels
- [ ] Confirm CorrelationId propagation appears in Tempo traces

## Phase 2 — Baseline arch rules, report-only

- [ ] Add the shared baseline rules and the service's own ArchTests project
- [ ] Instantiate baseline rules with service-specific namespaces (domain, infrastructure, transport)
- [ ] Run rules via `Evaluate()` (non-blocking) in CI, logging violation counts
- [ ] Record baseline violation count per rule
- [ ] Triage violations: fix now vs. waive
- [ ] For each waived violation, file a ticket with expiry date and apply
      `[Waiver(ticketReference, expiresOn)]` to the corresponding test

**Exit criteria:** every baseline rule has either zero violations or an explicit ticketed, dated waiver.

## Phase 3 — Make baseline rules blocking

- [ ] Confirm Phase 2 exit criteria met for all baseline rules
- [ ] Switch `Evaluate()` calls to `Check()` one rule at a time
- [ ] Sanity-test: confirm CI fails correctly on a deliberately reintroduced violation
- [ ] Communicate to the team that these rules now block merges
- [ ] Document who owns approving changes to these rules (governance)
- [ ] Set a recurring reminder to review waiver expiry dates

**Exit criteria:** all baseline structural rules are merge-blocking in this service's CI.

## Phase 4 — Service-specific & dynamic fitness functions

- [ ] Identify service-specific structural conventions to enforce (naming, handler patterns)
- [ ] Identify critical paths needing latency budgets (using `ChannelExecutionResult.Duration`)
- [ ] Identify resiliency behavior worth asserting (timeout config, circuit breaker behavior)
- [ ] Identify security fitness functions relevant to this service (authz coverage, etc.)
- [ ] Tag all new rules with `[FitnessFunction(category, cadence, owner, rationale)]`

**Exit criteria:** service has at least one fitness function beyond the shared baseline, tagged and cataloged.
