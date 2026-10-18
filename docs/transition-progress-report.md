# Transition Progress Report — Ultramaverick Dry → Modular Monolith

**Reported on:** 2026-10-16
**Source (legacy):** `C:\Users\Aldrin\Ultramaverick-Dry`
**Target (new):** `C:\Users\Aldrin\ultra-maverick-dry-modular-monolith`
**Branch / HEAD:** `feat/catalog-phase-0-shared-kernel` @ `141c244`
**Build:** `dotnet build Ultramaverick.slnx` → **succeeded**, 8 warnings, 0 errors
**Tests:** `dotnet test Ultramaverick.slnx` → **47 passed, 0 failed, 0 skipped** (35 unit + 8 integration + 4 authorization)

---

## 1. Executive verdict

The rewrite has a **strong, clean foundation**: correct layer boundaries, a fully working domain-events + transactional-outbox pipeline, real tests running in CI against SQL Server, and solid security hardening. The core architecture is *right*.

However, **the most recent step is unfinished and buggy**. Commit `141c244` ("extract shared kernel and scaffold Catalog module") left a root-level empty directory and an uncommitted, **unreferenced and broken `Ultramaverick.BuildingBlocks` project** whose code duplicates — with typos and real defects — the working outbox abstractions that already live inside the Identity module.

**Verdict: excellent direction, ~10% functional coverage, but stop and consolidate before scaffolding more modules.** Fix the shared-kernel/BuildingBlocks step, then continue.

---

## 2. Scope of the transition

The legacy system is a 2-project monolith (`ELIXIR.API` + `ELIXIR.DATA`) with a single `StoreContext`, plaintext passwords and all authorization disabled. The target is a **modular monolith**: one deployable API, one SQL Server database, divided into nine bounded contexts that each own their code and tables, communicating asynchronously through an outbox.

| Dimension | Legacy | New (as-built) |
| --- | --- | --- |
| Projects | 2 (`ELIXIR.API`, `ELIXIR.DATA`) | 11 production + 3 test |
| Target framework | (older .NET) | .NET 10 |
| Structure | Layered monolith | Per-module Clean Architecture (Domain / Application / Persistence / Infrastructure) |
| API endpoints | **464** (30 controllers) | **47** (6 controllers) — ≈ 10% |
| Persisted models | **62 DbSet** | **6 domain entities** (+3 persistence entities) — ≈ 10% |
| Auth | disabled (all `[Authorize]` commented out) | policy-based JWT, PBKDF2, rate-limited |
| Tests | (none meaningful) | 47 green, run in CI |
| Bounded contexts | 1 | 1 in progress (Identity), 1 scaffolded (Catalog), 7 not started |

---

## 3. What changed — commit timeline

17 commits between **2026-09-22** and **2026-09-24**:

| # | Commit | Change |
| --- | --- | --- |
| 1 | `0be86c0` | Initial commit: Identity slice (Domain/Application/Persistence/Infrastructure + API) |
| 2 | `f49467e` | Postman collection for Identity endpoints |
| 3 | `e025ad9` | README (architecture, setup, config, API, roadmap) |
| 4 | `4047eae` | GitHub Actions CI (build + test) |
| 5 | `5aed043` | Bump `actions/checkout`, `actions/setup-dotnet` to v5 |
| 6 | `65afe62` | Refresh/revoke endpoints + token-rotation hardening |
| 7 | `d3de6a6` | Docker support; make refresh rotation atomic |
| 8 | `48162cf` | Group use cases by subject (Commands/Authentication, Queries/Users) |
| 9 | `392e5ee` | Read queries for roles, departments, modules |
| 10 | `8334e7d` | Role write use cases (create, rename, activate, tag/untag modules) |
| 11 | `46ec53a` | Identity test suite + run it in CI |
| 12 | `fc87302` | Transactional outbox publisher |
| 13 | `779f67e` | Department and module write use cases |
| 14 | `8d2e90c` | Main-menu read and write use cases |
| 15 | `110c526` | Harden Identity: constant-time login, rate limiting, token pruning |
| 16 | `d8f5616` | Standardize API error responses; run FluentValidation in the pipeline |
| 17 | `141c244` | **Extract shared kernel and scaffold Catalog module** ← latest |

**Uncommitted working tree** (not yet in any commit, invisible to CI):
- `M Ultramaverick.slnx` — adds `Ultramaverick.BuildingBlocks` to the solution
- `?? src/Shared/Ultramaverick.BuildingBlocks/` — the new project (all files untracked)

