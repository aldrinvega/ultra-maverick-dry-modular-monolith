# CRUD parity plan

Generated 2026-10-19.

Legacy and new action counts are measured from `[Http*]` attributes per controller. This is a sizing tool: a controller appearing in both lists is not proof of parity — the actions inside must be compared name by name before any wave is marked done.

| Controller | Legacy | New |
|---|---|---|
| AccountTitleController | 4 | 0 |
| AccountTitlesController | 0 | 6 |
| AuthController | 0 | 3 |
| BugController | 4 | 0 |
| CancelledOrdersController | 5 | 0 |
| CatalogImportController | 0 | 2 |
| ChecklistController | 12 | 0 |
| CustomerController | 20 | 0 |
| CustomersController | 0 | 6 |
| DepartmentsController | 0 | 9 |
| FormulasController | 0 | 6 |
| ImportController | 10 | 0 |
| InventoryController | 4 | 0 |
| ItemsController | 0 | 6 |
| LaboratoryTestController | 12 | 0 |
| LabTestMasterListController | 47 | 0 |
| LabTestsController | 0 | 5 |
| LoginController | 1 | 0 |
| LotController | 20 | 0 |
| LotNamesController | 0 | 6 |
| MainMenusController | 0 | 9 |
| ManufacturingController | 0 | 8 |
| MiscellaneousController | 18 | 0 |
| ModuleController | 20 | 0 |
| ModulesController | 0 | 10 |
| OrderingController | 54 | 0 |
| OrdersController | 0 | 8 |
| PlanningController | 18 | 0 |
| PreparationController | 13 | 0 |
| ProcurementImportController | 0 | 1 |
| ProductTypeController | 6 | 0 |
| ProductTypesController | 0 | 6 |
| PurchaseOrdersController | 0 | 3 |
| QcReceiptsController | 0 | 4 |
| RawMaterialController | 21 | 0 |
| ReasonController | 10 | 0 |
| ReasonsController | 0 | 6 |
| ReceivingController | 30 | 0 |
| ReportController | 27 | 0 |
| ReportsController | 0 | 1 |
| RoleController | 20 | 0 |
| RolesController | 0 | 10 |
| SupplierController | 10 | 0 |
| SuppliersController | 0 | 6 |
| TransactionController | 8 | 0 |
| TransformationController | 18 | 0 |
| UomController | 10 | 0 |
| UomsController | 0 | 6 |
| UserController | 22 | 0 |
| UsersController | 0 | 7 |
| WarehouseController | 20 | 0 |
| WarehouseReceiptsController | 0 | 10 |

## Wave order

1. Controllers with no new counterpart at all (largest gaps first): Import, Preparation, Planning, Transformation, Report.
2. Controllers present on both sides but with more legacy actions: Identity, Catalog, Procurement, Quality, Warehouse, Orders.
3. Any legacy action with no equivalent is either ported, replaced by a module read with filters, or retired with a written reason.
