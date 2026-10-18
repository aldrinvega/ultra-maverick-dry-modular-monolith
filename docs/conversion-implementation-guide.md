# Ultramaverick Dry → Modular Monolith

## Code-Intensive Conversion Guide

### From the current commit to the last implementation

---

**Subject repository (target):** `C:\Users\Aldrin\ultra-maverick-dry-modular-monolith`
**Legacy repository (source):** `C:\Users\Aldrin\Ultramaverick-Dry`
**Starting point:** branch `feat/catalog-phase-0-shared-kernel`, `HEAD = 141c244`, with uncommitted `.slnx` change and untracked `src/Shared/Ultramaverick.BuildingBlocks/`
**Runtime:** .NET 10 (SDK 10.0.400) · EF Core 10 · SQL Server 2022/2025 · MediatR · FluentValidation
**Produced:** 2026-10-16

<!-- pagebreak -->

# 1. How to use this document

This is a **build manual**. Every step gives you: the goal, the exact files, the **full code to type**, the **commands to run**, and the **evidence** that proves the step worked.

It starts exactly where the repository currently stands and carries the conversion forward, module by module, until the last implementation (Reporting) is reached.

## 1.1 Reading order

- **Part 0** is mandatory and comes first. It cleans up the in-flight refactor so the rest of the build rests on a stable base.
- **Part 1** teaches the repeatable module anatomy. Read it once; every later module is an application of it.
- **Parts 2–10** build the modules. Part 2 finishes Identity; Parts 3–10 build the other eight contexts.
- **Part 11** wires the cross-cutting concerns (shared building blocks, outbox routing, API versioning).
- **Part 12** migrates legacy data and cuts over.
- **Appendices** hold the schema DDL, the endpoint-to-module map, and card templates.

## 1.2 Conventions used in every code block

| Element | Convention |
| --- | --- |
| Namespace root | `Ultramaverick.<Context>.<Layer>` |
| Project name | `Ultramaverick.<Context>.<Layer>` |
| Folder | `src/Modules/<Context>/Ultramaverick.<Context>.<Layer>/` |
| Aggregate root base | `Entity` from `Ultramaverick.SharedKernel` |
| Cross-module identity | scalar IDs only (e.g. `ItemId`), never an object reference |
| Command/Query result | `Result` / `Result<T>` (from the Application layer) |
| Persistence error surface | never leak EF types across the Application boundary |
| Tests | xUnit; integration tests use a throwaway SQL Server database |

## 1.3 The four rules the architecture enforces

1. `Domain` references nothing but `Ultramaverick.SharedKernel`.
2. `Application` references `Domain` (and MediatR/FluentValidation), and **nothing else**.
3. `Persistence` and `Infrastructure` reference inward (Application, Domain) and implement the ports Application declares.
4. No module references another module's projects. Communication is asynchronous (outbox) or via a query port.

## 1.4 What each step card looks like

```
Step X.Y — Title

Goal        : one sentence, outcome first.
Project     : the .csproj this touches.
File        : the exact path(s).
Code        : the full implementation.
Command     : the shell command to run.
Evidence    : the observable proof the step passed.
```

---

## 1.5 Current state snapshot (evidence)

Captured from the repository at the start of this build:

| Check | Result |
| --- | --- |
| `dotnet build Ultramaverick.slnx` | succeeded — 8 warnings, 0 errors |
| `dotnet test Ultramaverick.slnx` | 47 passed, 0 failed, 0 skipped |
| Unit tests | 35 passed |
| Integration tests | 8 passed (real SQL Server, throwaway DB) |
| Authorization tests | 4 passed |
| Production projects | 11 (Api + Identity×4 + Catalog×4 + Shared×2) |
| Test projects | 3 |
| Endpoints implemented | 47 across 6 controllers |
| Domain entities implemented | 6 (User, Role, Department, Module, MainMenu, RoleModule) |
| Legacy endpoints to reach | 464 across 30 controllers |
| Legacy persisted models to reach | 62 `DbSet`s |
| Normalized schema prepared | 80 tables, 10 schemas (`database/normalized-schema.sql`) |

The gap is therefore **~90% of endpoints and models**, spread across eight contexts that have not started plus the tail of Identity.

<!-- pagebreak -->

# 2. Phase 0 — Consolidate (start here)

You are on `feat/catalog-phase-0-shared-kernel` with two loose ends that must be resolved before any new code:

1. An **untracked, unreferenced, and buggy** `Ultramaverick.BuildingBlocks` project.
2. An **empty root-level** `Ultramaverick.SharedKernel/` directory.

Leaving these in place means the next module is built on sand.

## 2.1 Inspect the current mess

```powershell
cd C:\Users\Aldrin\ultra-maverick-dry-modular-monolith
git status --short
git log --oneline -3
```

Expected:

```
 M Ultramaverick.slnx
?? src/Shared/Ultramaverick.BuildingBlocks/
141c244 refactor: extract shared kernel and scaffold Catalog module
```

## 2.2 Step 0.1 — Resolve `Ultramaverick.BuildingBlocks`

The Identity module already has a **working, tested** outbox implementation:

| Concern | Working file (keep) |
| --- | --- |
| Outbox abstractions + `OutboxRecord` | `src/Modules/Identity/Ultramaverick.Identity.Application/Abstractions/IOutboxStore.cs` |
| Dispatcher + processed-event abstractions | `src/Modules/Identity/Ultramaverick.Identity.Application/Abstractions/IEventDispatcher.cs` |
| Outbox entity + config | `src/Modules/Identity/Ultramaverick.Identity.Persistence/Entities/OutboxMessage.cs` |
| Processed-event entity + config | `src/Modules/Identity/Ultramaverick.Identity.Persistence/Entities/ProcessedEvent.cs` |
| Publisher background service | `src/Modules/Identity/Ultramaverick.Identity.Infrastructure/BackgroundServices/OutboxPublisherService.cs` |

The new `BuildingBlocks` duplicates all of it **and** ships real defects: a self-assigning constructor that silently breaks the exclude-from-migrations flag, seven unread primary-constructor parameters, a class named `ProcessEvent` in a file named `ProcessedEvent.cs`, an empty `OutboxDbContext` and `ProcessedEventConfiguration`, and misspellings (`IIntengrationEventHandler`, `MasProcessAsync`, `eventtId`, `Attemps`, `exlude`).

Pick **one** of the two options below.

### Option A (recommended) — Delete BuildingBlocks now, extract later when a second module needs it

```powershell
# Remove the folder and drop it from the solution file
Remove-Item -LiteralPath "src\Shared\Ultramaverick.BuildingBlocks" -Recurse -Force
```

Then edit `Ultramaverick.slnx` and delete this single line from the `/src/Shared/` folder block:

```xml
<Project Path="src/Shared/Ultramaverick.BuildingBlocks/Ultramaverick.BuildingBlocks.csproj" />
```

Rationale: with one module, shared messaging code has exactly one consumer. Extraction earns its keep when the second module (Catalog) needs the same abstractions. This keeps the tree honest and green.

### Option B — Keep BuildingBlocks, but make it the single source of truth

Only choose this if you want the extraction now. Move the **working** types out of Identity into `BuildingBlocks`, fix the defects, and make Identity reference the new project. The full, corrected `BuildingBlocks` is given in **Part 11.2**. For now, to unblock, delete it and proceed with Option A; Part 11.2 shows the consolidation when Catalog arrives.

## 2.3 Step 0.2 — Delete the empty root `Ultramaverick.SharedKernel`

The `141c244` move left an empty directory behind.

```powershell
# Confirm it is empty first
(Get-ChildItem "Ultramaverick.SharedKernel" -Force -ErrorAction SilentlyContinue | Measure-Object).Count   # expect 0
Remove-Item -LiteralPath "Ultramaverick.SharedKernel" -Recurse -Force
```

## 2.4 Step 0.3 — Central package management

Today package versions are duplicated across projects (FluentValidation `12.1.1` and MediatR `14.2.0` appear in two `.csproj` files each; EF Core `10.0.12` in several). Centralize them so versions cannot drift as nine modules are added.

**Create `Directory.Packages.props` at the repository root:**

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>

  <ItemGroup>
    <!-- Application / web -->
    <PackageVersion Include="MediatR" Version="14.2.0" />
    <PackageVersion Include="FluentValidation" Version="12.1.1" />
    <PackageVersion Include="FluentValidation.DependencyInjectionExtensions" Version="12.1.1" />
    <PackageVersion Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.12" />
    <PackageVersion Include="Microsoft.AspNetCore.OpenApi" Version="10.0.11" />
    <PackageVersion Include="System.IdentityModel.Tokens.Jwt" Version="8.22.0" />

    <!-- Persistence / EF Core -->
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="10.0.12" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Relational" Version="10.0.12" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.12" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.12" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.12" />

    <!-- Extensions -->
    <PackageVersion Include="Microsoft.Extensions.Configuration.Abstractions" Version="10.0.12" />
    <PackageVersion Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="10.0.12" />
    <PackageVersion Include="Microsoft.Extensions.Hosting.Abstractions" Version="10.0.12" />
    <PackageVersion Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.12" />
    <PackageVersion Include="Microsoft.Extensions.Http" Version="10.0.12" />

    <!-- Test -->
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="18.10.1" />
    <PackageVersion Include="xunit" Version="2.9.3" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="4.0.0" />
    <PackageVersion Include="coverlet.collector" Version="10.0.1" />
    <PackageVersion Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.12" />
    <PackageVersion Include="Testcontainers.MsSql" Version="4.1.0" />
  </ItemGroup>
</Project>
```

**Create `Directory.Build.props` at the repository root:**

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>latest</LangVersion>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <AnalysisLevel>latest</AnalysisLevel>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  </PropertyGroup>
</Project>
```

Then strip the now-redundant `TargetFramework`/`Nullable`/`ImplicitUsings` from every `.csproj` and remove `Version=` from every `PackageReference`. Example — before and after for the Api project:

```xml
<!-- BEFORE -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UserSecretsId>d4d8c524-a7c3-4327-ab74-99b55ea8b982</UserSecretsId>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="FluentValidation" Version="12.1.1" />
    <PackageReference Include="MediatR" Version="14.2.0" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.12" />
  </ItemGroup>
</Project>
```

```xml
<!-- AFTER -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <UserSecretsId>d4d8c524-a7c3-4327-ab74-99b55ea8b982</UserSecretsId>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="FluentValidation" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" />
    <PackageReference Include="MediatR" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>
</Project>
```

Apply the same trim to all 14 projects. Note that `Microsoft.EntityFrameworkCore.Tools`/`.Design` keep their `PrivateAssets`.

### Command

```powershell
dotnet restore Ultramaverick.slnx
dotnet build Ultramaverick.slnx -c Debug
```

### Evidence

```
Build succeeded.  0 Warning(s)  0 Error(s)
```

If a version conflict appears, it is because a project still has a literal `Version=` — remove it.

## 2.5 Step 0.4 — Turn warnings into errors

`TreatWarningsAsErrors` is now set in `Directory.Build.props`. This is deliberate: the eight warnings that shipped on a green build were all real defects in `BuildingBlocks`. With Option A (deleted) the build is already warning-free. With Option B you must fix them (Part 11.2).

```powershell
dotnet build Ultramaverick.slnx -c Release /warnaserror
```

### Evidence

```
Build succeeded.  0 Warning(s)  0 Error(s)
```

## 2.6 Step 0.5 — Correct the stale README and Dockerfile

The README currently claims the Catalog module is "Not started", that "Test projects contain no tests yet", and that the outbox publisher "is not implemented yet" — all false as of commits `fc87302` and `46ec53a`. Update the status table:

```markdown
## Roadmap

| Context | Status |
| --- | --- |
| Identity | In progress — login plus user/role/module/department/menu administration |
| Catalog | Scaffolded — projects created, implementation starting |
| Procurement | Not started |
| Quality | Not started |
| Laboratory | Not started |
| Warehouse | Not started |
| Orders | Not started |
| Manufacturing | Not started |
| Reporting | Not started |

## Testing

47 tests: 35 unit, 8 integration (real SQL Server), 4 authorization.

## Domain events and the outbox

Entities raise `UserChanged`/`RoleChanged`. `IdentityDbContext.SaveChangesAsync` writes each event to
`Infrastructure.OutboxMessages` in the same transaction as the data change. `OutboxPublisherService`
dispatches them; `EventDispatcher` guarantees idempotency via `Infrastructure.ProcessedEvents`.
```

The Dockerfile's layer-cached restore copies only the Api and Identity `.csproj` files. Once the Api references a second module it must copy those too. Add the Catalog and Shared csproj lines now so the cache stays valid:

```dockerfile
COPY src/Ultramaverick.Api/Ultramaverick.Api.csproj src/Ultramaverick.Api/
COPY src/Modules/Identity/Ultramaverick.Identity.Domain/Ultramaverick.Identity.Domain.csproj src/Modules/Identity/Ultramaverick.Identity.Domain/
COPY src/Modules/Identity/Ultramaverick.Identity.Application/Ultramaverick.Identity.Application.csproj src/Modules/Identity/Ultramaverick.Identity.Application/
COPY src/Modules/Identity/Ultramaverick.Identity.Persistence/Ultramaverick.Identity.Persistence.csproj src/Modules/Identity/Ultramaverick.Identity.Persistence/
COPY src/Modules/Identity/Ultramaverick.Identity.Infrastructure/Ultramaverick.Identity.Infrastructure.csproj src/Modules/Identity/Ultramaverick.Identity.Infrastructure/
COPY src/Modules/Catalog/Ultramaverick.Catalog.Domain/Ultramaverick.Catalog.Domain.csproj src/Modules/Catalog/Ultramaverick.Catalog.Domain/
COPY src/Modules/Catalog/Ultramaverick.Catalog.Application/Ultramaverick.Catalog.Application.csproj src/Modules/Catalog/Ultramaverick.Catalog.Application/
COPY src/Modules/Catalog/Ultramaverick.Catalog.Persistence/Ultramaverick.Catalog.Persistence.csproj src/Modules/Catalog/Ultramaverick.Catalog.Persistence/
COPY src/Modules/Catalog/Ultramaverick.Catalog.Infrastructure/Ultramaverick.Catalog.Infrastructure.csproj src/Modules/Catalog/Ultramaverick.Catalog.Infrastructure/
COPY src/Shared/Ultramaverick.SharedKernel/Ultramaverick.SharedKernel.csproj src/Shared/Ultramaverick.SharedKernel/
RUN dotnet restore src/Ultramaverick.Api/Ultramaverick.Api.csproj
```

## 2.7 Step 0.6 — Commit and merge

```powershell
git add -A
git commit -m "chore: adopt central package management and consolidate shared projects

- remove the duplicate BuildingBlocks project (kept the working Identity outbox)
- delete the leftover empty root SharedKernel directory
- add Directory.Packages.props and Directory.Build.props (CPM + warnings as errors)
- refresh README status and complete the Dockerfile restore list"
git push -u origin feat/catalog-phase-0-shared-kernel

# open a PR into main; CI runs build + test on the SQL Server service
```

### Evidence

The CI workflow (`.github/workflows/ci.yml`) goes green: restore, build (Release), test — 47 passing.

Create the PR with `gh`:

```powershell
gh pr create --title "Consolidate shared projects and adopt CPM" --fill --base main
```

<!-- pagebreak -->

# 3. Phase 1 — The module anatomy (learn once, repeat eight times)

Every bounded context is built the same way. Identity is the reference; the other modules are the same shape with different words. Internalize this part and the remaining modules become mechanical.

## 3.1 Scaffold a new module