---

## 4. Current progress — architecture as-built

### 4.1 Module map

```
src/
  Ultramaverick.Api/                     composition root, controllers, auth policies, error middleware
  Modules/
    Identity/
      Ultramaverick.Identity.Domain/         6 entities, PasswordHash, UserChanged/RoleChanged events
      Ultramaverick.Identity.Application/    commands, queries, handlers, validators, ports
      Ultramaverick.Identity.Persistence/    IdentityDbContext, configurations, repositories, 2 migrations, outbox
      Ultramaverick.Identity.Infrastructure/ PBKDF2 hasher, JWT service, current user, outbox publisher
    Catalog/
      (4 csproj only — Application/Domain/Persistence/Infrastructure — 0 .cs files)
  Shared/
    Ultramaverick.SharedKernel/            Entity base, IDomainEvent
    Ultramaverick.BuildingBlocks/          UNTRACKED, UNREFERENCED, BUGGY (see §6)
tests/
  Ultramaverick.Identity.UnitTests/        35 tests
  Ultramaverick.Identity.IntegrationTests/ 8 tests (real SQL Server, throwaway DB)
  Ultramaverick.Api.AuthorizationTests/    4 tests (401/403 per protected route)
```

### 4.2 Dependency direction (correct)

Verified from every `.csproj`:

- `*.Domain` → only `SharedKernel` (no EF Core, no MediatR, no ASP.NET).
- `*.Application` → `*.Domain` + MediatR/FluentValidation. **No EF Core, no Persistence, no Infrastructure.**
- `*.Persistence` / `*.Infrastructure` → inward only; implement ports declared by Application.
- `Ultramaverick.Api` → references Identity Application/Persistence/Infrastructure. Composition root.
- **No module references another module's projects.** No EF Core usings inside Domain/Application.

### 4.3 Domain events + outbox (fully implemented)

Matches the workbook design end-to-end:

- `Entity` raises `UserChanged` / `RoleChanged`; `User.Deactivate/Activate` are idempotent (early-return, no duplicate event).
- `IdentityDbContext.SaveChangesAsync` uses **two saves inside one transaction**: first assigns identity values, then converts tracked `Entity.DomainEvents` into `OutboxMessages` rows and saves them; commits or rolls back atomically; clears event lists after.
- `OutboxPublisherService` (background service) polls `Infrastructure.OutboxMessages`, dispatches, marks published/failed.
- `EventDispatcher` invokes `IIntegrationEventHandler`s, guarded by `ProcessedEvent` for idempotency.

### 4.4 Security & operations (strong)

- PBKDF2-SHA256 password hashing; plaintext never stored/returned.
- Constant-time login, per-IP fixed-window rate limiter on login (10/min → 429).
- Refresh tokens stored as SHA-256 hashes, single-use, rotated; background pruner.
- Policy-based authorization per module (`RequireModule(...)`); `ValidateOnStart` for JWT config.
- Secrets kept out of git (`user-secrets`, `.env` ignored); Docker + CI + Postman + README present.

---

## 5. What is good (keep doing)

1. **Correct, enforced layering.** Boundaries are expressed in `.csproj`, not just conventions.
2. **The outbox is real and tested**, not a stub — write side, publisher, dispatcher, idempotency.
3. **Tests actually run** against a real database in CI (47 green).
4. **Security by default** — the opposite of the legacy app.
5. **Operational hygiene** — Docker Compose, env-driven config, secret-free git, healthchecks.
6. **Migrations in place** for Identity (`InitialIdentity`, `AddProcessedEvents`).

---

## 6. What needs to change (findings, prioritized)

### P0 — the in-flight refactor is broken and must be resolved

**P0-1. `Ultramaverick.BuildingBlocks` is an unreferenced, buggy duplicate.**
Nothing references it (0 files use `using Ultramaverick.BuildingBlocks`; only the `.slnx` lists it). It duplicates types that already exist and work in `Identity.Application/Abstractions` and `Identity.Persistence/Entities`. Confirmed defects (all produce build warnings):

