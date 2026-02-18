#!/usr/bin/env python3
"""
Batch create Azure DevOps tasks for OpenLifter Blazor migration.
Uses the scrum-master skill to create detailed tasks with acceptance criteria.
"""

import subprocess
import json
import sys

# Epic IDs from Azure DevOps
EPICS = {
    "foundation": 407,
    "auth": 408,
    "database": 411,
    "meet_mgmt": 412,
    "registration": 413,
    "live_scoring": 414,
    "results": 415,
    "public_site": 416,
    "import": 417,
    "testing": 418
}

# Define all tasks with detailed descriptions
TASKS = [
    # EPIC 1: Foundation & Infrastructure (407)
    {
        "title": "TASK-001: Create .NET Aspire Solution Structure",
        "epic": "foundation",
        "effort": 4,
        "priority": 1,
        "description": """Create complete .NET 10 Aspire solution with all projects.

WHAT TO BUILD:
- .NET Aspire AppHost for local orchestration
- ServiceDefaults project for shared configuration
- API project (ASP.NET Core Web API)
- AdminWeb project (Blazor Server)
- PublicWeb project (Blazor Web App)
- Core project (domain models, interfaces)
- Infrastructure project (EF Core, repositories)
- Test projects (Unit, E2E)

ACCEPTANCE CRITERIA:
- Solution builds without errors
- All projects target .NET 10
- Aspire AppHost can launch all services
- Service discovery configured
- .editorconfig and .gitignore configured
- NuGet package references correct

FILES TO CREATE:
- OpenLifter.sln
- src/OpenLifter.AppHost/Program.cs
- src/OpenLifter.ServiceDefaults/Extensions.cs
- src/OpenLifter.API/Program.cs
- src/OpenLifter.AdminWeb/Program.cs
- src/OpenLifter.PublicWeb/Program.cs
- src/OpenLifter.Core/*.csproj
- src/OpenLifter.Infrastructure/*.csproj

AGENT: APIDev
SPEC: 02-technology-stack.md Section 2"""
    },
    {
        "title": "TASK-002: Setup CI/CD Pipeline with GitHub Actions",
        "epic": "foundation",
        "effort": 6,
        "priority": 1,
        "description": """Configure GitHub Actions workflows for build, test, and deployment.

WHAT TO BUILD:
- Build and test workflow (on push to main)
- Code coverage reporting (minimum 70%)
- Azure App Service deployment pipeline
- Database migration automation
- Environment-specific configurations (dev/staging/prod)

ACCEPTANCE CRITERIA:
- Workflow runs on every push to main
- All tests execute and must pass
- Code coverage report generated
- Deployment to Azure dev environment succeeds
- Database migrations apply automatically
- Secrets managed via GitHub Secrets

FILES TO CREATE:
- .github/workflows/build-and-test.yml
- .github/workflows/deploy-api.yml
- .github/workflows/deploy-admin.yml
- .github/workflows/deploy-public.yml

AGENT: APIDev
SPEC: 02-technology-stack.md Section 9"""
    },
    {
        "title": "TASK-003: Provision Azure Infrastructure with Bicep",
        "epic": "foundation",
        "effort": 8,
        "priority": 1,
        "description": """Create Infrastructure as Code for Azure resources.

WHAT TO BUILD:
- Bicep templates for all Azure resources
- Resource Groups (dev, staging, prod)
- App Service Plans (Linux, P1V3)
- App Services (API, AdminWeb)
- PostgreSQL Flexible Server (D2s v3)
- Azure Cache for Redis (Standard C1)
- Azure SignalR Service (Standard S1)
- Blob Storage Account
- Key Vault
- Application Insights

ACCEPTANCE CRITERIA:
- Bicep templates deploy without errors
- All resources created in Azure
- Managed Identity configured for Key Vault
- Connection strings stored in Key Vault
- Application Insights instrumentation configured
- Resource tags applied (environment, project, cost-center)

FILES TO CREATE:
- infrastructure/main.bicep
- infrastructure/parameters.dev.json
- infrastructure/parameters.staging.json
- infrastructure/parameters.prod.json

AGENT: APIDev
SPEC: 03-architecture.md Section 5"""
    },
    {
        "title": "TASK-004: Configure Logging and Monitoring",
        "epic": "foundation",
        "effort": 4,
        "priority": 1,
        "description": """Setup Serilog and Application Insights for observability.

WHAT TO BUILD:
- Serilog configuration with multiple sinks
- Application Insights integration
- Structured logging with properties
- Custom telemetry for key metrics
- Exception tracking and alerting
- Performance monitoring dashboards

ACCEPTANCE CRITERIA:
- Logs appear in Application Insights within 60 seconds
- Structured logging includes UserId, MeetId, etc.
- Exception telemetry includes stack traces
- Custom metrics tracked (API response times, SignalR connections)
- Alert rules configured in Azure Monitor
- Log levels configurable per environment

FILES TO CREATE/MODIFY:
- src/OpenLifter.API/appsettings.json
- src/OpenLifter.API/Program.cs
- src/OpenLifter.Core/Telemetry/TelemetryConfig.cs

AGENT: APIDev
SPEC: 02-technology-stack.md Section 10"""
    },
    
    # EPIC 2: Authentication & User Management (408)
    {
        "title": "TASK-005: Implement ASP.NET Core Identity",
        "epic": "auth",
        "effort": 6,
        "priority": 1,
        "description": """Configure ASP.NET Core Identity with custom user model.

WHAT TO BUILD:
- ApplicationUser extending IdentityUser
- Custom claims (FederationId, ApprovalStatus)
- Password requirements (min 8 chars, complexity)
- Email confirmation workflow
- Account lockout (5 failed attempts, 15 min lockout)

ACCEPTANCE CRITERIA:
- Users can register with email/password
- Email confirmation required before login
- Password hashing uses BCrypt (12 rounds)
- Account locked after 5 failed attempts
- Session timeout: 8 hours active, 30 minutes idle
- Remember Me stores token for 30 days

FILES TO CREATE:
- src/OpenLifter.Infrastructure/Identity/ApplicationUser.cs
- src/OpenLifter.API/Controllers/AuthController.cs
- src/OpenLifter.Infrastructure/Data/ApplicationDbContext.cs

AGENT: APIDev
SPEC: 01-requirements.md FR-AUTH-001 to FR-AUTH-007"""
    },
    {
        "title": "TASK-006: Implement JWT Authentication",
        "epic": "auth",
        "effort": 6,
        "priority": 1,
        "description": """Build JWT-based authentication for API access.

WHAT TO BUILD:
- JWT token generation (15-minute expiry)
- Refresh token rotation (7-day expiry)
- Token validation middleware
- Signing key from Azure Key Vault
- Claims mapping (userId, roles, email)

ACCEPTANCE CRITERIA:
- Login returns access and refresh tokens
- JWT validates issuer, audience, signing key
- Expired token returns 401 Unauthorized
- Refresh endpoint generates new access token
- Logout revokes refresh token
- Tokens stored securely (httpOnly cookies or secure storage)

FILES TO CREATE:
- src/OpenLifter.API/Services/TokenService.cs
- src/OpenLifter.API/DTOs/Auth/LoginResponse.cs
- src/OpenLifter.API/Middleware/JwtMiddleware.cs

AGENT: APIDev
SPEC: 06-acceptance-criteria.md Authentication scenarios"""
    },
    {
        "title": "TASK-007: Configure Role-Based Authorization",
        "epic": "auth",
        "effort": 5,
        "priority": 1,
        "description": """Setup authorization policies for 5 user roles.

WHAT TO BUILD:
- PlatformAdmin role (full system access)
- FederationAdmin role (manage federation)
- MeetDirector role (manage meets)
- Athlete role (register, view results)
- Spectator role (view-only access)
- Policy-based authorization requirements

ACCEPTANCE CRITERIA:
- All 5 roles registered in Identity
- Authorization policies configured
- Role assignment during registration
- PlatformAdmin can access all features
- MeetDirector can only manage own meets
- Athletes can only register for meets
- Spectators have read-only access
- Unauthorized access returns 403 Forbidden

FILES TO CREATE/MODIFY:
- src/OpenLifter.API/Authorization/Policies.cs
- src/OpenLifter.API/Authorization/MeetOwnerRequirement.cs
- src/OpenLifter.API/Program.cs

AGENT: APIDev
SPEC: 03-architecture.md Section 4.1"""
    },
    
    # EPIC 3: Database & Data Layer (411)
    {
        "title": "TASK-008: Create EF Core Entity Models",
        "epic": "database",
        "effort": 8,
        "priority": 1,
        "description": """Build all EF Core entities for domain model.

WHAT TO BUILD:
- User entity (extends ApplicationUser)
- Federation entity
- Meet entity (name, date, venue, status, etc.)
- Entry entity (athlete registration)
- Attempt entity (lift attempts with weight, status)
- Platform entity
- Flight entity
- Division and WeightClass entities
- Base entities (IEntity, IAuditable, ISoftDeletable)

ACCEPTANCE CRITERIA:
- All entities inherit from base interfaces
- Navigation properties configured
- Audit fields (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
- Soft delete support (IsDeleted, DeletedAt)
- Proper indexes on foreign keys
- Validation attributes on required fields

FILES TO CREATE:
- src/OpenLifter.Domain/Entities/*.cs
- src/OpenLifter.Contracts/Common/IEntity.cs
- src/OpenLifter.Contracts/Common/IAuditable.cs

AGENT: APIDev
SPEC: 04-api-design.md Section 2 (Entities)"""
    },
    {
        "title": "TASK-009: Configure ApplicationDbContext and Migrations",
        "epic": "database",
        "effort": 6,
        "priority": 1,
        "description": """Setup EF Core DbContext and initial migration.

WHAT TO BUILD:
- ApplicationDbContext with DbSets
- Entity configurations (Fluent API)
- Database migrations
- Seed data for development
- Connection string configuration

ACCEPTANCE CRITERIA:
- DbContext registered in DI
- All entities have DbSets
- Fluent configurations for relationships
- Initial migration creates all tables
- Seed data includes test federation and users
- Connection string from Azure Key Vault in production
- Migration runs automatically on deployment

FILES TO CREATE:
- src/OpenLifter.Infrastructure/Data/ApplicationDbContext.cs
- src/OpenLifter.Infrastructure/Data/Configurations/*.cs
- src/OpenLifter.Infrastructure/Data/Migrations/Initial.cs

AGENT: APIDev
SPEC: 02-technology-stack.md Section 3 (Database)"""
    },
    {
        "title": "TASK-010: Implement Repository Pattern",        "epic": "database",
        "effort": 5,
        "priority": 1,
        "description": """Build generic repository and unit of work pattern.

WHAT TO BUILD:
- IRepository<T> interface
- Generic Repository<T> implementation
- IUnitOfWork interface
- UnitOfWork implementation
- Specialized repositories (IMeetRepository, IEntryRepository)

ACCEPTANCE CRITERIA:
- Generic CRUD operations (Add, Update, Delete, GetById, GetAll)
- Async methods with CancellationToken
- IQueryable support for complex queries
- Unit of Work coordinates transactions
- Repositories registered in DI
- Transaction rollback on failure

FILES TO CREATE:
- src/OpenLifter.Infrastructure/Repositories/IRepository.cs
- src/OpenLifter.Infrastructure/Repositories/Repository.cs
- src/OpenLifter.Infrastructure/Repositories/IUnitOfWork.cs
- src/OpenLifter.Infrastructure/Repositories/UnitOfWork.cs

AGENT: APIDev
SPEC: 03-architecture.md Section 2.4"""
    },
    
    # EPIC 4: Meet Management (412)
    {
        "title": "TASK-011: Build Meet CRUD API Endpoints",
        "epic": "meet_mgmt",
        "effort": 8,
        "priority": 1,
        "description": """Implement REST API endpoints for meet management.

WHAT TO BUILD:
- POST /api/v1/meets (Create meet)
- GET /api/v1/meets/{id} (Get meet details)
- PUT /api/v1/meets/{id} (Update meet)
- DELETE /api/v1/meets/{id} (Delete meet)
- GET /api/v1/meets (List meets with filtering)
- MediatR commands and handlers
- FluentValidation validators

ACCEPTANCE CRITERIA:
- POST creates meet and returns 201 with MeetDto
- GET returns 200 with MeetDto or 404
- PUT updates and returns 200 or 404
- DELETE returns 204 or 404
- GET list supports filtering by date, status, federation
- Validation: name required, date in future, venue max 200 chars
- Authorization: Only MeetDirector or FederationAdmin
- Error responses use ProblemDetails (RFC 7807)

FILES TO CREATE:
- src/OpenLifter.Application/Features/Meets/Commands/CreateMeet/*
- src/OpenLifter.Application/Features/Meets/Commands/UpdateMeet/*
- src/OpenLifter.Application/Features/Meets/Queries/GetMeet/*
- src/OpenLifter.API/Endpoints/MeetsEndpoints.cs

AGENT: APIDev
SPEC: 04-api-design.md Section 4.2 (Meet Endpoints)"""
    },
    {
        "title": "TASK-012: Build Meet Creation Wizard UI",
        "epic": "meet_mgmt",
        "effort": 8,
        "priority": 1,
        "description": """Create 7-step wizard for meet creation in Admin portal.

WHAT TO BUILD:
- Step 1: Basic Info (name, location, federation)
- Step 2: Dates & Platforms
- Step 3: Rules & Formulas
- Step 4: Divisions
- Step 5: Weight Classes
- Step 6: Equipment & Plates
- Step 7: Review & Publish
- Auto-save draft every 30 seconds
- Progress indicator at top

ACCEPTANCE CRITERIA:
- 7 steps displayed with progress bar
- Can navigate back/forward between steps
- Draft auto-saves to localStorage every 30s
- Validation on each step before advancing
- Final review shows summary of all data
- Save Draft saves to database without publishing
- Publish changes status to Published
- Responsive design (mobile, tablet, desktop)

FILES TO CREATE:
- src/OpenLifter.AdminWeb/Pages/Meets/CreateMeet.razor
- src/OpenLifter.AdminWeb/Components/MeetWizard/*.razor
- src/OpenLifter.AdminWeb/Services/MeetService.cs

AGENT: BlazorDev
SPEC: 05-ui-ux-design.md Create/Edit Meet Page"""
    },
    {
        "title": "TASK-013: Style Meet Management Pages",
        "epic": "meet_mgmt",
        "effort": 4,
        "priority": 2,
        "description": """Create CSS for meet management UI components.

WHAT TO BUILD:
- Wizard progress indicator styling
- Form input styles with validation states
- Responsive layout for 7 wizard steps
- Button states (Save Draft, Continue, Publish)
- Loading indicators for auto-save
- Error message styling

ACCEPTANCE CRITERIA:
- Progress bar shows current step
- Active step highlighted
- Completed steps marked with checkmark
- Validation errors displayed in red
- Success message displayed in green
- Mobile-friendly (stacks vertically on small screens)
- WCAG 2.1 AA compliant color contrast

FILES TO CREATE:
- src/OpenLifter.AdminWeb/Pages/Meets/CreateMeet.razor.css
- src/OpenLifter.AdminWeb/Components/MeetWizard/WizardStep.razor.css
- src/OpenLifter.AdminWeb/wwwroot/css/meet-management.css

AGENT: UIDev
SPEC: 05-ui-ux-design.md Design System"""
    },
]

