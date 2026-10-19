# Legacy data migration

Migrates the legacy `dbo`-only database (`ElixirDepotDry`) into the nine per-module schemas owned by
this solution. Written against the 2024-07-04 backup restored on 2026-10-18.

## Verified source facts

| Fact | Value |
|---|---|
| Tables | 62, all in `dbo`, no per-context schemas |
| `dbo.Orders` | 90,082 rows, **16,203** distinct `OrderNoPKey`, 4 distinct `OrderNo` |
| `dbo.POSummary` | 4,196 rows, **2,157** distinct `PO_Number` |
| `dbo.MoveOrders` | 68,921 rows (`OrderNoPKey` header key) |
| `dbo.WarehouseReceived` | 4,343 rows |
| `dbo.QC_Receiving` | 2,942 rows |
| `dbo.RawMaterials` | 1,220 rows |
| Empty | `Transformation_*`, `Formulas`, `FormulaRequirements`, `LabTestRequests` and all lab detail tables |

Two consequences drive the whole design:

1. **Legacy order tables are line-level.** `Orders` carries one row per item, not per order
   (`OrderNo` is a 4-value bucket, not the identity). Headers must be reconstructed by grouping on
   `OrderNoPKey`; `POSummary` groups on `PO_Number`. The new schemas are header + line, and both
   unique indexes (`Orders.OrderNumber`, `PurchaseOrders.PoNumber`) will reject a naive row copy.
2. **The guide's §14 SQL is illustrative, not literal.** It references a legacy `Items` table and an
   `Inventory` table; neither exists. Catalog items come from `RawMaterials`; on-hand must be derived
   from receipts, issues and move orders.

## Target provisioning

All nine contexts share one database (`ConnectionStrings:Default`), each owning a schema. No module
creates a cross-schema foreign key, so migration order is for clarity, not constraint enforcement.

```
pwsh tools/migration/provision-target.ps1 -TargetDb UltraMaverickDry_Migrated
```

Applies, in order: Identity → Catalog → Procurement → Quality → Warehouse → Laboratory → Orders →
Manufacturing → Reporting, then creates `dbo.LegacyIdMap`.

## LegacyIdMap

```sql
CREATE TABLE [dbo].[LegacyIdMap] (
    [TableName] sysname NOT NULL,
    [LegacyId]  int     NOT NULL,
    [NewId]     int     NOT NULL,
    PRIMARY KEY ([TableName], [LegacyId])
);
```

Every reference copy inserts its own mapping by selecting the target table's `LegacyId`:

```sql
INSERT INTO [dbo].[LegacyIdMap] ([TableName],[LegacyId],[NewId])
SELECT 'RawMaterials', [LegacyId], [Id] FROM [Catalog].[Items] WHERE [LegacyId] IS NOT NULL;
```

Transactional loads translate every scalar reference through the map (`JOIN dbo.LegacyIdMap`), never
through names. A missing map row is a hard failure, not a silent NULL.

## Per-module strategy

### Identity
`UserRole` → `Roles`; `Department` → `Departments`; `MainMenu` → `MainMenus`; `Module` → `Modules`;
`UserRole_Modules` → `RoleModules`; `Users` → `Users`.
Legacy passwords are stored in a form the new hasher cannot verify, so users are imported with an
unusable hash and must reset. Text actors are resolved by name against the imported user set.

### Catalog
`UOM` → `Uoms`; `RawMaterials` → `Items`; `ItemCategory`, `ProductType`, `Reason`, `LotCategory`,
`LotName`, `AccountTitle`, `FarmType`, `Supplier`, `Customer` → their Catalog tables.
`Customers.AccountTitle` is free text and must be resolved to `AccountTitleId` by name.
There is no legacy `Locations` source, so location columns stay NULL.

### Procurement
Group `POSummary` by `PO_Number` → `PurchaseOrders` (`PoNumber = CAST(PO_Number)`,
`SupplierId` via `VendorName`, `Status = 'Open'`), one `PurchaseOrderLines` row per source row.
Record each source row's `Id` under `TableName = 'POSummary'` pointing at its line, so Quality can
resolve `QC_Receiving.PO_Number` + `ItemCode` back to a purchase-order line.

### Quality
`QC_Receiving` → `QcReceipts`; `QC_Reject` → `QcRejects`; the checklist chain
(`QcChecklists`, `ChecklistQuestions`, `ChecklistAnswers`, `QChecklistOpenFieldAnswers`,
`QChecklistProductDimensions`, `ChecklistOtherObservations`, `ChecklistCompliances`,
`ChecklistReviewVerificationLogs`) → their Quality tables.
Two source/target mismatches need explicit handling:
`Expected_Delivery` is numeric in the source but a date in the target, and
`QChecklistProductDimensions.Standard/Actual` are strings that must parse to `decimal`; values that
do not parse go to a quarantine table rather than being lost.

