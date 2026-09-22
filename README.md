# Ultramaverick Dry — Modular Monolith

Backend rewrite of the Ultramaverick Dry system: a barcode-driven inventory platform for the Central Depot Plant. The original application is being rebuilt as a **modular monolith** with a normalized database, a documented API surface, and enforced authentication/authorization.

This repository contains the new implementation. The original solution and the planning documents live in the `Ultramaverick-Dry` repository.

## Architecture

A single deployable API with one SQL Server database, divided into bounded contexts that each own their tables and code.

```
                       Ultramaverick.Api                (composition root, HTTP)
                              │
          ┌───────────────────┴───────────────────┐
          ▼                                       ▼
   Identity.Application                    Identity.Infrastructure
   (use cases, ports)                      (adapters: hashing, JWT)
          │                                       │
          └───────────────────┬───────────────────┘
                              ▼
                    Identity.Domain                 (entities, value objects, events)
                              ▲
                              │ implements Application ports
                      Identity.Persistence
                    (EF Core, repositories, outbox)
```

Rules:

- Dependencies point inward. `Domain` references nothing; `Application` references only `Domain`; `Persistence` and `Infrastructure` reference inward and implement the interfaces (ports) `Application` declares.
- Modules never share entities, repositories, or foreign keys. Cross-module references are scalar IDs.
- Cross-module communication is asynchronous through the **transactional outbox** (`Infrastructure.OutboxMessages`), written in the same transaction as the data change. Synchronous reads use query ports.

Full architecture notes are in the planning repository (`docs/bounded-contexts.md`, `docs/module-relationships.md`).

## Tech stack

| Concern | Choice |
| --- | --- |
| Runtime | .NET 10 |
| Web | ASP.NET Core controllers |
| Persistence | EF Core 10 + SQL Server |
| Use cases | MediatR (commands/queries/handlers) |
| Validation | FluentValidation |
| Auth | JWT bearer (HMAC-SHA256), PBKDF2 password hashing |
| API docs | OpenAPI (development only) |

## Repository layout

```
src/
  Ultramaverick.Api/                          host: controllers, auth policies, composition root
  Modules/
    Identity/
      Ultramaverick.Identity.Domain/          entities, value objects, domain events
      Ultramaverick.Identity.Application/     commands, queries, handlers, validators, ports
      Ultramaverick.Identity.Persistence/     DbContext, configurations, repositories, migrations
      Ultramaverick.Identity.Infrastructure/  password hasher, JWT token service, current user
tests/
  Ultramaverick.Identity.UnitTests/
  Ultramaverick.Identity.IntegrationTests/
  Ultramaverick.Api.AuthorizationTests/
postman/
  Ultramaverick-Identity.postman_collection.json
```

## Prerequisites

- .NET SDK 10
- SQL Server (LocalDB, Express, or a container)
- `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`

## Getting started

```powershell
git clone https://github.com/aldrinvega/ultra-maverick-dry-modular-monolith.git
cd ultra-maverick-dry-modular-monolith

dotnet restore
dotnet build
```

### 1. Set the JWT signing key (never committed)

The host validates configuration on startup and will not start without a key.

```powershell
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$bytes = New-Object byte[] 64
$rng.GetBytes($bytes)
$key = [Convert]::ToBase64String($bytes)

dotnet user-secrets set "Jwt:Key" $key --project src/Ultramaverick.Api
```

### 2. Point at your database

`src/Ultramaverick.Api/appsettings.Development.json`:

```json
"ConnectionStrings": {
  "Default": "Server=.\\SQLEXPRESS;Database=ElixirDepotDry;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 3. Apply migrations

```powershell
dotnet ef database update `
  -p src/Modules/Identity/Ultramaverick.Identity.Persistence `
  -s src/Ultramaverick.Api
```

### 4. Run

```powershell
dotnet run --project src/Ultramaverick.Api
```

The `http` launch profile listens on `http://localhost:5160`. The `https` profile uses `https://localhost:7244`.

## Run with Docker

Two containers: the API and SQL Server 2022. All configuration is supplied through environment variables, so nothing is baked into the image.

### 1. Create your environment file

```powershell
Copy-Item .env.example .env
# then edit .env and set MSSQL_SA_PASSWORD and JWT_KEY
```

Generate a JWT key if you need one:

```powershell
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$b = New-Object byte[] 64; $rng.GetBytes($b); [Convert]::ToBase64String($b)
```

### 2. Start SQL Server and the API

```powershell
docker compose up -d --build
```

- API: `http://localhost:5007`
- SQL Server: `localhost:1433`

The API waits for SQL Server's health check before starting.

### 3. Create the schema and seed

The API does **not** migrate or seed on startup, so run both once against the container.

