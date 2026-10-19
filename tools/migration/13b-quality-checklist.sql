SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

INSERT INTO [Quality].[ChecklistTypes] ([LegacyId], [Name], [ProductTypeId], [IsActive])
SELECT s.[Id], ISNULL(NULLIF(LTRIM(RTRIM(s.[ChecklistType])), ''), 'Checklist type ' + CAST(s.[Id] AS nvarchar(20))), pt.[NewId], s.[IsActive]
FROM [$(LegacyDb)].[dbo].[ChecklistTypes] s
LEFT JOIN [dbo].[LegacyIdMap] pt ON pt.[TableName] = 'ProductTypes' AND pt.[LegacyId] = s.[ProductTypeId]
WHERE NOT EXISTS (SELECT 1 FROM [Quality].[ChecklistTypes] t WHERE t.[LegacyId] = s.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'ChecklistTypes', t.[LegacyId], t.[Id] FROM [Quality].[ChecklistTypes] t
WHERE t.[LegacyId] IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'ChecklistTypes' AND m.[LegacyId] = t.[LegacyId]);

INSERT INTO [Quality].[ChecklistQuestions] ([LegacyId], [ChecklistTypeId], [Question], [AnswerType], [SortOrder], [IsActive])
SELECT s.[Id], ct.[NewId], ISNULL(s.[ChecklistQuestion], ''), s.[AnswerType], ISNULL(s.[OrderId], 0), s.[IsActive]
FROM [$(LegacyDb)].[dbo].[ChecklistQuestions] s
JOIN [dbo].[LegacyIdMap] ct ON ct.[TableName] = 'ChecklistTypes' AND ct.[LegacyId] = s.[ChecklistTypeId]
WHERE NOT EXISTS (SELECT 1 FROM [Quality].[ChecklistQuestions] t WHERE t.[LegacyId] = s.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'ChecklistQuestions', t.[LegacyId], t.[Id] FROM [Quality].[ChecklistQuestions] t
WHERE t.[LegacyId] IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'ChecklistQuestions' AND m.[LegacyId] = t.[LegacyId]);

INSERT INTO [Quality].[Checklists] ([LegacyId], [QcReceiptId], [ProductTypeId], [CreatedAtUtc], [CreatedByUserId])
SELECT s.[Id], qm.[NewId], pt.[NewId], s.[CreatedAt], NULL
FROM [$(LegacyDb)].[dbo].[QcChecklists] s
JOIN [dbo].[LegacyIdMap] qm ON qm.[TableName] = 'QC_Receiving' AND qm.[LegacyId] = s.[ReceivingId]
LEFT JOIN [dbo].[LegacyIdMap] pt ON pt.[TableName] = 'ProductTypes' AND pt.[LegacyId] = s.[ProductTypeId]
WHERE NOT EXISTS (SELECT 1 FROM [Quality].[Checklists] t WHERE t.[LegacyId] = s.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'QcChecklists', t.[LegacyId], t.[Id] FROM [Quality].[Checklists] t
WHERE t.[LegacyId] IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'QcChecklists' AND m.[LegacyId] = t.[LegacyId]);

INSERT INTO [Quality].[ChecklistAnswers] ([LegacyId], [ChecklistId], [QuestionId], [Status], [IsActive], [CreatedAtUtc])
SELECT s.[Id], cm.[NewId], qm.[NewId], s.[Status], 1, SYSUTCDATETIME()
FROM [$(LegacyDb)].[dbo].[ChecklistAnswers] s
JOIN [dbo].[LegacyIdMap] cm ON cm.[TableName] = 'QcChecklists' AND cm.[LegacyId] = s.[QCChecklistId]
JOIN [dbo].[LegacyIdMap] qm ON qm.[TableName] = 'ChecklistQuestions' AND qm.[LegacyId] = s.[ChecklistQuestionsId]
WHERE NOT EXISTS (SELECT 1 FROM [Quality].[ChecklistAnswers] t WHERE t.[LegacyId] = s.[Id]);

INSERT INTO [Quality].[ChecklistOpenFieldAnswers] ([LegacyId], [ChecklistId], [QuestionId], [Remarks], [IsActive], [CreatedAtUtc])
SELECT s.[Id], cm.[NewId], qm.[NewId], ISNULL(s.[Remarks], ''), 1, SYSUTCDATETIME()
FROM [$(LegacyDb)].[dbo].[QChecklistOpenFieldAnswers] s
JOIN [dbo].[LegacyIdMap] cm ON cm.[TableName] = 'QcChecklists' AND cm.[LegacyId] = s.[QCChecklistId]
JOIN [dbo].[LegacyIdMap] qm ON qm.[TableName] = 'ChecklistQuestions' AND qm.[LegacyId] = s.[ChecklistQuestionId]
WHERE NOT EXISTS (SELECT 1 FROM [Quality].[ChecklistOpenFieldAnswers] t WHERE t.[LegacyId] = s.[Id]);

