# Import parity (Phase C2, wave 2)

Legacy `ImportController` (`ELIXIR.API\Controllers\IMPORT_CONTROLLER\ImportController.cs`, 549 lines) exposes
10 actions. Only three of them accept a file; the rest are manual add or read actions that map onto commands
and queries the modules already have.

## Action inventory

| # | Legacy action | Shape | New equivalent |
|---|---|---|---|
| 1 | `UploadPOSummaryReport` | `POST`, `IFormFile` | none — needs porting |
| 2 | `UploadRawMaterialsReport` | `POST`, `IFormFile` | none — needs porting |
| 3 | `UploadFormulaReport` | `POST`, `IFormFile` | none — needs porting |
| 4 | `AddNewPOManual` | `POST`, JSON | `Procurement` `AddPurchaseOrder` |
| 5 | `AddNewRawMaterialManual` | `POST`, JSON | `Catalog` `AddNewRawMaterial` |
| 6 | `AddNewFormulaManual` | `POST`, JSON | `Catalog` `AddNewFormula` |
| 7 | `AddNewCustomerManual` | `POST`, JSON | `Catalog` `AddNewCustomer` |
| 8 | `GetAllPoSummary` | `GET` | `Procurement` `GetPurchaseOrders` |
| 9 | `GetPoSummaryById` | `GET` | needs verification against `Procurement` queries |
| 10 | second `GET` | `GET` | needs verification |

Actions 4-7 are equivalent, though the route names differ. Actions 8-10 need a per-action check against the
Procurement query surface before they are marked ported; the static name comparison in
`docs/endpoint-parity.md` cannot decide this because the legacy controller declares routes in `[Route]`
attributes rather than in `[Http*("...")]`.

## Decision: upload contract

The three uploads are the only genuinely new surface. They are ported as a single shared shape rather than
three bespoke endpoints.

- **Transport**: `multipart/form-data` with one `IFormFile`. Accept `.xlsx` only, reject by content type and
  extension, cap at 10 MB, and read the file as a stream rather than buffering it whole.
- **Ownership**: each upload belongs to the module that owns the data — Procurement for the PO summary, Catalog
  for raw materials and formulas. The API controller only performs transport validation and forwards a stream
  reference; it does not parse the workbook.
- **Validation**: every row is validated against the same rules as the equivalent manual command. A row that
  fails does not abort the file; the response reports counts of imported, skipped and failed rows together with
  the failing row numbers and reasons.
- **Idempotency**: each import is keyed on the natural business key already enforced by a unique index
  (`PoNumber`, `ItemCode`, `(ItemId, Version)` for formulas). A re-upload of the same file must create no
  duplicates; it reports the rows as skipped rather than failing.
- **Partial failure**: the whole file is processed inside one transaction only when every row is valid.
  Otherwise nothing is committed and the response is a 422 carrying the row-level report, so a half-imported
  workbook is impossible.

## Testing

Each upload needs three tests at minimum: a healthy file imports the expected row count; a file with one invalid
row commits nothing and reports that row; and re-uploading the same file adds nothing.
