SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @defaultWarehouseId int = (SELECT [Id] FROM [Warehouse].[Warehouses] WHERE [Code] = 'LEGACY-DEFAULT');
DECLARE @unknownItemId      int = (SELECT [Id] FROM [Catalog].[Items] WHERE [ItemCode] = 'ITEM-LEGACY-UNKNOWN');
DECLARE @fallbackUomId      int = (SELECT TOP 1 [Id] FROM [Catalog].[Uoms] ORDER BY [Id]);

INSERT INTO [Warehouse].[MiscellaneousReceipts]
    ([LegacyId], [WarehouseId], [RefNo], [ReceivedAtUtc], [Remarks], [ReceivedByUserId], [IsActive])
SELECT r.[Id],
       @defaultWarehouseId,
       NULL,
       r.[TransactionDate],
       COALESCE(NULLIF(r.[Remarks], ''), r.[Details]),
       u.[Id],
       r.[IsActive]
FROM [$(LegacyDb)].[dbo].[MiscellaneousReceipts] r
LEFT JOIN [Identity].[Users] u ON u.[UserName] = r.[PreparedBy]
WHERE NOT EXISTS (SELECT 1 FROM [Warehouse].[MiscellaneousReceipts] t WHERE t.[LegacyId] = r.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'MiscellaneousReceipts', t.[LegacyId], t.[Id] FROM [Warehouse].[MiscellaneousReceipts] t
WHERE t.[LegacyId] IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'MiscellaneousReceipts' AND m.[LegacyId] = t.[LegacyId]);

INSERT INTO [Warehouse].[MiscellaneousIssues]
    ([LegacyId], [WarehouseId], [RefNo], [IssuedAtUtc], [Remarks], [IssuedByUserId], [IsActive])
SELECT i.[Id],
       @defaultWarehouseId,
       NULL,
       i.[TransactionDate],
       COALESCE(NULLIF(i.[Remarks], ''), i.[Details]),
       u.[Id],
       i.[IsActive]
FROM [$(LegacyDb)].[dbo].[MiscellaneousIssues] i
LEFT JOIN [Identity].[Users] u ON u.[UserName] = i.[PreparedBy]
WHERE NOT EXISTS (SELECT 1 FROM [Warehouse].[MiscellaneousIssues] t WHERE t.[LegacyId] = i.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'MiscellaneousIssues', t.[LegacyId], t.[Id] FROM [Warehouse].[MiscellaneousIssues] t
WHERE t.[LegacyId] IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'MiscellaneousIssues' AND m.[LegacyId] = t.[LegacyId]);

INSERT INTO [Warehouse].[MiscellaneousIssueLines]
    ([LegacyId], [MiscellaneousIssueId], [ItemId], [UomId], [LotNo], [Quantity], [Remarks])
SELECT d.[Id],
       im.[NewId],
       COALESCE(i.[Id], @unknownItemId),
       COALESCE(i.[UomId], @fallbackUomId),
       NULL,
       d.[Quantity],
       d.[Remarks]
FROM [$(LegacyDb)].[dbo].[MiscellaneousIssueDetails] d
JOIN [dbo].[LegacyIdMap] im ON im.[TableName] = 'MiscellaneousIssues' AND im.[LegacyId] = d.[IssuePKey]
LEFT JOIN [Catalog].[Items] i ON i.[ItemCode] = d.[ItemCode]
WHERE NOT EXISTS (SELECT 1 FROM [Warehouse].[MiscellaneousIssueLines] t WHERE t.[LegacyId] = d.[Id]);

COMMIT TRANSACTION;
