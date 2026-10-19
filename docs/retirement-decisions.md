# Retirement decisions

Decided 2026-10-19 as part of the final migration pass. The authority for these calls was delegated for this pass. The machine-readable ruling for every legacy action is in `tools/parity/rulings.json`; the generated audit is `docs/action-parity.md` and the per-action table is `docs/action-rulings.md`.

## Reports

| Legacy action | Decision | Evidence |
|---|---|---|
| `ConsolidatedReport` | Retire | Hardcodes company `31`, department `5001` and account title; joins a dozen tables by string keys. The only in-repo consumers are the report controller and `ExportConsolidatedReport`; both are retired. |
| `OrderVsServeReports` (+ pagination) | Retire | The `!order.IsReject != true` filter inverts the selection and the paginated variant divides by an unset quantity. The export consumer is retired with it. |
| All other report actions | Replace | Each maps to a module read or a Reporting projection; see `docs/reporting-port-decisions.md`. |

If the business still needs a consolidated ledger or serve-vs-order, it must be specified as a new Reporting projection. No legacy consumer is re-pointed.

## Legacy workflows

| Workflow | Decision | Reason |
|---|---|---|
| Order scheduling, allocation and move-order write flow | Retire | Historical move orders and transacted move orders remain readable through `GetMoveOrders` and `GetTransactedMoveOrders`. New orders use the modular create/cancel lifecycle; stock changes use Warehouse receipts and `AdjustStock`. |
| Planning and preparation sub-steps (requirements, versions, mixing batches) | Replace/Retire | The transformation request carries lines and per-line preparations; `Prepare` and `Finish` replace the sub-step commands. Batch-remaining detail is not carried forward. |
| Checklist master-data administration | Retire | Checklist types, questions and answers are migration-managed; the QC receipt read exposes the answers. |
| Laboratory master lists (swab type, analysis, product condition, disposition, parameter) | Retire | Source tables hold no rows; SampleType (one row) is ported. |
| Laboratory return/reject bookkeeping | Retire | Acceptance and results are recorded through the new laboratory request lifecycle. |
| Miscellaneous receipt/issue write flow | Retire | Migrated reads remain; new stock enters through warehouse receipts, QC completion and stock adjustments. |
| Warehouse console scan/reject/return/cancel flow | Retire | Superseded by `AddWarehouseReceipt`, `AdjustStock`, QC rejection and the QC completion event; receipt reads remain. |
| Legacy manual import routes | Retire | Superseded by the xlsx import endpoints and the single-row Add endpoints. |
| Error-simulation routes (`BugController`) | Retire | Not a business API. |
| Excel export handlers (`ExportRawMaterials`, `ExportMoveOrderRepors`, `ExportMoveOrderHistoryReports`, `ExportMoveOrderReport`, `ExportItemsWithBbd`, `ExportMiscIssueHistoryReports`, `ExportMiscReceiptHistoryReports`, `ExportServedVsOrdered`, `ExportConsolidatedReports`) | Retire | The modular API returns the same reads as JSON; server-side Excel generation is not carried forward. The two reports that these exports consumed (`ConsolidatedReport`, serve-vs-order) are retired. |

## Consequences

- 456 legacy actions are accounted for: 40 ported (reference-data CRUD), 235 replaced by an existing modular endpoint, 181 retired with a written reason.
- The parity gate fails if an action is unruled or a port target is missing.
- Retired workflows have no compatibility shim. Any external client still calling them must move to the replacement named in `docs/action-rulings.md`.
