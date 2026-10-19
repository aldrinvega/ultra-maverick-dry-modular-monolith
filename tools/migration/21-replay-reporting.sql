SET NOCOUNT ON;
SET XACT_ABORT ON;
SET LANGUAGE us_english;

BEGIN TRANSACTION;

DELETE FROM [Reporting].[InventoryPositions];
DELETE FROM [Reporting].[OrderFulfillment];
DELETE FROM [Reporting].[QcStatus];
DELETE FROM [Reporting].[ProcessedEvents];

DELETE FROM [Warehouse].[OutboxMessages] WHERE [AggregateType] LIKE 'Replay.%';
DELETE FROM [Orders].[OutboxMessages] WHERE [AggregateType] LIKE 'Replay.%';
DELETE FROM [Quality].[OutboxMessages] WHERE [AggregateType] LIKE 'Replay.%';

INSERT INTO [Warehouse].[OutboxMessages]
    ([EventId], [OccurredAtUtc], [AggregateType], [AggregateId], [EventType], [Version], [Payload], [PublishedAtUtc], [Attempts], [LastError])
SELECT NEWID(),
       SYSUTCDATETIME(),
       'Replay.WarehouseReceipt',
       CAST(r.[Id] AS nvarchar(64)),
       'StockReceived',
       1,
       '{"WarehouseId":' + CONVERT(varchar(20), r.[WarehouseId]) +
       ',"ItemId":' + CONVERT(varchar(20), r.[ItemId]) +
       ',"UomId":' + CONVERT(varchar(20), r.[UomId]) +
       ',"LotNo":' + CASE
           WHEN r.[LotNo] IS NULL OR LTRIM(RTRIM(r.[LotNo])) = '' THEN 'null'
           ELSE '"' + STRING_ESCAPE(r.[LotNo], 'json') + '"'
       END +
       ',"Quantity":' + REPLACE(CONVERT(varchar(50), r.[Quantity]), ',', '.') +
       ',"OccurredAtUtc":"' + CONVERT(varchar(33), r.[ReceivedAtUtc], 126) + 'Z"}',
       NULL,
       0,
       NULL
FROM [Warehouse].[WarehouseReceipts] r;

INSERT INTO [Orders].[OutboxMessages]
    ([EventId], [OccurredAtUtc], [AggregateType], [AggregateId], [EventType], [Version], [Payload], [PublishedAtUtc], [Attempts], [LastError])
SELECT NEWID(),
       SYSUTCDATETIME(),
       'Replay.Order',
       CAST(o.[Id] AS nvarchar(64)),
       'OrderCreated',
       1,
       '{"OrderNumber":"' + STRING_ESCAPE(o.[OrderNumber], 'json') +
       '","CustomerId":' + CONVERT(varchar(20), o.[CustomerId]) +
       ',"OccurredAtUtc":"' + CONVERT(varchar(33), o.[CreatedAtUtc], 126) + 'Z"}',
       NULL,
       0,
       NULL
FROM [Orders].[Orders] o;

INSERT INTO [Orders].[OutboxMessages]
    ([EventId], [OccurredAtUtc], [AggregateType], [AggregateId], [EventType], [Version], [Payload], [PublishedAtUtc], [Attempts], [LastError])
SELECT NEWID(),
       SYSUTCDATETIME(),
       'Replay.Order',
       CAST(o.[Id] AS nvarchar(64)),
       'OrderConfirmed',
       1,
       '{"OrderNumber":"' + STRING_ESCAPE(o.[OrderNumber], 'json') +
       '","OccurredAtUtc":"' + CONVERT(varchar(33), ISNULL(ISNULL(o.[ApprovedAtUtc], o.[ModifiedAtUtc]), o.[CreatedAtUtc]), 126) + 'Z"}',
       NULL,
       0,
       NULL
FROM [Orders].[Orders] o
WHERE o.[Status] = 'Confirmed';

INSERT INTO [Orders].[OutboxMessages]
    ([EventId], [OccurredAtUtc], [AggregateType], [AggregateId], [EventType], [Version], [Payload], [PublishedAtUtc], [Attempts], [LastError])
SELECT NEWID(),
       SYSUTCDATETIME(),
       'Replay.Order',
       CAST(o.[Id] AS nvarchar(64)),
       'OrderCancelled',
       1,
       '{"OrderNumber":"' + STRING_ESCAPE(o.[OrderNumber], 'json') +
       '","Reason":null,"OccurredAtUtc":"' + CONVERT(varchar(33), ISNULL(ISNULL(o.[CancelledAtUtc], o.[ModifiedAtUtc]), o.[CreatedAtUtc]), 126) + 'Z"}',
       NULL,
       0,
       NULL
FROM [Orders].[Orders] o
WHERE o.[Status] = 'Cancelled';

INSERT INTO [Quality].[OutboxMessages]
    ([EventId], [OccurredAtUtc], [AggregateType], [AggregateId], [EventType], [Version], [Payload], [PublishedAtUtc], [Attempts], [LastError])
SELECT NEWID(),
       SYSUTCDATETIME(),
       'Replay.QcReceipt',
       CAST(q.[Id] AS nvarchar(64)),
       'QcReceiptCompleted',
       1,
       '{"QcReceiptId":' + CONVERT(varchar(20), q.[Id]) +
       ',"ItemId":' + CONVERT(varchar(20), q.[ItemId]) +
       ',"UomId":' + CONVERT(varchar(20), q.[UomId]) +
       ',"LotNo":' + CASE
           WHEN q.[BatchNo] IS NULL OR LTRIM(RTRIM(q.[BatchNo])) = '' THEN 'null'
           ELSE '"' + STRING_ESCAPE(q.[BatchNo], 'json') + '"'
       END +
       ',"NetQuantity":' + REPLACE(CONVERT(varchar(50), q.[ActualDelivered] - q.[TotalReject]), ',', '.') +
       ',"ManufacturingDate":' + CASE
           WHEN q.[ManufacturingDate] IS NULL THEN 'null'
           ELSE '"' + CONVERT(varchar(33), q.[ManufacturingDate], 126) + 'Z"'
       END +
       ',"ExpiryDate":' + CASE
           WHEN q.[ExpiryDate] IS NULL THEN 'null'
           ELSE '"' + CONVERT(varchar(33), q.[ExpiryDate], 126) + 'Z"'
       END +
       ',"CompletedBy":' + CASE
           WHEN q.[QcByUserId] IS NULL THEN 'null'
           ELSE CONVERT(varchar(20), q.[QcByUserId])
       END +
       ',"OccurredAtUtc":"' + CONVERT(varchar(33), SYSUTCDATETIME(), 126) + 'Z"}',
       NULL,
       0,
       NULL
FROM [Quality].[QcReceipts] q
WHERE q.[Status] = 'Completed';

COMMIT TRANSACTION;

SELECT 'replay.enqueued' AS Measure,
       (SELECT COUNT(*) FROM [Warehouse].[OutboxMessages] WHERE [AggregateType] LIKE 'Replay.%') +
       (SELECT COUNT(*) FROM [Orders].[OutboxMessages] WHERE [AggregateType] LIKE 'Replay.%') +
       (SELECT COUNT(*) FROM [Quality].[OutboxMessages] WHERE [AggregateType] LIKE 'Replay.%') AS Total;