| File | Defect |
| --- | --- |
| `Messaging/OutboxMessageConfiguration.cs:14` | `exludeFromMigrations = exludeFromMigrations;` — self-assignment (CS1717); `_excludeFromMigrations` never set, so the flag silently does nothing. Typo `exlude`. |
| `Messaging/OutboxRecord.cs:8-13` | Primary constructor with **7 unread parameters** (CS9113); typo `Attemps`; dead type (used only in interface signatures, never constructed). |
| `Messaging/ProcessedEvent.cs` | Class is named `ProcessEvent` (file/name mismatch). |
| `Messaging/ProcessedEventConfiguration.cs`, `Messaging/OutboxDbContext.cs` | Empty stub classes. |
| `Messaging/IIntengrationEventHandler.cs` | Misspelled file + type (`IIntengration` → should be `IIntegration`). |
| `Messaging/IProcessedEventStore.cs` | Typos `eventtId`, `MasProcessAsync` (should be `MarkProcessedAsync`). |
| All files | Template-noise `using System; using System.Collections.Generic; using System.Text;` everywhere. |

**Recommended:** delete `src/Shared/Ultramaverick.BuildingBlocks/` and its `.slnx` entry, **or** — if the intent is to genuinely extract shared messaging — move the *already-working* abstractions out of Identity into `BuildingBlocks` and make Identity reference it. Do **not** keep two copies. Given only one module exists, deferring BuildingBlocks until a second module needs shared code (YAGNI) is also defensible.

**P0-2. Empty root-level `Ultramaverick.SharedKernel/` directory.**
Left over from the `Common → src/Shared/Ultramaverick.SharedKernel` move in `141c244`. It is empty. Delete it.

**P0-3. Uncommitted work on a feature branch.**
`.slnx` (modified) and `BuildingBlocks` (untracked) are neither committed nor reverted. Untracked files never reach CI. Resolve P0-1/P0-2, then commit or revert cleanly.

### P1 — architecture / governance

**P1-1. No central package management.** No `Directory.Build.props` / `Directory.Packages.props`. Versions are duplicated per project (FluentValidation `12.1.1` in both Api and Application; MediatR `14.2.0` in both; EF Core `10.0.12` across several; `System.IdentityModel.Tokens.Jwt` `8.22.0` duplicated). Risk of version drift as modules multiply. Add `Directory.Packages.props` (CPM) + `Directory.Build.props` (shared `TargetFramework`, `Nullable`, `ImplicitUsings`, `LangVersion`).

**P1-2. Warnings are not errors.** 8 warnings ship on a green build. Enable `TreatWarningsAsErrors` (at least for the new shared projects) and fix them — they currently hide the P0-1 defects.

**P1-3. README is stale.** It states Catalog "Not started" (a scaffold now exists), "Test projects contain no tests yet" (there are 47), and "the outbox publisher … is not implemented yet" / "Planned for Identity: … the outbox publisher, and tests" — both **done** (commits `fc87302`, `46ec53a`). Update the roadmap/status sections, and remove `Ultramaverick.BuildingBlocks`/Catalog drift from the repository-layout section.

**P1-4. Dockerfile restore list will not scale.** The layer-cached restore copies only `Ultramaverick.Api` + Identity `.csproj` files. It works today only because the Api references only Identity. The moment the Api wires in a second module (Catalog) or `BuildingBlocks`, the restore layer is incomplete and the cached build can fail. Update the `COPY ... csproj` list whenever a project is added, or restore from the `.slnx`.

**P1-5. Catalog is an empty scaffold.** Four `.csproj` and zero code, referenced by nothing. Fine as a placeholder — but mark it explicitly (e.g., a `README` or a `.gitkeep` note) so it is not mistaken for progress, and add a decision record of what Phase 1 will contain.

### P2 — code quality / consistency

**P2-1. Domain-event naming is inconsistent.** `UserChanged` (class) vs `RoleChangedEvent` (class) in `RoleChanged.cs`; change-type holders `UserChangeTypes` vs `RoleChangedTypes`; `UserChanged` sets `OccurredAtUtc` from the passed value while `RoleChangedEvent` ignores it and uses `DateTime.UtcNow`. Standardize on one pattern (e.g., `sealed record UserChanged(string ChangeType, DateTime OccurredAtUtc) : IDomainEvent`, and `RoleChanged` likewise).

**P2-2. Event constructor style.** `UserChanged(string ChangeType, DateTime OccurredAtUtc)` uses PascalCase parameters assigned via `this.X` — prefer a positional `record` for brevity and value semantics.

