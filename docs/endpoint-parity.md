# Endpoint parity

Generated 2026-10-18.

| Side | Actions |
|---|---|
| Legacy API | 464 |
| New API | 118 |
| Legacy actions with no same-named new action | 461 |

## Per-controller counts

| Controller | Legacy | New |
|---|---|---|
| AccountTitleController | 4 | 0 |
| AccountTitlesController | 0 | 6 |
| AuthController | 0 | 3 |
| BugController | 4 | 0 |
| CancelledOrdersController | 5 | 0 |
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
| LabTestsController | 0 | 3 |
| LoginController |  | 0 |
| LotController | 20 | 0 |
| LotNamesController | 0 | 6 |
| MainMenusController | 0 | 9 |
| ManufacturingController | 0 | 2 |
| MiscellaneousController | 18 | 0 |
| ModuleController | 20 | 0 |
| ModulesController | 0 | 10 |
| OrderingController | 54 | 0 |
| OrdersController | 0 | 2 |
| PlanningController | 18 | 0 |
| PreparationController | 13 | 0 |
| ProductTypeController | 6 | 0 |
| ProductTypesController | 0 | 6 |
| PurchaseOrdersController | 0 | 2 |
| QcReceiptsController | 0 | 2 |
| RawMaterialController | 21 | 0 |
| ReasonController | 10 | 0 |
| ReasonsController | 0 | 6 |
| ReceivingController | 30 | 0 |
| ReportController | 27 | 0 |
| ReportsController | 0 |  |
| RoleController | 20 | 0 |
| RolesController | 0 | 10 |
| SupplierController | 10 | 0 |
| SuppliersController | 0 | 6 |
| TransactionController | 8 | 0 |
| TransformationController | 18 | 0 |
| UomController | 10 | 0 |
| UomsController | 0 | 6 |
| UserController | 22 | 0 |
| UsersController | 0 | 6 |
| WarehouseController | 20 | 0 |
| WarehouseReceiptsController | 0 | 5 |

## Port waves (module-bounded, one PR each)

1. Identity parity remainder
2. Catalog parity remainder
3. Procurement parity remainder
4. Quality parity remainder
5. Warehouse parity remainder
6. Orders parity remainder
7. Reporting report endpoints (port or retire each legacy report action)
