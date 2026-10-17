# Catalog Module — Build Split (Learning Contract)

> **Purpose:** divide the Catalog module into two tracks so the boring 80% is scaffolded for you and the depth-building 20% is *yours to design, write, and debug*. The point is not to finish Catalog fast. The point is that when Catalog is done, **you** designed the aggregate boundaries, enforced the invariants, and proved the migration — that is what rebuilds engineering depth.

---

## 0. The contract (read this first)

1. **Track A (I scaffold)** — projects, persistence plumbing, the 15 uniform lookup masters, DTOs, migrations scaffold, test harness. Mechanical work. You never hand-write it.
2. **Track B (you own)** — the 6 core tables and every invariant around them. **I will not write, edit, or "helpfully finish" Track B files.** If you ask me to, I'll ask why.
3. **Draft before you look.** For every Track B item: write your design, make it compile, write the failing-then-passing test, **then** compare with guide §5.2–§5.3. Looking first turns this into transcription.
4. **Review, don't accept.** You send me your diff; I review as a skeptical peer (correctness, boundary, hidden invariants, failure modes). I critique; you fix.
5. **File ownership.** A file belongs to exactly one track. I never edit Track B. You never need to write Track A.

---

## 1. Catalog scope map (21 tables, measured)

**Simple, uniform lookup masters (15) — Track A, template-generated:**

| # | Table | Shape |
|---|---|---|
| 1 | `ItemCategories` | code + description + IsActive |
| 2 | `Farms` | name + IsActive |
| 3 | `Locations` | name + IsActive |
| 4 | `AccountTitles` | code + name + IsActive |
| 5 | `LotCategories` | name + IsActive |
| 6 | `LotNames` | name + IsActive |
| 7 | `Reasons` | code + description + IsActive |
| 8 | `TransactionTypes` | name + IsActive |
| 9 | `ProductTypes` | name + IsActive |
| 10 | `Dispositions` | name + IsActive |
| 11 | `Analyses` | name + IsActive |
| 12 | `Parameters` | name + IsActive |
| 13 | `SampleTypes` | name + IsActive |
| 14 | `SwabTypes` | name + IsActive |
| 15 | `ProductConditions` | name + IsActive |

These are all the same CRUD. Generated from one template, mapped to one shared base — you should be able to delete one and regenerate it in seconds.

**Core tables (6) — Track B, you design:**

| Table | Why it's yours |
|---|---|
| `Uoms` | Unit semantics; referenced by Items and every transactional module |
| `Items` | The central entity; referenced by **every** other module (Warehouse, Orders, …) |
| `Suppliers` | Referenced by Procurement (cross-module) |
| `Customers` | Referenced by Orders (cross-module) |
| `FormulaHeaders` | The true aggregate root (with `FormulaRequirements`) |
| `FormulaRequirements` | Aggregate child; carries the real invariants |

---

## 2. Track A — what I scaffold (mechanical)

| ID | Deliverable | Evidence it's done |
|---|---|---|
| A1 | Solution skeleton: 4 `.csproj` (Domain/Application/Persistence/Api) wired with **inward-only** references, mirroring Identity | `dotnet build` green, no domain→EF reference |
| A2 | The 15 lookup masters: base entity + 15 concrete types + configurations, from one generator template | 15 tables mapped; a scratch test inserts+reads one |
| A3 | Persistence plumbing: `CatalogDbContext`, per-entity configs (table, key, `RowVersion`, unique indexes), UnitOfWork, repositories, `ServiceCollectionExtensions` | `dotnet ef migrations script` emits all 15 tables |
| A4 | Application plumbing: `Result`, `PagedResult`, `ValidationBehavior`, abstractions, DI extension | builds; a sample handler round-trips |
| A5 | API shells: controller + DTO + command/query for **one** lookup (you copy the pattern or I generate the rest) | one lookup endpoint returns 200 |
| A6 | Migration scaffold: empty `InitialCatalog` containing the 15 lookup tables + `LegacyId` columns | migration applies to a scratch DB |
| A7 | Test harness: WebApplicationFactory fixture + a shared SQL Server test container config | a trivial integration test is green |

**Not in Track A:** anything touching `Items`, `Uoms`, `Suppliers`, `Customers`, `Formula*`, or any cross-aggregate rule.

---

## 3. Track B — what you design and write (the 20%)

Each brief gives you **the decision, the constraints, and the acceptance test — never the answer.**

### B1. The Formula aggregate ← *the exam*
- **Decision:** `FormulaHeaders` + `FormulaRequirements` is one aggregate. Decide: is a requirement an **entity inside** the aggregate (loaded/saved together) or a **separate aggregate**? Defend it with the transaction boundary — what must change atomically, and what may lag?
- **Invariants you must enforce in the domain** (note which the DB already enforces — row `UQ` means "also defend it in code, don't rely on the exception"):
  - a formula with **zero requirements is invalid** ← *the DB does NOT enforce this; this is your job*
  - every requirement quantity `> 0`
  - no duplicate `RawMaterialId` inside one formula (DB has `UQ_FormulaRequirements_Header_Material`)
  - `(ItemId, Version)` is unique (DB has `UQ_FormulaHeaders_Item_Version`)
  - **decide & justify:** may the output `ItemId` also appear as a `RawMaterialId`?
  - **decide & justify:** is there at most **one active version** per item? (the DB has no constraint for this — if you want it, *you* enforce it)