**P2-3. Documented design vs code drift.** The workbook describes a `SetAuditFields` helper and a soft-delete global query filter in `IdentityDbContext`; the code sets audit fields inside entity methods instead and has no query filter. Either is acceptable, but reconcile the doc with the implementation (or implement the filter) so the playbook stays trustworthy.

**P2-4. Schema placement note.** `OutboxMessages` is configured into the `Infrastructure` schema while the context's default schema is `Identity`. This is consistent with the one-database decision, but worth a one-line comment/doc so it is not read as a bug.

---

## 7. Recommended action plan (ordered)

1. **Resolve BuildingBlocks (P0-1).** Delete it and its `.slnx` entry, *or* extract the working abstractions into it and wire Identity to reference it. Pick one; keep one copy.
2. **Delete the empty root `Ultramaverick.SharedKernel/` (P0-2).**
3. **Commit the consolidated state (P0-3)** with a message in the repo's existing style (`refactor:` / `chore:`), and merge the branch to `main` so CI runs.
4. **Turn warnings into errors and fix them (P1-2)** — this makes P0-1-class defects impossible to ship silently.
5. **Add `Directory.Build.props` + `Directory.Packages.props` (P1-1).**
6. **Update README status/roadmap (P1-3)** and the Dockerfile restore list note (P1-4).
7. **Standardize domain-event types (P2-1/P2-2)**; reconcile doc drift (P2-3).
8. **Then resume feature work:** finish the Identity slice (remaining role/module/department/user parity vs legacy) and start Catalog Phase 1.

---

## 8. Evidence appendix

- Build: `dotnet build Ultramaverick.slnx` → Build succeeded; 8 warning(s); 0 error(s). Warnings all in `src/Shared/Ultramaverick.BuildingBlocks/Messaging/*`.
- Tests: `dotnet test Ultramaverick.slnx` → `Failed: 0, Passed: 35` (Unit), `Failed: 0, Passed: 8` (Integration), `Failed: 0, Passed: 4` (Authorization).
- Git: 17 commits, `2026-09-22` → `2026-09-24`; `git status` = 1 modified (`Ultramaverick.slnx`), 1 untracked (`src/Shared/Ultramaverick.BuildingBlocks/`).
- Legacy counts: 30 controllers, 464 HTTP-verb attributes, 62 `DbSet<>`, 273 `.cs` under `ELIXIR.DATA`.
- New counts: 6 controllers, 47 HTTP-verb attributes, 6 domain entities.
- Root `Ultramaverick.SharedKernel/` → 0 children (empty).
- `Ultramaverick.Catalog.*` → 4 `.csproj`, 0 `.cs`.
- Zero `.cs` files reference the `Ultramaverick.BuildingBlocks` namespace.

---

## 9. Update — Wave 1 (Part 0) and Wave 2 (Catalog) complete

**Reported on:** 2026-10-17
**Branch / HEAD:** `main` @ `4b24fd4`
**Build:** `dotnet build Ultramaverick.slnx` → **succeeded**, 0 warnings, 0 errors
**Tests:** `dotnet test Ultramaverick.slnx` → **71 passed, 0 failed** (35 Identity unit + 16 Catalog unit + 8 Identity integration + 5 Catalog integration + 7 authorization)

Sections 1–8 above are the original 2026-10-16 snapshot (HEAD `141c244`) and are kept verbatim as the point-in-time record. This section records what changed since.

### 9.1 Original findings resolved

| Finding | Status |
| --- | --- |
| P0-1 `Ultramaverick.BuildingBlocks` duplicate | **Resolved** — deleted; messaging stays per module. |
| P0-2 empty root `Ultramaverick.SharedKernel/` | **Resolved** — deleted. |
| P0-3 uncommitted work | **Resolved** — Wave 1 merged to `main` (`#14`). |
| P1-1 no central package management | **Resolved** — `Directory.Build.props` + `Directory.Packages.props` (CPM); `Microsoft.Data.SqlClient` pinned `10.0.13` for a transitive advisory. |
| P1-2 warnings not errors | **Resolved** — `TreatWarningsAsErrors`, `AnalysisLevel=latest`, `EnforceCodeStyleInBuild` enabled. |
| P1-3 stale README | **Resolved** — status/roadmap/layout refreshed in Wave 1. |
| P1-4 Dockerfile restore list | **Resolved** — restore list updated. |
| P1-5 empty Catalog scaffold | **Resolved** - Wave 2 built the full Catalog module (see 9.3). |
| P2-1 / P2-2 event naming and style | Open — deferred, non-blocking. |
| P2-4 `Infrastructure` schema note | **Superseded** — see 9.3 (per-module outbox). |

