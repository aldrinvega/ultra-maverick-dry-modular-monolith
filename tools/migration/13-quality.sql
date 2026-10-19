SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM [Catalog].[Items] WHERE [ItemCode] = 'ITEM-LEGACY-UNKNOWN')
BEGIN
    INSERT INTO [Catalog].[Items] ([ItemCode], [ItemDescription], [ItemCategoryId], [UomId], [BufferLevel], [IsExpirable], [IsActive], [CreatedAtUtc])
    SELECT 'ITEM-LEGACY-UNKNOWN', 'Legacy unknown item',
           (SELECT TOP 1 [Id] FROM [Catalog].[ItemCategories] ORDER BY [Id]),
           (SELECT TOP 1 [Id] FROM [Catalog].[Uoms] ORDER BY [Id]),
           0, 0, 1, SYSUTCDATETIME();
END;

DECLARE @unknownItemId int = (SELECT [Id] FROM [Catalog].[Items] WHERE [ItemCode] = 'ITEM-LEGACY-UNKNOWN');
DECLARE @fallbackUomId  int = (SELECT TOP 1 [Id] FROM [Catalog].[Uoms] ORDER BY [Id]);

IF OBJECT_ID(N'[dbo].[QcExpectedDeliveryQuarantine]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[QcExpectedDeliveryQuarantine]
    (
        [LegacyId] int NOT NULL PRIMARY KEY,
        [RawValue] decimal(18,4) NULL,
        [Reason] nvarchar(200) NOT NULL
    );
END;

INSERT INTO [dbo].[QcExpectedDeliveryQuarantine] ([LegacyId], [RawValue], [Reason])
SELECT q.[Id], q.[Expected_Delivery], 'Ambiguous legacy numeric date encoding; not converted'
FROM [$(LegacyDb)].[dbo].[QC_Receiving] q
WHERE q.[Expected_Delivery] IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM [dbo].[QcExpectedDeliveryQuarantine] x WHERE x.[LegacyId] = q.[Id]);

INSERT INTO [Quality].[QcReceipts]
    ([LegacyId], [PurchaseOrderId], [PurchaseOrderLineId], [ItemId], [UomId], [ProductTypeId], [BatchNo],
     [ManufacturingDate], [ExpectedDeliveryDate], [ExpiryDate], [ActualDelivered], [TotalReject], [IsNearlyExpire],
     [ExpiryApprovedByUserId], [ExpiryApprovedAtUtc], [QcByUserId], [MonitoredByUserId], [IsWarehouseReceived],
     [Status], [CancelReasonId], [CancelRemarks], [IsActive])
SELECT q.[Id],
       pol.[PurchaseOrderId],
       pol.[Id],
       COALESCE(i.[Id], @unknownItemId),
       COALESCE(i.[UomId], @fallbackUomId),
       NULL,
       q.[Batch_No],
       q.[Manufacturing_Date],
       NULL,
       q.[Expiry_Date],
       q.[Actual_Delivered],
       q.[TotalReject],
       ISNULL(q.[IsNearlyExpire], 0),
       ue.[Id],
       q.[ExpiryDateOfApprove],
       uq.[Id],
       um.[Id],
       ISNULL(q.[IsWareHouseReceive], 0),
       CASE WHEN q.[TotalReject] > 0 THEN 'Rejected' ELSE 'Completed' END,
       r.[Id],
       q.[CancelRemarks],
       q.[IsActive]
FROM [$(LegacyDb)].[dbo].[QC_Receiving] q
JOIN [dbo].[LegacyIdMap] pm ON pm.[TableName] = 'POSummary' AND pm.[LegacyId] = q.[PO_Summary_Id]
JOIN [Procurement].[PurchaseOrderLines] pol ON pol.[Id] = pm.[NewId]
LEFT JOIN [Catalog].[Items] i ON i.[ItemCode] = q.[ItemCode]
LEFT JOIN [Identity].[Users] ue ON ue.[UserName] = q.[ExpiryApproveBy]
LEFT JOIN [Identity].[Users] uq ON uq.[UserName] = q.[QcBy]
LEFT JOIN [Identity].[Users] um ON um.[UserName] = q.[MonitoredBy]
LEFT JOIN [Catalog].[Reasons] r ON r.[Name] = q.[Reason]
WHERE NOT EXISTS (SELECT 1 FROM [Quality].[QcReceipts] t WHERE t.[LegacyId] = q.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'QC_Receiving', t.[LegacyId], t.[Id]
FROM [Quality].[QcReceipts] t
WHERE t.[LegacyId] IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'QC_Receiving' AND m.[LegacyId] = t.[LegacyId]);

INSERT INTO [Quality].[QcRejects]
    ([LegacyId], [QcReceiptId], [Quantity], [ReasonId], [Remarks], [RejectedByUserId], [IsActive], [CreatedAtUtc])
SELECT qr.[Id],
       qm.[NewId],
       qr.[Quantity],
       NULL,
       qr.[Remarks],
       NULL,
       1,
       SYSUTCDATETIME()
FROM [$(LegacyDb)].[dbo].[QC_Reject] qr
JOIN [dbo].[LegacyIdMap] qm ON qm.[TableName] = 'QC_Receiving' AND qm.[LegacyId] = qr.[PO_ReceivingId]
WHERE NOT EXISTS (SELECT 1 FROM [Quality].[QcRejects] t WHERE t.[LegacyId] = qr.[Id]);

COMMIT TRANSACTION;