def create_task(task):
    """Create a single task in Azure DevOps."""
    cmd = [
        "python3",
        ".github/skills/scrum-master/azdo_workitems.py",
        "create",
        "--title", task["title"],
        "--type", "Task",
        "--description", task["description"],
        "--effort", str(task["effort"]),
        "--priority", str(task["priority"]),
        "--parent-id", str(EPICS[task["epic"]])
    ]
    
    try:
        result = subprocess.run(cmd, capture_output=True, text=True, check=True)
        data = json.loads(result.stdout)
        print(f"✅ Created: {data['title']} (ID: {data['id']})")
        print(f"   URL: {data['url']}")
        return data
    except subprocess.CalledProcessError as e:
        print(f"❌ Failed to create: {task['title']}")
        print(f"   Error: {e.stderr}")
        return None
    except json.JSONDecodeError as e:
        print(f"❌ Failed to parse response for: {task['title']}")
        print(f"   Output: {result.stdout if 'result' in locals() else 'N/A'}")
        return None

def main():
    """Create all tasks."""
    print("=" * 80)
    print("Creating Azure DevOps Tasks for OpenLifter Blazor Migration")
    print("=" * 80)
    print(f"\nTotal tasks to create: {len(TASKS)}\n")
    
    created = []
    failed = []
    
    for i, task in enumerate(TASKS, 1):
        print(f"\n[{i}/{len(TASKS)}] Creating {task['title']}...")
        result = create_task(task)
        if result:
            created.append(result)
        else:
            failed.append(task)
    
    print("\n" + "=" * 80)
    print(f"SUMMARY")
    print("=" * 80)
    print(f"✅ Successfully created: {len(created)} tasks")
    print(f"❌ Failed: {len(failed)} tasks")
    
    if created:
        print(f"\n📋 Created Tasks:")
        for task in created:
            print(f"  - {task['title']} (ID: {task['id']})")
    
    if failed:
        print(f"\n⚠️  Failed Tasks:")
        for task in failed:
            print(f"  - {task['title']}")
        return 1
    
    return 0

if __name__ == "__main__":
    sys.exit(main())