### 9.2 Wave 1 — Part 0 (shared kernel) — merged `#14`

`b13f56f refactor: extract shared kernel and scaffold Catalog module (#14)`. `BuildingBlocks` removed; CPM and root props added; 13 `.csproj` stripped; SqlClient pinned; README and Dockerfile fixed.

### 9.3 Wave 2 — Catalog module — merged `#15` (`4b24fd4`)

Full four-layer Catalog module:

- **Domain** — 21 entities + 20 change events.
- **Application** — CQRS for 9 surfaces (Items, Uoms, Suppliers, Customers, LotNames, Reasons, AccountTitles, ProductTypes, Formulas), ports, `ValidationBehavior`.
- **Persistence** — `CatalogDbContext` (schema `Catalog`) with the transactional outbox, 23 configurations, migration `20261017135130_InitialCatalog`.
- **Infrastructure** — `CatalogCurrentUser`.
- **Api** — 9 controllers under `api/`, composition-root wiring, `CatalogResultExtensions`.

Notable decisions:

- **Per-module outbox (reverses the original shared-`Infrastructure`-schema plan, P2-4).** Catalog owns `Catalog.OutboxMessages` and `Catalog.ProcessedEvents`. The guide's §3.6 fixtures apply only Catalog migrations yet assert `context.OutboxMessages`, which is only possible when each module owns its outbox; guide notes at L788 and L3264 corroborate. Waves 3–9 follow the same pattern.
- **`Policies.Catalog` constant added** — the guide's `Policies.RequireModule("Catalog")` does not compile as an attribute argument, so a `const` was added alongside `RequireModule`.
- **`SharedKernel.ValidationException` consolidated** — Identity's duplicate `Application.Exceptions.ValidationException` was removed; both modules throw the shared type and `ExceptionMiddleware` keeps a single catch.
- **Cross-context `IDepartmentDirectory`** implemented at the composition root (`CatalogDepartmentDirectory`), keeping Catalog free of an Identity dependency.
- **CI** — added `CATALOG_TEST_CONNECTION` alongside `IDENTITY_TEST_CONNECTION`.

Known follow-up: Catalog's outbox rows are written but not yet dispatched; the multi-store dispatcher lands in Part 11.

### 9.4 Coverage vs legacy

| Dimension | Legacy | New (after Wave 2) |
| --- | --- | --- |
| Controllers | 30 | **15** (6 Identity + 9 Catalog) |
| HTTP-verb endpoints | 464 | **101** |
| `DbSet<>` declarations | 62 | **32** (9 Identity + 23 Catalog) |
| Domain entities | — | **27** (6 Identity + 21 Catalog) |
| Bounded contexts done | 0 | **2** (Identity, Catalog); 7 not started |
| Tests | none meaningful | **71 green, run in CI** |

---

## 10. Update — Wave 3 (Procurement) complete

**Reported on:** 2026-10-18
**Branch / HEAD:** `feature/procurement-module` @ `f82f2b9`
**Build:** `dotnet build Ultramaverick.slnx` → **succeeded**, 0 warnings, 0 errors
**Tests:** `dotnet test Ultramaverick.slnx` → **83 passed, 0 failed** (10 Procurement unit + 2 Procurement integration + 16 Catalog unit + 5 Catalog integration + 35 Identity unit + 8 Identity integration + 7 authorization)

### 10.1 Wave 3 — Procurement module

Full four-layer Procurement module (guide §6), modelled on the approved Catalog module:

- **Domain** — `PurchaseOrder` aggregate with `PurchaseOrderLine`; `PurchaseOrderChanged` (Created/LineAdded) and `PurchaseOrderReceived` events. `SupplierId`/`ItemId`/`UomId` are scalars, not cross-module foreign keys (guide §6.1).
- **Application** — `CreatePurchaseOrderCommand` (+ handler + validator), `GetPurchaseOrdersQuery` (+ handler), `PurchaseOrderDto`, `Result`/`PagedResult`, `ValidationBehavior`, and the ports `IProcurementUnitOfWork`, `IPurchaseOrderRepository`, `ISupplierLookup`, `ICurrentUser`.
- **Persistence** — `ProcurementDbContext` (schema `Procurement`) with the transactional outbox, 4 configurations, migration `20261017231822_InitialProcurement`, repositories, and `SupplierLookup` (read-only cross-schema query into `Catalog.Suppliers`).
- **Infrastructure** — `ProcurementCurrentUser`.
- **Api** — `PurchaseOrdersController` (routes `api/Ordering/AddPurchaseOrder`, `api/Ordering/GetPurchaseOrders`), composition-root wiring, and `Policies.Procurement`.