```powershell
$ctx = "Catalog"   # change per module
$root = "src/Modules/$ctx"

dotnet new classlib -n "Ultramaverick.$ctx.Domain"         -o "$root/Ultramaverick.$ctx.Domain"
dotnet new classlib -n "Ultramaverick.$ctx.Application"    -o "$root/Ultramaverick.$ctx.Application"
dotnet new classlib -n "Ultramaverick.$ctx.Persistence"    -o "$root/Ultramaverick.$ctx.Persistence"
dotnet new classlib -n "Ultramaverick.$ctx.Infrastructure" -o "$root/Ultramaverick.$ctx.Infrastructure"

# Remove the template stub
Get-ChildItem "$root" -Recurse -Filter Class1.cs | Remove-Item -Force

# Add to the solution
dotnet sln Ultramaverick.slnx add "$root/Ultramaverick.$ctx.Domain/Ultramaverick.$ctx.Domain.csproj"
dotnet sln Ultramaverick.slnx add "$root/Ultramaverick.$ctx.Application/Ultramaverick.$ctx.Application.csproj"
dotnet sln Ultramaverick.slnx add "$root/Ultramaverick.$ctx.Persistence/Ultramaverick.$ctx.Persistence.csproj"
dotnet sln Ultramaverick.slnx add "$root/Ultramaverick.$ctx.Infrastructure/Ultramaverick.$ctx.Infrastructure.csproj"

# Wire the references (direction is inward only)
dotnet add "$root/Ultramaverick.$ctx.Domain/Ultramaverick.$ctx.Domain.csproj" `
    reference "src/Shared/Ultramaverick.SharedKernel/Ultramaverick.SharedKernel.csproj"
dotnet add "$root/Ultramaverick.$ctx.Application/Ultramaverick.$ctx.Application.csproj" `
    reference "$root/Ultramaverick.$ctx.Domain/Ultramaverick.$ctx.Domain.csproj"
dotnet add "$root/Ultramaverick.$ctx.Persistence/Ultramaverick.$ctx.Persistence.csproj" `
    reference "$root/Ultramaverick.$ctx.Application/Ultramaverick.$ctx.Application.csproj"
dotnet add "$root/Ultramaverick.$ctx.Persistence/Ultramaverick.$ctx.Persistence.csproj" `
    reference "$root/Ultramaverick.$ctx.Domain/Ultramaverick.$ctx.Domain.csproj"
dotnet add "$root/Ultramaverick.$ctx.Infrastructure/Ultramaverick.$ctx.Infrastructure.csproj" `
    reference "$root/Ultramaverick.$ctx.Application/Ultramaverick.$ctx.Application.csproj"
```

### Target folder tree

```
src/Modules/Catalog/
  Ultramaverick.Catalog.Domain/
    Entity base is in SharedKernel
    Entities/            aggregate roots and child entities
    ValueObjects/        small immutable types
    Events/              domain event records + change-type constants
    AssemblyMarker.cs
  Ultramaverick.Catalog.Application/
    Abstractions/        ports (repositories, unit of work, services)
    Commands/<Subject>/  command + handler + validator
    Queries/<Subject>/   query + handler
    Models/              Result<T>, PagedResult<T>, DTOs
    Behaviors/           ValidationBehavior<TRequest,TResponse>
    ServiceCollectionExtensions.cs
  Ultramaverick.Catalog.Persistence/
    CatalogDbContext.cs
    Configurations/      IEntityTypeConfiguration per entity
    Entities/            OutboxMessage, ProcessedEvent
    Repositories/        repository impls + unit of work
    Migrations/
    ServiceCollectionExtensions.cs
  Ultramaverick.Catalog.Infrastructure/
    ServiceCollectionExtensions.cs
```

## 3.2 Domain layer

### 3.2.1 The shared base (already in SharedKernel)

```csharp
namespace Ultramaverick.SharedKernel;

public interface IDomainEvent
{
    DateTime OccurredAtUtc { get; }
}

public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

### 3.2.2 An aggregate root

A Catalog aggregate — `Uom` (unit of measure) — is a good first entity because it is a simple master record.

```csharp
using Ultramaverick.Catalog.Domain.Events;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Catalog.Domain.Entities;

public sealed class Uom : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    private Uom() { }

    public static Uom Create(string code, string name, int? createdByUserId)
    {
        code = (code ?? string.Empty).Trim();
        name = (name ?? string.Empty).Trim();

        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var uom = new Uom
        {
            Code = code,
            Name = name,
            IsActive = true,
            CreatedByUserId = createdByUserId
        };

        uom.RaiseDomainEvent(new UomChanged(UomChangeTypes.Created, DateTime.UtcNow));
        return uom;
    }

    public void Rename(string code, string name, int modifiedByUserId)
    {
        code = (code ?? string.Empty).Trim();
        name = (name ?? string.Empty).Trim();

        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Code = code;
        Name = name;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;

        RaiseDomainEvent(new UomChanged(UomChangeTypes.Updated, DateTime.UtcNow));
    }

    public void Deactivate(int modifiedByUserId)
    {
        if (!IsActive) return;
        IsActive = false;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new UomChanged(UomChangeTypes.Deactivated, DateTime.UtcNow));
    }

    public void Activate(int modifiedByUserId)
    {
        if (IsActive) return;
        IsActive = true;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new UomChanged(UomChangeTypes.Activated, DateTime.UtcNow));
    }
}
```

### 3.2.3 The domain event

```csharp
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Catalog.Domain.Events;

public static class UomChangeTypes
{
    public const string Created = nameof(Created);
    public const string Updated = nameof(Updated);
    public const string Deactivated = nameof(Deactivated);
    public const string Activated = nameof(Activated);
}

public sealed record UomChanged(string ChangeType, DateTime OccurredAtUtc) : IDomainEvent;
```

Standards: events are `sealed record`s, carry `ChangeType` and time only (never the id — the id becomes the outbox `AggregateId`), and live in `Events/`.

## 3.3 Application layer

### 3.3.1 Result and paging models

```csharp
namespace Ultramaverick.Catalog.Application.Models;

public readonly record struct Result(bool Succeeded, string? Error)
{
    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
}

public readonly record struct Result<T>(bool Succeeded, T? Value, string? Error)
{
    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
}
```

### 3.3.2 Ports (interfaces)

```csharp
namespace Ultramaverick.Catalog.Application.Abstractions;

public interface IUomRepository
{
    Task<Uom?> GetByIdAsync(int id, CancellationToken ct);
    Task<Uom?> GetByCodeAsync(string code, CancellationToken ct);
    Task<IReadOnlyList<Uom>> ListAsync(string? search, bool? isActive, int skip, int take, CancellationToken ct);
    Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct);
    Task AddAsync(Uom uom, CancellationToken ct);
}

public interface ICatalogUnitOfWork
{
    IUomRepository Uoms { get; }
    Task<int> SaveChangesAsync(CancellationToken ct);
}

public interface ICurrentUser
{
    int? UserId { get; }
    string? UserName { get; }
    IReadOnlyCollection<string> Modules { get; }
}
```

### 3.3.3 DTOs

DTOs are designed **from the schema**, never from the entity.

```csharp
namespace Ultramaverick.Catalog.Application.Models;

public sealed record UomDto(int Id, string Code, string Name, bool IsActive, DateTime CreatedAtUtc);
```

### 3.3.4 Command, handler, validator

```csharp
using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Uoms;

public sealed record CreateUomCommand(string Code, string Name) : IRequest<Result<int>>;
```

```csharp
using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Commands.Uoms;

public sealed class CreateUomCommandHandler : IRequestHandler<CreateUomCommand, Result<int>>
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public CreateUomCommandHandler(ICatalogUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(CreateUomCommand request, CancellationToken ct)
    {
        var code = request.Code.Trim();

        if (await _uow.Uoms.GetByCodeAsync(code, ct) is not null)
            return Result<int>.Failure($"UOM code '{code}' already exists.");

        var uom = Uom.Create(request.Code, request.Name, _currentUser.UserId);

        await _uow.Uoms.AddAsync(uom, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<int>.Success(uom.Id);
    }
}
```

```csharp
using FluentValidation;

namespace Ultramaverick.Catalog.Application.Commands.Uoms;

public sealed class CreateUomCommandValidator : AbstractValidator<CreateUomCommand>
{
    public CreateUomCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
```

### 3.3.5 Query and handler

```csharp
using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Uoms;

public sealed record GetUomsQuery(int Page, int PageSize, string? Search, bool? IsActive)
    : IRequest<PagedResult<UomDto>>;
```

```csharp
using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Uoms;

public sealed class GetUomsQueryHandler : IRequestHandler<GetUomsQuery, PagedResult<UomDto>>
{
    private readonly ICatalogUnitOfWork _uow;

    public GetUomsQueryHandler(ICatalogUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<UomDto>> Handle(GetUomsQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;
        var skip = (page - 1) * pageSize;

        var items = await _uow.Uoms.ListAsync(request.Search, request.IsActive, skip, pageSize, ct);
        var total = await _uow.Uoms.CountAsync(request.Search, request.IsActive, ct);

        var dtos = items.Select(u => new UomDto(u.Id, u.Code, u.Name, u.IsActive, u.CreatedAtUtc)).ToList();

        return new PagedResult<UomDto>(dtos, page, pageSize, total);
    }
}
```

### 3.3.6 Validation behavior (once per module, copied)

```csharp
using FluentValidation;
using MediatR;

namespace Ultramaverick.Catalog.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, ct))))
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (failures.Count > 0)
                throw new ValidationException(failures);
        }

        return await next();
    }
}
```

### 3.3.7 Application DI registration

```csharp
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Catalog.Application.Behaviors;

namespace Ultramaverick.Catalog.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCatalogApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
```

Note: the `ValidationException` thrown here is mapped to HTTP 400 by the shared `ExceptionMiddleware` in the Api project.

## 3.4 Persistence layer

### 3.4.1 The outbox entities (copied per module; single source in Part 11)

```csharp
namespace Ultramaverick.Catalog.Persistence.Entities;

public sealed class OutboxMessage
{
    public Guid EventId { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
    public string AggregateType { get; private set; } = null!;
    public string AggregateId { get; private set; } = null!;
    public string EventType { get; private set; } = null!;
    public long Version { get; private set; }
    public string Payload { get; private set; } = null!;
    public DateTime? PublishedAtUtc { get; private set; }
    public int Attempts { get; private set; }
    public string? LastError { get; private set; }

    private OutboxMessage() { }

    public static OutboxMessage Create(string aggregateType, string aggregateId, string eventType, string payload)
        => new()
        {
            EventId = Guid.NewGuid(),
            OccurredAtUtc = DateTime.UtcNow,
            AggregateType = aggregateType,
            AggregateId = aggregateId,
            EventType = eventType,
            Version = 1,
            Payload = payload,
            Attempts = 0
        };
}
```

### 3.4.2 The DbContext

Copy `IdentityDbContext` and change the schema name, the `DbSet`s, and the aggregate assembly.

```csharp
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ultramaverick.Catalog.Domain.Entities;
using Ultramaverick.Catalog.Persistence.Entities;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Catalog.Persistence;

public sealed class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    public DbSet<Uom> Uoms => Set<Uom>();
    public DbSet<Item> Items => Set<Item>();
    // ... one DbSet per aggregate root
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Catalog");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var ownsTransaction = Database.CurrentTransaction is null;
        var transaction = ownsTransaction
            ? await Database.BeginTransactionAsync(cancellationToken)
            : Database.CurrentTransaction!;

        try
        {
            var result = await base.SaveChangesAsync(cancellationToken);

            var pending = ChangeTracker.Entries()
                .Where(e => e.Entity is Entity)
                .Select(e => new
                {
                    Entry = e,
                    Entity = (Entity)e.Entity,
                    AggregateId = e.Property("Id").CurrentValue?.ToString() ?? string.Empty
                })
                .Where(x => x.Entity.DomainEvents.Count > 0)
                .ToList();

            foreach (var item in pending)
            {
                foreach (var domainEvent in item.Entity.DomainEvents)
                {
                    OutboxMessages.Add(OutboxMessage.Create(
                        aggregateType: item.Entry.Metadata.ClrType.Name,
                        aggregateId: item.AggregateId,
                        eventType: domainEvent.GetType().Name,
                        payload: JsonSerializer.Serialize(domainEvent, domainEvent.GetType())));
                }

                item.Entity.ClearDomainEvents();
            }

            if (pending.Count > 0)
                await base.SaveChangesAsync(cancellationToken);

            if (ownsTransaction)
                await transaction.CommitAsync(cancellationToken);

            return result;
        }
        catch
        {
            if (ownsTransaction)
                await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
```

### 3.4.3 An entity configuration

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Persistence.Configurations;

public sealed class UomConfiguration : IEntityTypeConfiguration<Uom>
{
    public void Configure(EntityTypeBuilder<Uom> builder)
    {
        builder.ToTable("Uoms", "Catalog");
        builder.HasKey(u => u.Id);
        builder.Ignore(u => u.DomainEvents);

        builder.Property(u => u.Code).IsRequired().HasMaxLength(20);
        builder.Property(u => u.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(u => u.Code).IsUnique();

        builder.Property(u => u.IsActive).IsRequired();
        builder.Property(u => u.CreatedAtUtc).IsRequired();
        builder.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
```

### 3.4.4 Repository and unit of work

```csharp
using Microsoft.EntityFrameworkCore;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Persistence.Repositories;

public sealed class UomRepository : IUomRepository
{
    private readonly CatalogDbContext _context;
    public UomRepository(CatalogDbContext context) => _context = context;

    public Task<Uom?> GetByIdAsync(int id, CancellationToken ct)
        => _context.Uoms.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<Uom?> GetByCodeAsync(string code, CancellationToken ct)
        => _context.Uoms.FirstOrDefaultAsync(u => u.Code == code, ct);

    public async Task<IReadOnlyList<Uom>> ListAsync(
        string? search, bool? isActive, int skip, int take, CancellationToken ct)
        => await Build(search, isActive).OrderBy(u => u.Code).Skip(skip).Take(take).ToListAsync(ct);

    public Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct)
        => Build(search, isActive).CountAsync(ct);

    public async Task AddAsync(Uom uom, CancellationToken ct) => await _context.Uoms.AddAsync(uom, ct);

    private IQueryable<Uom> Build(string? search, bool? isActive)
    {
        var query = _context.Uoms.AsNoTracking();
        if (isActive is not null) query = query.Where(u => u.IsActive == isActive);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u => u.Name.Contains(term) || u.Code.Contains(term));
        }
        return query;
    }
}
```

```csharp
using Ultramaverick.Catalog.Application.Abstractions;

namespace Ultramaverick.Catalog.Persistence.Repositories;

public sealed class CatalogUnitOfWork : ICatalogUnitOfWork
{
    private readonly CatalogDbContext _context;

    public CatalogUnitOfWork(CatalogDbContext context)
    {
        _context = context;
        Uoms = new UomRepository(context);
        // ... other repositories
    }

