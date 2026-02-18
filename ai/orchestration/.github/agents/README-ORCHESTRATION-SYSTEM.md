# Idea-to-Implementation Orchestration System

A multi-agent system for transforming rough ideas into implementation-ready specifications, then building them.

## Overview

This system provides two orchestration workflows:

1. **Idea → Specification** (Documentation Phase)
2. **Specification → Implementation** (Build Phase)

## System Architecture

```
┌─────────────┐
│ Your Idea   │
└──────┬──────┘
       │
       v
┌─────────────────────────────────────────┐
│  Idea Documentation Orchestrator        │
│  (Transforms ideas into specs)          │
└──────┬──────────────────────────────────┘
       │
       ├──> ConceptClarifier
       │    (Asks questions, clarifies vague ideas)
       │
       ├──> RequirementsElicitor
       │    (Extracts functional & non-functional requirements)
       │
       ├──> ArchitectureDocumenter
       │    (Designs system, creates diagrams)
       │
       ├──> APIDesigner
       │    (Defines interfaces, DTOs, contracts)
       │
       └──> AcceptanceCriteriaWriter
            (Creates testable scenarios)
       │
       v
┌─────────────────────────────────────────┐
│  Implementation Specification           │
│  (Complete, unambiguous, ready to build)│
└──────┬──────────────────────────────────┘
       │
       │ [Handoff Button]
       │
       v
┌─────────────────────────────────────────┐
│  Main Orchestrator                      │
│  (Builds the feature)                   │
└──────┬──────────────────────────────────┘
       │
       ├──> Planner
       ├──> Coder
       ├──> Designer
       └──> Tester
       │
       v
┌─────────────────────────────────────────┐
│  Working Implementation                 │
└─────────────────────────────────────────┘
```

## Installation

### Step 1: Install Agent Files

Place these files in your project:

```
your-project/
├── .github/
│   └── agents/
│       ├── idea-to-spec-orchestrator.agent.md
│       ├── concept-clarifier.agent.md
│       ├── requirements-elicitor.agent.md
│       ├── architecture-documenter.agent.md
│       ├── api-designer.agent.md
│       ├── acceptance-criteria-writer.agent.md
│       ├── orchestrator.agent.md          # Main build orchestrator
│       ├── planner.agent.md
│       ├── coder.agent.md
│       └── designer.agent.md
```

### Step 2: Configure VS Code

1. Open VS Code Insiders
2. Open the Command Palette (`Ctrl+Shift+P` / `Cmd+Shift+P`)
3. Run: `Chat: Reload Custom Agents`
4. Verify agents appear in the agent dropdown

## Usage Workflow

### Phase 1: Document Your Idea

#### Step 1: Start with Raw Idea

Open VS Code Chat and select the **Idea Documentation Orchestrator** agent:

```
@idea-documentation-orchestrator

I want to add circuit breaker protection to our FX pricing service
```

#### Step 2: Answer Clarifying Questions

The orchestrator will call **ConceptClarifier** to ask questions:

```
ConceptClarifier: Let's make sure we're solving the right problem. 
What specific issues are you experiencing that a circuit breaker would help with?
- Unable to detect failures quickly?
- Cascading failures to other services?
- Poor experience during external API outages?
```

Answer these questions honestly - the agent is trying to understand your real needs.

#### Step 3: Review and Approve Requirements

The orchestrator will then call **RequirementsElicitor** which produces:

```markdown
## Functional Requirements
- FR-1: Circuit breaker opens after 5 consecutive failures
- FR-2: Fallback to cached prices when circuit open
- FR-3: Circuit attempts recovery every 30 seconds

## Non-Functional Requirements
- Performance: <5ms circuit breaker overhead
- Availability: System remains partially available with cache
- Security: Cached data follows same auth rules
```

Review this output. If something is wrong, tell the agent to revise.

#### Step 4: Review Architecture

**ArchitectureDocumenter** creates:
- System context diagram
- Component diagram
- Sequence diagrams
- Architecture Decision Records (ADRs)

