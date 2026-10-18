SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @passwordSentinel nvarchar(200) =
    'PBKDF2;SHA256;210000;AAAAAAAAAAAAAAAAAAAAAA==;AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=';

BEGIN TRANSACTION;

INSERT INTO [Identity].[Departments] ([LegacyId], [Name], [IsActive], [CreatedAtUtc])
SELECT s.[Id], s.[DepartmentName], s.[IsActive], ISNULL(s.[DateAdded], SYSUTCDATETIME())
FROM [$(LegacyDb)].[dbo].[Departments] s
WHERE NOT EXISTS (SELECT 1 FROM [Identity].[Departments] t WHERE t.[LegacyId] = s.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'Departments', t.[LegacyId], t.[Id]
FROM [Identity].[Departments] t
WHERE t.[LegacyId] IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'Departments' AND m.[LegacyId] = t.[LegacyId]);

INSERT INTO [Identity].[MainMenus] ([LegacyId], [Name], [Path], [IsActive], [SortOrder], [CreatedAtUtc])
SELECT s.[Id], s.[ModuleName], ISNULL(s.[MenuPath], ''), s.[IsActive], 0, ISNULL(s.[DateAdded], SYSUTCDATETIME())
FROM [$(LegacyDb)].[dbo].[MainMenus] s
WHERE NOT EXISTS (SELECT 1 FROM [Identity].[MainMenus] t WHERE t.[LegacyId] = s.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'MainMenus', t.[LegacyId], t.[Id]
FROM [Identity].[MainMenus] t
WHERE t.[LegacyId] IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'MainMenus' AND m.[LegacyId] = t.[LegacyId]);

INSERT INTO [Identity].[Roles] ([LegacyId], [Name], [IsActive], [CreatedAtUtc])
SELECT s.[Id], s.[RoleName], s.[IsActive], ISNULL(s.[DateAdded], SYSUTCDATETIME())
FROM [$(LegacyDb)].[dbo].[Roles] s
WHERE NOT EXISTS (SELECT 1 FROM [Identity].[Roles] t WHERE t.[LegacyId] = s.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'Roles', t.[LegacyId], t.[Id]
FROM [Identity].[Roles] t
WHERE t.[LegacyId] IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'Roles' AND m.[LegacyId] = t.[LegacyId]);

INSERT INTO [Identity].[Modules] ([LegacyId], [MainMenuId], [Name], [SubMenuName], [IsActive], [CreatedAtUtc])
SELECT s.[Id], mm.[NewId], s.[ModuleName], s.[SubMenuName], s.[IsActive], ISNULL(s.[DateAdded], SYSUTCDATETIME())
FROM [$(LegacyDb)].[dbo].[Modules] s
JOIN [dbo].[LegacyIdMap] mm ON mm.[TableName] = 'MainMenus' AND mm.[LegacyId] = s.[MainMenuId]
WHERE NOT EXISTS (SELECT 1 FROM [Identity].[Modules] t WHERE t.[LegacyId] = s.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'Modules', t.[LegacyId], t.[Id]
FROM [Identity].[Modules] t
WHERE t.[LegacyId] IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'Modules' AND m.[LegacyId] = t.[LegacyId]);

INSERT INTO [Identity].[Users] ([LegacyId], [FullName], [UserName], [PasswordHash], [RoleId], [DepartmentId], [IsActive], [CreatedAtUtc])
SELECT s.[Id], s.[FullName], s.[UserName], @passwordSentinel, r.[NewId], d.[NewId], s.[IsActive], ISNULL(s.[DateAdded], SYSUTCDATETIME())
FROM [$(LegacyDb)].[dbo].[Users] s
JOIN [dbo].[LegacyIdMap] r ON r.[TableName] = 'Roles' AND r.[LegacyId] = s.[UserRoleId]
JOIN [dbo].[LegacyIdMap] d ON d.[TableName] = 'Departments' AND d.[LegacyId] = s.[DepartmentId]
WHERE NOT EXISTS (SELECT 1 FROM [Identity].[Users] t WHERE t.[LegacyId] = s.[Id]);

INSERT INTO [dbo].[LegacyIdMap] ([TableName], [LegacyId], [NewId])
SELECT 'Users', t.[LegacyId], t.[Id]
FROM [Identity].[Users] t
WHERE t.[LegacyId] IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM [dbo].[LegacyIdMap] m WHERE m.[TableName] = 'Users' AND m.[LegacyId] = t.[LegacyId]);

INSERT INTO [Identity].[RoleModules] ([RoleId], [ModuleId], [IsActive], [CreatedAtUtc])
SELECT r.[NewId], m.[NewId], MAX(CAST(s.[IsActive] AS int)), SYSUTCDATETIME()
FROM [$(LegacyDb)].[dbo].[RoleModules] s
JOIN [dbo].[LegacyIdMap] r ON r.[TableName] = 'Roles' AND r.[LegacyId] = s.[RoleId]
JOIN [dbo].[LegacyIdMap] m ON m.[TableName] = 'Modules' AND m.[LegacyId] = s.[ModuleId]
WHERE NOT EXISTS (
    SELECT 1 FROM [Identity].[RoleModules] t
    WHERE t.[RoleId] = r.[NewId] AND t.[ModuleId] = m.[NewId])
GROUP BY r.[NewId], m.[NewId];

COMMIT TRANSACTION;
