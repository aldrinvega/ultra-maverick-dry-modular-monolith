# Reporting port-or-retire decisions

The legacy `ReportController` exposes 25 route-based actions (plus `ItemsWithBbdPagination` declared inline),
nearly all duplicated as a plain and a paginated variant. Porting them one-for-one would create a second,
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
| `MiscellaneousReceiptReport` (+Pagination) | Misc receipt list | Port (small) | Add `GET api/WarehouseReceiving/GetMiscellaneousReceiptsWithPagination`; data is already migrated |
| `MiscellaneousIssueReport` (+Pagination) | Misc issue list | Port (small) | Add `GET api/WarehouseReceiving/GetMiscellaneousIssuesWithPagination`; data is already migrated |
| `ItemsWithBbdPagination` | Expiry list | Port (small) | Warehouse query over receipt expiry; no cross-module join needed |
| `NearlyExpireItemsReport` (+Pagination) | Derived stock | Port | Reporting/ Warehouse query combining on-hand with expiry; must not reuse the legacy join fan-out |
| `InventoryMovementReport` (+Pagination) | Ledger aggregation | Port | Reporting read model over `Warehouse.InventoryMovements`, which the migration now populates |
| `OrderVsServeReports` (+Pagination) | Serve-vs-order aggregate | Retire | Legacy implementation is buggy: the `!order.IsReject != true` filter inverts the selection and the paginated variant divides by an unset quantity. Rebuild from Reporting if the business still needs it, as a new projection with its own spec |
| `ConsolidatedReport` (+Pagination) | Mixed cross-module ledger | Retire | Hardcodes company code `31`, department `5001` and account title, and joins a dozen tables by string keys. Superseded by per-module reads plus Reporting projections |

## Consequences

- **11 report actions are retired or replaced outright**; no new code beyond query filters on existing endpoints.
- **3 small endpoints** are added for miscellaneous receipts, miscellaneous issues and BBD/expiry.
- **3 projections** are added to Reporting (nearly-expire, inventory movement, and optionally a rebuilt
  serve-vs-order) — these are the only reports that genuinely need a read model.
- Any consumer of `ConsolidatedReport` must be re-pointed before cutover; that is a product decision that
  should be confirmed with the business.
