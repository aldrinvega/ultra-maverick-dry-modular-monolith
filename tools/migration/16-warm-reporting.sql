SET NOCOUNT ON;
SET XACT_ABORT ON;

-- Warming the Reporting read models after the bulk load. The projections are deterministic
-- functions of the migrated rows, so this backfill produces the same state the outbox replay
-- would; an outbox replay remains available for a live system because projections are
-- idempotent on (EventId, ProjectionName).

BEGIN TRANSACTION;

INSERT INTO [Reporting].[InventoryPositions] ([WarehouseId], [ItemId], [UomId], [LotNo], [Quantity], [UpdatedAtUtc])
SELECT b.[WarehouseId],
       b.[ItemId],
       b.[UomId],
       ISNULL(b.[LotNo], ''),
       b.[OnHand],
       b.[UpdatedAtUtc]
FROM [Warehouse].[InventoryBalances] b
WHERE NOT EXISTS (
    SELECT 1 FROM [Reporting].[InventoryPositions] p
    WHERE p.[WarehouseId] = b.[WarehouseId]
      AND p.[ItemId] = b.[ItemId]
      AND p.[LotNo] = ISNULL(b.[LotNo], '')
);

INSERT INTO [Reporting].[OrderFulfillment] ([OrderNumber], [CustomerId], [Status], [OccurredAtUtc], [UpdatedAtUtc])
SELECT o.[OrderNumber], o.[CustomerId],
       CASE o.[Status]
           WHEN 'Confirmed' THEN 'Confirmed'
           WHEN 'Cancelled' THEN 'Cancelled'
           ELSE 'Created'
       END,
       o.[CreatedAtUtc], ISNULL(o.[ModifiedAtUtc], o.[CreatedAtUtc])
FROM [Orders].[Orders] o
WHERE NOT EXISTS (SELECT 1 FROM [Reporting].[OrderFulfillment] f WHERE f.[OrderNumber] = o.[OrderNumber]);

INSERT INTO [Reporting].[QcStatus] ([QcReceiptId], [ItemId], [UomId], [LotNo], [NetQuantity], [Status], [OccurredAtUtc], [UpdatedAtUtc])
SELECT q.[Id], q.[ItemId], q.[UomId], q.[BatchNo], q.[ActualDelivered] - q.[TotalReject], 'Completed', SYSUTCDATETIME(), SYSUTCDATETIME()
FROM [Quality].[QcReceipts] q
WHERE q.[Status] = 'Completed'
  AND NOT EXISTS (SELECT 1 FROM [Reporting].[QcStatus] s WHERE s.[QcReceiptId] = q.[Id]);

COMMIT TRANSACTION;
