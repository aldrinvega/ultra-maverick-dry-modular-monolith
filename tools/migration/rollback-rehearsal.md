# Rollback rehearsal

The legacy database is only ever read by the migration, so a rollback never has to undo source
changes. Rehearsal proves the target can be discarded and the application re-pointed without data
loss.

## Preconditions

- Pre-ETL snapshot of the target database (`UltraMaverickDry_Migrated` empty schema, or the last good state).
- The legacy database `ElixirDepotDry` restored and readable.
- Application connection string is configuration, not code.

## Procedure

1. **Snapshot the migrated target**

   ```sql
   BACKUP DATABASE [UltraMaverickDry_Migrated]
   TO DISK = N'C:\ProgramData\UltramaverickRestore\target_post_migration.bak'
   WITH INIT, COPY_ONLY;
   ```

2. **Simulate failure**: stop the API and drop the target database.

   ```sql
   ALTER DATABASE [UltraMaverickDry_Migrated] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
   DROP DATABASE [UltraMaverickDry_Migrated];
   ```

3. **Restore the snapshot** (or re-run `provision-target.ps1` plus the ETL scripts, which are re-runnable).

   ```sql
   RESTORE DATABASE [UltraMaverickDry_Migrated]
   FROM DISK = N'C:\ProgramData\UltramaverickRestore\target_post_migration.bak'
   WITH REPLACE, RECOVERY;
   ```

4. **Point the application at the legacy database** and start it. Reads must still work because the
   legacy schema is untouched.

5. **Acceptance**
   - API starts with `ConnectionStrings:Default` aimed at `ElixirDepotDry`.
   - `GET /health` returns 200.
   - A read endpoint (for example `GET /api/Reports/InventoryPosition`) responds, even if projections
     are empty on the legacy database.
   - Restore of the snapshot restores the post-migration row counts (see `90-gates.sql`).

6. **Record** the wall-clock time of steps 2-3 as the recovery time objective for the cutover runbook.

## Rehearsal result (2026-10-18)

Executed against `UltraMaverickDry_Migrated` after the full load:

| Step | Outcome |
|---|---|
| Snapshot to `target_post_migration.bak` | succeeded |
| Drop target database | succeeded |
| Restore from snapshot | succeeded |
| `90-gates.sql` after restore | **all 28 gates pass** |
| Spot-check counts after restore | Orders 16,203 · OrderLines 90,082 · ChecklistAnswers 83,765 · InventoryPositions 1,530 · OrderFulfillment 16,203 |

Measured recovery time objective: **0.8 seconds** (backup + drop + restore on local SQL Express).
That figure covers only the database round-trip; add application restart time when rehearsing on
staging.

## Notes

- Because the ETL is idempotent (guarded by `LegacyIdMap` and target `LegacyId`), the alternative to
  restoring a snapshot is dropping and re-provisioning, then replaying `10-identity.sql` through
  `16-warm-reporting.sql` in order.
- Keep the legacy database read-only for seven days after cutover, as the guide requires.
