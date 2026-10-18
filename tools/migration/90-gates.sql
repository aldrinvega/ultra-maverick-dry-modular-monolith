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
('Warehouse.WarehouseReceipts',
    (SELECT COUNT(*) FROM [$(LegacyDb)].[dbo].[WarehouseReceived]),
    (SELECT COUNT(*) FROM [Warehouse].[WarehouseReceipts])),
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
    0,
    (SELECT COUNT(*) FROM [Reporting].[InventoryPositions]));

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
