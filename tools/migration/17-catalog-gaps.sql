SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

INSERT INTO [Catalog].[TransactionTypes] ([LegacyId], [Name], [IsActive], [CreatedAtUtc])
SELECT s.[Id],
       ISNULL(NULLIF(LTRIM(RTRIM(s.[TransactionName])), ''), 'Transaction ' + CAST(s.[Id] AS nvarchar(20))),
       s.[IsActive],
       ISNULL(s.[DateAdded], SYSUTCDATETIME())
FROM [$(LegacyDb)].[dbo].[Transactions] s
WHERE NOT EXISTS (SELECT 1 FROM [Catalog].[TransactionTypes] t WHERE t.[LegacyId] = s.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'Transactions', t.[LegacyId], t.[Id]
FROM [Catalog].[TransactionTypes] t
WHERE t.[LegacyId] IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'Transactions' AND m.[LegacyId] = t.[LegacyId]);

INSERT INTO [Catalog].[SampleTypes] ([LegacyId], [Name], [IsActive], [CreatedAtUtc])
SELECT s.[Id],
       ISNULL(NULLIF(LTRIM(RTRIM(s.[SampleTypeName])), ''), 'Sample type ' + CAST(s.[Id] AS nvarchar(20))),
       s.[IsActive],
       ISNULL(s.[DateAdded], SYSUTCDATETIME())
FROM [$(LegacyDb)].[dbo].[SampleTypes] s
WHERE NOT EXISTS (SELECT 1 FROM [Catalog].[SampleTypes] t WHERE t.[LegacyId] = s.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'SampleTypes', t.[LegacyId], t.[Id]
FROM [Catalog].[SampleTypes] t
WHERE t.[LegacyId] IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'SampleTypes' AND m.[LegacyId] = t.[LegacyId]);

WITH perHeader AS (
    SELECT o.[OrderNoPKey], MIN(o.[TransactionType]) AS [TransactionType]
    FROM [$(LegacyDb)].[dbo].[Orders] o
    WHERE o.[TransactionType] IS NOT NULL
    GROUP BY o.[OrderNoPKey]
)
UPDATE o
SET o.[TransactionTypeId] = tt.[Id]
FROM [Orders].[Orders] o
JOIN perHeader h ON CAST(h.[OrderNoPKey] AS nvarchar(64)) = o.[OrderNumber]
JOIN [Catalog].[TransactionTypes] tt ON tt.[Name] = h.[TransactionType]
WHERE o.[TransactionTypeId] IS NULL;

COMMIT TRANSACTION;
