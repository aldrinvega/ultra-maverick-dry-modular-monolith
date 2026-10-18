SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM [Catalog].[Customers] WHERE [CustomerCode] = 'CUST-LEGACY-UNKNOWN')
BEGIN
    INSERT INTO [Catalog].[Customers] ([CustomerCode], [CustomerName], [IsActive], [CreatedAtUtc])
    VALUES ('CUST-LEGACY-UNKNOWN', 'Legacy unknown customer', 1, SYSUTCDATETIME());
END;

DECLARE @unknownCustomerId int = (SELECT [Id] FROM [Catalog].[Customers] WHERE [CustomerCode] = 'CUST-LEGACY-UNKNOWN');
DECLARE @unknownItemId     int = (SELECT [Id] FROM [Catalog].[Items] WHERE [ItemCode] = 'ITEM-LEGACY-UNKNOWN');
DECLARE @fallbackUomId     int = (SELECT TOP 1 [Id] FROM [Catalog].[Uoms] ORDER BY [Id]);

WITH grouped AS (
    SELECT o.[OrderNoPKey],
           MIN(o.[CustomerPosition]) AS [CustomerPosition],
           MIN(o.[FarmCode]) AS [FarmCode],
           MIN(o.[CustomerId]) AS [LegacyCustomerId],
           MIN(o.[CustomerName]) AS [CustomerName],
           MIN(o.[OrderDate]) AS [OrderDate],
           MIN(o.[DateNeeded]) AS [DeliveryDate],
           MIN(o.[PreparedDate]) AS [PreparedAtUtc],
           MIN(o.[ApprovedDate]) AS [ApprovedAtUtc],
           MIN(o.[RejectedDate]) AS [RejectedAtUtc],
           MIN(o.[CancelDate]) AS [CancelledAtUtc],
           MIN(o.[PreparedBy]) AS [PreparedBy],
           MIN(o.[RejectBy]) AS [RejectBy],
           MIN(o.[IsCancelBy]) AS [IsCancelBy],
           MIN(o.[SetBy]) AS [SetBy],
           MIN(COALESCE(NULLIF(o.[PreparingCancellationRemarks], ''), o.[OrderCancellationRemarks])) AS [Remarks],
           MAX(CAST(o.[IsMove] AS int)) AS [IsMove],
           MAX(CAST(ISNULL(o.[IsCancelledOrder], 0) AS int)) AS [IsCancelledOrder],
           MAX(CAST(o.[IsActive] AS int)) AS [IsActive],
           MAX(CASE
                   WHEN ISNULL(o.[IsCancel], 0) = 1 OR ISNULL(o.[IsCancelledOrder], 0) = 1 THEN 'Cancelled'
                   WHEN ISNULL(o.[IsApproved], 0) = 1 THEN 'Confirmed'
                   ELSE 'Draft'
               END) AS [Status]
    FROM [$(LegacyDb)].[dbo].[Orders] o
    GROUP BY o.[OrderNoPKey]
)
INSERT INTO [Orders].[Orders]
    ([LegacyId], [OrderNumber], [CustomerId], [CustomerPosition], [FarmId], [OrderDate], [DeliveryDate],
     [Status], [PreparedByUserId], [PreparedAtUtc], [ApprovedAtUtc], [RejectedByUserId], [RejectedAtUtc],
     [CancelledByUserId], [CancelledAtUtc], [SetByUserId], [IsMove], [IsCancelledOrder], [Remarks],
     [IsActive], [CreatedAtUtc])
SELECT g.[OrderNoPKey],
       CAST(g.[OrderNoPKey] AS nvarchar(64)),
       COALESCE(cm.[NewId], cn.[Id], @unknownCustomerId),
       g.[CustomerPosition],
       f.[Id],
       g.[OrderDate],
       g.[DeliveryDate],
       g.[Status],
       up.[Id],
       g.[PreparedAtUtc],
       g.[ApprovedAtUtc],
       ur.[Id],
       g.[RejectedAtUtc],
       uc.[Id],
       g.[CancelledAtUtc],
       us.[Id],
       g.[IsMove],
       g.[IsCancelledOrder],
       g.[Remarks],
       g.[IsActive],
       g.[OrderDate]
FROM grouped g
LEFT JOIN [dbo].[LegacyIdMap] cm ON cm.[TableName] = 'Customers' AND cm.[LegacyId] = g.[LegacyCustomerId]
LEFT JOIN [Catalog].[Customers] cn ON cn.[CustomerName] = g.[CustomerName]
LEFT JOIN [Catalog].[Farms] f ON f.[FarmCode] = g.[FarmCode]
LEFT JOIN [Identity].[Users] up ON up.[UserName] = g.[PreparedBy]
LEFT JOIN [Identity].[Users] ur ON ur.[UserName] = g.[RejectBy]
LEFT JOIN [Identity].[Users] uc ON uc.[UserName] = g.[IsCancelBy]
LEFT JOIN [Identity].[Users] us ON us.[UserName] = g.[SetBy]
WHERE NOT EXISTS (SELECT 1 FROM [Orders].[Orders] t WHERE t.[LegacyId] = g.[OrderNoPKey]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'Orders', t.[LegacyId], t.[Id]
FROM [Orders].[Orders] t
WHERE t.[LegacyId] IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'Orders' AND m.[LegacyId] = t.[LegacyId]);

INSERT INTO [Orders].[OrderLines]
    ([LegacyId], [OrderId], [ItemId], [UomId], [OriginalQuantityOrdered], [QuantityOrdered],
     [AllocatedQuantity], [ForAllocation], [IsBeingPrepared], [PlateNumber], [DeliveryStatus], [ReceivedDate], [IsActive], [CreatedAtUtc])
SELECT o.[Id],
       om.[NewId],
       COALESCE(i.[Id], @unknownItemId),
       COALESCE(i.[UomId], @fallbackUomId),
       COALESCE(o.[OriginalQuantityOrdered], o.[QuantityOrdered]),
       o.[QuantityOrdered],
       CAST(ISNULL(o.[AllocatedQuantity], 0) AS decimal(18,4)),
       ISNULL(o.[ForAllocation], 0),
       ISNULL(o.[IsBeingPrepared], 0),
       o.[PlateNumber],
       o.[DeliveryStatus],
       o.[ReceivedDate],
       o.[IsActive],
       ISNULL(o.[OrderDate], SYSUTCDATETIME())
FROM [$(LegacyDb)].[dbo].[Orders] o
JOIN [dbo].[LegacyIdMap] om ON om.[TableName] = 'Orders' AND om.[LegacyId] = o.[OrderNoPKey]
LEFT JOIN [Catalog].[Items] i ON i.[ItemCode] = o.[ItemCode]
WHERE NOT EXISTS (SELECT 1 FROM [Orders].[OrderLines] t WHERE t.[LegacyId] = o.[Id]);

COMMIT TRANSACTION;
