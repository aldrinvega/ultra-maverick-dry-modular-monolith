SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

INSERT INTO [Catalog].[Uoms] ([LegacyId], [Code], [Description], [IsActive], [CreatedAtUtc])
SELECT s.[Id], ISNULL(s.[UOM_Code], 'UOM-' + CAST(s.[Id] AS nvarchar(20))), ISNULL(s.[UOM_Description], ''), s.[IsActive], ISNULL(s.[DateAdded], SYSUTCDATETIME())
FROM [$(LegacyDb)].[dbo].[UOMS] s
WHERE NOT EXISTS (SELECT 1 FROM [Catalog].[Uoms] t WHERE t.[LegacyId] = s.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'UOMS', t.[LegacyId], t.[Id] FROM [Catalog].[Uoms] t
WHERE t.[LegacyId] IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'UOMS' AND m.[LegacyId] = t.[LegacyId]);

INSERT INTO [Catalog].[ItemCategories] ([LegacyId], [Name], [IsActive], [CreatedAtUtc])
SELECT s.[Id], ISNULL(s.[ItemCategoryName], 'Category ' + CAST(s.[Id] AS nvarchar(20))), s.[IsActive], ISNULL(s.[DateAdded], SYSUTCDATETIME())
FROM [$(LegacyDb)].[dbo].[ItemCategories] s
WHERE NOT EXISTS (SELECT 1 FROM [Catalog].[ItemCategories] t WHERE t.[LegacyId] = s.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'ItemCategories', t.[LegacyId], t.[Id] FROM [Catalog].[ItemCategories] t
WHERE t.[LegacyId] IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'ItemCategories' AND m.[LegacyId] = t.[LegacyId]);

INSERT INTO [Catalog].[ProductTypes] ([LegacyId], [Name], [IsActive], [CreatedAtUtc])
SELECT s.[Id], ISNULL(s.[ProductTypeName], 'Product type ' + CAST(s.[Id] AS nvarchar(20))), s.[IsActive], ISNULL(s.[DateAdded], SYSUTCDATETIME())
FROM [$(LegacyDb)].[dbo].[ProductTypes] s
WHERE NOT EXISTS (SELECT 1 FROM [Catalog].[ProductTypes] t WHERE t.[LegacyId] = s.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'ProductTypes', t.[LegacyId], t.[Id] FROM [Catalog].[ProductTypes] t
WHERE t.[LegacyId] IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'ProductTypes' AND m.[LegacyId] = t.[LegacyId]);

INSERT INTO [Catalog].[Reasons] ([LegacyId], [Name], [MenuId], [IsActive], [CreatedAtUtc])
SELECT s.[Id], ISNULL(s.[ReasonName], 'Reason ' + CAST(s.[Id] AS nvarchar(20))), mm.[NewId], s.[IsActive], ISNULL(s.[DateAdded], SYSUTCDATETIME())
FROM [$(LegacyDb)].[dbo].[Reasons] s
LEFT JOIN [dbo].[LegacyIdMap] mm ON mm.[TableName] = 'MainMenus' AND mm.[LegacyId] = s.[MenuId]
WHERE NOT EXISTS (SELECT 1 FROM [Catalog].[Reasons] t WHERE t.[LegacyId] = s.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'Reasons', t.[LegacyId], t.[Id] FROM [Catalog].[Reasons] t
WHERE t.[LegacyId] IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'Reasons' AND m.[LegacyId] = t.[LegacyId]);

;WITH src AS (
    SELECT s.[Id],
           s.[AccountTitleCode],
           ISNULL(NULLIF(LTRIM(RTRIM(s.[AccountTitleName])), ''), ISNULL(s.[AccountTitleCode], 'Account title ' + CAST(s.[Id] AS nvarchar(20)))) AS [Name],
           s.[IsActive],
           s.[CreatedAt],
           ROW_NUMBER() OVER (
               PARTITION BY ISNULL(NULLIF(LTRIM(RTRIM(s.[AccountTitleName])), ''), ISNULL(s.[AccountTitleCode], 'Account title ' + CAST(s.[Id] AS nvarchar(20))))
               ORDER BY s.[Id]) AS [rn]
    FROM [$(LegacyDb)].[dbo].[AccountTitles] s
)
INSERT INTO [Catalog].[AccountTitles] ([LegacyId], [Code], [Name], [IsActive], [CreatedAtUtc])
SELECT src.[Id], src.[AccountTitleCode], src.[Name], src.[IsActive], ISNULL(src.[CreatedAt], SYSUTCDATETIME())
FROM src
WHERE src.[rn] = 1
  AND NOT EXISTS (SELECT 1 FROM [Catalog].[AccountTitles] t WHERE t.[Name] = src.[Name]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'AccountTitles', t.[LegacyId], t.[Id] FROM [Catalog].[AccountTitles] t
WHERE t.[LegacyId] IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'AccountTitles' AND m.[LegacyId] = t.[LegacyId]);