These are saved to `docs/specifications/[feature-name]/architecture/`

#### Step 5: Review API Contracts

**APIDesigner** creates:
- C# interfaces
- DTOs (requests/responses)
- Error types
- Configuration models

These are saved to `docs/specifications/[feature-name]/api-contracts/`

#### Step 6: Get Complete Specification

The orchestrator assembles everything into:

```
docs/specifications/fx-pricing-circuit-breaker/
├── SPECIFICATION.md                  # Master specification
├── architecture/
│   ├── ARCHITECTURE.md
│   ├── system-context.mmd
│   ├── component-diagram.mmd
│   ├── sequence-diagrams/
│   └── adr/
│       ├── 001-circuit-breaker-library.md
│       └── 002-cache-strategy.md
└── api-contracts/
    ├── interfaces.cs
    ├── dtos.cs
    ├── errors.cs
    └── configuration.cs
```

### Phase 2: Implement the Specification

#### Use the Handoff Button

When the specification is complete, you'll see a handoff button:

```
✅ Specification complete!

[Ready for Implementation] → Start building
```

Click this to automatically transition to the **Main Orchestrator** with the full specification as context.

#### Or Start Manually

```
@orchestrator

Please implement the specification at:
docs/specifications/fx-pricing-circuit-breaker/SPECIFICATION.md
```

The main orchestrator will:
1. Call **Planner** to create implementation steps
2. Call **Coder** in parallel for different components
3. Call **Designer** for any UI work
4. Integrate everything
5. Verify the implementation

## Example: End-to-End

### Your Input

```
@idea-documentation-orchestrator

I want to add health checks to all our Orleans grains
```

### ConceptClarifier Asks

```
Let me understand what you're trying to achieve:

1. What problems are you experiencing that health checks would solve?
   - Can't detect failing grains?
   - Need better Kubernetes integration?
   - Something else?

2. What should these health checks verify?
   - Grain activation state?
   - External dependencies (Redis, database)?
   - Processing capacity?

3. Who/what will consume these health checks?
   - Kubernetes liveness probes?
   - .NET Aspire dashboard?
   - External monitoring system?
```

### Your Answers

```
1. We need Kubernetes liveness probes
2. Check if grain can access Redis and process requests
3. Kubernetes will consume them via HTTP endpoints
```

### Specification Produced

After going through all phases, you get:

```markdown
# Orleans Grain Health Checks - Implementation Specification

## 1. Overview
Implement health check endpoints for Orleans grains that Kubernetes can use
for liveness and readiness probes...

## 2. Requirements
- FR-1: HTTP endpoint at /health/live
- FR-2: Checks Redis connectivity
- FR-3: Returns 200 OK when healthy, 503 when unhealthy
- NFR-1: Health check completes in <100ms

## 3. Architecture
[Component diagram showing HealthCheckGrain, Redis, K8s]

## 4. API Design
```csharp
public interface IHealthCheckGrain : IGrainWithStringKey
{
    Task<HealthCheckResult> CheckHealthAsync();
}
```

## 5. Acceptance Criteria
- GIVEN grain is active and Redis is accessible
  WHEN /health/live is called
  THEN returns 200 OK
  
- GIVEN Redis is unreachable
  WHEN /health/live is called  
  THEN returns 503 Service Unavailable
```

### Implementation (via handoff)

Click [Ready for Implementation] and the main orchestrator:

1. **Planner** creates steps:
   - Implement IHealthCheckGrain
   - Add Redis connectivity check
   - Configure ASP.NET Core health checks
   - Update K8s deployment manifests

2. **Coder** implements in parallel:
   - HealthCheckGrain.cs
   - RedisHealthCheck.cs
   - Startup.cs modifications

3. **Tester** creates:
   - Unit tests for HealthCheckGrain
   - Integration tests with TestContainers
   - Health check endpoint tests

4. **DevOps** updates:
   - deployment.yaml with liveness probe
   - service.yaml with readiness probe

## Agent Roles Reference

### Documentation Phase Agents