### Warehouse
`WarehouseReceived` → `WarehouseReceipts`. The source has no warehouse identity, so receipts land in a
seeded "Legacy Default" warehouse unless their QC receipt already resolved one. `Warehouse_Reject`
has no target table and is intentionally retired (recorded in the reconciliation report).
`MiscellaneousReceipts` maps header-only (its `Details` is free text); `MiscellaneousIssues` +
`MiscellaneousIssueDetails` become issues + lines.
`InventoryBalances` is derived from receipts, issues and move orders — there is no source table.

### Orders
Group `Orders` by `OrderNoPKey` → `Orders.Orders` (`OrderNumber = CAST(OrderNoPKey)`), one
`OrderLines` row per source row. `MoveOrders` → `MoveOrders`; `TransactMoveOrder` →
`TransactedMoveOrders`; `CancelledOrders` → `CancelledOrders`; `AdvancesToEmployees` →
`AdvancesToEmployees`. `GenerateOrderNos` (17,810 rows) has no target and is retired.
Actor columns (`PreparedBy`, `RejectBy`, `IsCancelBy`, `SetBy`) are names, not ids.

### Manufacturing and Laboratory
Both sources are empty in this backup. The loads are parameterised and run as explicit no-ops that
assert source and target counts are zero, so they will still execute correctly against a populated
backup.

### Reporting
Nothing to copy: Reporting owns projections only. After the transactional loads, warm it by replaying
`StockReceived`, `OrderCreated`, `OrderConfirmed`, `OrderCancelled` and `QcReceiptCompleted` through
the existing outbox dispatcher. Projections are idempotent on `(EventId, projectionName)`, so a
second replay must change nothing.

## Reconciliation gates

Each gate returns zero rows on success and aborts the run otherwise.

| Gate | Assertion |
|---|---|
| Reference parity | target row count equals mapped source count for every reference table |
| Header/line parity | `Orders.Orders` = 16,203 and `OrderLines` = 90,082; `PurchaseOrders` = 2,157 and `PurchaseOrderLines` = 4,196 |
| Orphan check | no line row lacking a header; no transactional row with an unmapped scalar reference |
| Actor check | every non-null legacy actor name either resolves to a user or is recorded as unresolved |
| Empty modules | Manufacturing and Laboratory target counts are zero |
| Inventory derivation | derived on-hand matches receipts minus issues/moves for every item/warehouse pair |

## Rollback

The legacy database is only read, never mutated. Rollback is therefore: stop the API, restore the
pre-ETL snapshot of the target database (or drop and re-provision it), and point the connection string
back at the legacy database. The rehearsal records wall-clock RTO.

## Layout

```
tools/migration/
  README.md                  this plan
  00-provision.ps1           apply all nine module migrations to a target database
  01-legacy-id-map.sql       staging map
  10-identity.sql            reference data
  11-catalog.sql
  12-procurement.sql
  13-quality.sql
  14-warehouse.sql
  15-orders.sql
  16-warm-reporting.sql
  20-password-change-required.sql  flag sentinel-hashed users
  90-gates.sql               reconciliation gates
  99-run-all.ps1             orchestrator with abort-on-gate-failure
```

## Retired legacy tables

Three legacy tables hold data but have no target in the new schema. They are retired deliberately and
asserted by the coverage gate in `90-gates.sql`:

| Table | Rows | Why retired |
|---|---|---|
| `GenerateOrderNos` | 17,810 | Order numbers are generated by `OrderNumberGenerator` per day; migrating the legacy counters would collide with it. |
| `CheckListStrings` | 108 | Legacy free-text checklist values tied to a PO summary; the Quality checklist model stores answers on `ChecklistOpenFieldAnswers`. No target shape exists. |
| `Warehouse_Reject` | 1 | A single receipt-rejection row; Warehouse models rejections through `WarehouseReceipts` totals, not a separate table. |

`__EFMigrationsHistory` is an EF system table and is ignored.

## Gap closure

`Transactions` (6) and `SampleTypes` (1) were initially missed even though the Catalog schema already had
`TransactionTypes` and `SampleTypes`. Both are now loaded by `17-catalog-gaps.sql`.

The order backfill in that script matched nothing: legacy `Orders.TransactionType` is NULL on every row, so
no order carries a transaction type. Only move orders reference a transaction, and their target table has no
transaction-type column.

## Identity module-name mapping (authorization)

Legacy `Modules.Name` values are menu route paths (`/ordering/orders`, `/setup/uom-management`), not
module identifiers, so JWT role claims never matched the `RequireModule:*` policies and every
authenticated call returned 403. `19-identity-module-map.sql` inserts the nine canonical module
identifiers and remaps each role's menu grants by path family
(`/ordering/*` and `/move-order-approval/*` → Orders, `/inventory/*` and `/receiving/*` → Warehouse,
`/qc-module/*` → Quality, `/import/*` → Procurement, `/user-management/*` → Identity,
`/reports/*` → Reporting, `/setup/*` → Catalog).

Run order: `10-identity.sql` then `19-identity-module-map.sql` before any authenticated smoke test.

For local API testing a dedicated dev account can be created directly in the migrated database with a
PBKDF2 hash (`PBKDF2;SHA256;210000;<base64 salt 16>;<base64 key 32>`, see
`PasswordHasher.cs`) and granted the nine canonical modules. Never carry that account into an
environment that holds real users.