    public IUomRepository Uoms { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
}
```

### 3.4.5 Persistence DI registration

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Persistence.Repositories;

namespace Ultramaverick.Catalog.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCatalogPersistence(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        services.AddDbContext<CatalogDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IUomRepository, UomRepository>();
        services.AddScoped<ICatalogUnitOfWork, CatalogUnitOfWork>();
        // ... other repositories

        return services;
    }
}
```

### 3.4.6 Create the migration

```powershell
dotnet ef migrations add InitialCatalog `
    -p src/Modules/Catalog/Ultramaverick.Catalog.Persistence `
    -s src/Ultramaverick.Api `
    -c CatalogDbContext `
    -o Migrations

dotnet ef database update `
    -p src/Modules/Catalog/Ultramaverick.Catalog.Persistence `
    -s src/Ultramaverick.Api `
    -c CatalogDbContext
```

If the design-time factory is missing, add one (same shape as `IdentityDbContextFactory`):

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ultramaverick.Catalog.Persistence;

public sealed class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("CATALOG_DESIGN_CONNECTION")
                 ?? @"Server=.\SQLEXPRESS;Database=ElixirDepotDry;Trusted_Connection=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseSqlServer(cs)
            .Options;

        return new CatalogDbContext(options);
    }
}
```

## 3.5 API layer

### 3.5.1 Controller

```csharp
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Api.Errors;
using Ultramaverick.Catalog.Application.Commands.Uoms;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Application.Queries.Uoms;

namespace Ultramaverick.Api.Controllers;

[ApiController]
[Route("api/Uom")]
[Authorize(Policy = Policies.RequireModule("Catalog"))]
public sealed class UomsController : ControllerBase
{
    private readonly IMediator _mediator;
    public UomsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("GetAllUomsWithPagination")]
    public async Task<ActionResult<PagedResult<UomDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? status = null,
        CancellationToken ct = default)
        => Ok(await _mediator.Send(new GetUomsQuery(page, pageSize, search, status), ct));

    [HttpGet("GetById/{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => (await _mediator.Send(new GetUomByIdQuery(id), ct)).ToOkOrNotFound();

    [HttpPost("AddNewUom")]
    public async Task<IActionResult> Create([FromBody] CreateUomCommand command, CancellationToken ct)
        => (await _mediator.Send(command, ct)).ToOk();

    [HttpPut("InActiveUom/{id}")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
        => (await _mediator.Send(new SetUomActiveCommand(id, false), ct)).ToNoContent();

    [HttpPut("ActivateUom/{id}")]
    public async Task<IActionResult> Activate(int id, CancellationToken ct)
        => (await _mediator.Send(new SetUomActiveCommand(id, true), ct)).ToNoContent();
}
```

### 3.5.2 Register the module in `Program.cs`

```csharp
// Application + Persistence + Infrastructure, one line per project
builder.Services.AddCatalogApplication();
builder.Services.AddCatalogPersistence(builder.Configuration);
builder.Services.AddCatalogInfrastructure(builder.Configuration);
```

The policy `RequireModule("Catalog")` already exists — `Program.cs` loops the nine module names and registers one policy per module.

## 3.6 Tests

### 3.6.1 Unit test (no database)

```csharp
using Ultramaverick.Catalog.Domain.Entities;
using Ultramaverick.Catalog.Domain.Events;
using Xunit;

namespace Ultramaverick.Catalog.UnitTests.Domain;

public sealed class UomTests
{
    [Fact]
    public void Create_raises_one_created_event()
    {
        var uom = Uom.Create("KG", "Kilogram", createdByUserId: 1);

        var evt = Assert.Single(uom.DomainEvents);
        var changed = Assert.IsType<UomChanged>(evt);
        Assert.Equal(UomChangeTypes.Created, changed.ChangeType);
    }

    [Fact]
    public void Deactivate_twice_raises_one_event()
    {
        var uom = Uom.Create("KG", "Kilogram", 1);
        uom.ClearDomainEvents();

        uom.Deactivate(1);
        uom.Deactivate(1);

        Assert.Single(uom.DomainEvents);
    }
}
```

### 3.6.2 Integration test (real database)

```csharp
using Microsoft.EntityFrameworkCore;
using Ultramaverick.Catalog.Domain.Entities;
using Ultramaverick.Catalog.IntegrationTests;
using Xunit;

namespace Ultramaverick.Catalog.IntegrationTests;

public sealed class UomOutboxTests : IClassFixture<CatalogDatabaseFixture>
{
    private readonly CatalogDatabaseFixture _fixture;
    public UomOutboxTests(CatalogDatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Creating_a_uom_writes_one_outbox_row_in_the_same_save()
    {
        await using var context = _fixture.CreateContext();
        context.Uoms.Add(Uom.Create("KG", "Kilogram", 1));

        await context.SaveChangesAsync();

        Assert.Equal(1, await context.OutboxMessages.CountAsync());
    }
}
```

### 3.6.3 The database fixture (copy `DatabaseFixture`, change context + catalog)

```csharp
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Ultramaverick.Catalog.Persistence;

namespace Ultramaverick.Catalog.IntegrationTests;

public sealed class CatalogDatabaseFixture : IAsyncLifetime
{
    private const string DefaultServer =
        @"Server=.\SQLEXPRESS;Trusted_Connection=True;TrustServerCertificate=True";

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        var baseConnection = Environment.GetEnvironmentVariable("IDENTITY_TEST_CONNECTION") ?? DefaultServer;
        ConnectionString = new SqlConnectionStringBuilder(baseConnection)
        {
            InitialCatalog = $"UltramaverickCatalogTests_{Guid.NewGuid():N}"
        }.ConnectionString;

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
    }

    public CatalogDbContext CreateContext()
        => new(new DbContextOptionsBuilder<CatalogDbContext>().UseSqlServer(ConnectionString).Options);
}
```

## 3.7 Standard module checklist

For every context, tick these before moving on:

1. Four projects created, references point inward only.
2. Aggregate roots derive from `Entity`; factories and mutators raise domain events.
3. `Result`/`PagedResult` and DTOs live in Application.
4. Ports in Application; implementations in Persistence.
5. `*DbContext` with default schema, `ApplyConfigurationsFromAssembly`, and the two-phase `SaveChangesAsync`.
6. One configuration per entity with keys, lengths, indexes, and a `RowVersion` row.
7. Migration created and applied.
8. Controllers use legacy routes and `RequireModule("<Context>")`.
9. Unit tests (events, invariants) and one integration test (outbox row on write).
10. `dotnet build` and `dotnet test` green.

<!-- pagebreak -->

# 4. Phase 2 — Finish Identity to full parity

Identity is ~80% done. The remaining work is: complete the audit/legacy columns on every aggregate, wire the module claims into the token, add the password and navigation endpoints the legacy `UserController` exposed, and prove it with tests.

## 4.1 Step 2.1 — Add `LegacyId` and the missing columns to the aggregates

`User` already has `LegacyId`. Add it to `Role`, `Department`, `Module`, `MainMenu`, and bring the two Shell entities up to the normalized schema (`MainMenus.Path`, `MainMenus.SortOrder`, `Modules.MainMenuId`, `Modules.SubMenuName`, `Modules.ModuleStatus`).

```csharp
// src/Modules/Identity/Ultramaverick.Identity.Domain/Entities/Role.cs
using Ultramaverick.Identity.Domain.Events;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Identity.Domain.Entities;

public sealed class Role : Entity
{
    private readonly List<RoleModule> _modules = new();

    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    public IReadOnlyCollection<RoleModule> Modules => _modules.AsReadOnly();

    private Role() { }

    public static Role Create(string name, int? createdByUserId)
    {
        name = (name ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var role = new Role { Name = name, IsActive = true, CreatedByUserId = createdByUserId };
        role.RaiseDomainEvent(new RoleChangedEvent(RoleChangedTypes.Created));
        return role;
    }

    public void Rename(string name, int modifiedByUserId)
    {
        name = (name ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new RoleChangedEvent(RoleChangedTypes.Updated));
    }

    public void Deactivate(int modifiedByUserId)
    {
        if (!IsActive) return;
        IsActive = false;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new RoleChangedEvent(RoleChangedTypes.Deactivated));
    }

    public void Activate(int modifiedByUserId)
    {
        if (IsActive) return;
        IsActive = true;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new RoleChangedEvent(RoleChangedTypes.Activated));
    }

    public void TagModule(int moduleId)
    {
        if (_modules.Any(m => m.ModuleId == moduleId)) return;
        _modules.Add(RoleModule.Create(Id, moduleId));
        RaiseDomainEvent(new RoleChangedEvent(RoleChangedTypes.Updated));
    }

    public void UntagModule(int moduleId)
    {
        var existing = _modules.FirstOrDefault(m => m.ModuleId == moduleId);
        if (existing is null) return;
        _modules.Remove(existing);
        RaiseDomainEvent(new RoleChangedEvent(RoleChangedTypes.Updated));
    }
}
```

```csharp
// MainMenu.cs — add Path and SortOrder
public int? LegacyId { get; private set; }
public string Name { get; private set; } = null!;
public string Path { get; private set; } = null!;
public int SortOrder { get; private set; } = 0;
public bool IsActive { get; private set; }
// + audit columns, private ctor, Create(...) and SetActive(...) raising MainMenuChanged
```

```csharp
// Module.cs — add MainMenuId, SubMenuName, ModuleStatus
public int? LegacyId { get; private set; }
public int MainMenuId { get; private set; }
public string Name { get; private set; } = null!;
public string? SubMenuName { get; private set; }
public string? ModuleStatus { get; private set; }
public int SortOrder { get; private set; } = 0;
public bool IsActive { get; private set; }
// + audit columns, private ctor, Create(...) and SetActive(...) raising ModuleChanged
```

Then extend the matching configurations with the new columns and unique constraints:

```csharp
// RoleConfiguration.cs
builder.Property(r => r.LegacyId).HasColumnName("LegacyId");
builder.HasIndex(r => r.LegacyId).IsUnique().HasFilter("[LegacyId] IS NOT NULL");
```

Apply the same filtered-unique index on `LegacyId` to `Users`, `Departments`, `MainMenus`, `Modules`.

### Schema + migration

```powershell
dotnet ef migrations add IdentityParityColumns `
    -p src/Modules/Identity/Ultramaverick.Identity.Persistence `
    -s src/Ultramaverick.Api
dotnet ef database update `
    -p src/Modules/Identity/Ultramaverick.Identity.Persistence `
    -s src/Ultramaverick.Api
```

## 4.2 Step 2.2 — Put the granted modules into the JWT

The authorization policies read a `modules` claim. Build it from the user's role's tagged modules.

```csharp
// src/Modules/Identity/Ultramaverick.Identity.Infrastructure/Security/JwtTokenService.cs
public async Task<string> CreateAccessTokenAsync(User user, CancellationToken ct)
{
    var moduleIds = await _roleModuleQuery.GetActiveModuleNamesForRoleAsync(user.RoleId, ct);

    var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new(JwtRegisteredClaimNames.Name, user.FullName),
        new(ClaimTypes.Role, user.RoleId.ToString()),
        new("department", user.DepartmentId.ToString())
    };

    // One claim per granted module, so policy checks match any of them.
    claims.AddRange(moduleIds.Select(m => new Claim(ModuleClaimType, m)));

    return BuildToken(claims);
}
```

Where the query port is:

```csharp
// Application/Abstractions/IRoleModuleQuery.cs
public interface IRoleModuleQuery
{
    Task<IReadOnlyList<string>> GetActiveModuleNamesForRoleAsync(int roleId, CancellationToken ct);
}
```

```csharp
// Persistence/Repositories/RoleModuleQuery.cs
public sealed class RoleModuleQuery : IRoleModuleQuery
{
    private readonly IdentityDbContext _context;
    public RoleModuleQuery(IdentityDbContext context) => _context = context;

    public async Task<IReadOnlyList<string>> GetActiveModuleNamesForRoleAsync(int roleId, CancellationToken ct)
        => await (from rm in _context.Set<RoleModule>()
                  join m in _context.Set<Module>() on rm.ModuleId equals m.Id
                  where rm.RoleId == roleId && rm.IsActive && m.IsActive && m.ModuleStatus == "Active"
                  select m.Name)
                 .Distinct()
                 .ToListAsync(ct);
}
```

Register it in `AddIdentityPersistence`:

```csharp
services.AddScoped<IRoleModuleQuery, RoleModuleQuery>();
```

## 4.3 Step 2.3 — Change-password and admin-reset endpoints

The legacy `UserController` exposed password change and reset. Add both.

```csharp
// Application/Commands/Users/ChangePasswordCommand.cs
using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Users;

public sealed record ChangePasswordCommand(int UserId, string CurrentPassword, string NewPassword)
    : IRequest<Result>;
```

```csharp
// Application/Commands/Users/ChangePasswordCommandHandler.cs
using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;
using Ultramaverick.Identity.Domain.ValueObjects;

namespace Ultramaverick.Identity.Application.Commands.Users;

public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IIdentityUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly ICurrentUser _currentUser;

    public ChangePasswordCommandHandler(IIdentityUnitOfWork uow, IPasswordHasher hasher, ICurrentUser currentUser)
    {
        _uow = uow;
        _hasher = hasher;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        var user = await _uow.Users.GetByIdAsync(request.UserId, ct);
        if (user is null) return Result.Failure("User not found.");

        if (!_hasher.Verify(request.CurrentPassword, user.Password.Value))
            return Result.Failure("Current password is incorrect.");

        var newHash = PasswordHash.FromEncoded(_hasher.Hash(request.NewPassword));
        user.ChangePassword(newHash, _currentUser.UserId ?? 0);

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
```

```csharp
// Application/Commands/Users/ChangePasswordCommandValidator.cs
using FluentValidation;

namespace Ultramaverick.Identity.Application.Commands.Users;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(256);
    }
}
```

Controller actions:

```csharp
[HttpPut("ChangePassword/{id}")]
public async Task<IActionResult> ChangePassword(
    int id, [FromBody] ChangePasswordCommand command, CancellationToken ct)
    => id != command.UserId
        ? ResultExtensions.BadRequestError("Route id and body id must match.")
        : (await _mediator.Send(command, ct)).ToNoContent();
```

## 4.4 Step 2.4 — Navigation menu tree

The front end needs the menus/modules a user may see.

```csharp
// Application/Queries/Navigation/GetMyMenuQuery.cs
using MediatR;

namespace Ultramaverick.Identity.Application.Queries.Navigation;

public sealed record MenuModuleDto(int Id, string Name, string? SubMenuName, string? Status);
public sealed record MenuDto(int Id, string Name, string Path, IReadOnlyList<MenuModuleDto> Modules);

public sealed record GetMyMenuQuery(int RoleId) : IRequest<IReadOnlyList<MenuDto>>;
```

```csharp
// Application/Queries/Navigation/GetMyMenuQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ultramaverick.Identity.Application.Abstractions;

namespace Ultramaverick.Identity.Application.Queries.Navigation;

public sealed class GetMyMenuQueryHandler : IRequestHandler<GetMyMenuQuery, IReadOnlyList<MenuDto>>
{
    private readonly IIdentityUnitOfWork _uow;

    public GetMyMenuQueryHandler(IIdentityUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<MenuDto>> Handle(GetMyMenuQuery request, CancellationToken ct)
    {
        var menus = await _uow.MainMenus.ListWithModulesAsync(request.RoleId, ct);

        return menus
            .Select(m => new MenuDto(
                m.Id, m.Name, m.Path,
                m.Modules
                 .Where(mm => mm.GrantedToRole)
                 .Select(mm => new MenuModuleDto(mm.Id, mm.Name, mm.SubMenuName, mm.ModuleStatus))
                 .ToList()))
            .Where(m => m.Modules.Count > 0 || m.Path is not null)
            .ToList();
    }
}
```

## 4.5 Step 2.5 — Tests to lock Identity

```csharp
// tests/Ultramaverick.Identity.UnitTests/Domain/RoleTests.cs  (add these)
[Fact]
public void TagModule_is_idempotent()
{
    var role = Role.Create("Supervisor", 1);
    role.ClearDomainEvents();

    role.TagModule(10);
    role.TagModule(10);

    Assert.Single(role.Modules);
    Assert.Single(role.DomainEvents);
}
```

```csharp
// tests/Ultramaverick.Identity.IntegrationTests/IdentityParityTests.cs
[Fact]
public async Task User_with_role_module_gets_the_module_claim()
{
    await using var ctx = _fixture.CreateContext();
    // seed role, module, rolemodule, user ...
    var token = await _tokenService.CreateAccessTokenAsync(user, CancellationToken.None);
    Assert.Contains("Identity", ReadClaims(token)["modules"]);
}
```

### Evidence

```powershell
dotnet build Ultramaverick.slnx -c Release
dotnet test  Ultramaverick.slnx -c Release
```

```
Passed: 47 -> 60+   Failed: 0
```

## 4.6 Step 2.6 — Backfill Identity from the legacy database

```sql
USE ElixirDepotDry;
GO

-- Roles
SET IDENTITY_INSERT [Identity].[Roles] OFF;
INSERT INTO [Identity].[Roles] ([LegacyId],[Name],[IsActive])
SELECT r.RoleId, r.RoleName, COALESCE(r.IsActive, 1)
FROM [legacy].[Roles] r
WHERE NOT EXISTS (SELECT 1 FROM [Identity].[Roles] t WHERE t.LegacyId = r.RoleId);

-- Map legacy users, hashing the plaintext password on the application side, not in SQL.
-- Users require a RoleId and DepartmentId; resolve via the LegacyId mapping created above.
```

The password re-hash must run in application code (PBKDF2). Write a one-off console or an integration test that reads legacy users, calls `IPasswordHasher.Hash`, and inserts. Never store the legacy plaintext.

<!-- pagebreak -->

# 5. Phase 3 — Catalog module (full)

Catalog owns the master data everything else references: items, units, suppliers, customers, farms, lots, lookups, formulas, and QC/lab master lists.

## 5.1 Step 3.1 — Schema

| Table | Kind | Key columns |
| --- | --- | --- |
| `Catalog.Uoms` | lookup | `Code` (UQ), `Description` |
| `Catalog.ItemCategories` | lookup | `Name` (UQ) |
| `Catalog.Items` | master | `ItemCode` (UQ), `ItemDescription`, `ItemCategoryId` (FK), `UomId` (FK), `BufferLevel`, `IsExpirable` |
| `Catalog.Suppliers` | master | `SupplierCode` (UQ), `SupplierName`, `SupplierAddress` |
| `Catalog.Farms` | master | `FarmCode` (UQ), `FarmName` |
| `Catalog.Locations` | lookup | `Code`, `Name` |
| `Catalog.AccountTitles` | lookup | `Code`, `Name` |
| `Catalog.Customers` | master | `CustomerCode` (UQ), `CustomerName`, `FarmId`, `AccountTitleId`, `LocationId`, `DepartmentId` (scalar), contact fields |
| `Catalog.LotCategories` | lookup | `Name` (UQ) |
| `Catalog.LotNames` | master | `Code` (UQ), `LotCategoryId` (FK), `SectionName` |
| `Catalog.Reasons` | lookup | `Name`, `MenuId` (scalar) |
| `Catalog.TransactionTypes` | lookup | `Name` (UQ) |
| `Catalog.ProductTypes`, `Dispositions`, `Analyses`, `Parameters`, `SampleTypes`, `SwabTypes`, `ProductConditions` | lookup | `Name` (UQ) |
| `Catalog.FormulaHeaders` | master | `ItemId` (FK), `Version`, `Quantity`, UQ(`ItemId`,`Version`) |
| `Catalog.FormulaRequirements` | master | `FormulaHeaderId` (FK), `RawMaterialId` (FK), `Quantity`, UQ(`FormulaHeaderId`,`RawMaterialId`) |

Scaffold the module with **Part 3.1** (the script) using `$ctx = "Catalog"`.

## 5.2 Step 3.2 — Master entities

### Item

```csharp
using Ultramaverick.Catalog.Domain.Events;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Catalog.Domain.Entities;

public sealed class Item : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public string ItemCode { get; private set; } = null!;
    public string ItemDescription { get; private set; } = null!;
    public int ItemCategoryId { get; private set; }
    public int UomId { get; private set; }
    public decimal? BufferLevel { get; private set; }
    public bool IsExpirable { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    private Item() { }

    public static Item Create(
        string itemCode, string itemDescription, int itemCategoryId, int uomId,
        decimal? bufferLevel, bool isExpirable, int? createdByUserId)
    {
        itemCode = (itemCode ?? string.Empty).Trim();
        itemDescription = (itemDescription ?? string.Empty).Trim();

        ArgumentException.ThrowIfNullOrWhiteSpace(itemCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(itemDescription);
        if (itemCategoryId <= 0) throw new ArgumentOutOfRangeException(nameof(itemCategoryId));
        if (uomId <= 0) throw new ArgumentOutOfRangeException(nameof(uomId));

        var item = new Item
        {
            ItemCode = itemCode,
            ItemDescription = itemDescription,
            ItemCategoryId = itemCategoryId,
            UomId = uomId,
            BufferLevel = bufferLevel,
            IsExpirable = isExpirable,
            IsActive = true,
            CreatedByUserId = createdByUserId
        };

        item.RaiseDomainEvent(new ItemChanged(ItemChangeTypes.Created, DateTime.UtcNow));
        return item;
    }

    public void Update(string itemDescription, int itemCategoryId, int uomId,
        decimal? bufferLevel, bool isExpirable, int modifiedByUserId)
    {
        itemDescription = (itemDescription ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(itemDescription);

        ItemDescription = itemDescription;
        ItemCategoryId = itemCategoryId;
        UomId = uomId;
        BufferLevel = bufferLevel;
        IsExpirable = isExpirable;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;

        RaiseDomainEvent(new ItemChanged(ItemChangeTypes.Updated, DateTime.UtcNow));
    }

    public void SetActive(bool active, int modifiedByUserId)
    {
        if (IsActive == active) return;
        IsActive = active;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new ItemChanged(active ? ItemChangeTypes.Activated : ItemChangeTypes.Deactivated, DateTime.UtcNow));
    }
}
```

### The simple lookup families (generated)

All the pure lookups (`ProductTypes`, `Dispositions`, `Analyses`, `Parameters`, `SampleTypes`, `SwabTypes`, `ProductConditions`, `TransactionTypes`, `AccountTitles`, `Locations`, `LotCategories`) share one shape. Generate them rather than typing them:

```powershell
$simple = @(
  "ProductType","Disposition","Analysis","Parameter","SampleType","SwabType",
  "ProductCondition","TransactionType","AccountTitle","Location","LotCategory"
)

foreach ($n in $simple) {
  $domain = @"
using Ultramaverick.Catalog.Domain.Events;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Catalog.Domain.Entities;

public sealed class $n : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public string? Code { get; private set; }
    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    private $n() { }

    public static $n Create(string name, int? createdByUserId, string? code = null)
    {
        name = (name ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var e = new $n { Name = name, Code = code?.Trim(), IsActive = true, CreatedByUserId = createdByUserId };
        e.RaiseDomainEvent(new ${n}Changed("Created", DateTime.UtcNow));
        return e;
    }

    public void SetActive(bool active, int modifiedByUserId)
    {
        if (IsActive == active) return;
        IsActive = active;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new ${n}Changed(active ? "Activated" : "Deactivated", DateTime.UtcNow));
    }
}
"@
  Set-Content -Path "src/Modules/Catalog/Ultramaverick.Catalog.Domain/Entities/$n.cs" -Value $domain -Encoding utf8

  $event = @"
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Catalog.Domain.Events;

public sealed record ${n}Changed(string ChangeType, DateTime OccurredAtUtc) : IDomainEvent;
"@
  Set-Content -Path "src/Modules/Catalog/Ultramaverick.Catalog.Domain/Events/${n}Changed.cs" -Value $event -Encoding utf8
}
```

### Supplier, Farm, and Customer (master data)

```csharp
public sealed class Supplier : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public string SupplierCode { get; private set; } = null!;
    public string SupplierName { get; private set; } = null!;
    public string? SupplierAddress { get; private set; }
    public bool IsActive { get; private set; }
    // + audit columns

    private Supplier() { }

    public static Supplier Create(string code, string name, string? address, int? createdByUserId)
    {
        code = (code ?? string.Empty).Trim();
        name = (name ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var s = new Supplier { SupplierCode = code, SupplierName = name, SupplierAddress = address?.Trim(), IsActive = true, CreatedByUserId = createdByUserId };
        s.RaiseDomainEvent(new SupplierChanged(SupplierChangeTypes.Created, DateTime.UtcNow));
        return s;
    }

    public void SetActive(bool active, int modifiedByUserId) { /* same shape */ }
    public static class SupplierChangeTypes
    {
        public const string Created = nameof(Created);
        public const string Updated = nameof(Updated);
        public const string Activated = nameof(Activated);
        public const string Deactivated = nameof(Deactivated);
    }
}
```

```csharp
public sealed class Customer : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public string CustomerCode { get; private set; } = null!;
    public string CustomerName { get; private set; } = null!;
    public int? FarmId { get; private set; }
    public string? CompanyCode { get; private set; }
    public string? CompanyName { get; private set; }
    public int? AccountTitleId { get; private set; }
    public int? LocationId { get; private set; }
    public int? DepartmentId { get; private set; }   // scalar cross-context, no FK
    public string? MobileNumber { get; private set; }
    public string? LeadMan { get; private set; }
    public string? Address { get; private set; }
    public bool IsActive { get; private set; }
    // + audit columns

    private Customer() { }

    public static Customer Create(
        string code, string name, int? farmId, string? companyCode, string? companyName,
        int? accountTitleId, int? locationId, int? departmentId,
        string? mobileNumber, string? leadMan, string? address, int? createdByUserId)
    {
        code = (code ?? string.Empty).Trim();
        name = (name ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var c = new Customer
        {
            CustomerCode = code, CustomerName = name, FarmId = farmId,
            CompanyCode = companyCode, CompanyName = companyName,
            AccountTitleId = accountTitleId, LocationId = locationId, DepartmentId = departmentId,
            MobileNumber = mobileNumber, LeadMan = leadMan, Address = address,
            IsActive = true, CreatedByUserId = createdByUserId
        };
        c.RaiseDomainEvent(new CustomerChanged("Created", DateTime.UtcNow));
        return c;
    }
}
```

> **Cross-context rule:** `DepartmentId` on `Customer` is a **scalar** reference into `Identity.Departments`. There is no database FK and no EF navigation — modules stay independent. Enforce the reference in the handler by calling an Identity query port.

### Formula aggregate (header + requirement child)

```csharp
public sealed class FormulaHeader : Entity
{
    private readonly List<FormulaRequirement> _requirements = new();

    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int ItemId { get; private set; }
    public int Version { get; private set; }
    public decimal Quantity { get; private set; }
    public bool IsActive { get; private set; }
    // + audit columns

    public IReadOnlyCollection<FormulaRequirement> Requirements => _requirements.AsReadOnly();

    private FormulaHeader() { }

    public static FormulaHeader Create(int itemId, int version, decimal quantity, int? createdByUserId)
    {
        if (itemId <= 0) throw new ArgumentOutOfRangeException(nameof(itemId));
        if (version <= 0) throw new ArgumentOutOfRangeException(nameof(version));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

        var f = new FormulaHeader { ItemId = itemId, Version = version, Quantity = quantity, IsActive = true, CreatedByUserId = createdByUserId };
        f.RaiseDomainEvent(new FormulaChanged("Created", DateTime.UtcNow));
        return f;
    }

    public void AddRequirement(int rawMaterialId, decimal quantity)
    {
        if (_requirements.Any(r => r.RawMaterialId == rawMaterialId))
            throw new InvalidOperationException($"Requirement for item {rawMaterialId} already exists.");
        _requirements.Add(FormulaRequirement.Create(Id, rawMaterialId, quantity));
        RaiseDomainEvent(new FormulaChanged("Updated", DateTime.UtcNow));
    }
}

public sealed class FormulaRequirement : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int FormulaHeaderId { get; private set; }
    public int RawMaterialId { get; private set; }
    public decimal Quantity { get; private set; }
    public bool IsActive { get; private set; }

    private FormulaRequirement() { }

    internal static FormulaRequirement Create(int headerId, int rawMaterialId, decimal quantity)
        => new() { FormulaHeaderId = headerId, RawMaterialId = rawMaterialId, Quantity = quantity, IsActive = true };
}
```

## 5.3 Step 3.3 — The Catalog DbContext

```csharp
public sealed class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    public DbSet<Uom> Uoms => Set<Uom>();
    public DbSet<ItemCategory> ItemCategories => Set<ItemCategory>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Farm> Farms => Set<Farm>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<AccountTitle> AccountTitles => Set<AccountTitle>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<LotCategory> LotCategories => Set<LotCategory>();
    public DbSet<LotName> LotNames => Set<LotName>();
    public DbSet<Reason> Reasons => Set<Reason>();
    public DbSet<TransactionType> TransactionTypes => Set<TransactionType>();
    public DbSet<ProductType> ProductTypes => Set<ProductType>();
    public DbSet<Disposition> Dispositions => Set<Disposition>();
    public DbSet<Analysis> Analyses => Set<Analysis>();
    public DbSet<Parameter> Parameters => Set<Parameter>();
    public DbSet<SampleType> SampleTypes => Set<SampleType>();
    public DbSet<SwabType> SwabTypes => Set<SwabType>();
    public DbSet<ProductCondition> ProductConditions => Set<ProductCondition>();
    public DbSet<FormulaHeader> FormulaHeaders => Set<FormulaHeader>();
    public DbSet<FormulaRequirement> FormulaRequirements => Set<FormulaRequirement>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Catalog");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    // SaveChangesAsync identical to IdentityDbContext (Part 3.4.2)
}
```

Configurations — copy the pattern for each; the important ones:

```csharp
public sealed class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> b)
    {
        b.ToTable("Items", "Catalog");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);
        b.Property(x => x.ItemCode).IsRequired().HasMaxLength(50);
        b.Property(x => x.ItemDescription).IsRequired().HasMaxLength(300);
        b.Property(x => x.BufferLevel).HasPrecision(18, 4);
        b.HasIndex(x => x.ItemCode).IsUnique();
        b.HasOne<ItemCategory>().WithMany().HasForeignKey(x => x.ItemCategoryId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Uom>().WithMany().HasForeignKey(x => x.UomId).OnDelete(DeleteBehavior.Restrict);
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> b)
    {
        b.ToTable("Customers", "Catalog");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);
        b.Property(x => x.CustomerCode).IsRequired().HasMaxLength(50);
        b.Property(x => x.CustomerName).IsRequired().HasMaxLength(200);
        b.HasIndex(x => x.CustomerCode).IsUnique();
        b.HasOne<Farm>().WithMany().HasForeignKey(x => x.FarmId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<AccountTitle>().WithMany().HasForeignKey(x => x.AccountTitleId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Location>().WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
        // DepartmentId has NO relationship: it is a scalar cross-context reference.
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}

public sealed class FormulaHeaderConfiguration : IEntityTypeConfiguration<FormulaHeader>
{
    public void Configure(EntityTypeBuilder<FormulaHeader> b)
    {
        b.ToTable("FormulaHeaders", "Catalog");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);
        b.Property(x => x.Quantity).HasPrecision(18, 4);
        b.HasIndex(x => new { x.ItemId, x.Version }).IsUnique();
        b.HasMany(x => x.Requirements).WithOne().HasForeignKey(r => r.FormulaHeaderId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Requirements).HasField("_requirements").UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
```

## 5.4 Step 3.4 — A representative command set (Items)

```csharp
// Application/Commands/Items/CreateItemCommand.cs
using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Items;

public sealed record CreateItemCommand(
    string ItemCode, string ItemDescription, int ItemCategoryId, int UomId,
    decimal? BufferLevel, bool IsExpirable) : IRequest<Result<int>>;
```

```csharp
// Application/Commands/Items/CreateItemCommandHandler.cs
using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Commands.Items;

public sealed class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, Result<int>>
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public CreateItemCommandHandler(ICatalogUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow; _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(CreateItemCommand request, CancellationToken ct)
    {
        if (await _uow.Items.GetByCodeAsync(request.ItemCode, ct) is not null)
            return Result<int>.Failure($"Item code '{request.ItemCode}' already exists.");
        if (await _uow.ItemCategories.GetByIdAsync(request.ItemCategoryId, ct) is null)
            return Result<int>.Failure("Item category does not exist.");
        if (await _uow.Uoms.GetByIdAsync(request.UomId, ct) is null)
            return Result<int>.Failure("UOM does not exist.");

        var item = Item.Create(request.ItemCode, request.ItemDescription,
            request.ItemCategoryId, request.UomId, request.BufferLevel, request.IsExpirable, _currentUser.UserId);

        await _uow.Items.AddAsync(item, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<int>.Success(item.Id);
    }
}
```

```csharp
// Application/Commands/Items/CreateItemCommandValidator.cs
using FluentValidation;

namespace Ultramaverick.Catalog.Application.Commands.Items;

public sealed class CreateItemCommandValidator : AbstractValidator<CreateItemCommand>
{
    public CreateItemCommandValidator()
    {
        RuleFor(x => x.ItemCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ItemDescription).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ItemCategoryId).GreaterThan(0);
        RuleFor(x => x.UomId).GreaterThan(0);
        RuleFor(x => x.BufferLevel).GreaterThanOrEqualTo(0).When(x => x.BufferLevel.HasValue);
    }
}
```

## 5.5 Step 3.5 — Catalog controller (legacy routes)

The legacy controllers were `RawMaterialController` (`api/RawMaterial`), `UomController` (`api/Uom`), `SupplierController` (`api/Supplier`), `CustomerController` (`api/Customer`), `LotController`, `ReasonController`, `AccountTitleController`, `ProductTypeController`, `TransformationController`. Preserve those route prefixes.

```csharp
[ApiController]
[Route("api/RawMaterial")]
[Authorize(Policy = Policies.RequireModule("Catalog"))]
public sealed class ItemsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ItemsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("GetAllRawMaterialsWithPagination")]
    public async Task<ActionResult<PagedResult<ItemDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, [FromQuery] bool? status = null,
        CancellationToken ct = default)
        => Ok(await _mediator.Send(new GetItemsQuery(page, pageSize, search, status), ct));

    [HttpGet("GetRawMaterialById/{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => (await _mediator.Send(new GetItemByIdQuery(id), ct)).ToOkOrNotFound();

    [HttpPost("AddNewRawMaterial")]
    public async Task<IActionResult> Create([FromBody] CreateItemCommand cmd, CancellationToken ct)
        => (await _mediator.Send(cmd, ct)).ToOk();

    [HttpPut("UpdateRawMaterial/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateItemCommand cmd, CancellationToken ct)
        => id != cmd.ItemId ? ResultExtensions.BadRequestError("Route id and body id must match.")
                            : (await _mediator.Send(cmd, ct)).ToNoContent();

    [HttpPut("InActiveRawMaterial/{id}")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
        => (await _mediator.Send(new SetItemActiveCommand(id, false), ct)).ToNoContent();

    [HttpPut("ActivateRawMaterial/{id}")]
    public async Task<IActionResult> Activate(int id, CancellationToken ct)
        => (await _mediator.Send(new SetItemActiveCommand(id, true), ct)).ToNoContent();
}
```

Repeat the same six-action controller for `Uom`, `Supplier`, `Customer`, `LotName`, `Reason`, `AccountTitle`, `ProductType`.

## 5.6 Step 3.6 — Migration and verification

```powershell
dotnet ef migrations add InitialCatalog `
    -p src/Modules/Catalog/Ultramaverick.Catalog.Persistence `
    -s src/Ultramaverick.Api -c CatalogDbContext -o Migrations

dotnet ef database update `
    -p src/Modules/Catalog/Ultramaverick.Catalog.Persistence `
    -s src/Ultramaverick.Api -c CatalogDbContext

dotnet build Ultramaverick.slnx -c Release
dotnet test  Ultramaverick.slnx -c Release
```

### Evidence

- `Catalog` schema present in `ElixirDepotDry` with all 23 tables.
- `GET /api/Uom/GetAllUomsWithPagination` returns `200` for an authorized caller.
- Integration test writes an outbox row on `Item.Create`.

<!-- pagebreak -->

# 6. Phase 4 — Procurement

Scaffold with `$ctx = "Procurement"`. Owns import batches, purchase orders, and lines. Legacy: `POSummary` (flat header+line rows) and `ImportController`.

## 6.1 Schema

| Table | Notes |
| --- | --- |
| `Procurement.ImportBatches` | file name, imported-at/by, row count, status |
| `Procurement.PurchaseOrders` | `PoNumber` (UQ), `PrNumber`, dates, `SupplierId` (scalar→Catalog), `ImportBatchId` (FK), status |
| `Procurement.PurchaseOrderLines` | `PurchaseOrderId` (FK), `ItemId`/`UomId` (scalar→Catalog), ordered/delivered/billed qty, unit price |

## 6.2 Entities

```csharp
using Ultramaverick.Procurement.Domain.Events;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Procurement.Domain.Entities;

public sealed class PurchaseOrder : Entity
{
    private readonly List<PurchaseOrderLine> _lines = new();

    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public string PoNumber { get; private set; } = null!;
    public string? PrNumber { get; private set; }
    public DateTime? PoDate { get; private set; }
    public DateTime? PrDate { get; private set; }
    public int SupplierId { get; private set; }        // scalar → Catalog.Suppliers
    public int? ImportBatchId { get; private set; }
    public string Status { get; private set; } = "Open";
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    public IReadOnlyCollection<PurchaseOrderLine> Lines => _lines.AsReadOnly();

    private PurchaseOrder() { }

    public static PurchaseOrder Create(
        string poNumber, string? prNumber, DateTime? poDate, DateTime? prDate,
        int supplierId, int? importBatchId, int? createdByUserId)
    {
        if (supplierId <= 0) throw new ArgumentOutOfRangeException(nameof(supplierId));
        poNumber = (poNumber ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(poNumber);

        var po = new PurchaseOrder
        {
            PoNumber = poNumber, PrNumber = prNumber, PoDate = poDate, PrDate = prDate,
            SupplierId = supplierId, ImportBatchId = importBatchId,
            Status = "Open", IsActive = true, CreatedByUserId = createdByUserId
        };
        po.RaiseDomainEvent(new PurchaseOrderChanged("Created", DateTime.UtcNow));
        return po;
    }

    public PurchaseOrderLine AddLine(int itemId, int uomId, decimal orderedQuantity, decimal unitPrice)
    {
        if (orderedQuantity <= 0) throw new ArgumentOutOfRangeException(nameof(orderedQuantity));

        var line = PurchaseOrderLine.Create(Id, itemId, uomId, orderedQuantity, unitPrice);
        _lines.Add(line);
        RaiseDomainEvent(new PurchaseOrderChanged("LineAdded", DateTime.UtcNow));
        return line;
    }

    public void MarkReceived(int modifiedByUserId)
    {
        if (Status == "Received") return;
        Status = "Received";
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new PurchaseOrderReceived(Id, SupplierId, DateTime.UtcNow));
    }
}
```

```csharp
public sealed class PurchaseOrderLine : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int PurchaseOrderId { get; private set; }
    public int ItemId { get; private set; }        // scalar → Catalog.Items
    public int UomId { get; private set; }         // scalar → Catalog.Uoms
    public decimal OrderedQuantity { get; private set; }
    public decimal DeliveredQuantity { get; private set; }
    public decimal BilledQuantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public bool IsActive { get; private set; }

    private PurchaseOrderLine() { }

    internal static PurchaseOrderLine Create(
        int headerId, int itemId, int uomId, decimal orderedQuantity, decimal unitPrice)
        => new()
        {
            PurchaseOrderId = headerId, ItemId = itemId, UomId = uomId,
            OrderedQuantity = orderedQuantity, UnitPrice = unitPrice, IsActive = true
        };

    internal void AddDelivery(decimal quantity) => DeliveredQuantity += quantity;
}
```

## 6.3 Events

```csharp
namespace Ultramaverick.Procurement.Domain.Events;

public sealed record PurchaseOrderChanged(string ChangeType, DateTime OccurredAtUtc) : Ultramaverick.SharedKernel.IDomainEvent;

// Published to the outbox for Quality to consume.
public sealed record PurchaseOrderReceived(int PurchaseOrderId, int SupplierId, DateTime OccurredAtUtc)
    : Ultramaverick.SharedKernel.IDomainEvent;
```

## 6.4 Command handler and aggregation

```csharp
// Application/Commands/PurchaseOrders/CreatePurchaseOrderCommand.cs
public sealed record PurchaseOrderLineInput(int ItemId, int UomId, decimal Quantity, decimal UnitPrice);
public sealed record CreatePurchaseOrderCommand(
    string PoNumber, string? PrNumber, DateTime? PoDate, DateTime? PrDate,
    int SupplierId, IReadOnlyList<PurchaseOrderLineInput> Lines) : IRequest<Result<int>>;
```

```csharp
// Application/Commands/PurchaseOrders/CreatePurchaseOrderCommandHandler.cs
public sealed class CreatePurchaseOrderCommandHandler : IRequestHandler<CreatePurchaseOrderCommand, Result<int>>
{
    private readonly IProcurementUnitOfWork _uow;
    private readonly ISupplierLookup _suppliers;   // query port into Catalog
    private readonly ICurrentUser _currentUser;

    public CreatePurchaseOrderCommandHandler(
        IProcurementUnitOfWork uow, ISupplierLookup suppliers, ICurrentUser currentUser)
    {
        _uow = uow; _suppliers = suppliers; _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(CreatePurchaseOrderCommand request, CancellationToken ct)
    {
        if (await _uow.PurchaseOrders.GetByNumberAsync(request.PoNumber, ct) is not null)
            return Result<int>.Failure($"PO '{request.PoNumber}' already exists.");

        if (!await _suppliers.ExistsAsync(request.SupplierId, ct))
            return Result<int>.Failure("Supplier does not exist.");

        var po = PurchaseOrder.Create(request.PoNumber, request.PrNumber, request.PoDate,
            request.PrDate, request.SupplierId, importBatchId: null, _currentUser.UserId);

        foreach (var line in request.Lines)
            po.AddLine(line.ItemId, line.UomId, line.Quantity, line.UnitPrice);

        await _uow.PurchaseOrders.AddAsync(po, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<int>.Success(po.Id);
    }
}
```

The `ISupplierLookup` port is implemented in `Procurement.Persistence` by reading the **Catalog** schema directly (a read-only cross-schema query, no shared entity):

```csharp
// Persistence/Lookups/SupplierLookup.cs
public sealed class SupplierLookup : ISupplierLookup
{
    private readonly ProcurementDbContext _context;
    public SupplierLookup(ProcurementDbContext context) => _context = context;

    public Task<bool> ExistsAsync(int supplierId, CancellationToken ct)
        => _context.Database
            .SqlQuery<bool>($"SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM [Catalog].[Suppliers] WHERE [Id] = {supplierId} AND [IsActive] = 1) THEN 1 ELSE 0 END AS BIT) AS [Value]")
            .SingleAsync(ct);
}
```

> Alternative (preferred at scale): a Catalog query port exposed over the application layer. Both keep the modules decoupled; the raw SQL version avoids a project reference but couples to the schema. Choose per your tolerance.

## 6.5 Controller

```csharp
[ApiController]
[Route("api/Ordering")]
[Authorize(Policy = Policies.RequireModule("Procurement"))]
public sealed class PurchaseOrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    public PurchaseOrdersController(IMediator mediator) => _mediator = mediator;

    [HttpPost("AddPurchaseOrder")]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderCommand cmd, CancellationToken ct)
        => (await _mediator.Send(cmd, ct)).ToOk();

    [HttpGet("GetPurchaseOrders")]
    public async Task<ActionResult<PagedResult<PurchaseOrderDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, CancellationToken ct = default)
        => Ok(await _mediator.Send(new GetPurchaseOrdersQuery(page, pageSize, search), ct));
}
```

<!-- pagebreak -->

# 7. Phase 5 — Quality

Scaffold with `$ctx = "Quality"`. Owns QC receipts, rejects, and the checklist engine. Legacy: `QC_Receiving`, `QC_Reject`, and the eleven checklist tables.

## 7.1 Schema

| Table | Notes |
| --- | --- |
| `Quality.QcReceipts` | links `PurchaseOrderId` (scalar→Procurement), `ItemId`/`ProductTypeId` (scalar→Catalog), batch/expiry dates, delivered/reject qty, status |
| `Quality.QcRejects` | `QcReceiptId` (FK), quantity, `ReasonId` (scalar) |
| `Quality.ChecklistTypes` | template header, `ProductTypeId` (scalar) |
| `Quality.ChecklistQuestions` | `ChecklistTypeId` (FK), question, `AnswerType` (0=YesNo,1=OpenField,2=Dimensions) |
| `Quality.Checklists` | a filled checklist per receipt |
| `Quality.ChecklistAnswers` | Yes/No answers (UQ Checklist+Question) |
| `Quality.ChecklistOpenFieldAnswers` | free-text answers |
| `Quality.ChecklistProductDimensions` | standard/actual values |
| `Quality.ChecklistOtherObservations`, `ChecklistCompliances` | narrative rows |
| `Quality.ChecklistReviewVerificationLogs` | the review/verify decision and disposition |

## 7.2 The receipt aggregate consuming `PurchaseOrderReceived`

```csharp
using Ultramaverick.Quality.Domain.Events;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Quality.Domain.Entities;

public sealed class QcReceipt : Entity
{
    private readonly List<QcReject> _rejects = new();
    private readonly List<Checklist> _checklists = new();

    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int PurchaseOrderId { get; private set; }        // scalar → Procurement
    public int? PurchaseOrderLineId { get; private set; }
    public int ItemId { get; private set; }                 // scalar → Catalog.Items
    public int? ProductTypeId { get; private set; }
    public string? BatchNo { get; private set; }
    public DateTime? ManufacturingDate { get; private set; }
    public DateTime? ExpectedDeliveryDate { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public decimal ActualDelivered { get; private set; }
    public decimal TotalReject { get; private set; }
    public bool IsNearlyExpire { get; private set; }
    public int? ExpiryApprovedByUserId { get; private set; }
    public DateTime? ExpiryApprovedAtUtc { get; private set; }
    public int? QcByUserId { get; private set; }
    public int? MonitoredByUserId { get; private set; }
    public bool IsWarehouseReceived { get; private set; }
    public string Status { get; private set; } = "ForReceiving";
    public int? CancelReasonId { get; private set; }
    public string? CancelRemarks { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<QcReject> Rejects => _rejects.AsReadOnly();
    public IReadOnlyCollection<Checklist> Checklists => _checklists.AsReadOnly();

    private QcReceipt() { }

    public static QcReceipt Create(int purchaseOrderId, int itemId, int? productTypeId, string? batchNo, decimal actualDelivered)
    {
        if (purchaseOrderId <= 0) throw new ArgumentOutOfRangeException(nameof(purchaseOrderId));
        if (itemId <= 0) throw new ArgumentOutOfRangeException(nameof(itemId));

        var r = new QcReceipt
        {
            PurchaseOrderId = purchaseOrderId, ItemId = itemId, ProductTypeId = productTypeId,
            BatchNo = batchNo, ActualDelivered = actualDelivered, Status = "ForReceiving", IsActive = true
        };
        r.RaiseDomainEvent(new QcReceiptChanged("Created", DateTime.UtcNow));
        return r;
    }

    public void Reject(decimal quantity, int? reasonId, string? remarks, int? rejectedByUserId)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (quantity > ActualDelivered - TotalReject)
            throw new InvalidOperationException("Reject quantity exceeds remaining delivered quantity.");

        _rejects.Add(QcReject.Create(Id, quantity, reasonId, remarks, rejectedByUserId));
        TotalReject += quantity;
        RaiseDomainEvent(new QcReceiptChanged("Rejected", DateTime.UtcNow));
    }

    public void ApproveExpiry(int approvedByUserId)
    {
        if (!IsNearlyExpire) return;
        IsNearlyExpire = false;
        ExpiryApprovedByUserId = approvedByUserId;
        ExpiryApprovedAtUtc = DateTime.UtcNow;
        RaiseDomainEvent(new QcReceiptChanged("ExpiryApproved", DateTime.UtcNow));
    }

    public void Complete(int qcByUserId)
    {
        if (Status == "Completed") return;
        Status = "Completed";
        QcByUserId = qcByUserId;
        RaiseDomainEvent(new QcReceiptCompleted(Id, ItemId, ActualDelivered - TotalReject, DateTime.UtcNow));
    }
}
```

## 7.3 Checklist aggregate (largest in the system)

```csharp
public sealed class Checklist : Entity
{
    private readonly List<ChecklistAnswer> _answers = new();
    private readonly List<ChecklistOpenFieldAnswer> _openFields = new();
    private readonly List<ChecklistProductDimension> _dimensions = new();
    private readonly List<ChecklistOtherObservation> _observations = new();
    private readonly List<ChecklistCompliance> _compliances = new();

    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int QcReceiptId { get; private set; }
    public int? ProductTypeId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }

    public IReadOnlyCollection<ChecklistAnswer> Answers => _answers.AsReadOnly();
    public IReadOnlyCollection<ChecklistOpenFieldAnswer> OpenFields => _openFields.AsReadOnly();
    public IReadOnlyCollection<ChecklistProductDimension> Dimensions => _dimensions.AsReadOnly();

    private Checklist() { }

    public static Checklist For(int qcReceiptId, int? productTypeId, int? createdByUserId)
        => new() { QcReceiptId = qcReceiptId, ProductTypeId = productTypeId, CreatedByUserId = createdByUserId };

    public void Answer(int questionId, bool status) => _answers.Add(ChecklistAnswer.Create(Id, questionId, status));
    public void AnswerOpenField(int questionId, string remarks) => _openFields.Add(ChecklistOpenFieldAnswer.Create(Id, questionId, remarks));
    public void RecordDimension(int questionId, decimal? standard, decimal? actual)
        => _dimensions.Add(ChecklistProductDimension.Create(Id, questionId, standard, actual));
    public void AddObservation(string text) => _observations.Add(ChecklistOtherObservation.Create(Id, text));
    public void AddCompliance(string compliance, string? description, string? rootCause)
        => _compliances.Add(ChecklistCompliance.Create(Id, compliance, description, rootCause));

    public void Submit() => RaiseDomainEvent(new ChecklistSubmitted(Id, QcReceiptId, DateTime.UtcNow));
}
```

The five child collections each have a small entity with a `Create` factory and are mapped with `HasMany(...).WithOne().HasForeignKey(...)` + a backing-field navigation, exactly like `FormulaHeader` (Part 5.2).

## 7.4 The cross-context handler: react to a received PO

```csharp
// Application/EventHandlers/PurchaseOrderReceivedHandler.cs
using MediatR;
using Ultramaverick.Quality.Application.Abstractions;
using Ultramaverick.Quality.Domain.Entities;

namespace Ultramaverick.Quality.Application.EventHandlers;

public sealed class PurchaseOrderReceivedHandler : INotificationHandler<PurchaseOrderReceivedNotification>
{
    private readonly IQualityUnitOfWork _uow;
    private readonly ICatalogItemLookup _items;

    public PurchaseOrderReceivedHandler(IQualityUnitOfWork uow, ICatalogItemLookup items)
    {
        _uow = uow; _items = items;
    }

    public async Task Handle(PurchaseOrderReceivedNotification notification, CancellationToken ct)
    {
        // Idempotency: the dispatcher already guarantees once-per-EventId via ProcessedEvents,
        // but guard domain state too.
        if (await _uow.QcReceipts.ExistsForPurchaseOrderAsync(notification.PurchaseOrderId, ct))
            return;

        foreach (var lineId in notification.LineItemIds)
        {
            var item = await _items.GetAsync(lineId, ct);
            var receipt = QcReceipt.Create(notification.PurchaseOrderId, item.Id, item.ProductTypeId, item.BatchNo, item.Quantity);
            await _uow.QcReceipts.AddAsync(receipt, ct);
        }

        await _uow.SaveChangesAsync(ct);
    }
}
```

**Wiring the integration event across modules** — the payload is deserialized from the outbox and dispatched. Part 11.3 shows the `IIntegrationEventHandler` that bridges `PurchaseOrderReceived` (a Procurement outbox row) into this `INotification`.

## 7.5 Controller

```csharp
[ApiController]
[Route("api/Receiving")]
[Authorize(Policy = Policies.RequireModule("Quality"))]
public sealed class QcReceiptsController : ControllerBase
{
    private readonly IMediator _mediator;
    public QcReceiptsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("GetQcReceiptsWithPagination")]
    public async Task<ActionResult<PagedResult<QcReceiptDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null,
        CancellationToken ct = default)
        => Ok(await _mediator.Send(new GetQcReceiptsQuery(page, pageSize, search), ct));

    [HttpPut("Reject/{id}")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectQcReceiptCommand cmd, CancellationToken ct)
        => id != cmd.QcReceiptId ? ResultExtensions.BadRequestError("Route id and body id must match.")
                                 : (await _mediator.Send(cmd, ct)).ToNoContent();
}
```

<!-- pagebreak -->

# 8. Phase 6 — Laboratory

Scaffold with `$ctx = "Laboratory"`. Owns lab requests and their five JSON-array join tables, receive/reject/return, and results. Legacy: `LabTestRequests` (with JSON), `ReceiveRequests`, `ReturnedItems`, `RejectedItems`, `LabTestResults`.

## 8.1 Schema

| Table | Notes |
| --- | --- |
| `Laboratory.LabTestRequests` | `WarehouseReceiptId` (scalar), batch, quantity, test type, dates, status |
| `Laboratory.LabTestAnalyses` | join `(LabTestRequestId, AnalysisId)` — scalar→Catalog |
| `Laboratory.LabTestParameters` | join `(LabTestRequestId, ParameterId)` |
| `Laboratory.LabTestProductConditions` | join `(LabTestRequestId, ProductConditionId)` |
| `Laboratory.LabTestSampleTypes` | join `(LabTestRequestId, SampleTypeId)` |
| `Laboratory.LabTestSwabTypes` | join `(LabTestRequestId, SwabTypeId)` |
| `Laboratory.LabReceiveRequests` | accepted request, `DispositionId` (scalar) |
| `Laboratory.LabRejectedItems` / `LabReturnedItems` | disposition / reason outcomes |
| `Laboratory.LabTestResults` | `LabReceiveRequestId` (real FK — fixes the legacy shadow-FK defect) |

## 8.2 Aggregate with join collections

```csharp
using Ultramaverick.Laboratory.Domain.Events;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Laboratory.Domain.Entities;

public sealed class LabTestRequest : Entity
{
    private readonly HashSet<int> _analysisIds = new();
    private readonly HashSet<int> _parameterIds = new();
    private readonly HashSet<int> _productConditionIds = new();
    private readonly HashSet<int> _sampleTypeIds = new();
    private readonly HashSet<int> _swabTypeIds = new();

    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int? WarehouseReceiptId { get; private set; }   // scalar → Warehouse
    public string? BatchNo { get; private set; }
    public decimal Quantity { get; private set; }
    public string? TestType { get; private set; }
    public DateTime? DateRequested { get; private set; }
    public DateTime? DateNeeded { get; private set; }
    public string Status { get; private set; } = "Requested";
    public string? Remarks { get; private set; }
    public int? RequestedByUserId { get; private set; }
    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<int> AnalysisIds => _analysisIds;
    public IReadOnlyCollection<int> ParameterIds => _parameterIds;
    public IReadOnlyCollection<int> ProductConditionIds => _productConditionIds;
    public IReadOnlyCollection<int> SampleTypeIds => _sampleTypeIds;
    public IReadOnlyCollection<int> SwabTypeIds => _swabTypeIds;

    private LabTestRequest() { }

    public static LabTestRequest Create(
        int? warehouseReceiptId, string? batchNo, decimal quantity, string? testType,
        DateTime? dateRequested, DateTime? dateNeeded, int? requestedByUserId)
    {
        var r = new LabTestRequest
        {
            WarehouseReceiptId = warehouseReceiptId, BatchNo = batchNo, Quantity = quantity,
            TestType = testType, DateRequested = dateRequested, DateNeeded = dateNeeded,
            RequestedByUserId = requestedByUserId, Status = "Requested", IsActive = true
        };
        r.RaiseDomainEvent(new LabTestRequested(r.Id, DateTime.UtcNow));
        return r;
    }

    public void TagAnalyses(IEnumerable<int> ids) { foreach (var id in ids) _analysisIds.Add(id); }
    public void TagParameters(IEnumerable<int> ids) { foreach (var id in ids) _parameterIds.Add(id); }
    public void TagProductConditions(IEnumerable<int> ids) { foreach (var id in ids) _productConditionIds.Add(id); }
    public void TagSampleTypes(IEnumerable<int> ids) { foreach (var id in ids) _sampleTypeIds.Add(id); }
    public void TagSwabTypes(IEnumerable<int> ids) { foreach (var id in ids) _swabTypeIds.Add(id); }
}
```

Configurations map each `HashSet<int>` to a join table using `.ToTable(...)` and a shadow-free composite key:

```csharp
public sealed class LabTestRequestConfiguration : IEntityTypeConfiguration<LabTestRequest>
{
    public void Configure(EntityTypeBuilder<LabTestRequest> b)
    {
        b.ToTable("LabTestRequests", "Laboratory");
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.OwnsMany?  // not owned: use a join entity
    }
}
```

Because EF cannot map a `HashSet<int>` directly, model each join as a small entity and keep the aggregate's collection typed as that entity (the guide's pattern). Replace `HashSet<int>` with `List<LabTestAnalysis>` etc., and configure:

