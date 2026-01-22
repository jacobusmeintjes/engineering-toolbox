# GitHub Copilot Skills for .NET Development

This collection contains 7 comprehensive skills designed for expert-level .NET development, architecture, and testing.

## Skills Included

1. **dotnet-expert** - Modern .NET development with C# best practices
2. **code-reviewer** - Expert code review focusing on security, performance, and maintainability
3. **orleans-expert** - Distributed systems with Microsoft Orleans
4. **aws-architect** - AWS cloud architecture and services
5. **aws-cdk-expert** - Infrastructure as Code with AWS CDK (C#)
6. **testing-expert** - Comprehensive testing with xUnit, FluentAssertions, Testcontainers, BDD
7. **performance-expert** - Performance testing with k6 and BenchmarkDotNet

## Installation

### Personal Skills (Recommended)

Install these skills for your personal use across all projects:

```bash
# Create the skills directory if it doesn't exist
mkdir -p ~/.copilot/skills

# Copy each skill directory
cp -r copilot-skills/* ~/.copilot/skills/
```

### Workspace Skills (Project-specific)

Install skills for a specific project:

```bash
# In your project root
mkdir -p .github/skills

# Copy specific skills you need for this project
cp -r copilot-skills/dotnet-expert .github/skills/
cp -r copilot-skills/testing-expert .github/skills/
# ... add more as needed
```

## Directory Structure After Installation

### Personal Installation
```
~/.copilot/skills/
├── dotnet-expert/
│   └── SKILL.md
├── code-reviewer/
│   └── SKILL.md
├── orleans-expert/
│   └── SKILL.md
├── aws-architect/
│   └── SKILL.md
├── aws-cdk-expert/
│   └── SKILL.md
├── testing-expert/
│   └── SKILL.md
└── performance-expert/
    └── SKILL.md
```

### Workspace Installation
```
your-project/
└── .github/
    └── skills/
        ├── dotnet-expert/
        │   └── SKILL.md
        ├── testing-expert/
        │   └── SKILL.md
        └── ...
```

## How Skills Work

### Automatic Activation
Skills are automatically activated by GitHub Copilot based on your prompts. You don't need to manually select them.

**Example:**
- When you ask: "How do I implement a grain in Orleans?"
  - The `orleans-expert` skill automatically loads
  - Copilot provides expert Orleans guidance

- When you ask: "Create a unit test for this service"
  - The `testing-expert` skill automatically loads
  - Copilot generates xUnit tests with FluentAssertions

### How It Works
1. **Discovery**: Copilot reads the name and description from YAML frontmatter
2. **Relevance Check**: Matches your prompt against skill descriptions
3. **Loading**: Loads the full SKILL.md content when relevant
4. **Response**: Provides expert guidance based on the skill

## Skill Descriptions

### 1. dotnet-expert
**Use when:** Writing or refactoring C# code, building ASP.NET Core apps, implementing DI patterns, working with async/await, designing microservices.

**Covers:**
- Modern C# practices (nullable reference types, pattern matching, primary constructors)
- Async/await best practices
- Dependency injection patterns
- ASP.NET Core APIs (minimal APIs, controllers)
- Entity Framework Core
- Error handling and resilience

### 2. code-reviewer
**Use when:** Reviewing pull requests, identifying security issues, checking performance, evaluating maintainability.

**Covers:**
- Security vulnerabilities (SQL injection, XSS, secrets management)
- Performance issues (N+1 queries, memory leaks, string concatenation)
- Code smells and anti-patterns
- SOLID principles violations
- Best practices enforcement

### 3. orleans-expert
**Use when:** Building distributed systems, implementing virtual actors, working with grain state, clustering, streaming.

**Covers:**
- Grain design patterns
- State management and persistence
- Orleans streaming
- Timers and reminders
- Cluster configuration
- Production deployment (Azure, AWS, K8s)
- Event sourcing and saga patterns

### 4. aws-architect
**Use when:** Designing AWS architectures, selecting services, implementing HA/DR, optimizing costs.

**Covers:**
- Compute services (EC2, ECS, Lambda, App Runner)
- Database services (RDS, DynamoDB, ElastiCache)
- Storage (S3, EFS, EBS)
- Networking (VPC, ALB, NLB, API Gateway)
- Security (IAM, encryption, Secrets Manager)
- High availability and disaster recovery
- Cost optimization

### 5. aws-cdk-expert
**Use when:** Writing infrastructure as code with AWS CDK in C#, designing reusable constructs, testing CDK stacks.

**Covers:**
- CDK project structure and organization
- Stack composition and cross-stack references
- Custom construct patterns
- Testing CDK infrastructure
- CI/CD pipelines with CDK
- Best practices for production deployments

### 6. testing-expert
**Use when:** Writing tests, creating integration tests, implementing BDD scenarios, using test doubles.

**Covers:**
- xUnit fundamentals (Facts, Theories, Fixtures)
- FluentAssertions for readable assertions
- Testcontainers for integration tests (PostgreSQL, Redis, RabbitMQ)
- BDD with SpecFlow
- Mocking with NSubstitute
- Test patterns and best practices

### 7. performance-expert
**Use when:** Benchmarking code, load testing APIs, profiling applications, optimizing performance.

**Covers:**
- BenchmarkDotNet for microbenchmarks
- k6 for load testing (spike, stress, soak tests)
- Performance optimization patterns
- Profiling tools (dotnet-trace, dotnet-counters, PerfView)
- Memory allocation optimization
- Async/await performance

## Usage Examples

### Example 1: Building an Orleans Application
```
You: "I need to create an Orleans grain that manages customer orders with persistence"

Copilot: [Loads orleans-expert skill]
         [Provides grain implementation with state persistence]
```

### Example 2: Code Review
```
You: "Review this code for security issues"

Copilot: [Loads code-reviewer skill]
         [Identifies SQL injection, missing authorization, etc.]
```

### Example 3: Performance Testing
```
You: "Create a k6 load test for my API that simulates 100 concurrent users"

Copilot: [Loads performance-expert skill]
         [Generates k6 script with stages and checks]
```

### Example 4: AWS Infrastructure
```
You: "Design a highly available architecture for a .NET microservices application on AWS"

Copilot: [Loads aws-architect skill]
         [Provides architecture with ECS, RDS Multi-AZ, ALB, etc.]
```

## Combining Skills

Skills can work together naturally:

```
You: "Create a CDK stack for an Orleans cluster on AWS with RDS persistence"

Copilot: [Loads aws-cdk-expert AND orleans-expert skills]
         [Provides CDK code that creates Orleans infrastructure]
```

## Tips for Best Results

1. **Be Specific**: The more specific your prompt, the better Copilot can help
   - Good: "Create a unit test for OrderService.CreateOrderAsync with FluentAssertions"
   - Better: "Create a unit test that verifies OrderService throws ValidationException when customer ID is empty"

2. **Reference Technologies**: Mention specific tools/frameworks in your prompts
   - "Use xUnit and FluentAssertions"
   - "Deploy to AWS ECS Fargate"
   - "Implement with Orleans grains"

3. **Ask for Reviews**: Use the code-reviewer skill proactively
   - "Review this code for performance issues"
   - "Check this for security vulnerabilities"

4. **Request Patterns**: Ask for design patterns and best practices
   - "What's the best way to implement retries in Orleans?"
   - "Show me the saga pattern for distributed transactions"

## Verifying Installation

To verify skills are installed correctly:

1. Check the directory exists:
   ```bash
   ls -la ~/.copilot/skills
   # or
   ls -la .github/skills
   ```

2. Each skill should have a SKILL.md file:
   ```bash
   ls -la ~/.copilot/skills/*/SKILL.md
   ```

3. Test with Copilot:
   - Open any .NET project in VS Code
   - Ask Copilot: "How do I create an Orleans grain?"
   - If the skill is loaded, you'll get detailed Orleans guidance

## Troubleshooting

### Skills Not Loading
- Verify SKILL.md files are in the correct directories
- Restart VS Code
- Check that file permissions allow reading
- Ensure YAML frontmatter is valid

### Unexpected Behavior
- Skills load based on prompt relevance
- Try being more specific in your prompts
- Reference the technology explicitly ("using Orleans", "with xUnit")

## Updating Skills

To update skills:
1. Download updated skill files
2. Replace the existing SKILL.md files
3. Restart VS Code

## Contributing

These skills are based on best practices and real-world experience. Feel free to customize them for your team's specific needs by editing the SKILL.md files.

## Resources

- [GitHub Copilot Documentation](https://docs.github.com/en/copilot)
- [VS Code Copilot Skills](https://code.visualstudio.com/docs/copilot/customization/agent-skills)
- [awesome-copilot Repository](https://github.com/github/awesome-copilot)

## License

These skills are provided as examples and templates for your use.

---

**Created for:** Enterprise .NET development with focus on distributed systems, cloud-native applications, and comprehensive testing.

**Tech Stack:** .NET 8+, C#, ASP.NET Core, Orleans, AWS, xUnit, FluentAssertions, Testcontainers, BenchmarkDotNet, k6
