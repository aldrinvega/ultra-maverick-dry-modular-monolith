SET NOCOUNT ON;

DECLARE @gate TABLE ([Gate] sysname NOT NULL, [SourceCount] int NOT NULL, [TargetCount] int NOT NULL);

INSERT INTO @gate ([Gate], [SourceCount], [TargetCount]) VALUES
('Identity.Users',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[Users]),
    (SELECT COUNT(*) FROM [Identity].[Users])),
('Catalog.Items',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[RawMaterials]),
    (SELECT COUNT(*) FROM [Catalog].[Items] WHERE [ItemCode] <> 'ITEM-LEGACY-UNKNOWN')),
('Catalog.Customers',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[Customers]),
    (SELECT COUNT(*) FROM [Catalog].[Customers] WHERE [CustomerCode] <> 'CUST-LEGACY-UNKNOWN')),
('Catalog.Suppliers',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[Suppliers]),
    (SELECT COUNT(*) FROM [Catalog].[Suppliers] WHERE [SupplierCode] <> 'SUP-LEGACY-UNKNOWN')),
('Procurement.PurchaseOrders',
    (SELECT COUNT(DISTINCT [PO_Number]) FROM [$(LegacyDb)].[dbo].[POSummary]),
    (SELECT COUNT(*) FROM [Procurement].[PurchaseOrders])),
('Procurement.PurchaseOrderLines',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[POSummary]),
    (SELECT COUNT(*) FROM [Procurement].[PurchaseOrderLines])),
('Quality.QcReceipts',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[QC_Receiving]),
    (SELECT COUNT(*) FROM [Quality].[QcReceipts])),
('Quality.QcRejects',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[QC_Reject]),
    (SELECT COUNT(*) FROM [Quality].[QcRejects])),
('Quality.Checklists',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[QcChecklists]),
    (SELECT COUNT(*) FROM [Quality].[Checklists])),
('Quality.ChecklistAnswers',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[ChecklistAnswers]),
    (SELECT COUNT(*) FROM [Quality].[ChecklistAnswers])),
('Quality.ChecklistOpenFieldAnswers',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[QChecklistOpenFieldAnswers]),
    (SELECT COUNT(*) FROM [Quality].[ChecklistOpenFieldAnswers])),
('Quality.ChecklistProductDimensions',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[QChecklistProductDimensions]),
    (SELECT COUNT(*) FROM [Quality].[ChecklistProductDimensions])),
('Warehouse.WarehouseReceipts',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[WarehouseReceived]),
    (SELECT COUNT(*) FROM [Warehouse].[WarehouseReceipts])),
('Warehouse.MiscellaneousReceipts',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[MiscellaneousReceipts]),
    (SELECT COUNT(*) FROM [Warehouse].[MiscellaneousReceipts])),
('Warehouse.MiscellaneousIssues',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[MiscellaneousIssues]),
    (SELECT COUNT(*) FROM [Warehouse].[MiscellaneousIssues])),
('Warehouse.MiscellaneousIssueLines',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[MiscellaneousIssueDetails]),
    (SELECT COUNT(*) FROM [Warehouse].[MiscellaneousIssueLines])),
('Orders.Orders',
    (SELECT COUNT(DISTINCT [OrderNoPKey]) FROM [$(LegacyDb)].[dbo].[Orders]),
    (SELECT COUNT(*) FROM [Orders].[Orders])),
('Orders.OrderLines',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[Orders]),
    (SELECT COUNT(*) FROM [Orders].[OrderLines])),
('Orders.MoveOrders',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[MoveOrders]),
    (SELECT COUNT(*) FROM [Orders].[MoveOrders])),
('Orders.TransactedMoveOrders',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[TransactMoveOrder]),
    (SELECT COUNT(*) FROM [Orders].[TransactedMoveOrders])),
('Orders.CancelledOrders',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[CancelledOrders]),
    (SELECT COUNT(*) FROM [Orders].[CancelledOrders])),
('Orders.AdvancesToEmployees',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[AdvancesToEmployees]),
    (SELECT COUNT(*) FROM [Orders].[AdvancesToEmployees])),