| Agent | Purpose | When to Use |
|-------|---------|-------------|
| **Idea Documentation Orchestrator** | Coordinates the entire documentation phase | Always start here with rough ideas |
| **ConceptClarifier** | Asks questions to understand vague ideas | Automatically called by orchestrator |
| **RequirementsElicitor** | Extracts comprehensive requirements | Automatically called by orchestrator |
| **ArchitectureDocumenter** | Designs system architecture | Automatically called by orchestrator |
| **APIDesigner** | Defines interfaces and contracts | Automatically called by orchestrator |
| **AcceptanceCriteriaWriter** | Creates testable scenarios | Automatically called by orchestrator |

### Implementation Phase Agents

| Agent | Purpose | When to Use |
|-------|---------|-------------|
| **Main Orchestrator** | Coordinates implementation | Use handoff or start manually with spec |
| **Planner** | Creates implementation steps | Automatically called by orchestrator |
| **Coder** | Writes production code | Automatically called by orchestrator |
| **Designer** | Handles UI/UX work | Automatically called if needed |
| **Tester** | Creates tests | Automatically called by orchestrator |

## Tips for Best Results

### 1. Start with "What" Not "How"

❌ Bad: "Add a Polly circuit breaker to FXPricingGrain"
✅ Good: "Protect our FX pricing service from external API failures"

Let the agents figure out HOW (they might suggest Polly, or something else).

### 2. Be Honest About Constraints

Tell the agents about:
- Budget limitations
- Timeline requirements
- Team skill levels
- Existing technical debt
- Compliance requirements

### 3. Challenge the Agents

If something doesn't make sense, say so:

```
That architecture seems overengineered. We're only handling 100 req/sec,
do we really need a distributed cache cluster?
```

The agents will adjust.

### 4. Iterate on Requirements

After reviewing requirements, you can ask for changes:

```
@requirements-elicitor

The 99.99% availability requirement is too strict for this service.
We can accept 99.5%. Please update the requirements.
```

### 5. Use Context

The agents have access to your codebase. Tell them to look at existing patterns:

```
@architecture-documenter

We already have circuit breakers in PaymentService. 
Can we follow the same pattern here?
```

## Customization

### Add Your Own Specialist Agents

Create domain-specific agents:

**`fx-domain-expert.agent.md`**
```markdown
---
name: FX Domain Expert
description: Expert in foreign exchange trading systems and regulations
model: Claude Sonnet 4.5 (copilot)
tools:
  - read
  - search
  - context7/*
---

You are an expert in FX trading systems. You understand:
- Trading regulations and compliance
- Market microstructure
- Pricing models
- Risk management

When reviewing specifications, you check for:
- Regulatory compliance (MiFID II, Dodd-Frank)
- Market abuse prevention
- Best execution requirements
- Post-trade reporting obligations
```

Then update the orchestrator to use it:

```markdown
## Specialist Agents
- **ConceptClarifier** - ...
- **RequirementsElicitor** - ...
- **FXDomainExpert** - Validates FX-specific requirements  # NEW
- **ArchitectureDocumenter** - ...
```

### Modify Agent Behavior

Edit the agent files to adjust behavior:

**Example: Make RequirementsElicitor More Detailed**

Edit `requirements-elicitor.agent.md`:

```markdown
## Output Guidelines

- ✅ Every requirement must have 3-5 acceptance criteria (not 1-2)
- ✅ Include performance percentiles: p50, p95, p99, p999
- ✅ Specify exact error messages for each failure mode
```

### Change Model Selection

Agents can use different models:

```markdown
---
name: Architecture Documenter
model: Claude Opus 4.5 (copilot)  # Most capable for complex architecture
---
```

```markdown
---
name: Coder
model: GPT-5.2-Codex (copilot)  # Specialized for code generation
---
```

## Troubleshooting

### Agent Not Appearing in Dropdown

1. Verify file is in `.github/agents/` directory
2. Verify filename ends with `.agent.md`
3. Run: `Chat: Reload Custom Agents` from command palette
4. Check `.agent.md` file has valid YAML frontmatter

