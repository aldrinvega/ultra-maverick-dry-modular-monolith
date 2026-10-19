SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

INSERT INTO [Warehouse].[InventoryMovements]
    ([WarehouseId], [ItemId], [UomId], [LotNo], [Delta], [Source], [SourceDocumentId], [OccurredAtUtc])
SELECT r.[WarehouseId], r.[ItemId], r.[UomId], NULLIF(r.[LotNo], ''),
       r.[Quantity], 'WarehouseReceipt', r.[Id], r.[ReceivedAtUtc]
FROM [Warehouse].[WarehouseReceipts] r
WHERE NOT EXISTS (
    SELECT 1 FROM [Warehouse].[InventoryMovements] m
    WHERE m.[Source] = 'WarehouseReceipt' AND m.[SourceDocumentId] = r.[Id]);

INSERT INTO [Warehouse].[InventoryMovements]
    ([WarehouseId], [ItemId], [UomId], [LotNo], [Delta], [Source], [SourceDocumentId], [OccurredAtUtc])
SELECT i.[WarehouseId], l.[ItemId], l.[UomId],
       (SELECT TOP 1 NULLIF(r.[LotNo], '')
        FROM [Warehouse].[WarehouseReceipts] r
        WHERE r.[WarehouseId] = i.[WarehouseId]
          AND r.[ItemId] = l.[ItemId]
          AND r.[IsActive] = 1
        ORDER BY r.[ReceivedAtUtc], r.[Id]),
       -l.[Quantity], 'MiscellaneousIssue', l.[Id], i.[IssuedAtUtc]
FROM [Warehouse].[MiscellaneousIssueLines] l
JOIN [Warehouse].[MiscellaneousIssues] i ON i.[Id] = l.[MiscellaneousIssueId]
WHERE NOT EXISTS (
    SELECT 1 FROM [Warehouse].[InventoryMovements] m
    WHERE m.[Source] = 'MiscellaneousIssue' AND m.[SourceDocumentId] = l.[Id]);

DELETE FROM [Warehouse].[InventoryBalances];

INSERT INTO [Warehouse].[InventoryBalances]
    ([WarehouseId], [ItemId], [UomId], [LotNo], [OnHand], [UpdatedAtUtc])
SELECT m.[WarehouseId],
       m.[ItemId],
       MIN(m.[UomId]),
       ISNULL(m.[LotNo], ''),
       SUM(m.[Delta]),
       SYSUTCDATETIME()
FROM [Warehouse].[InventoryMovements] m
GROUP BY m.[WarehouseId], m.[ItemId], ISNULL(m.[LotNo], '');

COMMIT TRANSACTION;