('Manufacturing.TransformationRequests',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[Transformation_Request]),
    (SELECT COUNT(*) FROM [Manufacturing].[TransformationRequests])),
('Manufacturing.ProductionPlans',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[Transformation_Planning]),
    (SELECT COUNT(*) FROM [Manufacturing].[ProductionPlans])),
('Laboratory.LabTestRequests',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[LabTestRequests]),
    0),
('Reporting.InventoryPositions',
    (SELECT COUNT(*) FROM [Warehouse].[InventoryBalances]),
    (SELECT COUNT(*) FROM [Reporting].[InventoryPositions])),
('Reporting.OrderFulfillment',
    (SELECT COUNT(*) FROM [Orders].[Orders]),
    (SELECT COUNT(*) FROM [Reporting].[OrderFulfillment])),
('Reporting.QcStatus',
    (SELECT COUNT(*) FROM [Quality].[QcReceipts] WHERE [Status] = 'Completed'),
    (SELECT COUNT(*) FROM [Reporting].[QcStatus])),
('Identity.Departments',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[Departments]),
    (SELECT COUNT(*) FROM [Identity].[Departments] WHERE [LegacyId] IS NOT NULL)),
('Identity.Roles',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[Roles]),
    (SELECT COUNT(*) FROM [Identity].[Roles] WHERE [LegacyId] IS NOT NULL)),
('Identity.Modules',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[Modules]),
    (SELECT COUNT(*) FROM [Identity].[Modules] WHERE [LegacyId] IS NOT NULL)),
('Catalog.Uoms',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[UOMS]),
    (SELECT COUNT(*) FROM [Catalog].[Uoms] WHERE [LegacyId] IS NOT NULL)),
('Catalog.Farms',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[Farms]),
    (SELECT COUNT(*) FROM [Catalog].[Farms] WHERE [LegacyId] IS NOT NULL)),
('Catalog.LotNames',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[LotNames]),
    (SELECT COUNT(*) FROM [Catalog].[LotNames] WHERE [LegacyId] IS NOT NULL)),
('Catalog.LotCategories',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[LotCategories]),
    (SELECT COUNT(*) FROM [Catalog].[LotCategories] WHERE [LegacyId] IS NOT NULL)),
('Catalog.ItemCategories',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[ItemCategories]),
    (SELECT COUNT(*) FROM [Catalog].[ItemCategories] WHERE [LegacyId] IS NOT NULL)),
('Catalog.Reasons',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[Reasons]),
    (SELECT COUNT(*) FROM [Catalog].[Reasons] WHERE [LegacyId] IS NOT NULL)),
('Catalog.AccountTitles',
    (SELECT COUNT(DISTINCT [AccountTitleName]) FROM [$(LegacyDb)].[dbo].[AccountTitles]),
    (SELECT COUNT(*) FROM [Catalog].[AccountTitles] WHERE [LegacyId] IS NOT NULL)),
('Catalog.ProductTypes',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[ProductTypes]),
    (SELECT COUNT(*) FROM [Catalog].[ProductTypes] WHERE [LegacyId] IS NOT NULL)),
('Catalog.TransactionTypes',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[Transactions]),
    (SELECT COUNT(*) FROM [Catalog].[TransactionTypes] WHERE [LegacyId] IS NOT NULL)),
('Catalog.SampleTypes',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[SampleTypes]),
    (SELECT COUNT(*) FROM [Catalog].[SampleTypes] WHERE [LegacyId] IS NOT NULL)),
('Quality.ChecklistTypes',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[ChecklistTypes]),
    (SELECT COUNT(*) FROM [Quality].[ChecklistTypes] WHERE [LegacyId] IS NOT NULL)),
('Quality.ChecklistQuestions',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[ChecklistQuestions]),
    (SELECT COUNT(*) FROM [Quality].[ChecklistQuestions] WHERE [LegacyId] IS NOT NULL)),
('Orders.OrphanLines',
    0,
    (SELECT COUNT(*) FROM [Orders].[OrderLines] l
     LEFT JOIN [Orders].[Orders] o ON o.[Id] = l.[OrderId]
     WHERE o.[Id] IS NULL)),