IF OBJECT_ID(N'[dbo].[ChecklistDimensionQuarantine]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ChecklistDimensionQuarantine]
    (
        [LegacyId] int NOT NULL PRIMARY KEY,
        [Standard] nvarchar(200) NULL,
        [Actual] nvarchar(200) NULL,
        [Reason] nvarchar(200) NOT NULL
    );
END;

INSERT INTO [dbo].[ChecklistDimensionQuarantine] ([LegacyId], [Standard], [Actual], [Reason])
SELECT s.[Id], s.[Standard], s.[Actual], 'Non-numeric dimension value'
FROM [$(LegacyDb)].[dbo].[QChecklistProductDimensions] s
WHERE ((s.[Standard] IS NOT NULL AND TRY_CAST(s.[Standard] AS decimal(18,4)) IS NULL)
    OR (s.[Actual] IS NOT NULL AND TRY_CAST(s.[Actual] AS decimal(18,4)) IS NULL))
  AND NOT EXISTS (SELECT 1 FROM [dbo].[ChecklistDimensionQuarantine] q WHERE q.[LegacyId] = s.[Id]);

INSERT INTO [Quality].[ChecklistProductDimensions] ([LegacyId], [ChecklistId], [QuestionId], [StandardValue], [ActualValue], [IsActive], [CreatedAtUtc])
SELECT s.[Id], cm.[NewId], qm.[NewId], TRY_CAST(s.[Standard] AS decimal(18,4)), TRY_CAST(s.[Actual] AS decimal(18,4)), 1, SYSUTCDATETIME()
FROM [$(LegacyDb)].[dbo].[QChecklistProductDimensions] s
JOIN [dbo].[LegacyIdMap] cm ON cm.[TableName] = 'QcChecklists' AND cm.[LegacyId] = s.[QCChecklistId]
JOIN [dbo].[LegacyIdMap] qm ON qm.[TableName] = 'ChecklistQuestions' AND qm.[LegacyId] = s.[ChecklistQuestionId]
WHERE NOT EXISTS (SELECT 1 FROM [Quality].[ChecklistProductDimensions] t WHERE t.[LegacyId] = s.[Id]);

INSERT INTO [Quality].[ChecklistOtherObservations] ([LegacyId], [ChecklistId], [Observation], [IsActive], [CreatedAtUtc])
SELECT s.[Id], cm.[NewId], ISNULL(s.[Observation], ''), 1, SYSUTCDATETIME()
FROM [$(LegacyDb)].[dbo].[ChecklistOtherObservations] s
JOIN [dbo].[LegacyIdMap] cm ON cm.[TableName] = 'QcChecklists' AND cm.[LegacyId] = s.[QCChecklistId]
WHERE NOT EXISTS (SELECT 1 FROM [Quality].[ChecklistOtherObservations] t WHERE t.[LegacyId] = s.[Id]);

INSERT INTO [Quality].[ChecklistCompliances] ([LegacyId], [ChecklistId], [Compliance], [Description], [RootCause], [IsActive], [CreatedAtUtc])
SELECT s.[Id], cm.[NewId], ISNULL(s.[Compliance], ''), s.[Description], s.[RootCause], 1, SYSUTCDATETIME()
FROM [$(LegacyDb)].[dbo].[ChecklistCompliances] s
JOIN [dbo].[LegacyIdMap] cm ON cm.[TableName] = 'QcChecklists' AND cm.[LegacyId] = s.[QCChecklistId]
WHERE NOT EXISTS (SELECT 1 FROM [Quality].[ChecklistCompliances] t WHERE t.[LegacyId] = s.[Id]);

INSERT INTO [Quality].[ChecklistReviewVerificationLogs] ([LegacyId], [ChecklistId], [Disposition], [Remarks], [ReviewedByUserId], [ReviewedAtUtc])
SELECT s.[Id], cm.[NewId], CAST(s.[DispositionId] AS nvarchar(50)), NULL, u.[Id], SYSUTCDATETIME()
FROM [$(LegacyDb)].[dbo].[ChecklistReviewVerificationLogs] s
JOIN [dbo].[LegacyIdMap] cm ON cm.[TableName] = 'QcChecklists' AND cm.[LegacyId] = s.[QCChecklistId]
LEFT JOIN [Identity].[Users] u ON u.[FullName] = s.[ReviewedBy]
WHERE NOT EXISTS (SELECT 1 FROM [Quality].[ChecklistReviewVerificationLogs] t WHERE t.[LegacyId] = s.[Id]);

COMMIT TRANSACTION;
