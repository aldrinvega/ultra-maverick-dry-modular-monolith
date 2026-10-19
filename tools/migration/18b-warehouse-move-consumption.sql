SET NOCOUNT ON;
SET XACT_ABORT ON;

-- Move orders that were actually transacted have left the warehouse, but the migration only
-- posted receipts and miscellaneous issues, so on-hand overstated stock. Post the consumption
-- and recompute balances from the full ledger.

BEGIN TRANSACTION;

INSERT INTO [Warehouse].[InventoryMovements]
    ([WarehouseId], [ItemId], [UomId], [LotNo], [Delta], [Source], [SourceDocumentId], [OccurredAtUtc])
SELECT m.[WarehouseId],
       m.[ItemId],
       m.[UomId],
       NULL,
       -m.[QuantityOrdered],
       'MoveOrder',
       t.[Id],
       ISNULL(t.[DeliveryDate], ISNULL(m.[OrderDate], SYSUTCDATETIME()))
FROM [Orders].[TransactedMoveOrders] t
JOIN [Orders].[MoveOrders] m ON m.[Id] = t.[MoveOrderId]
WHERE t.[IsTransact] = 1
  AND m.[WarehouseId] IS NOT NULL
  AND m.[QuantityOrdered] > 0
  AND NOT EXISTS (
      SELECT 1 FROM [Warehouse].[InventoryMovements] x
      WHERE x.[Source] = 'MoveOrder' AND x.[SourceDocumentId] = t.[Id]);

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