```csharp
b.HasMany<LabTestAnalysis>().WithOne().HasForeignKey(a => a.LabTestRequestId).OnDelete(DeleteBehavior.Cascade);
b.Metadata.FindNavigation(nameof(LabTestRequest.Analyses))!.SetPropertyAccessMode(PropertyAccessMode.Field);
b.Metadata.FindProperty("_analysisIds")!.SetIsUnmapped();
```

```csharp
public sealed class LabTestAnalysis
{
    public int LabTestRequestId { get; set; }
    public int AnalysisId { get; set; }     // scalar → Catalog.Analyses
}
```

## 8.3 Results (fix the legacy defect)

```csharp
public sealed class LabTestResult : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int LabReceiveRequestId { get; private set; }   // real FK
    public DateTime? DateAnalyzed { get; private set; }
    public string? Result { get; private set; }
    public string Status { get; private set; } = "ForQaApproval";
    public string? Remarks { get; private set; }
    public int? AnalyzedByUserId { get; private set; }

    private LabTestResult() { }

    public static LabTestResult Record(int labReceiveRequestId, DateTime analyzedAt, string result, int analyzedByUserId)
    {
        var r = new LabTestResult
        {
            LabReceiveRequestId = labReceiveRequestId, DateAnalyzed = analyzedAt,
            Result = result, AnalyzedByUserId = analyzedByUserId, Status = "ForQaApproval"
        };
        r.RaiseDomainEvent(new LabResultRecorded(r.Id, labReceiveRequestId, result, DateTime.UtcNow));
        return r;
    }

    public void Approve() { Status = "Approved"; RaiseDomainEvent(new LabResultRecorded(Id, LabReceiveRequestId, Result!, DateTime.UtcNow)); }
}
```

