SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM [Warehouse].[Warehouses] WHERE [Code] = 'LEGACY-DEFAULT')
BEGIN
    INSERT INTO [Warehouse].[Warehouses] ([Code], [Name], [IsActive], [CreatedAtUtc])
    VALUES ('LEGACY-DEFAULT', 'Legacy default warehouse', 1, SYSUTCDATETIME());
END;

DECLARE @defaultWarehouseId int = (SELECT [Id] FROM [Warehouse].[Warehouses] WHERE [Code] = 'LEGACY-DEFAULT');
DECLARE @unknownItemId      int = (SELECT [Id] FROM [Catalog].[Items] WHERE [ItemCode] = 'ITEM-LEGACY-UNKNOWN');
DECLARE @fallbackUomId      int = (SELECT TOP 1 [Id] FROM [Catalog].[Uoms] ORDER BY [Id]);

INSERT INTO [Warehouse].[WarehouseReceipts]
    ([LegacyId], [QcReceiptId], [WarehouseId], [ItemId], [UomId], [LotNo], [Quantity],
     [ManufacturingDate], [ExpiryDate], [ReceivedAtUtc], [ReceivedByUserId], [IsActive])
SELECT w.[Id],
       qm.[NewId],
       @defaultWarehouseId,
       COALESCE(i.[Id], @unknownItemId),
       COALESCE(i.[UomId], @fallbackUomId),
       w.[LotCategory],
       w.[QuantityGood],
       w.[ManufacturingDate],
       w.[Expiration],
       w.[ReceivingDate],
       u.[Id],
       w.[IsActive]
FROM [$(LegacyDb)].[dbo].[WarehouseReceived] w
LEFT JOIN [dbo].[LegacyIdMap] qm ON qm.[TableName] = 'QC_Receiving' AND qm.[LegacyId] = w.[QcReceivingId]
LEFT JOIN [Catalog].[Items] i ON i.[ItemCode] = w.[ItemCode]
LEFT JOIN [Identity].[Users] u ON u.[UserName] = w.[ReceivedBy]
WHERE NOT EXISTS (SELECT 1 FROM [Warehouse].[WarehouseReceipts] t WHERE t.[LegacyId] = w.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'WarehouseReceived', t.[LegacyId], t.[Id]
FROM [Warehouse].[WarehouseReceipts] t
WHERE t.[LegacyId] IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'WarehouseReceived' AND m.[LegacyId] = t.[LegacyId]);

COMMIT TRANSACTION;
