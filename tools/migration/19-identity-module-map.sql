SET NOCOUNT ON;
SET XACT_ABORT ON;

-- Legacy Modules are menu route paths (/ordering/orders, /setup/uom-management, ...), not module
-- identifiers, so role claims never matched the RequireModule policies. Insert the nine canonical
-- module identifiers and remap every role's menu grant onto them by path family.

BEGIN TRANSACTION;

DECLARE @canonical TABLE ([Name] nvarchar(100) PRIMARY KEY);
INSERT INTO @canonical ([Name]) VALUES
('Identity'),('Catalog'),('Procurement'),('Quality'),('Laboratory'),
('Warehouse'),('Orders'),('Manufacturing'),('Reporting');

INSERT INTO [Identity].[Modules] ([Name], [MainMenuId], [SubMenuName], [IsActive], [CreatedAtUtc])
SELECT c.[Name],
       (SELECT TOP 1 [Id] FROM [Identity].[MainMenus] ORDER BY [Id]),
       c.[Name],
       1,
       SYSUTCDATETIME()
FROM @canonical c
WHERE NOT EXISTS (SELECT 1 FROM [Identity].[Modules] m WHERE m.[Name] = c.[Name]);

INSERT INTO [Identity].[RoleModules] ([RoleId], [ModuleId], [IsActive], [CreatedAtUtc])
SELECT DISTINCT rm.[RoleId], m.[Id], 1, SYSUTCDATETIME()
FROM [Identity].[RoleModules] rm
JOIN [Identity].[Modules] legacy ON legacy.[Id] = rm.[ModuleId]
JOIN @canonical c ON c.[Name] = CASE
    WHEN legacy.[Name] LIKE '/ordering/%' OR legacy.[Name] LIKE '/move-order-approval/%' THEN 'Orders'
    WHEN legacy.[Name] LIKE '/inventory/%' OR legacy.[Name] LIKE '/receiving/%' THEN 'Warehouse'
    WHEN legacy.[Name] LIKE '/qc-module/%' THEN 'Quality'
    WHEN legacy.[Name] LIKE '/import/%' THEN 'Procurement'
    WHEN legacy.[Name] LIKE '/user-management/%' THEN 'Identity'
    WHEN legacy.[Name] LIKE '/reports/%' THEN 'Reporting'
    WHEN legacy.[Name] LIKE '/setup/%' THEN 'Catalog'
    END
JOIN [Identity].[Modules] m ON m.[Name] = c.[Name]
WHERE ISNULL(rm.[IsActive], 0) = 1
  AND ISNULL(legacy.[IsActive], 0) = 1
  AND NOT EXISTS (
    SELECT 1 FROM [Identity].[RoleModules] x
    WHERE x.[RoleId] = rm.[RoleId] AND x.[ModuleId] = m.[Id]);

COMMIT TRANSACTION;