## 8.4 Controller

```csharp
[ApiController]
[Route("api/LaboratoryTest")]
[Authorize(Policy = Policies.RequireModule("Laboratory"))]
public sealed class LabTestsController : ControllerBase
{
    private readonly IMediator _mediator;
    public LabTestsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("AddLabTestRequest")]
    public async Task<IActionResult> Create([FromBody] CreateLabTestRequestCommand cmd, CancellationToken ct)
        => (await _mediator.Send(cmd, ct)).ToOk();

    [HttpPut("RecordResult/{id}")]
    public async Task<IActionResult> Record(int id, [FromBody] RecordLabResultCommand cmd, CancellationToken ct)
        => id != cmd.LabReceiveRequestId ? ResultExtensions.BadRequestError("Route id and body id must match.")
                                         : (await _mediator.Send(cmd, ct)).ToNoContent();
}
```

<!-- pagebreak -->

# 9. Phase 7 — Warehouse

Scaffold with `$ctx = "Warehouse"`. Owns warehouse masters, receipts (fed by Quality), inventory balances/movements, reservations, and miscellaneous receipt/issue. Legacy: `Warehouses`, `WarehouseReceipt`, `Inventory`, `InventoryMovement`, `MiscReceipt`, `MiscIssue`.

## 9.1 Schema