INSERT INTO [Catalog].[Farms] ([LegacyId], [FarmCode], [FarmName], [IsActive], [CreatedAtUtc])
SELECT s.[Id], ISNULL(s.[FarmCode], 'FARM-' + CAST(s.[Id] AS nvarchar(20))), ISNULL(s.[FarmName], 'Farm ' + CAST(s.[Id] AS nvarchar(20))), s.[IsActive], ISNULL(s.[DateAdded], SYSUTCDATETIME())
FROM [$(LegacyDb)].[dbo].[Farms] s
WHERE NOT EXISTS (SELECT 1 FROM [Catalog].[Farms] t WHERE t.[LegacyId] = s.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'Farms', t.[LegacyId], t.[Id] FROM [Catalog].[Farms] t
WHERE t.[LegacyId] IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'Farms' AND m.[LegacyId] = t.[LegacyId]);

INSERT INTO [Catalog].[LotCategories] ([LegacyId], [Code], [Name], [IsActive], [CreatedAtUtc])
SELECT s.[Id], s.[LotCategoryCode], ISNULL(s.[LotCategoryName], 'Lot category ' + CAST(s.[Id] AS nvarchar(20))), s.[IsActive], ISNULL(s.[DateAdded], SYSUTCDATETIME())
FROM [$(LegacyDb)].[dbo].[LotCategories] s
WHERE NOT EXISTS (SELECT 1 FROM [Catalog].[LotCategories] t WHERE t.[LegacyId] = s.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'LotCategories', t.[LegacyId], t.[Id] FROM [Catalog].[LotCategories] t
WHERE t.[LegacyId] IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'LotCategories' AND m.[LegacyId] = t.[LegacyId]);

INSERT INTO [Catalog].[LotNames] ([LegacyId], [Code], [LotCategoryId], [SectionName], [IsActive], [CreatedAtUtc])
SELECT s.[Id], ISNULL(s.[LotNameCode], 'LOT-' + CAST(s.[Id] AS nvarchar(20))), lc.[NewId], s.[SectionName], s.[IsActive], ISNULL(s.[DateAdded], SYSUTCDATETIME())
FROM [$(LegacyDb)].[dbo].[LotNames] s
JOIN [dbo].[LegacyIdMap] lc ON lc.[TableName] = 'LotCategories' AND lc.[LegacyId] = s.[LotCategoryId]
WHERE NOT EXISTS (SELECT 1 FROM [Catalog].[LotNames] t WHERE t.[LegacyId] = s.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'LotNames', t.[LegacyId], t.[Id] FROM [Catalog].[LotNames] t
WHERE t.[LegacyId] IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'LotNames' AND m.[LegacyId] = t.[LegacyId]);

;WITH src AS (
    SELECT s.[Id],
           ISNULL(s.[SupplierCode], 'SUP-' + CAST(s.[Id] AS nvarchar(20))) AS [Code],
           s.[SupplierName],
           s.[SupplierAddress],
           s.[IsActive],
           s.[DateAdded],
           ROW_NUMBER() OVER (
               PARTITION BY ISNULL(s.[SupplierCode], 'SUP-' + CAST(s.[Id] AS nvarchar(20)))
               ORDER BY s.[Id]) AS [rn]
    FROM [$(LegacyDb)].[dbo].[Suppliers] s
)
INSERT INTO [Catalog].[Suppliers] ([LegacyId], [SupplierCode], [SupplierName], [SupplierAddress], [IsActive], [CreatedAtUtc])
SELECT src.[Id],
       CASE WHEN src.[rn] = 1 THEN src.[Code] ELSE src.[Code] + '-' + CAST(src.[Id] AS nvarchar(20)) END,
       ISNULL(src.[SupplierName], 'Supplier ' + CAST(src.[Id] AS nvarchar(20))),
       src.[SupplierAddress],
       src.[IsActive],
       ISNULL(src.[DateAdded], SYSUTCDATETIME())
