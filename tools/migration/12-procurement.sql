SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM [Catalog].[Suppliers] WHERE [SupplierCode] = 'SUP-LEGACY-UNKNOWN')
BEGIN
    INSERT INTO [Catalog].[Suppliers] ([SupplierCode], [SupplierName], [IsActive], [CreatedAtUtc])
    VALUES ('SUP-LEGACY-UNKNOWN', 'Legacy unknown supplier', 1, SYSUTCDATETIME());
END;

DECLARE @fallbackSupplierId int =
    (SELECT [Id] FROM [Catalog].[Suppliers] WHERE [SupplierCode] = 'SUP-LEGACY-UNKNOWN');

WITH grouped AS (
    SELECT s.[PO_Number],
           MIN(CAST(s.[PR_Number] AS nvarchar(50))) AS [PrNumber],
           MIN(s.[PO_Date]) AS [PoDate],
           MIN(s.[PR_Date]) AS [PrDate],
           MIN(s.[ImportDate]) AS [CreatedAtUtc],
           MAX(CAST(s.[IsActive] AS int)) AS [IsActive],
           MIN(s.[VendorName]) AS [VendorName]
    FROM [$(LegacyDb)].[dbo].[POSummary] s
    GROUP BY s.[PO_Number]
)
INSERT INTO [Procurement].[PurchaseOrders]
    ([LegacyId], [PoNumber], [PrNumber], [PoDate], [PrDate], [SupplierId], [Status], [IsActive], [CreatedAtUtc])
SELECT g.[PO_Number],
       CAST(g.[PO_Number] AS nvarchar(50)),
       g.[PrNumber],
       g.[PoDate],
       g.[PrDate],
       COALESCE(sup.[SupplierId], @fallbackSupplierId),
       'Open',
       g.[IsActive],
       ISNULL(g.[CreatedAtUtc], SYSUTCDATETIME())
FROM grouped g
LEFT JOIN (
    SELECT [SupplierName], MIN([Id]) AS [SupplierId]
    FROM [Catalog].[Suppliers]
    GROUP BY [SupplierName]
) sup ON sup.[SupplierName] = g.[VendorName]
WHERE NOT EXISTS (SELECT 1 FROM [Procurement].[PurchaseOrders] t WHERE t.[LegacyId] = g.[PO_Number]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'PurchaseOrders', t.[LegacyId], t.[Id]
FROM [Procurement].[PurchaseOrders] t
WHERE t.[LegacyId] IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'PurchaseOrders' AND m.[LegacyId] = t.[LegacyId]);

INSERT INTO [Procurement].[PurchaseOrderLines]
    ([LegacyId], [PurchaseOrderId], [ItemId], [UomId], [OrderedQuantity], [DeliveredQuantity], [BilledQuantity], [UnitPrice], [IsActive])
SELECT s.[Id],
       po.[NewId],
       i.[Id],
       COALESCE(u.[Id], i.[UomId]),
       s.[Ordered],
       s.[Delivered],
       s.[Billed],
       s.[UnitPrice],
       s.[IsActive]
FROM [$(LegacyDb)].[dbo].[POSummary] s
JOIN [dbo].[LegacyIdMap] po ON po.[TableName] = 'PurchaseOrders' AND po.[LegacyId] = s.[PO_Number]
JOIN [Catalog].[Items] i ON i.[ItemCode] = s.[ItemCode]
LEFT JOIN [Catalog].[Uoms] u ON u.[Code] = s.[UOM]
WHERE NOT EXISTS (SELECT 1 FROM [Procurement].[PurchaseOrderLines] t WHERE t.[LegacyId] = s.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'POSummary', t.[LegacyId], t.[Id]
FROM [Procurement].[PurchaseOrderLines] t
WHERE t.[LegacyId] IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'POSummary' AND m.[LegacyId] = t.[LegacyId]);

COMMIT TRANSACTION;