| Table | Notes |
| --- | --- |
| `Warehouse.Warehouses` | code (UQ), name, type (`WarehouseTypeId`→Catalog scalar), address |
| `Warehouse.WarehouseReceipts` | `QcReceiptId` (UQ, scalar→Quality), `WarehouseId`/`ItemId`/`UomId`, lot, qty, received-at/by |
| `Warehouse.InventoryBalances` | `(WarehouseId, ItemId, UomId, LotNo)` UQ; on-hand quantity — the hot row |
| `Warehouse.InventoryMovements` | append-only ledger; `+/-` quantity, source document ref, timestamp |
| `Warehouse.StockReservations` | `(OrderId, ItemId, WarehouseId)` held quantity, status |
| `Warehouse.MiscellaneousReceipts` | header + `...Lines` |
| `Warehouse.MiscellaneousIssues` | header + `...Lines` |

> **Design note:** on-hand is always derived from `InventoryMovements` (ledger) and *cached* in `InventoryBalances`. Every mutation writes a movement row and updates the balance in the same transaction. This is the invariant the Reporting projector re-validates.

## 9.2 Receipt aggregate consuming `QcReceiptCompleted`

```csharp
using Ultramaverick.SharedKernel;
using Ultramaverick.Warehouse.Domain.Events;

namespace Ultramaverick.Warehouse.Domain.Entities;

public sealed class WarehouseReceipt : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int QcReceiptId { get; private set; }        // scalar → Quality (UQ)
    public int WarehouseId { get; private set; }
    public int ItemId { get; private set; }
    public int UomId { get; private set; }
    public string? LotNo { get; private set; }
    public decimal Quantity { get; private set; }
    public DateTime? ManufacturingDate { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public DateTime ReceivedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? ReceivedByUserId { get; private set; }
    public bool IsActive { get; private set; } = true;

    private WarehouseReceipt() { }

    public static WarehouseReceipt Create(
        int qcReceiptId, int warehouseId, int itemId, int uomId,
        string? lotNo, decimal quantity, DateTime? manufacturingDate, DateTime? expiryDate, int? receivedBy)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        var r = new WarehouseReceipt
        {
            QcReceiptId = qcReceiptId, WarehouseId = warehouseId, ItemId = itemId, UomId = uomId,
            LotNo = lotNo, Quantity = quantity, ManufacturingDate = manufacturingDate,
            ExpiryDate = expiryDate, ReceivedByUserId = receivedBy
        };
        r.RaiseDomainEvent(new StockReceived(r.Id, itemId, uomId, warehouseId, lotNo, quantity, DateTime.UtcNow));
        return r;
    }
}
```

## 9.3 Inventory service — the ledger invariant