FROM src
WHERE NOT EXISTS (SELECT 1 FROM [Catalog].[Suppliers] t WHERE t.[LegacyId] = src.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'Suppliers', t.[LegacyId], t.[Id] FROM [Catalog].[Suppliers] t
WHERE t.[LegacyId] IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'Suppliers' AND m.[LegacyId] = t.[LegacyId]);

;WITH src AS (
    SELECT s.[Id],
           ISNULL(s.[CustomerCode], 'CUST-' + CAST(s.[Id] AS nvarchar(20))) AS [Code],
           s.[CustomerName],
           s.[FarmTypeId],
           s.[CompanyCode],
           s.[CompanyName],
           s.[AccountTitle],
           s.[MobileNumber],
           s.[LeadMan],
           s.[Address],
           s.[IsActive],
           s.[DateAdded],
           ROW_NUMBER() OVER (
               PARTITION BY ISNULL(s.[CustomerCode], 'CUST-' + CAST(s.[Id] AS nvarchar(20)))
               ORDER BY s.[Id]) AS [rn]
    FROM [$(LegacyDb)].[dbo].[Customers] s
)
INSERT INTO [Catalog].[Customers] ([LegacyId], [CustomerCode], [CustomerName], [FarmId], [CompanyCode], [CompanyName], [AccountTitleId], [MobileNumber], [LeadMan], [Address], [IsActive], [CreatedAtUtc])
SELECT src.[Id],
       CASE WHEN src.[rn] = 1 THEN src.[Code] ELSE src.[Code] + '-' + CAST(src.[Id] AS nvarchar(20)) END,
       ISNULL(src.[CustomerName], 'Customer ' + CAST(src.[Id] AS nvarchar(20))),
       f.[NewId],
       src.[CompanyCode],
       src.[CompanyName],
       a.[Id],
       src.[MobileNumber],
       src.[LeadMan],
       src.[Address],
       src.[IsActive],
       ISNULL(src.[DateAdded], SYSUTCDATETIME())
FROM src
LEFT JOIN [dbo].[LegacyIdMap] f ON f.[TableName] = 'Farms' AND f.[LegacyId] = src.[FarmTypeId]
LEFT JOIN [Catalog].[AccountTitles] a ON a.[Name] = src.[AccountTitle]
WHERE NOT EXISTS (SELECT 1 FROM [Catalog].[Customers] t WHERE t.[LegacyId] = src.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'Customers', t.[LegacyId], t.[Id] FROM [Catalog].[Customers] t
WHERE t.[LegacyId] IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'Customers' AND m.[LegacyId] = t.[LegacyId]);

INSERT INTO [Catalog].[Items] ([LegacyId], [ItemCode], [ItemDescription], [ItemCategoryId], [UomId], [BufferLevel], [IsExpirable], [IsActive], [CreatedAtUtc])
SELECT s.[Id],
       ISNULL(s.[ItemCode], 'ITEM-' + CAST(s.[Id] AS nvarchar(20))),
       ISNULL(s.[ItemDescription], ''),
       ic.[NewId],
       u.[NewId],
       s.[BufferLevel],
       s.[IsExpirable],
       s.[IsActive],
       ISNULL(s.[DateAdded], SYSUTCDATETIME())
FROM [$(LegacyDb)].[dbo].[RawMaterials] s
JOIN [dbo].[LegacyIdMap] ic ON ic.[TableName] = 'ItemCategories' AND ic.[LegacyId] = s.[ItemCategoryId]
JOIN [dbo].[LegacyIdMap] u ON u.[TableName] = 'UOMS' AND u.[LegacyId] = s.[UomId]
WHERE NOT EXISTS (SELECT 1 FROM [Catalog].[Items] t WHERE t.[LegacyId] = s.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'RawMaterials', t.[LegacyId], t.[Id] FROM [Catalog].[Items] t
WHERE t.[LegacyId] IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'RawMaterials' AND m.[LegacyId] = t.[LegacyId]);

COMMIT TRANSACTION;
