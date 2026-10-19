# Cutover runbook

This runbook moves the legacy `ElixirDepotDry` database into the modular monolith and proves the result on a staging copy before production. It is exercised by `tools/cutover/rehearse.ps1`; the rehearsal drops and rebuilds the staging database from the legacy source every run.

## Preconditions

- SQL Server Express reachable at `.\SQLEXPRESS` with `sqlcmd` on `PATH`.
- Legacy database `ElixirDepotDry` present and backed up.
- `.NET 10` SDK and the `dotnet-ef` tool installed.
- A JWT signing key of at least 32 bytes for the smoke token.
- The legacy application is frozen for the production run; the rehearsal reads it without writing.

## Automated rehearsal

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/cutover/rehearse.ps1
```

The script performs, in order:

1. Drops and recreates `UltraMaverickDry_Staging`.
2. Applies all nine module EF migrations (`tools/migration/provision-target.ps1`).
3. Runs the ordered ETL (`tools/migration/99-run-all.ps1`): identity, catalog, procurement, quality, warehouse, orders, reporting warm-up, gaps, balances, module map, password-change flag.
4. Runs `90-gates.sql`: all reconciliation gates plus the coverage gate.
5. Builds the solution and starts the API against the staging database from a cold start.
6. Waits for `/health` and records cold-start seconds.
7. Calls one read endpoint in each of the nine modules with a signed multi-module token.
8. Captures the Reporting baseline, clears the three projections and their checkpoints, and enqueues synthetic `StockReceived`, `OrderCreated`, `OrderConfirmed`, `OrderCancelled` and `QcReceiptCompleted` events into the producing outboxes.
9. Waits for every outbox to drain, records the replay result, republishes the same EventIds, and verifies that the second pass changes nothing.
10. Writes `tools/cutover/rehearsal-report.md` and stops the API.

## Recorded rehearsal (2026-10-19)

| Measure | Result |
|---|---|
| Provision + ETL + gates | 80.4 s |
| Cold start to `/health` | 0.7 s |
| Module smoke | 9/9 returned 200 |
| Replay events | 39,676 |
| First drain | 132.2 s |
| Second drain (same EventIds) | 99.1 s |
| Idempotent on redelivery | yes |
| Inventory baseline → replay | 1,530 rows / 30,442,813 → 1,556 rows / 32,237,007 |

The baseline reads `Warehouse.InventoryBalances` (receipts minus miscellaneous issues); the replay projects `StockReceived` only. The delta is the 191 QC-completed lots plus the issue/move exclusion explained below. The full output is in `tools/cutover/rehearsal-report.md`.

## Production cutover

1. Freeze writes in the legacy application and take a final backup.
2. Point `ConnectionStrings:Default` at the target database and run the same sequence as the rehearsal, without dropping the staging database: `tools/migration/99-run-all.ps1 -TargetDb <target>`.
3. Verify `90-gates.sql` prints `All reconciliation gates passed.` and `Coverage gate passed`.
4. Start the API against the target from a cold start; require `/health` 200 and a smoke call per module.
5. Replay the Reporting outbox and verify the projection counts and the second-pass idempotency check.
6. Switch the client/DNS to the modular API.
7. Keep the legacy database read-only for seven days.
8. Have every migrated user change their password on first sign-in; the migration marks all sentinel-hashed accounts with `MustChangePassword`.

## Rollback

The legacy database is never mutated by the ETL. To roll back:

1. Stop the modular API.
2. Restore the pre-ETL target snapshot, or drop and re-provision the target.
3. Point the connection string back at `ElixirDepotDry` and restart the legacy application.

The database-only rehearsal measured a 0.8 s recovery time. The application restart is included in the staging rehearsal cold-start measurement.

## Known cutover delta

`QcReceiptCompleted` is a real cross-module event: replaying it creates Warehouse receipts for completed QC lots that the legacy `WareHouseReceive` flag left null. The Reporting inventory projection also consumes `StockReceived` only, while the warm backfill reads `Warehouse.InventoryBalances` (receipts minus issues). The replay therefore proves the projection path and idempotency, not a full on-hand rebuild: `Warehouse.InventoryBalances` remains authoritative for stock. Review the affected lots with the warehouse team before the production run.

## Forced password change

Migrated users are imported with an unusable PBKDF2 sentinel and `MustChangePassword = 1`. Sign-in returns a token carrying `must_change_password`; the API blocks every endpoint except `POST /api/Login/change-password`, `/refresh` and `/revoke`. A successful password change clears the flag and returns a normal token. Administrator resets set the flag again.
