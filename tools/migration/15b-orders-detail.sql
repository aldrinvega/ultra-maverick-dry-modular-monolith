SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DELETE FROM [Orders].[CancelledOrders] WHERE [LegacyId] IS NOT NULL;

DECLARE @unknownItemId       int = (SELECT [Id] FROM [Catalog].[Items] WHERE [ItemCode] = 'ITEM-LEGACY-UNKNOWN');
DECLARE @fallbackUomId       int = (SELECT TOP 1 [Id] FROM [Catalog].[Uoms] ORDER BY [Id]);
DECLARE @defaultWarehouseId  int = (SELECT [Id] FROM [Warehouse].[Warehouses] WHERE [Code] = 'LEGACY-DEFAULT');

INSERT INTO [Orders].[AdvancesToEmployees] ([LegacyId], [UserId], [EmployeeName], [CreatedAtUtc])
SELECT a.[Id], u.[Id], ISNULL(a.[EmployeeName], ISNULL(a.[EmployeeId], 'Employee ' + CAST(a.[Id] AS nvarchar(20)))), ISNULL(a.[CreatedAt], SYSUTCDATETIME())
FROM [$(LegacyDb)].[dbo].[AdvancesToEmployees] a
LEFT JOIN [Identity].[Users] u ON u.[FullName] = a.[EmployeeName]
WHERE NOT EXISTS (SELECT 1 FROM [Orders].[AdvancesToEmployees] t WHERE t.[LegacyId] = a.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'AdvancesToEmployees', t.[LegacyId], t.[Id] FROM [Orders].[AdvancesToEmployees] t
WHERE t.[LegacyId] IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'AdvancesToEmployees' AND m.[LegacyId] = t.[LegacyId]);

INSERT INTO [Orders].[CancelledOrders] ([LegacyId], [OrderId], [CustomerId], [ReasonId], [CancelledAtUtc], [CreatedAtUtc])
SELECT c.[Id],
       om.[NewId],
       cm.[NewId],
       r.[Id],
       c.[CancellationDate],
       c.[CancellationDate]
FROM [$(LegacyDb)].[dbo].[CancelledOrders] c
JOIN [$(LegacyDb)].[dbo].[Orders] o ON o.[Id] = c.[OrderId]
JOIN [dbo].[LegacyIdMap] om ON om.[TableName] = 'Orders' AND om.[LegacyId] = o.[OrderNoPKey]
LEFT JOIN [dbo].[LegacyIdMap] cm ON cm.[TableName] = 'Customers' AND cm.[LegacyId] = c.[CustomerId]
LEFT JOIN [Catalog].[Reasons] r ON r.[Name] = c.[Reason]
WHERE NOT EXISTS (SELECT 1 FROM [Orders].[CancelledOrders] t WHERE t.[LegacyId] = c.[Id]);

INSERT INTO [Orders].[MoveOrders]
    ([LegacyId], [OrderId], [OrderNo], [FarmId], [ItemId], [UomId], [QuantityOrdered], [OrderDate], [DateNeeded],
     [WarehouseId], [ExpirationDate], [Status], [IsApproved], [ApprovedAtUtc], [IsPrepared], [PreparedByUserId],
     [PreparedAtUtc], [IsCancelled], [CancelledByUserId], [CancelledAtUtc], [IsRejected], [RejectedByUserId],
     [RejectedAtUtc], [PlateNumber], [DeliveryStatus], [AccountTitleId], [CompanyCode], [CompanyName],
     [AdvancesToEmployeeId], [CheckedByUserId], [Remarks], [IsActive], [CreatedAtUtc])
SELECT m.[Id],
       om.[NewId],
       m.[OrderNo],
       f.[Id],
       COALESCE(i.[Id], @unknownItemId),
       COALESCE(i.[UomId], @fallbackUomId),
       m.[QuantityOrdered],
       m.[OrderDate],
       m.[DateNeeded],
       CASE WHEN m.[WarehouseId] > 0 THEN @defaultWarehouseId ELSE NULL END,
       m.[ExpirationDate],
       CASE
           WHEN ISNULL(m.[IsCancel], 0) = 1 THEN 'Cancelled'
           WHEN ISNULL(m.[IsReject], 0) = 1 THEN 'Rejected'
           WHEN ISNULL(m.[IsApprove], 0) = 1 THEN 'Approved'
           WHEN m.[IsPrepared] = 1 THEN 'Prepared'
           ELSE 'Pending'
       END,
       ISNULL(m.[IsApprove], 0),
       m.[ApprovedDate],
       m.[IsPrepared],
       up.[Id],
       m.[PreparedDate],
       ISNULL(m.[IsCancel], 0),
       uc.[Id],
       m.[CancelledDate],
       ISNULL(m.[IsReject], 0),
       ur.[Id],
       m.[RejectedDate],
       m.[PlateNumber],
       m.[DeliveryStatus],
       at.[Id],
       m.[CompanyCode],
       m.[CompanyName],
       adv.[NewId],
       uck.[Id],
       m.[Remarks],
       m.[IsActive],
       ISNULL(m.[TimeStamp], ISNULL(m.[OrderDate], SYSUTCDATETIME()))
FROM [$(LegacyDb)].[dbo].[MoveOrders] m
LEFT JOIN [dbo].[LegacyIdMap] om ON om.[TableName] = 'Orders' AND om.[LegacyId] = m.[OrderNo]
LEFT JOIN [Catalog].[Farms] f ON f.[FarmCode] = m.[FarmCode]
LEFT JOIN [Catalog].[Items] i ON i.[ItemCode] = m.[ItemCode]
LEFT JOIN [Catalog].[AccountTitles] at ON at.[Name] = m.[AccountTitles]
LEFT JOIN [dbo].[LegacyIdMap] adv ON adv.[TableName] = 'AdvancesToEmployees' AND adv.[LegacyId] = m.[AdvancesToEmployeesId]
LEFT JOIN [Identity].[Users] up ON up.[UserName] = m.[PreparedBy]
LEFT JOIN [Identity].[Users] uc ON uc.[UserName] = m.[CancelBy]
LEFT JOIN [Identity].[Users] ur ON ur.[UserName] = m.[RejectBy]
LEFT JOIN [Identity].[Users] uck ON uck.[UserName] = m.[CheckedBy]
WHERE NOT EXISTS (SELECT 1 FROM [Orders].[MoveOrders] t WHERE t.[LegacyId] = m.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'MoveOrders', t.[LegacyId], t.[Id] FROM [Orders].[MoveOrders] t
WHERE t.[LegacyId] IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'MoveOrders' AND m.[LegacyId] = t.[LegacyId]);

INSERT INTO [Orders].[TransactedMoveOrders]
    ([LegacyId], [MoveOrderId], [OrderNo], [PreparedByUserId], [PreparedAtUtc], [DeliveryDate], [IsTransact], [CreatedAtUtc])
SELECT t.[Id],
       mm.[NewId],
       t.[OrderNo],
       u.[Id],
       t.[PreparedDate],
       t.[DeliveryDate],
       t.[IsTransact],
       ISNULL(t.[PreparedDate], SYSUTCDATETIME())
FROM [$(LegacyDb)].[dbo].[TransactMoveOrder] t
JOIN [dbo].[LegacyIdMap] mm ON mm.[TableName] = 'MoveOrders' AND mm.[LegacyId] = t.[Id]
LEFT JOIN [Identity].[Users] u ON u.[UserName] = t.[PreparedBy]
WHERE NOT EXISTS (SELECT 1 FROM [Orders].[TransactedMoveOrders] x WHERE x.[LegacyId] = t.[Id]);

COMMIT TRANSACTION;