- **Acceptance test (failing first):** creating a formula with no requirements fails; adding a duplicate raw material fails; the version rule you chose holds under a concurrent second insert.
- **Do not read guide §5.2/§5.3 until your draft compiles and this test passes.**

### B2. The Items aggregate
- **Decision:** `Items` references `ItemCategories` and `Uoms` by ID. Decide how the domain treats those references (id value objects? plain ints with a domain service validation?).
- **Invariants:** unique `ItemCode`; `BufferLevel >= 0`; `IsExpirable` interacts with downstream lots — decide what "expirable" must guarantee here.
- **Hard bit:** an Item is referenced by Warehouse, Orders, … which don't exist yet. Decide how Item deactivation behaves when downstream references exist (see B6).

### B3. UOM semantics
- **Decision:** is `Uoms` a **plain lookup**, or do units carry **conversion semantics** (base unit + factor)? Look at the legacy `UOM` model and transactions to find the truth. If there is no conversion in legacy, say so explicitly and keep it a lookup — *not every table needs to be clever.*
- **Acceptance test:** whatever you decide, pin it with a test that would fail if someone later assumed conversion.

### B4. Cross-module reference ports (Suppliers, Customers)
- **Decision:** Procurement and Orders will need Supplier/Customer data. Decide the **port**: a `Catalog.Contracts` query interface the other module consumes, vs. a read-only cross-schema query. Write the port interface now; implement it in Catalog.
- **Constraint:** no module references `Catalog.Domain` or `Catalog.Persistence`. Only `Catalog.Contracts`.
- **Acceptance test:** a fake consumer resolves a Supplier through the port with no compile-time reference to Catalog internals.

### B5. The lookup read model
- **Decision:** the paginated dropdown query — read directly from the write table, or project into a read model? For 15 tiny tables, justify the simpler choice (and when you'd switch).
- **Hard bit:** the query must be stable under concurrent writes and not leak EF `IQueryable` out of persistence.

### B6. Set-based deactivation policy ← *the genuinely hard DDD question*
- **Decision:** "you may not deactivate a `Uom` / `ItemCategory` / `Item` that is still referenced." That is a **set-based invariant** — it spans aggregates and lives in the DB, which pure DDD aggregation cannot enforce.
- **You must choose and defend one:** domain service before save · DB constraint/trigger · eventual reconciliation job that flags violators · or "allow and treat as soft".
- **Acceptance test:** deactivating an in-use master is rejected (or flagged, per your choice) — and an unused one succeeds.

### B7. Formula migration reconciliation
- **Decision:** how do you *prove* every legacy formula arrived intact — count parity, per-formula requirement count, and the `LegacyIdMap`.
- **Acceptance test:** the reconciliation query returns **zero rows** on a seeded fixture.

---

## 4. Milestones (order matters)

| M | Owner | Exit condition |
|---|---|---|
| M0 | Me | A1 skeleton builds |
| M1 | Me | A2–A4 lookups + plumbing; 15 tables migrate; tests green |
| M2 | You | B3 (UOM decision) + B5 (read model) drafted & tested |
| M3 | You | B2 Items aggregate + ACL via B4 |
| M4 | You | B6 deactivation policy enforced |
| M5 | You | **B1 Formula aggregate — the exam** |
| M6 | Me/You | I generate remaining controller/DTO shells; you wire Formula endpoints |
| M7 | You | B7 migration reconciliation returns zero rows |

---

## 5. How we work together

- **You start each Track B item by sending me:** the decision, the trade-off you rejected and why, and the acceptance test you wrote. **Then** I review the code.
- **I review, I don't rewrite.** My feedback format: *criterion cited → what's wrong → the failure scenario that proves it.*
- **When you're stuck, ask a design question, not "write it for me."** Example: "Should Formula be one aggregate or two?" — good. "Just implement Formula" — I'll push back.
- **Blocker?** Bring the compile error or failing test; I help you read it, not bypass it.

---

## 6. Definition of done — Catalog

- [ ] All 21 tables migrated with `LegacyId`.
- [ ] B1–B6 each have a failing-then-passing test that would catch a regression.
- [ ] Cross-aggregate deactivation rule (B6) enforced and tested.
- [ ] Item/Formula/Uom/Supplier/Customer endpoints answer on the legacy `SETUP` routes.
- [ ] B7 reconciliation returns zero rows.
- [ ] `dotnet build` warning-free; `dotnet test` green.
- [ ] You can explain, from memory, why Formula is one aggregate and where each invariant is enforced.

---

## 7. Start signal

To begin, reply with **your B3 answer** (is `Uoms` a plain lookup or does it carry conversion semantics?) — it is the cheapest decision and it unblocks Items and every formula requirement. Meanwhile I'll deliver **M0–M1** (Track A) on request.