### Agent Producing Poor Output

1. **Check context**: Make sure agent has access to codebase
   ```markdown
   tools:
     - read      # Can read files
     - search    # Can search codebase
   ```

2. **Improve instructions**: Be more specific in agent definition
   ```markdown
   ## Critical Rules
   - Always search for existing patterns first
   - Never invent conventions, follow what exists
   ```

3. **Add examples**: Show the agent what good output looks like

### Orchestrator Not Calling Sub-Agents

1. Verify sub-agent names match exactly (case-sensitive)
2. Check that sub-agents are listed in orchestrator's instructions
3. Review orchestrator's execution plan - is it identifying the right phases?

### Specification Too Generic

Tell the agents to be more specific:

```
@requirements-elicitor

These requirements are too vague. For the performance requirement,
specify exact latency targets in milliseconds. For availability,
specify the uptime percentage and what counts as downtime.
```

## Advanced Patterns

### Parallel Specification Development

For large features, develop spec sections in parallel:

```
# Terminal 1
@api-designer
Design the API contracts for the FX pricing circuit breaker

# Terminal 2  
@architecture-documenter
Design the architecture for the FX pricing circuit breaker
```

Then merge results manually.

### Incremental Refinement

1. Get initial spec from orchestrator
2. Review specific sections with specialist agents
3. Ask for revisions
4. Repeat until satisfied

```
@api-designer

I reviewed the API contracts. The PriceResult DTO should include
mid-price calculation and timestamp should be DateTimeOffset not DateTime.
Please revise.
```

### Specification Templates

Create reusable templates:

```markdown
## Our Standard Spec Template

Every specification must include:
1. Problem statement (1 paragraph)
2. Functional requirements (user stories)
3. Non-functional requirements (with metrics)
4. Architecture (C4 diagrams)
5. API contracts (actual C# code)
6. Acceptance criteria (Given-When-Then)
7. Risks and mitigations
```

Tell agents to follow your template:

```
@idea-documentation-orchestrator

Use our standard spec template from docs/templates/SPECIFICATION_TEMPLATE.md
```

## Integration with CI/CD

### Auto-Generate Specs from Issues

GitHub Action:

```yaml
name: Generate Specification
on:
  issues:
    types: [labeled]
    
jobs:
  generate-spec:
    if: github.event.label.name == 'needs-spec'
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Generate Spec
        run: |
          # Call Idea Documentation Orchestrator
          # Save output to docs/specifications/
          # Create PR with spec
```

### Validate Specs Before Implementation

```yaml
name: Validate Specification
on:
  pull_request:
    paths:
      - 'docs/specifications/**'
      
jobs:
  validate:
    runs-on: ubuntu-latest
    steps:
      - name: Check Specification Completeness
        run: |
          # Verify spec has all required sections
          # Check for "TBD" or "TODO" markers
          # Validate API contracts compile
          # Check diagrams are valid Mermaid
```

## Best Practices

1. **One Idea at a Time** - Don't combine multiple features in one request
2. **Start Small** - Begin with a small feature to learn the workflow
3. **Review Incrementally** - Don't wait until the end to review
4. **Challenge Assumptions** - If something seems wrong, say so
5. **Use Version Control** - Commit specs to Git for history
6. **Iterate** - Specs are living documents, update as you learn

## What This System Is NOT

- ❌ Not a replacement for thinking
- ❌ Not a silver bullet (garbage in = garbage out)
- ❌ Not fully autonomous (you still make decisions)
- ❌ Not a substitute for domain expertise

## What This System IS

- ✅ A thought partner for clarifying ideas
- ✅ A documentation accelerator
- ✅ A consistency enforcer
- ✅ A pattern library that writes itself
- ✅ A bridge between "I want..." and "Here's how..."

## Support and Feedback

Found a bug? Agent producing poor output? Have suggestions?

1. Create an issue in your project
2. Share agent outputs (sanitize sensitive data)
3. Describe expected vs actual behavior

## License

These agent definitions are provided as-is. Customize freely for your needs.