('Quality.ChecklistDimensionQuarantine',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[QChecklistProductDimensions] s
     WHERE ((s.[Standard] IS NOT NULL AND TRY_CAST(s.[Standard] AS decimal(18,4)) IS NULL)
         OR (s.[Actual] IS NOT NULL AND TRY_CAST(s.[Actual] AS decimal(18,4)) IS NULL))),
    (SELECT COUNT(*) FROM [dbo].[ChecklistDimensionQuarantine])),
('Quality.ExpectedDeliveryQuarantine',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[QC_Receiving] WHERE [Expected_Delivery] IS NOT NULL),
    (SELECT COUNT(*) FROM [dbo].[QcExpectedDeliveryQuarantine])),
('Orders.ActorResolution',
    (SELECT COUNT(*) FROM (
        SELECT [PreparedBy] AS [ActorName] FROM [$(LegacyDb)].[dbo].[Orders] WHERE [PreparedBy] IS NOT NULL
        UNION SELECT [RejectBy] FROM [$(LegacyDb)].[dbo].[Orders] WHERE [RejectBy] IS NOT NULL
        UNION SELECT [IsCancelBy] FROM [$(LegacyDb)].[dbo].[Orders] WHERE [IsCancelBy] IS NOT NULL
        UNION SELECT [SetBy] FROM [$(LegacyDb)].[dbo].[Orders] WHERE [SetBy] IS NOT NULL
     ) a WHERE NOT EXISTS (SELECT 1 FROM [Identity].[Users] u WHERE u.[UserName] = a.[ActorName])),
    (SELECT COUNT(*) FROM [dbo].[UnresolvedActors]));

SELECT [Gate], [SourceCount], [TargetCount]
FROM @gate
WHERE [SourceCount] <> [TargetCount]
ORDER BY [Gate];

IF EXISTS (SELECT 1 FROM @gate WHERE [SourceCount] <> [TargetCount])
BEGIN
    RAISERROR('Reconciliation gate failed: source and target counts differ.', 16, 1);
END
ELSE
BEGIN
    PRINT 'All reconciliation gates passed.';
END;

-- Coverage gate: every legacy table that holds rows must be either migrated or explicitly retired.
DECLARE @unexpected TABLE ([TableName] sysname NOT NULL);

INSERT INTO @unexpected ([TableName])
SELECT t.[name]
FROM [$(LegacyDb)].sys.tables t
JOIN [$(LegacyDb)].sys.partitions p
  ON p.[object_id] = t.[object_id] AND p.[index_id] IN (0, 1)
WHERE p.[rows] > 0
  AND t.[name] NOT IN (
      'Departments','MainMenus','Modules','RoleModules','Roles','Users',
      'AccountTitles','Customers','Farms','ItemCategories','LotCategories','LotNames',
      'ProductTypes','RawMaterials','Reasons','Suppliers','UOMS','Transactions','SampleTypes',
      'POSummary',
      'QC_Receiving','QC_Reject','ChecklistTypes','ChecklistQuestions','QcChecklists',
      'ChecklistAnswers','QChecklistOpenFieldAnswers','QChecklistProductDimensions',
      'ChecklistOtherObservations','ChecklistCompliances','ChecklistReviewVerificationLogs',
      'WarehouseReceived','MiscellaneousReceipts','MiscellaneousIssues','MiscellaneousIssueDetails',
      'Orders','MoveOrders','TransactMoveOrder','CancelledOrders','AdvancesToEmployees',
      'Transformation_Planning','Transformation_Request','Transformation_Preparation','Transformation_Reject',
      'Formulas','FormulaRequirements','LabTestRequests',
      'GenerateOrderNos','CheckListStrings','Warehouse_Reject','__EFMigrationsHistory'
  )
GROUP BY t.[name];

IF EXISTS (SELECT 1 FROM @unexpected)
BEGIN
    SELECT [TableName] FROM @unexpected;
    RAISERROR('Legacy tables with data are neither migrated nor retired.', 16, 1);
END
ELSE
BEGIN
    PRINT 'Coverage gate passed: every legacy table with data is migrated or retired.';
END;