Notable decisions:

- **Per-module outbox continues.** Procurement owns `Procurement.OutboxMessages` and `Procurement.ProcessedEvents`, matching the Wave 2 decision; no shared `Infrastructure` schema.
- **`Policies.Procurement` constant added.** The guide's `Policies.RequireModule("Procurement")` is not a compile-time constant, so `public const string Procurement = "RequireModule:Procurement";` was added beside `Policies.Catalog` and used in the controller's `[Authorize]`. The runtime policy already resolves because `Program.cs`'s module loop already contained `"Procurement"`.
- **Cross-context supplier check stays a port.** `ISupplierLookup` is implemented in Procurement.Persistence by a read-only `SqlQuery` against `Catalog.Suppliers`; Procurement takes no Catalog project reference.
- **`RowVersion` concurrency tokens** mirror Catalog's `ItemConfiguration` structure.
- **`Api.csproj` gained three `ProjectReference`s** to the Procurement projects (Application, Infrastructure, Persistence) — required for the composition root to compile; the file was not in the brief's edit list.
- **Procurement result mapping lives in the controller file.** The brief allowed only `PurchaseOrdersController.cs` under `src/Ultramaverick.Api/`, so the `.ToOk()` extension for `Ultramaverick.Procurement.Application.Models.Result<T>` is declared as an internal helper in that same file rather than in a new `Errors/` file.
- **Migration was scaffolded, not applied.** Like Catalog's `InitialCatalog`, `InitialProcurement` was generated with `dotnet ef migrations add` and committed; it is applied only at test runtime by the integration fixture (`MigrateAsync` on a throwaway DB). No production database was touched.
- **CI** — added `PROCUREMENT_TEST_CONNECTION` alongside `CATALOG_TEST_CONNECTION`.

### 10.2 Coverage vs legacy

| Dimension | Legacy | New (after Wave 3) |
| --- | --- | --- |
| Controllers | 30 | **16** (6 Identity + 9 Catalog + 1 Procurement) |
| HTTP-verb endpoints | 464 | **103** |
| `DbSet<>` declarations | 62 | **36** (9 Identity + 23 Catalog + 4 Procurement) |
| Domain entities | — | **29** (6 Identity + 21 Catalog + 2 Procurement) |
| Bounded contexts done | 0 | **3** (Identity, Catalog, Procurement); 6 not started |
| Tests | none meaningful | **83 green, run in CI** |

---

## 10. Completion status — 2026-10-18

All nine bounded contexts are implemented and merged on `main` (`654d63e`):

| Context | Status |
|---|---|
| Identity | Complete |
| Catalog | Complete |
| Procurement | Complete |
| Quality | Complete |
| Laboratory | Complete |
| Warehouse | Complete |
| Orders | Complete |
| Manufacturing | Complete |
| Reporting | Complete |

Verification at that commit: `dotnet build Ultramaverick.slnx -c Release` produces 0 warnings and
0 errors; `dotnet test` passes 341 tests across 19 projects; `dotnet format --verify-no-changes`
exits 0.

### Legacy data migration

The 2024-07-04 `ElixirDepotDry` backup was restored and migrated into the per-module schemas by
`tools/migration` (see that folder's README). Verified row parity, zero orphan rows and 29 passing
reconciliation gates:

- Identity 36 users, Catalog 1,220 items and 485 customers
- Procurement 2,157 purchase orders / 4,196 lines
- Quality 2,942 QC receipts, 83,765 checklist answers
- Warehouse 4,343 receipts, 586 miscellaneous receipts
- Orders 16,203 orders / 90,082 lines / 68,921 move orders
- Reporting warmed to 1,530 inventory positions, 16,203 order fulfillments, 2,942 QC statuses

Rollback was rehearsed (backup, drop, restore) with the gates re-passing afterwards.

### Outstanding

Endpoint parity is the remaining gap: the legacy API exposes roughly 464 actions while the new API
exposes 118. Cross-cutting items still open are DB-error to HTTP status mapping, health checks and
telemetry, API versioning, and a Manufacturing contracts project.