```powershell
# Schema (EF Core migration)
dotnet ef database update `
  -p src/Modules/Identity/Ultramaverick.Identity.Persistence `
  -s src/Ultramaverick.Api `
  --connection "Server=localhost,1433;Database=ElixirDepotDry;User Id=sa;Password=<MSSQL_SA_PASSWORD>;TrustServerCertificate=True"

# Baseline data: roles, departments, menus, modules, admin user
docker cp docker/seed.sql ultramaverick.sqlserver:/tmp/seed.sql
docker exec ultramaverick.sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "<MSSQL_SA_PASSWORD>" -C -i /tmp/seed.sql
```

### 4. Log in

```powershell
curl -X POST http://localhost:5007/api/Login/authenticate `
  -H "Content-Type: application/json" `
  -d "{\"userName\":\"admin\",\"password\":\"Admin@123\"}"
```

### Useful commands

```powershell
docker compose logs -f api     # follow API logs
docker compose down            # stop, keep the database volume
docker compose down -v         # stop and delete the database volume
```

The `mssql-data` volume persists between runs. After `down -v` the schema and seed must be reapplied.

## Configuration

| Setting | Where | Notes |
| --- | --- | --- |
| `Jwt:Key` | user-secrets / environment | required, minimum 32 bytes |
| `Jwt:Issuer`, `Jwt:Audience` | appsettings | must match between issuance and validation |
| `Jwt:AccessTokenMinutes`, `Jwt:RefreshTokenDays` | appsettings | token lifetimes |
| `ConnectionStrings:Default` | appsettings.{Environment} | SQL Server connection |

No secret is committed. `appsettings.json` holds empty placeholders and the host fails fast (`ValidateOnStart`) when a required value is missing.

## Seed data

A baseline admin is seeded for first login:

| Username | Password | Role | Department |
| --- | --- | --- | --- |
| `admin` | `Admin@123` | Administrator | Warehouse |

Change the password before any real use. The seed also creates roles (Administrator, Supervisor, Encoder), departments, main menus, modules, and grants the Administrator every module.

## API

Base route: `{{baseUrl}}` = `http://localhost:5160`.

| Method | Route | Auth | Purpose |
| --- | --- | --- | --- |
| POST | `/api/Login/authenticate` | anonymous | log in, returns access + refresh tokens |
| GET | `/api/User/GetAllUsersWithPagination/{status}` | Identity module | paged users (`?page&pageSize&search`) |
| GET | `/api/User/GetById/{id}` | Identity module | single user |
| POST | `/api/User/AddNewUser` | Identity module | create user (password hashed) |
| PUT | `/api/User/UpdateUserInfo/{id}` | Identity module | update profile/role/department |
| PUT | `/api/User/InActiveUser/{id}` | Identity module | deactivate |
| PUT | `/api/User/ActivateUser/{id}` | Identity module | activate |

Import `postman/Ultramaverick-Identity.postman_collection.json` into Postman. Run **Authenticate** first — it stores the token so the other requests are authorized automatically.

### Authentication and authorization

- Passwords are hashed with PBKDF2-SHA256 (210,000 iterations, per-password salt). Plaintext is never stored or returned.
- The access token carries `sub`, `name`, `role`, and one `modules` claim per granted module.
- Authorization is policy-based. `RequireModule("Identity")` (and one policy per module) grants access by checking the `modules` claims. Missing token → 401; token without the module → 403.
- Refresh tokens are stored as SHA-256 hashes, single-use, with revocation.

## Domain events and the outbox

Entities inherit `Entity` and raise domain events (`UserChanged`, `RoleChanged`). `IdentityDbContext.SaveChangesAsync` writes each event to `Infrastructure.OutboxMessages` **in the same transaction** as the data change, then clears the entity's event list.

The read half — a background publisher that dispatches unpublished outbox rows and records `Infrastructure.ProcessedEvents` for idempotency — is **not implemented yet**. Events are durably recorded but not yet delivered.

## Roadmap

The target system has nine bounded contexts:

| Context | Status |
| --- | --- |
| Identity | In progress (login, user admin, authorization) |
| Catalog | Not started |
| Procurement | Not started |
| Quality | Not started |
| Laboratory | Not started |
| Warehouse | Not started |
| Orders | Not started |
| Manufacturing | Not started |
| Reporting | Not started |

Planned for Identity: role/module/department endpoints, refresh and revoke endpoints, the outbox publisher, and tests.

## Testing

Test projects exist but contain no tests yet. Planned:

- Unit: password hasher, entity invariants, authenticator handler
- Integration: handlers against a disposable database (Testcontainers)
- Authorization: 401/403 per protected route

```powershell
dotnet test
```

## Disclaimer

This repository is solely owned by RDF Feed, Livestock and Foods Inc. Any unauthorized use or distribution is strictly prohibited.