```csharp
// Domain/Services/InventoryLedger.cs
public sealed class InventoryLedger
{
    private readonly IWarehouseUnitOfWork _uow;
    public InventoryLedger(IWarehouseUnitOfWork uow) => _uow = uow;

    public async Task ApplyMovementAsync(
        int warehouseId, int itemId, int uomId, string? lotNo, decimal delta,
        MovementSource source, int? sourceDocumentId, CancellationToken ct)
    {
        var balance = await _uow.InventoryBalances
            .GetForUpdateAsync(warehouseId, itemId, uomId, lotNo, ct)
            ?? InventoryBalance.Create(warehouseId, itemId, uomId, lotNo);

        balance.Apply(delta);                         // throws if it would go negative
        _uow.InventoryBalances.Upsert(balance);

        _uow.InventoryMovements.Add(
            InventoryMovement.Create(warehouseId, itemId, uomId, lotNo, delta, source, sourceDocumentId));
    }
}
```

```csharp
public sealed class InventoryBalance : Entity
{
    public int Id { get; private set; }
    public int WarehouseId { get; private set; }
    public int ItemId { get; private set; }
    public int UomId { get; private set; }
    public string? LotNo { get; private set; }
    public decimal OnHand { get; private set; }
    public byte[] RowVersion { get; private set; } = null!;   // optimistic concurrency

    private InventoryBalance() { }

    public static InventoryBalance Create(int warehouseId, int itemId, int uomId, string? lotNo)
        => new() { WarehouseId = warehouseId, ItemId = itemId, UomId = uomId, LotNo = lotNo };

    public void Apply(decimal delta)
    {
        var next = OnHand + delta;
        if (next < 0) throw new InvalidOperationException($"Insufficient stock for item {ItemId} (on hand {OnHand}, requested {delta}).");
        OnHand = next;
    }
}
```

`RowVersion` is mapped `b.Property(x => x.RowVersion).IsRowVersion();` so two concurrent receipts/orders on the same balance cause one to throw `DbUpdateConcurrencyException`, which the UnitOfWork retries once.

## 9.4 Cross-context handler (Quality → Warehouse)

```csharp
public sealed class QcReceiptCompletedHandler : INotificationHandler<QcReceiptCompletedNotification>
{
    private readonly IWarehouseUnitOfWork _uow;
    private readonly InventoryLedger _ledger;
    private readonly IWarehouseSelector _selector;   // picks default warehouse for the item

    public QcReceiptCompletedHandler(IWarehouseUnitOfWork uow, InventoryLedger ledger, IWarehouseSelector selector)
    { _uow = uow; _ledger = ledger; _selector = selector; }

    public async Task Handle(QcReceiptCompletedNotification n, CancellationToken ct)
    {
        if (await _uow.WarehouseReceipts.ExistsForQcReceiptAsync(n.QcReceiptId, ct)) return;  // idempotent

        var warehouseId = await _selector.DefaultForItemAsync(n.ItemId, ct);
        var receipt = WarehouseReceipt.Create(n.QcReceiptId, warehouseId, n.ItemId, n.UomId, n.LotNo, n.NetQuantity, n.ManufacturingDate, n.ExpiryDate, n.CompletedBy);
        await _uow.WarehouseReceipts.AddAsync(receipt, ct);

        await _ledger.ApplyMovementAsync(warehouseId, n.ItemId, n.UomId, n.LotNo, n.NetQuantity,
            MovementSource.QcReceipt, receipt.Id, ct);

        await _uow.SaveChangesAsync(ct);
    }
}
```

## 9.5 Controller

```csharp
[ApiController]
[Route("api/WarehouseReceiving")]
[Authorize(Policy = Policies.RequireModule("Warehouse"))]
public sealed class WarehouseReceiptsController : ControllerBase
{
    private readonly IMediator _mediator;
    public WarehouseReceiptsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("GetWarehouseReceiptsWithPagination")]
    public async Task<ActionResult<PagedResult<WarehouseReceiptDto>>> Get(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await _mediator.Send(new GetWarehouseReceiptsQuery(page, pageSize), ct));

    [HttpGet("GetInventoryBalance")]
    public async Task<ActionResult<IReadOnlyList<InventoryBalanceDto>>> Balance(
        [FromQuery] int? warehouseId, [FromQuery] int? itemId, CancellationToken ct = default)
        => Ok(await _mediator.Send(new GetInventoryBalanceQuery(warehouseId, itemId), ct));
}
```

<!-- pagebreak -->

# 10. Phase 8 — Orders

Scaffold with `$ctx = "Orders"`. Owns sales/production order lifecycle, moves, and reservations. Legacy: `Order`, `OrderLine`, `MoveOrder`, `TransactedMoveOrder`, `CancelledOrder`, `AdvanceToEmployees`.

## 10.1 Schema

| Table | Notes |
| --- | --- |
| `Orders.Orders` | `OrderNumber` (UQ), `CustomerId` (scalar→Catalog), dates, status |
| `Orders.OrderLines` | `OrderId` (FK), `ItemId`/`UomId`, quantity, `FormulaHeaderId` (scalar→Catalog) |
| `Orders.MoveOrders` | move instruction: from/to warehouse, status |
| `Orders.TransactedMoveOrders` | completed move result (immutable) |
| `Orders.CancelledOrders` | `OrderId` (FK), reason |
| `Orders.AdvanceToEmployees` | advance/assignment records |

## 10.2 Aggregate with a number generator

```csharp
using Ultramaverick.Orders.Domain.Events;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Orders.Domain.Entities;

public sealed class Order : Entity
{
    private readonly List<OrderLine> _lines = new();

    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public string OrderNumber { get; private set; } = null!;
    public int CustomerId { get; private set; }
    public DateTime? OrderDate { get; private set; }
    public DateTime? DeliveryDate { get; private set; }
    public string Status { get; private set; } = "Draft";
    public string? Remarks { get; private set; }
    public int? CreatedByUserId { get; private set; }
    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    private Order() { }

    public static Order Create(string orderNumber, int customerId, DateTime? orderDate, DateTime? deliveryDate, int? createdBy, IEnumerable<OrderLineInput> lines)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderNumber);
        if (customerId <= 0) throw new ArgumentOutOfRangeException(nameof(customerId));

        var order = new Order
        {
            OrderNumber = orderNumber, CustomerId = customerId, OrderDate = orderDate,
            DeliveryDate = deliveryDate, CreatedByUserId = createdBy, Status = "Draft"
        };
        foreach (var l in lines)
            order._lines.Add(OrderLine.Create(order.Id, l.ItemId, l.UomId, l.Quantity, l.FormulaHeaderId));

        order.RaiseDomainEvent(new OrderCreated(order.Id, orderNumber, customerId, DateTime.UtcNow));
        return order;
    }

    public void Confirm() { Status = "Confirmed"; RaiseDomainEvent(new OrderConfirmed(Id, DateTime.UtcNow)); }
    public void Cancel(string reason) { Status = "Cancelled"; RaiseDomainEvent(new OrderCancelled(Id, reason, DateTime.UtcNow)); }
}
```

```csharp
public sealed record OrderLineInput(int ItemId, int UomId, decimal Quantity, int? FormulaHeaderId);
```

## 10.3 Order number generator (per-scope sequence)

```csharp
// Persistence/OrderNumberGenerator.cs
public sealed class OrderNumberGenerator : IOrderNumberGenerator
{
    private readonly OrdersDbContext _context;
    public OrderNumberGenerator(OrdersDbContext context) => _context = context;

    public async Task<string> NextAsync(DateTime utcNow, CancellationToken ct)
    {
        var prefix = $"SO-{utcNow:yyyyMMdd}-";
        var last = await _context.Orders
            .Where(o => o.OrderNumber.StartsWith(prefix))
            .OrderByDescending(o => o.OrderNumber)
            .Select(o => o.OrderNumber)
            .FirstOrDefaultAsync(ct);

        var next = last is null ? 1 : int.Parse(last[^4..]) + 1;
        return $"{prefix}{next:D4}";
    }
}
```

## 10.4 Command handler

```csharp
public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<int>>
{
    private readonly IOrdersUnitOfWork _uow;
    private readonly IOrderNumberGenerator _numbers;
    private readonly ICustomerLookup _customers;
    private readonly ICurrentUser _currentUser;

    public CreateOrderCommandHandler(IOrdersUnitOfWork uow, IOrderNumberGenerator numbers, ICustomerLookup customers, ICurrentUser currentUser)
    { _uow = uow; _numbers = numbers; _customers = customers; _currentUser = currentUser; }

    public async Task<Result<int>> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        if (!await _customers.ExistsAsync(request.CustomerId, ct))
            return Result<int>.Failure("Customer does not exist.");
        if (request.Lines.Count == 0)
            return Result<int>.Failure("An order must have at least one line.");

        var orderNumber = await _numbers.NextAsync(DateTime.UtcNow, ct);
        var order = Order.Create(orderNumber, request.CustomerId, request.OrderDate, request.DeliveryDate,
            _currentUser.UserId, request.Lines);

        await _uow.Orders.AddAsync(order, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<int>.Success(order.Id);
    }
}
```

## 10.5 Controller

```csharp
[ApiController]
[Route("api/Ordering")]
[Authorize(Policy = Policies.RequireModule("Orders"))]
public sealed class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    public OrdersController(IMediator mediator) => _mediator = mediator;

    [HttpPost("AddOrder")]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand cmd, CancellationToken ct)
        => (await _mediator.Send(cmd, ct)).ToOk();

    [HttpPut("CancelOrder/{id}")]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelOrderCommand cmd, CancellationToken ct)
        => id != cmd.OrderId ? ResultExtensions.BadRequestError("Route id and body id must match.")
                             : (await _mediator.Send(cmd, ct)).ToNoContent();
}
```

<!-- pagebreak -->

# 11. Phase 9 — Manufacturing

Scaffold with `$ctx = "Manufacturing"`. Owns production plans and transformation requests. Legacy: `ProductionPlan`, `TransformationRequest`, `TransformationRequestLine`, `TransformationRequestPreparation`, `TransformationRequestReject`.

## 11.1 Schema

| Table | Notes |
| --- | --- |
| `Manufacturing.ProductionPlans` | `FormulaHeaderId` (scalar→Catalog), planned/batch quantity, dates, status |
| `Manufacturing.TransformationRequests` | plan link, `WarehouseId`, status |
| `Manufacturing.TransformationRequestLines` | input `ItemId`, required/actual quantity |
| `Manufacturing.TransformationRequestPreparations` | prepared-by, prepared-at, quantity |
| `Manufacturing.TransformationRequestRejects` | reject reason, quantity |

## 11.2 Transformation request aggregate

```csharp
using Ultramaverick.Manufacturing.Domain.Events;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Manufacturing.Domain.Entities;

public sealed class TransformationRequest : Entity
{
    private readonly List<TransformationRequestLine> _lines = new();
    private readonly List<TransformationRequestPreparation> _preparations = new();
    private readonly List<TransformationRequestReject> _rejects = new();

    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int ProductionPlanId { get; private set; }
    public int WarehouseId { get; private set; }
    public string Status { get; private set; } = "Requested";
    public DateTime? RequestedAtUtc { get; private set; }
    public int? RequestedByUserId { get; private set; }
    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<TransformationRequestLine> Lines => _lines.AsReadOnly();

    private TransformationRequest() { }

    public static TransformationRequest Create(int productionPlanId, int warehouseId, IEnumerable<TransformationLineInput> lines, int? requestedBy)
    {
        var t = new TransformationRequest
        {
            ProductionPlanId = productionPlanId, WarehouseId = warehouseId,
            RequestedAtUtc = DateTime.UtcNow, RequestedByUserId = requestedBy, Status = "Requested"
        };
        foreach (var l in lines)
            t._lines.Add(TransformationRequestLine.Create(t.Id, l.ItemId, l.RequiredQuantity));

        t.RaiseDomainEvent(new TransformationRequested(t.Id, productionPlanId, DateTime.UtcNow));
        return t;
    }

    public void Prepare(int preparedByUserId)
    {
        Status = "Prepared";
        _preparations.Add(TransformationRequestPreparation.Create(Id, preparedByUserId));
        RaiseDomainEvent(new TransformationPrepared(Id, DateTime.UtcNow));
    }

    public void Reject(int reasonId, decimal quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        _rejects.Add(TransformationRequestReject.Create(Id, reasonId, quantity));
        RaiseDomainEvent(new TransformationRequested(Id, ProductionPlanId, DateTime.UtcNow));
    }
}

public sealed record TransformationLineInput(int ItemId, decimal RequiredQuantity);
```

## 11.3 Controller

```csharp
[ApiController]
[Route("api/Manufacturing")]
[Authorize(Policy = Policies.RequireModule("Manufacturing"))]
public sealed class ManufacturingController : ControllerBase
{
    private readonly IMediator _mediator;
    public ManufacturingController(IMediator mediator) => _mediator = mediator;

    [HttpPost("AddTransformationRequest")]
    public async Task<IActionResult> Create([FromBody] CreateTransformationRequestCommand cmd, CancellationToken ct)
        => (await _mediator.Send(cmd, ct)).ToOk();

    [HttpPut("Prepare/{id}")]
    public async Task<IActionResult> Prepare(int id, CancellationToken ct)
        => (await _mediator.Send(new PrepareTransformationRequestCommand(id), ct)).ToNoContent();
}
```

<!-- pagebreak -->

# 12. Phase 10 — Reporting

Scaffold with `$ctx = "Reporting"`. This is the **only read-heavy module** and — unlike the others — has **no write domain**. It projects integration events into denormalized read models and exposes dashboards. Legacy: the various `Get...Report` / dashboard endpoints.

## 12.1 Read models (own schema, no FK to other modules)

| Table | Projected from |
| --- | --- |
| `Reporting.InventoryPositions` | `StockReceived`, `InventoryAdjusted`, `OrderFulfilled` |
| `Reporting.OrderFulfillment` | `OrderCreated`, `OrderConfirmed`, `OrderCancelled` |
| `Reporting.QcStatus` | `QcReceiptCompleted`, `LabResultRecorded` |
| `Reporting.ProjectionCheckpoints` | (infra) last processed outbox sequence per projection |

## 12.2 Projection handler (idempotent by event id)

```csharp
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Reporting.Application.Projections;

public sealed class InventoryPositionProjection
{
    private readonly ReportingDbContext _db;
    public InventoryPositionProjection(ReportingDbContext db) => _db = db;

    public async Task ProjectAsync(StockReceived e, CancellationToken ct)
    {
        // Checkpoint dedupe: skip if this outbox event was already applied.
        if (await _db.ProjectionCheckpoints.WasAppliedAsync(nameof(InventoryPositionProjection), e.EventId, ct))
            return;

        var pos = await _db.InventoryPositions.FindAsync(new object?[] { e.WarehouseId, e.ItemId, e.LotNo }, ct)
                  ?? _db.InventoryPositions.Add(new InventoryPosition(e.WarehouseId, e.ItemId, e.UomId, e.LotNo)).Entity;

        pos.AddOnHand(e.Quantity);

        _db.ProjectionCheckpoints.Mark(nameof(InventoryPositionProjection), e.EventId);
        await _db.SaveChangesAsync(ct);
    }
}
```

## 12.3 Reporting controller

```csharp
[ApiController]
[Route("api/Reports")]
[Authorize(Policy = Policies.RequireModule("Reporting"))]
public sealed class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ReportsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("InventoryPosition")]
    public async Task<ActionResult<IReadOnlyList<InventoryPositionDto>>> Inventory(
        [FromQuery] int? warehouseId, CancellationToken ct = default)
        => Ok(await _mediator.Send(new GetInventoryPositionQuery(warehouseId), ct));
}
```

> **Read-your-writes:** projections are eventually consistent. Anything the user just wrote must be read from the owning module's endpoint, not from Reporting. Document this in the API contract (Part 13).

