# Reporting port-or-retire decisions

The legacy `ReportController` exposes 26 route-based actions (plus `ItemsWithBbdPagination` declared inline):
27 actions, nearly all duplicated as a plain and a paginated variant. Porting them one-for-one would create a second,
parallel read surface beside the module endpoints and reintroduce the cross-module joins the modular design
removed.

Decision rule applied:

- **Replaced** — the report is a filtered list of one module's entities. The module endpoint already exists or
  gains query filters; no bespoke report endpoint is built.
- **Port** — the report is a genuine aggregation or projection that no single-module read can answer. These
  become Reporting read models.
- **Retire** — superseded, or built on legacy behaviour that must not be carried forward.

## Decisions

| Legacy action | Kind | Decision | Replacement / reason |
|---|---|---|---|
| `QcReceivingReport` (+Pagination) | QC list | Replaced | `GET api/Receiving/GetQcReceiptsWithPagination`; add date-range filters |
| `WarehouseReceivingReport` (+Pagination) | Receipt list | Replaced | `GET api/WarehouseReceiving/GetWarehouseReceiptsWithPagination`; add date-range filters |
| `MoveOrderHistory` (+Pagination) | Move-order list | Replaced | `GET api/Ordering/GetMoveOrders`; add date/customer filters |
| `TransactedMoveOrderReport` (+Pagination) | Transacted list | Replaced | `GET api/Ordering/GetTransactedMoveOrders` |
| `ApprovedMoveOrdersReports` (+Pagination) | Status-filtered move orders | Replaced | Move orders filtered by status |
| `CancelledOrderReport` (+Pagination) | Cancellation list | Replaced | `GetAllListOfCancelledOrdersPagination` / `GetCancelledOrdersById` |
| `TransformationHistoryReport` (+Pagination) | Manufacturing history | Replaced | `GET api/Manufacturing/GetTransformationRequests`; source data is empty in this backup |
| `MiscellaneousReceiptReport` (+Pagination) | Misc receipt list | Replaced | `GET api/WarehouseReceiving/GetMiscellaneousReceiptsWithPagination`; data is already migrated |
| `MiscellaneousIssueReport` (+Pagination) | Misc issue list | Replaced | `GET api/WarehouseReceiving/GetMiscellaneousIssuesWithPagination`; data is already migrated |
| `ItemsWithBbdPagination` | Expiry list | Replaced | `GET api/WarehouseReceiving/GetExpiringReceipts`; no cross-module join needed |
| `NearlyExpireItemsReport` (+Pagination) | Derived stock | Port | Reporting/ Warehouse query combining on-hand with expiry; must not reuse the legacy join fan-out |
| `InventoryMovementReport` (+Pagination) | Ledger aggregation | Port | Reporting read model over `Warehouse.InventoryMovements`, which the migration now populates |
| `OrderVsServeReports` (+Pagination) | Serve-vs-order aggregate | Retire | Legacy implementation is buggy: the `!order.IsReject != true` filter inverts the selection and the paginated variant divides by an unset quantity. Rebuild from Reporting if the business still needs it, as a new projection with its own spec |
| `ConsolidatedReport` (+Pagination) | Mixed cross-module ledger | Retire | Hardcodes company code `31`, department `5001` and account title, and joins a dozen tables by string keys. Superseded by per-module reads plus Reporting projections |

## Consequences

- **All 27 report actions are decided**: 18 route variants are replaced by module reads, 4 genuine
  aggregations are ported as Reporting projections, and the two broken/cross-module reports
  (`ConsolidatedReport`, serve-vs-order) are retired.
- **3 small module endpoints** are added for miscellaneous receipts, miscellaneous issues and BBD/expiry.
- **2 projections** are added to Reporting (nearly-expire, inventory movement); a rebuilt serve-vs-order
  projection is optional and would need its own spec.
- The retired report routes and their Excel exports are not re-pointed; external clients must move to the
  replacements named above. See `docs/retirement-decisions.md`.
