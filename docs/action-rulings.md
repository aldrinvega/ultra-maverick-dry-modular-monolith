# Action rulings

Generated: 2026-10-19T15:05:17.9415291+08:00

Every legacy action must have exactly one disposition: Port, Replace or Retire.

| Controller | Verb | Route | Disposition | Target | Reason |
|---|---|---|---|---|---|
| AccountTitleController | Post | AddNewAccountTitle | Replace | api/AccountTitle | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| AccountTitleController | Get | GetAllAccountTitleAsync | Replace | api/AccountTitle | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| AccountTitleController | Get | GetAllAccountTitleAsyncPagination/{status} | Replace | api/AccountTitle | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| AccountTitleController | Patch | UpdateAccountTitleStatus | Replace | api/AccountTitle | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| BugController | Get | badrequest | Retire |  | Error-simulation route; retired. |
| BugController | Get | badrequest/{id} | Retire |  | Error-simulation route; retired. |
| BugController | Get | notfound | Retire |  | Error-simulation route; retired. |
| BugController | Get | servererror | Retire |  | Error-simulation route; retired. |
| CancelledOrdersController | Post | CancelOrder | Replace | api/Ordering/CancelOrder/{id} | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| CancelledOrdersController | Get | GetAllListOfCancelledOrdersPagination | Replace | api/Ordering/GetAllListOfCancelledOrdersPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| CancelledOrdersController | Get | GetCancelledOrdersById/{customerId} | Replace | api/Ordering/GetCancelledOrdersById/{customerId} | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ChecklistController | Post | AddNewChecklist | Retire |  | Checklist master data is migration-managed; the new API exposes the checklist through the QC receipt read, with no runtime admin surface. |
| ChecklistController | Post | AddNewChecklistQuestion | Retire |  | Checklist master data is migration-managed; the new API exposes the checklist through the QC receipt read, with no runtime admin surface. |
| ChecklistController | Post | AddNewChecklistType | Retire |  | Checklist master data is migration-managed; the new API exposes the checklist through the QC receipt read, with no runtime admin surface. |
| ChecklistController | Get | GetAllChecklists | Replace | api/Receiving/GetQcReceiptById/{id} | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ChecklistController | Get | GetAllChecklistsQuestions | Replace | api/Receiving/GetChecklistQuestionsByType/{checklistTypeId} | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ChecklistController | Get | GetAllChecklistTypes | Retire |  | Checklist master data is migration-managed; the new API exposes the checklist through the QC receipt read, with no runtime admin surface. |
| ChecklistController | Get | GetChecklistAnswerById/{id} | Replace | api/Receiving/GetQcReceiptById/{id} | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ChecklistController | Patch | SortChecklistTypes | Retire |  | Checklist master data is migration-managed; the new API exposes the checklist through the QC receipt read, with no runtime admin surface. |
| ChecklistController | Put | UpdateChecklistQuestion/{id:int} | Retire |  | Checklist master data is migration-managed; the new API exposes the checklist through the QC receipt read, with no runtime admin surface. |
| ChecklistController | Patch | UpdateChecklistQuestionStatus | Retire |  | Checklist master data is migration-managed; the new API exposes the checklist through the QC receipt read, with no runtime admin surface. |
| ChecklistController | Put | UpdateChecklistType/{id} | Retire |  | Checklist master data is migration-managed; the new API exposes the checklist through the QC receipt read, with no runtime admin surface. |
| ChecklistController | Patch | UpdateChecklistTypeStatus/{id:int} | Retire |  | Checklist master data is migration-managed; the new API exposes the checklist through the QC receipt read, with no runtime admin surface. |
| CustomerController | Put | ActivateCustomer/{id} | Replace | api/Customer | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| CustomerController | Put | ActivateFarm/{id} | Port | api/Customer/ActivateFarm/{id} | Ported to the modular endpoint named by target. |
| CustomerController | Post | AddNewCustomer | Replace | api/Customer | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| CustomerController | Post | AddNewFarm | Port | api/Customer/AddNewFarm | Ported to the modular endpoint named by target. |
| CustomerController | Get | GetAllActiveCustomer | Replace | api/Customer | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| CustomerController | Get | GetAllActiveFarms | Port | api/Customer/GetAllActiveFarms | Ported to the modular endpoint named by target. |
| CustomerController | Get | GetAllCustomers | Replace | api/Customer | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| CustomerController | Get | GetAllCustomerWithPagination/{status} | Replace | api/Customer | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| CustomerController | Get | GetAllCustomerWithPaginationOrig/{status} | Retire |  | Legacy duplicate pagination variant. |
| CustomerController | Get | GetAllFarms | Port | api/Customer/GetAllFarms | Ported to the modular endpoint named by target. |
| CustomerController | Get | GetAllFarmWithPagination/{status} | Port | api/Customer/GetAllFarmWithPagination/{status} | Ported to the modular endpoint named by target. |
| CustomerController | Get | GetAllFarmWithPaginationOrig/{status} | Retire |  | Legacy duplicate of the canonical paginated read; the new list endpoint accepts paging, search and status. |
| CustomerController | Get | GetAllInActiveCustomer | Replace | api/Customer | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| CustomerController | Get | GetAllInActiveFarms | Port | api/Customer/GetAllInActiveFarms | Ported to the modular endpoint named by target. |
| CustomerController | Get | GetCustomerById/{id} | Replace | api/Customer | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| CustomerController | Get | GetFarmById/{id} | Port | api/Customer/GetFarmById/{id} | Ported to the modular endpoint named by target. |
| CustomerController | Put | InActiveCustomer/{id} | Replace | api/Customer | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| CustomerController | Put | InActiveFarm/{id} | Port | api/Customer/InActiveFarm/{id} | Ported to the modular endpoint named by target. |
| CustomerController | Put | UpdateCustomer/{id} | Replace | api/Customer | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| CustomerController | Put | UpdateFarm/{id} | Port | api/Customer/UpdateFarm/{id} | Ported to the modular endpoint named by target. |
| ImportController | Post | AddNewCustomerManual | Retire |  | Superseded by the xlsx import endpoints and single-row Add endpoints. |
| ImportController | Post | AddNewFormulaManual | Retire |  | Superseded by the xlsx import endpoints and single-row Add endpoints. |
| ImportController | Post | AddNewPOManual | Retire |  | Superseded by the xlsx import endpoints and single-row Add endpoints. |
| ImportController | Post | AddNewRawMaterialManual | Retire |  | Superseded by the xlsx import endpoints and single-row Add endpoints. |
| ImportController | Post | AddNewSupplierSummary | Retire |  | Superseded by the xlsx import endpoints and single-row Add endpoints. |
| ImportController | Get | GetAllPoSummary | Retire |  | Superseded by the xlsx import endpoints and single-row Add endpoints. |
| ImportController | Get | GetPoSummaryById/{id} | Retire |  | Superseded by the xlsx import endpoints and single-row Add endpoints. |
| ImportController | Post | UploadFormulaReport | Replace | api/Catalog/UploadFormulaReport | Ported to the Catalog xlsx import endpoint. |
| ImportController | Post | UploadPOSummaryReport | Replace | api/Procurement/UploadPOSummaryReport | Ported to the Procurement xlsx import endpoint. |
| ImportController | Post | UploadRawMaterialsReport | Replace | api/Catalog/UploadRawMaterialsReport | Ported to the Catalog xlsx import endpoint. |
| InventoryController | Get | GetAllItemForInventoryPagination | Replace | api/WarehouseReceiving/GetInventoryBalance | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| InventoryController | Get | GetAllItemForInventoryPaginationOrig | Retire |  | Legacy duplicate pagination variant. |
| InventoryController | Get | MRPInventory | Replace | api/WarehouseReceiving/GetInventoryBalance | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| InventoryController | Get | RawmaterialInventory | Replace | api/WarehouseReceiving/GetInventoryBalance | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| LaboratoryTestController | Post | AcceptLabTestRequest | Replace | api/LaboratoryTest/AcceptRequest | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| LaboratoryTestController | Get | GetAllAcceptedItemsForLabTest | Replace | api/LaboratoryTest/GetLabTestRequests?status=Accepted | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| LaboratoryTestController | Get | GetAllAcceptedItemsForLabTestPagination | Replace | api/LaboratoryTest/GetLabTestRequests?status=Accepted | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| LaboratoryTestController | Get | GetAllNearlyExpiryItemsPagination | Replace | api/WarehouseReceiving/GetExpiringReceipts | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| LaboratoryTestController | Get | GetAllRejectedItemsForLabTest | Retire |  | Legacy lab return/reject bookkeeping is retired; the new request lifecycle records acceptance and results. |
| LaboratoryTestController | Get | GetAllRejectedItemsForLabTestPagination | Retire |  | Legacy lab return/reject bookkeeping is retired; the new request lifecycle records acceptance and results. |
| LaboratoryTestController | Get | GetAllRequestedItemsForLabTest | Replace | api/LaboratoryTest/GetLabTestRequests?status=Requested | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| LaboratoryTestController | Get | GetAllReturnedItems | Retire |  | Legacy lab return/reject bookkeeping is retired; the new request lifecycle records acceptance and results. |
| LaboratoryTestController | Get | GetAllReturnedItemsPagination | Retire |  | Legacy lab return/reject bookkeeping is retired; the new request lifecycle records acceptance and results. |
| LaboratoryTestController | Post | RejectItemsForLabTest | Retire |  | Legacy lab return/reject bookkeeping is retired; the new request lifecycle records acceptance and results. |
| LaboratoryTestController | Post | RequestLabTest | Replace | api/LaboratoryTest/AddLabTestRequest | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| LaboratoryTestController | Post | ReturnItemsRequestedForLabTest | Retire |  | Legacy lab return/reject bookkeeping is retired; the new request lifecycle records acceptance and results. |
| LabTestMasterListController | Post | AddNewAnalysis | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Post | AddNewDisposition | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Post | AddNewParameter | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Post | AddNewProductCondition | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Post | AddNewSampleType | Port | api/LabTestMasterList/AddNewSampleType | Ported to the modular endpoint named by target. |
| LabTestMasterListController | Post | AddNewTypeOfSwab | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Get | GetAllAnalyses | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Get | GetAllAnalysesPagination | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Get | GetAllAnalysesPaginationOrig/{status} | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Get | GetAllAnalysisByStatus/{status} | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Get | GetAllDisposition | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Get | GetAllDispositionByStatus/{status} | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Get | GetAllDispositionPagination | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Get | GetAllDispositionPaginationOrig/{status} | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Get | GetAllParameter | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Get | GetAllParametersPagination | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Get | GetAllParametersPaginationOrig/{status} | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Get | GetAllProductCondition | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Get | GetAllProductConditionPagination | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Get | GetAllProductConditionPaginationOrig/{status} | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Get | GetAllSampleTypeByStatus/{status} | Port | api/LabTestMasterList/GetAllSampleTypeByStatus/{status} | Ported to the modular endpoint named by target. |
| LabTestMasterListController | Get | GetAllSampleTypePagination | Port | api/LabTestMasterList/GetAllSampleTypePagination | Ported to the modular endpoint named by target. |
| LabTestMasterListController | Get | GetAllSampleTypePaginationOrig/{status} | Retire |  | Legacy duplicate pagination variant. |
| LabTestMasterListController | Get | GetAllSampleTypes | Port | api/LabTestMasterList/GetAllSampleTypes | Ported to the modular endpoint named by target. |
| LabTestMasterListController | Get | GetAllTypeOfSwab | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Get | GetAllTypeOfSwabByStatus/{status} | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Get | GetAllTypeOfSwabPagination | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Get | GetAllTypeOfSwabPaginationOrig/{status} | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Get | GetAnalysisById{id} | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Get | GetDispositionById/{id} | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Get | GetParametersById/{id} | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Get | GetParametersByStatus/{status} | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Get | GetProductConditionById{id:int} | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Get | GetSampleTypeById{id} | Port | api/LabTestMasterList/GetSampleTypeById/{id} | Ported to the modular endpoint named by target. |
| LabTestMasterListController | Get | GetTypeOfSwabById{id} | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Put | UpdateAnalysis | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Put | UpdateAnalysisStatus | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Put | UpdateDisposition | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Put | UpdateDispositionStatus | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Put | UpdateParameter | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Put | UpdateParameterStatus | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Put | UpdateProductCondition | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Put | UpdateProductConditionStatus | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Put | UpdateSampleType | Port | api/LabTestMasterList/UpdateSampleType | Ported to the modular endpoint named by target. |
| LabTestMasterListController | Put | UpdateSampleTypeStatus | Port | api/LabTestMasterList/UpdateSampleTypeStatus | Ported to the modular endpoint named by target. |
| LabTestMasterListController | Put | UpdateTypeOfSwab | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LabTestMasterListController | Put | UpdateTypeOfSwabStatus | Retire |  | Source master table is empty and the target has no rows; the reference list is migration-managed. |
| LoginController | Post | authenticate | Replace | api/Login/authenticate | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| LotController | Put | ActivateLotCategory/{id} | Port | api/Lot/ActivateLotCategory/{id} | Ported to the modular endpoint named by target. |
| LotController | Put | ActivateLotName/{id} | Replace | api/Lot | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| LotController | Post | AddNewLotCategory | Port | api/Lot/AddNewLotCategory | Ported to the modular endpoint named by target. |
| LotController | Post | AddNewLotName | Replace | api/Lot | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| LotController | Get | GetAllActiveLotCategories | Port | api/Lot/GetAllActiveLotCategories | Ported to the modular endpoint named by target. |
| LotController | Get | GetAllActiveLotName | Replace | api/Lot | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| LotController | Get | GetAllInActiveLotCategory | Port | api/Lot/GetAllInActiveLotCategory | Ported to the modular endpoint named by target. |
| LotController | Get | GetAllInActiveLotName | Replace | api/Lot | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| LotController | Get | GetAllLotCategories | Port | api/Lot/GetAllLotCategories | Ported to the modular endpoint named by target. |
| LotController | Get | GetAllLotCategoryWithPagination/{status} | Port | api/Lot/GetAllLotCategoryWithPagination/{status} | Ported to the modular endpoint named by target. |
| LotController | Get | GetAllLotCategoryWithPaginationOrig/{status} | Retire |  | Legacy duplicate pagination variant. |
| LotController | Get | GetAllLotNames | Replace | api/Lot | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| LotController | Get | GetAllLotNameWithPagination/{status} | Replace | api/Lot | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| LotController | Get | GetAllLotNameWithPaginationOrig/{status} | Retire |  | Legacy duplicate pagination variant. |
| LotController | Get | GetLotCategoryById/{id} | Port | api/Lot/GetLotCategoryById/{id} | Ported to the modular endpoint named by target. |
| LotController | Get | GetLotNameById/{id} | Replace | api/Lot | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| LotController | Put | InActiveLotCategory/{id} | Port | api/Lot/InActiveLotCategory/{id} | Ported to the modular endpoint named by target. |
| LotController | Put | InActiveLotName/{id} | Replace | api/Lot | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| LotController | Put | UpdateLotCategory/{id} | Port | api/Lot/UpdateLotCategory/{id} | Ported to the modular endpoint named by target. |
| LotController | Put | UpdateLotName/{id} | Replace | api/Lot | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| MiscellaneousController | Put | ActivateIssue | Retire |  | Legacy miscellaneous receipt/issue write workflow is retired; the new API keeps the migrated reads and uses warehouse receipts/adjustments for stock. |
| MiscellaneousController | Put | ActivateReceipt | Retire |  | Legacy miscellaneous receipt/issue write workflow is retired; the new API keeps the migrated reads and uses warehouse receipts/adjustments for stock. |
| MiscellaneousController | Post | AddNewMiscellaneousIssue | Retire |  | Legacy miscellaneous receipt/issue write workflow is retired; the new API keeps the migrated reads and uses warehouse receipts/adjustments for stock. |
| MiscellaneousController | Post | AddNewMiscellaneousIssueDetails | Retire |  | Legacy miscellaneous receipt/issue write workflow is retired; the new API keeps the migrated reads and uses warehouse receipts/adjustments for stock. |
| MiscellaneousController | Post | AddNewMiscellaneousReceipt | Retire |  | Legacy miscellaneous receipt/issue write workflow is retired; the new API keeps the migrated reads and uses warehouse receipts/adjustments for stock. |
| MiscellaneousController | Post | AddNewMiscellaneousReceiptInWarehouse | Retire |  | Legacy miscellaneous receipt/issue write workflow is retired; the new API keeps the migrated reads and uses warehouse receipts/adjustments for stock. |
| MiscellaneousController | Put | CancelItemCodeInMiscellaneousIssue | Retire |  | Legacy miscellaneous receipt/issue write workflow is retired; the new API keeps the migrated reads and uses warehouse receipts/adjustments for stock. |
| MiscellaneousController | Get | GetAllActiveMiscellaneousIssueTransaction | Replace | api/WarehouseReceiving/GetMiscellaneousIssuesWithPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| MiscellaneousController | Get | GetAllAvailableStocksForMIsssue | Replace | api/WarehouseReceiving/GetInventoryBalance | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| MiscellaneousController | Get | GetAllDetailsFromWarehouseByMReceipt | Replace | api/WarehouseReceiving/GetMiscellaneousReceiptsWithPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| MiscellaneousController | Get | GetAllDetailsInMiscellaneousIssue | Replace | api/WarehouseReceiving/GetMiscellaneousIssuesWithPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| MiscellaneousController | Get | GetAllMiscellaneousIssuePagination | Replace | api/WarehouseReceiving/GetMiscellaneousIssuesWithPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| MiscellaneousController | Get | GetAllMiscellaneousIssuePaginationOrig | Retire |  | Legacy duplicate pagination variant. |
| MiscellaneousController | Get | GetAllMiscellaneousReceiptPagination | Replace | api/WarehouseReceiving/GetMiscellaneousReceiptsWithPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| MiscellaneousController | Get | GetAllMiscellaneousReceiptPaginationOrig | Retire |  | Legacy duplicate pagination variant. |
| MiscellaneousController | Put | InActiveIssue | Retire |  | Legacy miscellaneous receipt/issue write workflow is retired; the new API keeps the migrated reads and uses warehouse receipts/adjustments for stock. |
| MiscellaneousController | Put | InActiveReceipt | Retire |  | Legacy miscellaneous receipt/issue write workflow is retired; the new API keeps the migrated reads and uses warehouse receipts/adjustments for stock. |
| MiscellaneousController | Put | UpdateMiscellaneousIssuePKey | Retire |  | Legacy miscellaneous receipt/issue write workflow is retired; the new API keeps the migrated reads and uses warehouse receipts/adjustments for stock. |
| ModuleController | Put | ActivateMainMenu/{id} | Replace | api/Module | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ModuleController | Put | ActivateModule/{id} | Replace | api/Module | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ModuleController | Post | AddNewMenu | Replace | api/Module | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ModuleController | Post | AddNewModule | Replace | api/Module | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ModuleController | Get | GetAllActiveMenu | Replace | api/Module | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ModuleController | Get | GetAllActiveModules | Replace | api/Module | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ModuleController | Get | GetAllInActiveMenu | Replace | api/Module | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ModuleController | Get | GetAllInActiveModules | Replace | api/Module | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ModuleController | Get | GetAllMainMenu | Replace | api/Module | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ModuleController | Get | GetAllModules | Replace | api/Module | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ModuleController | Get | GetAllModulesWithPagination/{status} | Replace | api/Module | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ModuleController | Get | GetAllModulesWithPaginationOrig/{status} | Retire |  | Legacy duplicate of the canonical paginated read; the new list endpoint accepts paging, search and status. |
| ModuleController | Get | GetById/{id} | Replace | api/Module | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ModuleController | Get | GetMenuById/{id} | Replace | api/Module | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ModuleController | Get | GetMenuByStatus/{status} | Replace | api/Module | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ModuleController | Get | GetModuleByStatus/{status} | Replace | api/Module | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ModuleController | Put | InActiveMenu/{id} | Replace | api/Module | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ModuleController | Put | InActiveModule/{id} | Replace | api/Module | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ModuleController | Put | UpdateMenu/{id} | Replace | api/Module | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ModuleController | Put | UpdateModule/{id} | Replace | api/Module | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| OrderingController | Put | AddDeliveryStatus | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Post | AddNewOrder | Replace | api/Ordering/AddOrder | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| OrderingController | Put | AddPlateNumberInMoveOrder | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Put | Allocate | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Get | ApprovedMoveOrderPagination | Replace | api/Ordering/GetMoveOrders | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| OrderingController | Get | ApprovedMoveOrderPaginationOrig | Retire |  | Legacy duplicate pagination variant. |
| OrderingController | Put | ApproveListOfMoveOrder | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Put | ApprovePreparedDate | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Post | CancelForPendingAllocation | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Put | CancelOrders | Replace | api/Ordering/CancelOrder/{id} | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| OrderingController | Put | CancelOrdersInMoveOrder/{id} | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Put | CancelPreparedItems | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Get | DetailedListOfOrders | Replace | api/Ordering/GetOrders | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| OrderingController | Put | EditOrderQuantity | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Get | GetAllApprovedMoveOrder | Replace | api/Ordering/GetMoveOrders | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| OrderingController | Get | GetAllApprovedOrderCalendar | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Get | GetAllForApprovalMoveOrderPagination | Replace | api/Ordering/GetMoveOrders | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| OrderingController | Get | GetAllForApprovalMoveOrderPaginationOrig | Retire |  | Legacy duplicate pagination variant. |
| OrderingController | Get | GetAllListForMoveOrderPagination | Replace | api/Ordering/GetMoveOrders | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| OrderingController | Get | GetAllListForScheduleApproval | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Get | GetAllListOfApprovedPreparedForMoveOrder | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Get | GetAllListOfCancelledOrders | Replace | api/Ordering/GetAllListOfCancelledOrdersPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| OrderingController | Get | GetAllListofOrders | Replace | api/Ordering/GetOrders | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| OrderingController | Get | GetAllListofOrdersForAllocation | Replace | api/Ordering/GetOrders | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| OrderingController | Get | GetAllListOfOrdersForAllocationPagination | Replace | api/Ordering/GetOrders | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| OrderingController | Get | GetAllListOfOrdersForMoveOrder | Replace | api/Ordering/GetMoveOrders | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| OrderingController | Get | GetAllListOfOrdersPagination | Replace | api/Ordering/GetOrders | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| OrderingController | Put | GetAllListofPreparedDate | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Get | GetAllOrdersForScheduleApproval | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Get | GetAllOutOfStockByItemCodeAndOrderDate | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Get | GetAvailableStockFromWarehouse | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Get | GetTotalListForMoveOrder | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Get | ListOfMoveOrdersForTransact | Replace | api/Ordering/GetTransactedMoveOrders | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| OrderingController | Get | ListOfPreparedItemsForMoveOrder | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Put | ManualAllocation | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Get | OrderSummary | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Post | PrepareItemsForMoveOrder | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Get | PrintMultipleMoveOrderSlip | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Put | RejectApproveListOfMoveOrder | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Get | RejectedMoveOrderPagination | Replace | api/Ordering/GetMoveOrders | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| OrderingController | Get | RejectedMoveOrderPaginationOrig | Retire |  | Legacy duplicate pagination variant. |
| OrderingController | Put | RejectListOfMoveOrder | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Put | RejectPreparedDate | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Put | ReturnCancelledOrders | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Put | ReturnMoveOrderForApproval | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Put | SchedulePreparedOrderedDate | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Post | SetBeingPrepared | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Post | TransactListOfMoveOrders | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Post | UnsetBeingPrepared | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Put | UpdatePrintStatus | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Post | ValidateNewOrders | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| OrderingController | Get | ViewMoveOrderForApproval | Retire |  | Legacy scheduling/allocation/move-order write workflow is retired in favour of the modular order lifecycle plus Warehouse receipts and stock adjustments; historical move orders remain readable through GetMoveOrders/GetTransactedMoveOrders. |
| PlanningController | Post | AddNewTransformationRequest | Replace | api/Manufacturing/AddTransformationRequest | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| PlanningController | Post | AddNewTransformationRequirements | Retire |  | Legacy planning sub-step is superseded by the transformation request lines and preparations in the Manufacturing module. |
| PlanningController | Put | ApproveTransformRequest/{id} | Replace | api/Manufacturing/Approve/{id} | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| PlanningController | Put | CancelTransformationRequest/{id} | Replace | api/Manufacturing/Cancel/{id} | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| PlanningController | Put | EditTransformationRequest/{id} | Retire |  | Legacy planning sub-step is superseded by the transformation request lines and preparations in the Manufacturing module. |
| PlanningController | Get | GetAllApprovedTransformationRequest | Replace | api/Manufacturing/GetTransformationRequests?status=Approved | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| PlanningController | Get | GetAllAvailableFormulaCode | Replace | api/Transformation/GetAllFormulasWithPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| PlanningController | Get | GetAllCancelledTransformationRequest | Replace | api/Manufacturing/GetTransformationRequests?status=Cancelled | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| PlanningController | Get | GetAllCancelRequirementsStatus/{id} | Retire |  | Legacy planning sub-step is superseded by the transformation request lines and preparations in the Manufacturing module. |
| PlanningController | Get | GetAllPendingRequest | Replace | api/Manufacturing/GetTransformationRequests?status=Pending | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| PlanningController | Get | GetAllPendingRequestWithRequirements/{id} | Replace | api/Manufacturing/GetTransformationRequestById/{id} | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| PlanningController | Get | GetAllPendingTransformationRequest | Replace | api/Manufacturing/GetTransformationRequests?status=Pending | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| PlanningController | Get | GetAllRejectRequest | Replace | api/Manufacturing/GetTransformationRequests?status=Rejected | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| PlanningController | Get | GetAllRejectRequirementsStatus/{id} | Retire |  | Legacy planning sub-step is superseded by the transformation request lines and preparations in the Manufacturing module. |
| PlanningController | Get | GetAllRequirementsByTransformId/{id} | Replace | api/Manufacturing/GetTransformationRequestById/{id} | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| PlanningController | Get | GetAllVersionByItemCode | Retire |  | Legacy planning sub-step is superseded by the transformation request lines and preparations in the Manufacturing module. |
| PlanningController | Put | RejectTransformationRequest/{id} | Replace | api/Manufacturing/Reject/{id} | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| PlanningController | Put | RequestRejectTransformationRequest/{id} | Retire |  | Legacy planning sub-step is superseded by the transformation request lines and preparations in the Manufacturing module. |
| PreparationController | Put | FinishedMixedMaterialsForWarehouse/{id} | Replace | api/Manufacturing/Finish/{id} | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| PreparationController | Get | GetAllBatchRemainingPerMixing | Retire |  | Legacy mixing batch-remaining detail is retired; preparation quantities are carried per line on the transformation request. |
| PreparationController | Get | GetAllRequestForMixing | Replace | api/Manufacturing/GetTransformationRequests?status=Prepared | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| PreparationController | Get | GetAllRequirementsByTransformId | Replace | api/Manufacturing/GetTransformationRequestById/{id} | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| PreparationController | Get | GetAllRequirementsForMixing | Replace | api/Manufacturing/GetTransformationRequestById/{id} | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| PreparationController | Get | GetAllStocks | Replace | api/WarehouseReceiving/GetInventoryBalance | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| PreparationController | Get | GetAllTransformationForMixing | Replace | api/Manufacturing/GetTransformationRequests | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| PreparationController | Get | GetAllTransformationForMixingPagination | Replace | api/Manufacturing/GetTransformationRequests | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| PreparationController | Get | GetRawmaterialDetailsInWarehouse | Replace | api/WarehouseReceiving/GetInventoryBalance | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| PreparationController | Get | GetRemaining | Retire |  | Legacy mixing batch-remaining detail is retired; preparation quantities are carried per line on the transformation request. |
| PreparationController | Get | GetTransformationFormula | Replace | api/Transformation/GetFormulaById/{id} | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| PreparationController | Get | GetTransformationFormulaPagination | Replace | api/Transformation/GetAllFormulasWithPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| PreparationController | Put | PrepareMaterialsForRequest/{id} | Replace | api/Manufacturing/Prepare/{id} | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ProductTypeController | Post | AddNewProductType | Replace | api/ProductType | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ProductTypeController | Get | GetAllPaginationByStatus/{status} | Replace | api/ProductType | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ProductTypeController | Get | GetAllProductTypePagination | Replace | api/ProductType | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ProductTypeController | Get | GetProductTypeById/{id} | Replace | api/ProductType | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ProductTypeController | Put | UpdateProductType | Replace | api/ProductType | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ProductTypeController | Put | UpdateProductTypeStatus | Replace | api/ProductType | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RawMaterialController | Put | ActivateItemCategory/{id} | Port | api/RawMaterial/ActivateItemCategory/{id} | Ported to the modular endpoint named by target. |
| RawMaterialController | Put | ActivateRawMaterial/{id} | Replace | api/RawMaterial | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RawMaterialController | Post | AddNewItemCategories | Port | api/RawMaterial/AddNewItemCategories | Ported to the modular endpoint named by target. |
| RawMaterialController | Post | CreateNewRawMaterials | Replace | api/RawMaterial | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RawMaterialController | Get | GetAllActiveItemCategories | Port | api/RawMaterial/GetAllActiveItemCategories | Ported to the modular endpoint named by target. |
| RawMaterialController | Get | GetAllActiveRawMaterials | Replace | api/RawMaterial | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RawMaterialController | Get | GetAllInActiveItemCategories | Port | api/RawMaterial/GetAllInActiveItemCategories | Ported to the modular endpoint named by target. |
| RawMaterialController | Get | GetAllInActiveRawMaterials | Replace | api/RawMaterial | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RawMaterialController | Get | GetAllItemCategory | Port | api/RawMaterial/GetAllItemCategory | Ported to the modular endpoint named by target. |
| RawMaterialController | Get | GetAllItemCategoryById/{id} | Port | api/RawMaterial/GetAllItemCategoryById/{id} | Ported to the modular endpoint named by target. |
| RawMaterialController | Get | GetAllItemCategoryWithPagination/{status} | Port | api/RawMaterial/GetAllItemCategoryWithPagination/{status} | Ported to the modular endpoint named by target. |
| RawMaterialController | Get | GetAllItemCategoryWithPaginationOrig/{status} | Retire |  | Legacy duplicate pagination variant. |
| RawMaterialController | Get | GetAllRawMaterials | Replace | api/RawMaterial | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RawMaterialController | Get | GetAllRawMaterialWithPagination/{status} | Replace | api/RawMaterial | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RawMaterialController | Get | GetAllRawMaterialWithPaginationOrig/{status} | Retire |  | Legacy duplicate pagination variant. |
| RawMaterialController | Get | GetRawMaterialsById/{id} | Replace | api/RawMaterial | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RawMaterialController | Get | GetSample | Retire |  | Debug/sample route; not part of the business API. |
| RawMaterialController | Put | InActiveItemCategory/{id} | Port | api/RawMaterial/InActiveItemCategory/{id} | Ported to the modular endpoint named by target. |
| RawMaterialController | Put | InActiveRawMaterial/{id} | Replace | api/RawMaterial | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RawMaterialController | Put | UpdateItemCategories/{id} | Port | api/RawMaterial/UpdateItemCategories/{id} | Ported to the modular endpoint named by target. |
| RawMaterialController | Put | UpdateRawMaterials/{id} | Replace | api/RawMaterial | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReasonController | Put | ActivateReason/{id} | Replace | api/Reason | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReasonController | Post | AddNewReason | Replace | api/Reason | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReasonController | Get | GetAllActiveReason | Replace | api/Reason | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReasonController | Get | GetAllInActiveReason | Replace | api/Reason | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReasonController | Get | GetAllReason | Replace | api/Reason | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReasonController | Get | GetAllReasonById/{id} | Replace | api/Reason | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReasonController | Get | GetAllReasonWithPagination/{status} | Replace | api/Reason | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReasonController | Get | GetAllReasonWithPaginationOrig/{status} | Retire |  | Legacy duplicate of the canonical paginated read; the new list endpoint accepts paging, search and status. |
| ReasonController | Put | InActiveReason/{id} | Replace | api/Reason | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReasonController | Put | UpdateReason/{id} | Replace | api/Reason | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReceivingController | Post | AddNewReceivingInformationInPO | Retire |  | Legacy QC receiving sub-step is retired; the new Quality/Warehouse event flow covers receipt, rejection and expiry reads. |
| ReceivingController | Post | AddNewRejectInPo | Replace | api/Receiving/Reject/{id} | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReceivingController | Put | ApproveNearlyExpire/{id} | Retire |  | Legacy QC receiving sub-step is retired; the new Quality/Warehouse event flow covers receipt, rejection and expiry reads. |
| ReceivingController | Put | CancelPO/{id} | Retire |  | Legacy QC receiving sub-step is retired; the new Quality/Warehouse event flow covers receipt, rejection and expiry reads. |
| ReceivingController | Get | GetAllAvailableForWarehouseReceivingWithPagination | Replace | api/WarehouseReceiving/GetWarehouseReceiptsWithPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReceivingController | Get | GetAllAvailableForWarehouseReceivingWithPaginationOrig | Retire |  | Legacy duplicate pagination variant. |
| ReceivingController | Get | GetAllAvailablePo | Replace | api/Ordering/GetPurchaseOrders?status=Open | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReceivingController | Get | GetAllAvailablePoWithPagination | Replace | api/Ordering/GetPurchaseOrders?status=Open | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReceivingController | Get | GetAllAvailablePoWithPaginationOrig | Retire |  | Legacy duplicate pagination variant. |
| ReceivingController | Get | GetAllCancelledPo | Replace | api/Ordering/GetPurchaseOrders?status=Cancelled | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReceivingController | Get | GetAllCancelledPoWithPagination | Replace | api/Ordering/GetPurchaseOrders?status=Cancelled | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReceivingController | Get | GetAllCancelledPoWithPaginationOrig | Retire |  | Legacy duplicate pagination variant. |
| ReceivingController | Get | GetAllChecklist | Replace | api/Receiving/GetQcReceiptById/{id} | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReceivingController | Get | GetAllNearlyExpireRawMaterials | Replace | api/WarehouseReceiving/GetExpiringReceipts | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReceivingController | Get | GetAllNearlyExpireWithPagination | Replace | api/WarehouseReceiving/GetExpiringReceipts | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReceivingController | Get | GetAllNearlyExpireWithPaginationOrig | Retire |  | Legacy duplicate pagination variant. |
| ReceivingController | Get | GetAllWarehouseConfirmRejectWithPagination | Retire |  | Legacy QC reject-confirmation list retired; rejects are part of the QC receipt. |
| ReceivingController | Get | GetAllWarehouseConfirmRejectWithPaginationOrig | Retire |  | Legacy duplicate pagination variant. |
| ReceivingController | Get | GetAllWarehouseReceivingConfirmReject | Retire |  | Legacy QC reject-confirmation list retired; rejects are part of the QC receipt. |
| ReceivingController | Get | GetChecklistByPoSummaryId | Replace | api/Receiving/GetQcReceiptById/{id} | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReceivingController | Get | GetDetailsForNearlyExpire | Replace | api/WarehouseReceiving/GetExpiringReceipts | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReceivingController | Get | GetNotification | Retire |  | Legacy QC receiving sub-step is retired; the new Quality/Warehouse event flow covers receipt, rejection and expiry reads. |
| ReceivingController | Get | GetPoSummaryInformation | Replace | api/Ordering/GetPurchaseOrderById/{id} | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReceivingController | Put | ReceiveRawMaterialsById/{id} | Retire |  | Legacy QC receiving sub-step is retired; the new Quality/Warehouse event flow covers receipt, rejection and expiry reads. |
| ReceivingController | Put | RejectNearlyExpire/{id} | Retire |  | Legacy QC receiving sub-step is retired; the new Quality/Warehouse event flow covers receipt, rejection and expiry reads. |
| ReceivingController | Put | RejectRawMaterialsByReceivingId | Replace | api/Receiving/Reject/{id} | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReceivingController | Put | ReturnPoInAvailableList/{id} | Retire |  | Legacy QC receiving sub-step is retired; the new Quality/Warehouse event flow covers receipt, rejection and expiry reads. |
| ReceivingController | Put | UpdateReceivingId/{id} | Retire |  | Legacy QC receiving sub-step is retired; the new Quality/Warehouse event flow covers receipt, rejection and expiry reads. |
| ReceivingController | Put | WarehouseConfirmRejectbyQc/{id} | Retire |  | Legacy QC receiving sub-step is retired; the new Quality/Warehouse event flow covers receipt, rejection and expiry reads. |
| ReceivingController | Put | WarehouseReturnRejectbyQc/{id} | Retire |  | Legacy QC receiving sub-step is retired; the new Quality/Warehouse event flow covers receipt, rejection and expiry reads. |
| ReportController | Get | ApprovedMoveOrdersReports | Replace | api/Ordering/GetMoveOrders | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReportController | Get | ApprovedMoveOrdersReportsPagination | Replace | api/Ordering/GetMoveOrders | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReportController | Get | CancelledOrderReport | Replace | api/Ordering/GetAllListOfCancelledOrdersPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReportController | Get | CancelledOrderReportsPagination | Replace | api/Ordering/GetAllListOfCancelledOrdersPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReportController | Get | ConsolidatedReport | Retire |  | Retired by the reporting decision: broken cross-module logic or hardcoded company/department constants; rebuild as a new Reporting projection only if the business requires it. |
| ReportController | Get | InventoryMovementReport | Replace | api/WarehouseReceiving/GetInventoryMovementReport | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReportController | Get | InventoryMovementReportPagination | Replace | api/WarehouseReceiving/GetInventoryMovementReport | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReportController | Get | ItemsWithBbdPagination | Replace | api/WarehouseReceiving/GetExpiringReceipts | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReportController | Get | MiscellaneousIssueReport | Replace | api/WarehouseReceiving/GetMiscellaneousIssuesWithPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReportController | Get | MiscellaneousIssueReportPagination | Replace | api/WarehouseReceiving/GetMiscellaneousIssuesWithPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReportController | Get | MiscellaneousReceiptReport | Replace | api/WarehouseReceiving/GetMiscellaneousReceiptsWithPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReportController | Get | MiscellaneousReceiptReportPagination | Replace | api/WarehouseReceiving/GetMiscellaneousReceiptsWithPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReportController | Get | MoveOrderHistory | Replace | api/Ordering/GetMoveOrders | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReportController | Get | MoveOrderHistoryPagination | Replace | api/Ordering/GetMoveOrders | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReportController | Get | NearlyExpireItemsReport | Replace | api/WarehouseReceiving/GetExpiringReceipts | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReportController | Get | NearlyExpireItemsReportPagination | Replace | api/WarehouseReceiving/GetExpiringReceipts | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReportController | Get | OrderVsServeReports | Retire |  | Retired by the reporting decision: broken cross-module logic or hardcoded company/department constants; rebuild as a new Reporting projection only if the business requires it. |
| ReportController | Get | OrderVsServeReportsPagination | Retire |  | Retired by the reporting decision: broken cross-module logic or hardcoded company/department constants; rebuild as a new Reporting projection only if the business requires it. |
| ReportController | Get | QcReceivingReport | Replace | api/Receiving/GetQcReceiptsWithPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReportController | Get | QcReceivingReportPagination | Replace | api/Receiving/GetQcReceiptsWithPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReportController | Get | TransactedMoveOrderReport | Replace | api/Ordering/GetTransactedMoveOrders | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReportController | Get | TransactedMoveOrderReportPagination | Replace | api/Ordering/GetTransactedMoveOrders | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReportController | Get | TransformationHistoryReport | Replace | api/Manufacturing/GetTransformationRequests | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReportController | Get | TransformationHistoryReportPagination | Replace | api/Manufacturing/GetTransformationRequests | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReportController | Get | WarehouseReceivingReport | Replace | api/WarehouseReceiving/GetWarehouseReceiptsWithPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| ReportController | Get | WarehouseReceivingReportPagination | Replace | api/WarehouseReceiving/GetWarehouseReceiptsWithPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RoleController | Put | ActivateRole/{id} | Replace | api/Role | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RoleController | Post | AddNewRole | Replace | api/Role | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RoleController | Get | GetAllActiveRoles | Replace | api/Role | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RoleController | Get | GetAllInActiveRoles | Replace | api/Role | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RoleController | Get | GetAllRoleModules | Replace | api/Role | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RoleController | Get | GetAllRoles | Replace | api/Role | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RoleController | Get | GetAllRolesWithPagination/{status} | Replace | api/Role | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RoleController | Get | GetAllRolesWithPaginationOrig/{status} | Retire |  | Legacy duplicate of the canonical paginated read; the new list endpoint accepts paging, search and status. |
| RoleController | Get | GetbyId/{id} | Replace | api/Role | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RoleController | Get | GetRoleModulebyId/{id}/{menuid} | Replace | api/Role | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RoleController | Get | GetRoleModuleByIdAndParentId/{id}/{status}/{menuid} | Replace | api/Role | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RoleController | Get | GetRoleModulebyRole/{rolename} | Replace | api/Role | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RoleController | Get | GetRoleModuleWithId/{id} | Replace | api/Role | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RoleController | Get | GetRolesByStatus/{status} | Replace | api/Role | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RoleController | Get | GetUntagModuleByRoleId/{id}/{menuid} | Replace | api/Role | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RoleController | Put | InActiveRole/{id} | Replace | api/Role | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RoleController | Post | TagandModules | Replace | api/Role | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RoleController | Put | TagModuleinRole | Replace | api/Role | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RoleController | Put | UntagModule | Replace | api/Role | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| RoleController | Put | UpdateRole/{id} | Replace | api/Role | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| SupplierController | Put | ActivateSupplier/{id} | Replace | api/Supplier | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| SupplierController | Post | AddNewSupplier | Replace | api/Supplier | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| SupplierController | Get | GetAllActiveSupplier | Replace | api/Supplier | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| SupplierController | Get | GetAllInActiveSupplier | Replace | api/Supplier | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| SupplierController | Get | GetAllSupplier | Replace | api/Supplier | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| SupplierController | Get | GetAllSupplierWithPagination/{status} | Replace | api/Supplier | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| SupplierController | Get | GetAllSupplierWithPaginationOrig/{status} | Retire |  | Legacy duplicate of the canonical paginated read; the new list endpoint accepts paging, search and status. |
| SupplierController | Get | GetSupplierById/{id} | Replace | api/Supplier | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| SupplierController | Put | InActiveSupplier/{id} | Replace | api/Supplier | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| SupplierController | Put | UpdateSupplier/{id} | Replace | api/Supplier | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| TransactionController | Put | ActivateTransaction/{id} | Port | api/Transaction/ActivateTransaction/{id} | Ported to the modular endpoint named by target. |
| TransactionController | Post | AddNewTransaction | Port | api/Transaction/AddNewTransaction | Ported to the modular endpoint named by target. |
| TransactionController | Get | GetAllActiveTransactionName | Port | api/Transaction/GetAllActiveTransactionName | Ported to the modular endpoint named by target. |
| TransactionController | Get | GetAllTransactionPagination/{status} | Port | api/Transaction/GetAllTransactionPagination/{status} | Ported to the modular endpoint named by target. |
| TransactionController | Get | GetAllTransactionPaginationOrig/{status} | Retire |  | Legacy duplicate pagination variant. |
| TransactionController | Put | InActiveTransaction | Retire |  | Legacy duplicate of InActiveTransaction/{id}. |
| TransactionController | Put | InActiveTransaction/{id} | Port | api/Transaction/InActiveTransaction/{id} | Ported to the modular endpoint named by target. |
| TransactionController | Put | UpdateTransaction | Port | api/Transaction/UpdateTransaction | Ported to the modular endpoint named by target. |
| TransformationController | Put | ActivateFormula/{id} | Replace | api/Transformation/ActivateFormula/{id} | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| TransformationController | Put | ActivateRequirement/{id} | Retire |  | Legacy formula-requirement edit/delete route is retired; formulas are versioned and requirements are added to a new version. |
| TransformationController | Post | AddNewTransformationFormula | Replace | api/Transformation/AddNewFormula | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| TransformationController | Post | AddNewTransformationRequirementinFormula | Replace | api/Transformation/AddFormulaRequirement/{id} | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| TransformationController | Delete | DeleteRequirementinFormula/{id} | Retire |  | Legacy formula-requirement edit/delete route is retired; formulas are versioned and requirements are added to a new version. |
| TransformationController | Get | GetAllActiveFormula | Replace | api/Transformation/GetAllFormulasWithPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| TransformationController | Get | GetAllFormulaWithPagination/{status} | Replace | api/Transformation/GetAllFormulasWithPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| TransformationController | Get | GetAllFormulaWithPaginationOrig/{status} | Retire |  | Legacy duplicate pagination variant. |
| TransformationController | Get | GetAllFormulaWithRequirementByFormulaId/{id} | Replace | api/Transformation/GetAllFormulasWithPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| TransformationController | Get | GetAllInActiveFormula | Replace | api/Transformation/GetAllFormulasWithPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| TransformationController | Get | GetAllInActiveRequirement | Retire |  | Legacy formula-requirement edit/delete route is retired; formulas are versioned and requirements are added to a new version. |
| TransformationController | Get | GetAllRequirementsWithFormula | Replace | api/Transformation/GetAllFormulasWithPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| TransformationController | Get | GetAllTransformationFormula | Replace | api/Transformation/GetAllFormulasWithPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| TransformationController | Get | GetFormulaById/{id} | Replace | api/Transformation/GetFormulaById/{id} | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| TransformationController | Put | InActiveFormula/{id} | Replace | api/Transformation/InActiveFormula/{id} | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| TransformationController | Put | InActiveRequirements/{id} | Retire |  | Legacy formula-requirement edit/delete route is retired; formulas are versioned and requirements are added to a new version. |
| TransformationController | Put | UpdateFormulaInfo/{id} | Retire |  | Legacy formula-requirement edit/delete route is retired; formulas are versioned and requirements are added to a new version. |
| TransformationController | Put | UpdateQuantity/{id} | Retire |  | Legacy formula-requirement edit/delete route is retired; formulas are versioned and requirements are added to a new version. |
| UomController | Put | ActivateUom/{id} | Replace | api/Uom | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UomController | Post | AddNewUOM | Replace | api/Uom | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UomController | Get | GetAllActiveUOM | Replace | api/Uom | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UomController | Get | GetAllInActiveUOM | Replace | api/Uom | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UomController | Get | GetAllUOM | Replace | api/Uom | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UomController | Get | GetAllUomWithPagination/{status} | Replace | api/Uom | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UomController | Get | GetAllUomWithPaginationOrig/{status} | Retire |  | Legacy duplicate of the canonical paginated read; the new list endpoint accepts paging, search and status. |
| UomController | Get | GetById/{id} | Replace | api/Uom | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UomController | Put | InActiveUom/{id} | Replace | api/Uom | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UomController | Put | UpdateUom/{id} | Replace | api/Uom | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UserController | Put | ActivateDepartment/{id} | Replace | api/User | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UserController | Put | ActivateUser/{id} | Replace | api/User | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UserController | Post | AddNewDepartment | Replace | api/User | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UserController | Post | AddNewUser | Replace | api/User | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UserController | Get | GetAllActiveDepartment | Replace | api/User | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UserController | Get | GetAllActiveUsers | Replace | api/User | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UserController | Get | GetAllDepartmentById/{id} | Replace | api/User | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UserController | Get | GetAllDepartments | Replace | api/User | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UserController | Get | GetAllInActiveDepartment | Replace | api/User | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UserController | Get | GetAllInActiveUsers | Replace | api/User | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UserController | Get | GetAllRoles | Replace | api/User | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UserController | Get | GetAllUsers | Replace | api/User | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UserController | Get | GetAllUsersWithPagination/{status} | Replace | api/User | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UserController | Get | GetAllUsersWithPaginationOrig/{status} | Retire |  | Legacy duplicate of the canonical paginated read; the new list endpoint accepts paging, search and status. |
| UserController | Get | GetById/{id} | Replace | api/User | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UserController | Get | GetByUserName/{username?} | Replace | api/User | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UserController | Get | GetDepartmentByStatus/{status} | Replace | api/User | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UserController | Get | GetUserByStatus/{status} | Replace | api/User | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UserController | Put | InActiveDepartment/{id} | Replace | api/User | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UserController | Put | InActiveUser/{id} | Replace | api/User | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UserController | Put | UpdateDepartmentInfo/{id} | Replace | api/User | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| UserController | Put | UpdateUserInfo/{id} | Replace | api/User | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| WarehouseController | Put | CancelAndReturnForPoSummary/{id} | Retire |  | Legacy warehouse console write/scan flow is retired; the new API uses AddWarehouseReceipt, AdjustStock and the QC completion event. |
| WarehouseController | Put | CancelAndReturnForWarehouseReceive/{id} | Retire |  | Legacy warehouse console write/scan flow is retired; the new API uses AddWarehouseReceipt, AdjustStock and the QC completion event. |
| WarehouseController | Put | CancelPartialReceivingInQc/{id} | Retire |  | Legacy warehouse console write/scan flow is retired; the new API uses AddWarehouseReceipt, AdjustStock and the QC completion event. |
| WarehouseController | Get | GetAllAvailableForWarehouseReceiving | Replace | api/Ordering/GetPurchaseOrders?status=Open | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| WarehouseController | Get | GetAllListForWarehouseWithPagination | Replace | api/WarehouseReceiving/GetWarehouseReceiptsWithPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| WarehouseController | Get | GetAllListForWarehouseWithPaginationOrig | Retire |  | Legacy duplicate pagination variant. |
| WarehouseController | Get | GetAllListOfWarehouseIdPagination | Replace | api/WarehouseReceiving/GetWarehouses | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| WarehouseController | Get | GetAllListOfWarehouseIdPaginationOrig | Retire |  | Legacy duplicate pagination variant. |
| WarehouseController | Get | GetAllListOfWarehouseReceivingId | Retire |  | Legacy warehouse console write/scan flow is retired; the new API uses AddWarehouseReceipt, AdjustStock and the QC completion event. |
| WarehouseController | Get | GetAllListOfWarehouseReceivingIdNull | Retire |  | Legacy warehouse console write/scan flow is retired; the new API uses AddWarehouseReceipt, AdjustStock and the QC completion event. |
| WarehouseController | Get | GetAllRejectMaterialsFromWarehouse | Retire |  | Legacy warehouse console write/scan flow is retired; the new API uses AddWarehouseReceipt, AdjustStock and the QC completion event. |
| WarehouseController | Get | GetAllRejectMaterialsFromWarehouseOrig | Retire |  | Legacy duplicate pagination variant. |
| WarehouseController | Get | GetAllWarehouseReceived | Replace | api/WarehouseReceiving/GetWarehouseReceiptsWithPagination | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| WarehouseController | Get | GetRejectMaterialsForWarehouse | Retire |  | Legacy warehouse console write/scan flow is retired; the new API uses AddWarehouseReceipt, AdjustStock and the QC completion event. |
| WarehouseController | Get | GetRejectMaterialsInWarehouse/{warehouseid} | Retire |  | Legacy warehouse console write/scan flow is retired; the new API uses AddWarehouseReceipt, AdjustStock and the QC completion event. |
| WarehouseController | Get | ListForWarehouseReceiving | Retire |  | Legacy warehouse console write/scan flow is retired; the new API uses AddWarehouseReceipt, AdjustStock and the QC completion event. |
| WarehouseController | Put | ReceiveRawMaterialsInWarehouse/{itemcode} | Replace | api/WarehouseReceiving/AddWarehouseReceipt | Superseded by the existing module endpoint; the legacy route name is preserved or mapped. |
| WarehouseController | Put | RejectMaterialFromWarehouse | Retire |  | Legacy warehouse console write/scan flow is retired; the new API uses AddWarehouseReceipt, AdjustStock and the QC completion event. |
| WarehouseController | Put | ReturnRawmaterialByWarehouse/{id} | Retire |  | Legacy warehouse console write/scan flow is retired; the new API uses AddWarehouseReceipt, AdjustStock and the QC completion event. |
| WarehouseController | Get | ScanBarcode/{itemcode} | Retire |  | Legacy warehouse console write/scan flow is retired; the new API uses AddWarehouseReceipt, AdjustStock and the QC completion event. |