<!-- pagebreak -->

# 13. Cross-cutting Concerns

These are shared by every module. They live in `src/Shared/Ultramaverick.BuildingBlocks` — but note Part 0.1: the current untracked `BuildingBlocks` folder is a buggy duplicate. **Extract the good outbox pieces into `src/Shared/Ultramaverick.SharedKernel` (already referenced) and delete the duplicate** before relying on anything in this section.

## 13.1 The outbox dispatcher (the heart of cross-module messaging)

```csharp
namespace Ultramaverick.SharedKernel.Outbox;

public sealed class OutboxDispatcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<OutboxDispatcher> _logger;
    private static readonly TimeSpan Poll = TimeSpan.FromSeconds(5);

    public OutboxDispatcher(IServiceScopeFactory scopes, ILogger<OutboxDispatcher> logger)
    { _scopes = scopes; _logger = logger; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopes.CreateScope();
            var sp = scope.ServiceProvider;
            var stores = sp.GetServices<IOutboxStore>();   // one per module that publishes

            foreach (var store in stores)
            {
                var batch = await store.DequeueAsync(50, stoppingToken);
                foreach (var message in batch)
                {
                    try
                    {
                        await DispatchAsync(sp, message, stoppingToken);
                        await store.MarkProcessedAsync(message.Id, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Outbox dispatch failed for {Type} ({Id})", message.Type, message.Id);
                        await store.MarkFailedAsync(message.Id, ex.Message, stoppingToken);
                    }
                }
            }

            await Task.Delay(Poll, stoppingToken);
        }
    }

    private static async Task DispatchAsync(IServiceProvider sp, OutboxMessage message, CancellationToken ct)
    {
        var type = Type.GetType(message.Type) ?? throw new InvalidOperationException($"Unknown event type {message.Type}");
        var payload = JsonSerializer.Deserialize(message.Payload, type)!;
        var handlers = sp.GetServices(typeof(IIntegrationEventHandler<>).MakeGenericType(type));

        foreach (var handler in handlers)
        {
            var method = handler.GetType().GetMethod("HandleAsync")!;
            await (Task)method.Invoke(handler, new[] { payload, ct })!;
        }
    }
}
```

```csharp
public interface IIntegrationEventHandler<in TEvent> where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken ct);
}

public interface IOutboxStore
{
    Task<IReadOnlyList<OutboxMessage>> DequeueAsync(int max, CancellationToken ct);
    Task MarkProcessedAsync(Guid id, CancellationToken ct);
    Task MarkFailedAsync(Guid id, string error, CancellationToken ct);
}
```

## 13.2 SaveChanges interceptor — write events in the same transaction

```csharp
namespace Ultramaverick.SharedKernel.Persistence;

public sealed class DomainEventOutboxInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        var context = eventData.Context!;
        var outbox = context.Set<OutboxMessage>();

        foreach (var entry in context.ChangeTracker.Entries<Entity>())
        {
            if (entry.Entity.DomainEvents.Count == 0) continue;
            foreach (var domainEvent in entry.Entity.DomainEvents)
            {
                var msg = OutboxMessage.From(domainEvent);
                outbox.Add(msg);
            }
            entry.Entity.ClearDomainEvents();
        }
        return result;   // the caller's SaveChanges persists entity + outbox atomically
    }
}
```

Register once per module context: `optionsBuilder.AddInterceptors(new DomainEventOutboxInterceptor());`.

## 13.3 Bridging a Procurement event into a Quality notification

```csharp
namespace Ultramaverick.Quality.Infrastructure;

public sealed class PurchaseOrderReceivedBridge : IIntegrationEventHandler<Procurement.Contracts.PurchaseOrderReceived>
{
    private readonly IMediator _mediator;
    public PurchaseOrderReceivedBridge(IMediator mediator) => _mediator = mediator;

    public Task HandleAsync(Procurement.Contracts.PurchaseOrderReceived e, CancellationToken ct)
        => _mediator.Publish(new PurchaseOrderReceivedNotification(e.PurchaseOrderId, e.SupplierId, e.LineItemIds), ct);
}
```

Each module exposes a tiny `*.Contracts` project (POCO integration events only, no EF, no domain). Both modules reference the contract project — never each other's Domain/Persistence. This is the **only** legal cross-module reference.

> **Implementation note — deliberate deviations from §13.1–13.2.** Two parts of this section are not adopted.
>
> - The `DomainEventOutboxInterceptor` (§13.2) is skipped: it runs before SQL is sent, so it cannot capture database-generated identity keys. Each module context instead writes its outbox rows inline with the two-phase `SaveChangesAsync` shown in Part 5.
> - The per-handler processed guard of §13.1 has been removed in favour of consumer-owned idempotency — the guard was never atomic (it wrote to a different schema/DbContext after the handler ran), so handlers must be idempotent themselves.
> - The integration-handler abstraction is non-generic (`IIntegrationEventHandler` exposing a `string EventType`) rather than this section's generic `IIntegrationEventHandler<TEvent>`, because the dispatcher matches a record's stored `EventType` string at runtime.

## 13.4 API versioning + module authorisation policy

```csharp
builder.Services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new ApiVersion(1, 0);
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.ReportApiVersions = true;
}).AddApiExplorer(o => { o.GroupNameFormat = "'v'VVV"; o.SubstituteApiVersionInUrl = true; });

builder.Services.AddAuthorization(o =>
{
    foreach (var module in ModuleRegistry.All)   // "Identity","Catalog","Procurement",...
        o.AddPolicy(Policies.RequireModule(module), p => p.RequireClaim("module", module));
});

public static class Policies
{
    public static string RequireModule(string module) => $"module:{module}";
}
```

The Identity module stamps the `module` claim(s) into the JWT at login (Part 2.3).

## 13.5 Per-module DI registration

```csharp
// src/Modules/Warehouse/Ultramaverick.Warehouse.Api/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddWarehouseModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<WarehouseDbContext>(o => o
            .UseSqlServer(config.GetConnectionString("Default"))
            .AddInterceptors(new DomainEventOutboxInterceptor()));

        services.AddScoped<IWarehouseUnitOfWork, WarehouseUnitOfWork>();
        services.AddScoped<InventoryLedger>();
        services.AddScoped<IOutboxStore, WarehouseOutboxStore>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddControllers().AddApplicationPart(typeof(DependencyInjection).Assembly);
        return services;
    }
}
```

Compose in `Program.cs`: `builder.Services.AddIdentityModule(builder.Configuration);` … one line per module.

## 13.6 Health + observability

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy());
app.MapHealthChecks("/health");
```

Add an `ActivitySource` per module name and wrap command handlers with a MediatR `IPipelineBehavior<,>` that opens a span and records `Result.IsSuccess` as a tag.

<!-- pagebreak -->

# 14. Phase 11 — Data Migration & Cutover

The schema is backward compatible by design (`LegacyId` on every table; SQL schemas mirror bounded contexts). Migration is therefore a **one-shot, verifiable ETL**, not a rewrite.

## 14.1 Order of operations

1. **Freeze writes** to the legacy database (maintenance window).
2. **Create schemas + tables** in the new database (per-module migrations, applied in dependency order).
3. **Copy reference data first** (Identity, then Catalog).
4. **Copy transactional data** in FK order (Procurement → Quality → Warehouse → Laboratory → Orders → Manufacturing).
5. **Run reconciliation** (14.4). Non-zero diff aborts.
6. **Flip the connection string / DNS**; deploy the modular monolith.
7. **Warm Reporting** by replaying the outbox (14.5).

## 14.2 Reference data (Identity, Catalog)

```sql
-- Identity: users, roles, permissions (password hashes carry over)
INSERT INTO [Identity].[Users] ([LegacyId],[Username],[Email],[PasswordHash],[FullName],[RoleId],[IsActive],[CreatedAtUtc])
SELECT [UserID],[Username],[Email],[Password],[FullName],[RoleID],[IsActive],GETUTCDATE()
FROM   [legacy].[dbo].[Users];

INSERT INTO [Identity].[Roles] ([LegacyId],[Name]) SELECT [RoleID],[RoleName] FROM [legacy].[dbo].[Roles];

-- Catalog masters (each keeps LegacyId for traceability and later back-fills)
INSERT INTO [Catalog].[Items] ([LegacyId],[Code],[Name],[UomId],[ProductTypeId],[IsActive])
SELECT i.[ItemID], i.[Code], i.[Name], m.[NewUomId], i.[ProductTypeID], i.[IsActive]
FROM   [legacy].[dbo].[Items] i
JOIN   [dbo].[LegacyIdMap] m ON m.[Table]='Uom' AND m.[LegacyId]=i.[UomID];
```

A staging table `[dbo].[LegacyIdMap](Table, LegacyId, NewId)` records every remap so later passes can translate scalar FKs (Supplier/customer IDs referenced inside Procurement/Orders).

## 14.3 Transactional data (FK order)

```sql
-- Procurement first
INSERT INTO [Procurement].[PurchaseOrders]
    ([LegacyId],[PoNumber],[PrNumber],[PoDate],[SupplierId],[Status],[IsActive])
SELECT p.[PoID], p.[PONumber], p.[PRNumber], p.[PODate], s.[NewId], 'Open', p.[IsActive]
FROM   [legacy].[dbo].[POSummary] p
JOIN   [dbo].[LegacyIdMap] s ON s.[Table]='Supplier' AND s.[LegacyId]=p.[SupplierID];

INSERT INTO [Procurement].[PurchaseOrderLines]
    ([PurchaseOrderId],[ItemId],[UomId],[OrderedQuantity],[UnitPrice],[IsActive])
SELECT h.[Id], i.[NewId], u.[NewId], l.[Quantity], l.[UnitPrice], 1
FROM   [legacy].[dbo].[POSummary] l
JOIN   [Procurement].[PurchaseOrders] h ON h.[LegacyId]=l.[POID]
JOIN   [dbo].[LegacyIdMap] i ON i.[Table]='Item' AND i.[LegacyId]=l.[ItemID]
JOIN   [dbo].[LegacyIdMap] u ON u.[Table]='Uom'  AND u.[LegacyId]=l.[UomID];
```

Repeat the same shape for Quality, Warehouse, Laboratory, Orders, Manufacturing — each reading its legacy table(s) and translating every scalar FK through `LegacyIdMap`.

## 14.4 Reconciliation queries (gates)

```sql
-- Row-count parity per table
SELECT 'PurchaseOrders' AS T, (SELECT COUNT(*) FROM [legacy].[dbo].[POSummary] WHERE RowType='H') AS Src,
                              (SELECT COUNT(*) FROM [Procurement].[PurchaseOrders]) AS Dst
UNION ALL
SELECT 'QcReceipts', (SELECT COUNT(*) FROM [legacy].[dbo].[QC_Receiving]), (SELECT COUNT(*) FROM [Quality].[QcReceipts])
UNION ALL
SELECT 'WarehouseReceipts', (SELECT COUNT(*) FROM [legacy].[dbo].[WarehouseReceipt]), (SELECT COUNT(*) FROM [Warehouse].[WarehouseReceipts]);

-- Inventory ledger parity: derived on-hand must equal legacy Inventory
SELECT w.[LegacyId], l.[OnHand] AS Legacy, m.[OnHand] AS New
FROM   [legacy].[dbo].[Inventory] l
JOIN   [dbo].[LegacyIdMap] w ON w.[Table]='Warehouse' AND w.[LegacyId]=l.[WarehouseID]
JOIN   [Warehouse].[InventoryBalances] b ON b.[WarehouseId]=w.[NewId] AND b.[ItemId]=w.[NewId]
CROSS APPLY (SELECT SUM([OnHand]) AS [OnHand] FROM [Warehouse].[InventoryBalances] WHERE [WarehouseId]=w.[NewId]) m
WHERE  l.[Quantity] <> m.[OnHand];   -- zero rows expected
```

Capture the output of every gate as a signed run artifact. **Non-zero → abort and roll back.**

## 14.5 Warm the projections

After cutover, enqueue a synthetic replay: for each aggregate table, write one `OutboxMessage` per row of type `Replayed.<Entity>`; the Reporting projections consume them and rebuild `Reporting.*`. Because projections are idempotent by `EventId`, replay is safe to run twice.

```sql
INSERT INTO [Reporting].[ProjectionsToWarm] ([Entity],[EntityId],[EnqueuedAtUtc])
SELECT 'WarehouseReceipt', [Id], GETUTCDATE() FROM [Warehouse].[WarehouseReceipts];
```

## 14.6 Rollback

Keep the legacy database read-only for 7 days. Rollback = restore the old connection string and redeploy the monolith; the legacy DB is untouched by the migration (it was only read). This is why the migration never mutates source rows.

<!-- pagebreak -->

# 15. Appendices

## Appendix A — Legacy → module table map

| Legacy table | Module | New schema.table |
| --- | --- | --- |
| `Users` / `Roles` / `RolePermissions` | Identity | `Identity.*` |
| `Items` / `Uoms` / `Suppliers` / `Customers` / `FormulaHeader` | Catalog | `Catalog.*` |
| `POSummary` / `ImportBatch` | Procurement | `Procurement.*` |
| `QC_Receiving` / `QC_Reject` / `QC_Checklist*` | Quality | `Quality.*` |
| `WarehouseReceipt` / `Inventory` / `InventoryMovement` | Warehouse | `Warehouse.*` |
| `LabTestRequests` (+5 JSON arrays) / `ReceiveRequests` / `Results` | Laboratory | `Laboratory.*` |
| `Order` / `OrderLine` / `MoveOrder` | Orders | `Orders.*` |
| `ProductionPlan` / `TransformationRequest` | Manufacturing | `Manufacturing.*` |
| dashboard/report queries | Reporting | `Reporting.*` |

## Appendix B — Endpoint map (representative)

| Legacy route | New route | Module |
| --- | --- | --- |
| `POST /Ordering/AddPurchaseOrder` | `POST /api/Ordering/AddPurchaseOrder` | Procurement |
| `GET /Receiving/GetQcReceiptsWithPagination` | `GET /api/Receiving/GetQcReceiptsWithPagination` | Quality |
| `POST /LaboratoryTest/AddLabTestRequest` | `POST /api/LaboratoryTest/AddLabTestRequest` | Laboratory |
| `GET /WarehouseReceiving/GetInventory` | `GET /api/WarehouseReceiving/GetInventoryBalance` | Warehouse |
| `POST /Ordering/AddOrder` | `POST /api/Ordering/AddOrder` | Orders |
| `POST /Transformation/AddRequest` | `POST /api/Manufacturing/AddTransformationRequest` | Manufacturing |

## Appendix C — Per-module completion checklist

- [ ] Solution folders created; 4 projects reference the correct layers.
- [ ] Domain: entities, value objects, events — **no EF/Core references**.
- [ ] Application: commands/queries + handlers + validators; unit tests green.
- [ ] Persistence: `DbContext`, configurations, migrations applied; `LegacyId` present.
- [ ] Api: controllers wired with versioned routes + module policy.
- [ ] Outbox: publishes integration events; handlers consume upstream events idempotently.
- [ ] `*.Contracts` project defined for every published event.
- [ ] Integration tests: create → `dotnet test` green.
- [ ] `LegacyIdMap` translation covered by a migration test.
- [ ] `README` module section updated; Dockerfile module list updated.

## Appendix D — Definition of done (system)

- [ ] All 9 modules build; `dotnet build Ultramaverick.slnx` is warning-free.
- [ ] `dotnet test` green across unit + integration + auth suites.
- [ ] `dotnet format --verify-no-changes` clean.
- [ ] Reconciliation gates return zero rows.
- [ ] Reporting projections rebuild from a cold start within the cutover window.
- [ ] Rollback procedure rehearsed at least once on a staging copy.

<!-- END -->
