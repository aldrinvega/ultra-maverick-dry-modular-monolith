/* Baseline Identity seed for a fresh database (Docker/dev).
   Run AFTER the EF migration has created the Identity and Infrastructure schemas.
   Safe to re-run: every insert is guarded. */

USE [ElixirDepotDry];
GO

IF NOT EXISTS (SELECT 1 FROM [Identity].[Roles] WHERE [Name] = N'Administrator')
    INSERT INTO [Identity].[Roles] ([Name],[IsActive],[CreatedAtUtc]) VALUES (N'Administrator', 1, SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM [Identity].[Roles] WHERE [Name] = N'Supervisor')
    INSERT INTO [Identity].[Roles] ([Name],[IsActive],[CreatedAtUtc]) VALUES (N'Supervisor', 1, SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM [Identity].[Roles] WHERE [Name] = N'Encoder')
    INSERT INTO [Identity].[Roles] ([Name],[IsActive],[CreatedAtUtc]) VALUES (N'Encoder', 1, SYSUTCDATETIME());
GO

IF NOT EXISTS (SELECT 1 FROM [Identity].[Departments] WHERE [Name] = N'Warehouse')
    INSERT INTO [Identity].[Departments] ([Name],[IsActive],[CreatedAtUtc]) VALUES (N'Warehouse', 1, SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM [Identity].[Departments] WHERE [Name] = N'Quality Control')
    INSERT INTO [Identity].[Departments] ([Name],[IsActive],[CreatedAtUtc]) VALUES (N'Quality Control', 1, SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM [Identity].[Departments] WHERE [Name] = N'Production')
    INSERT INTO [Identity].[Departments] ([Name],[IsActive],[CreatedAtUtc]) VALUES (N'Production', 1, SYSUTCDATETIME());
GO

IF NOT EXISTS (SELECT 1 FROM [Identity].[MainMenus] WHERE [Name] = N'Master Data')
    INSERT INTO [Identity].[MainMenus] ([Name],[Path],[SortOrder],[IsActive],[CreatedAtUtc]) VALUES (N'Master Data', N'/master-data', 1, 1, SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM [Identity].[MainMenus] WHERE [Name] = N'Transactions')
    INSERT INTO [Identity].[MainMenus] ([Name],[Path],[SortOrder],[IsActive],[CreatedAtUtc]) VALUES (N'Transactions', N'/transactions', 2, 1, SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM [Identity].[MainMenus] WHERE [Name] = N'Reports')
    INSERT INTO [Identity].[MainMenus] ([Name],[Path],[SortOrder],[IsActive],[CreatedAtUtc]) VALUES (N'Reports', N'/reports', 3, 1, SYSUTCDATETIME());
GO

DECLARE @MasterData INT = (SELECT [Id] FROM [Identity].[MainMenus] WHERE [Name] = N'Master Data');
DECLARE @Transactions INT = (SELECT [Id] FROM [Identity].[MainMenus] WHERE [Name] = N'Transactions');

IF NOT EXISTS (SELECT 1 FROM [Identity].[Modules] WHERE [Name] = N'Identity')
    INSERT INTO [Identity].[Modules] ([MainMenuId],[Name],[SubMenuName],[IsActive],[CreatedAtUtc]) VALUES (@MasterData, N'Identity', N'Users, Roles, Modules', 1, SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM [Identity].[Modules] WHERE [Name] = N'Catalog')
    INSERT INTO [Identity].[Modules] ([MainMenuId],[Name],[SubMenuName],[IsActive],[CreatedAtUtc]) VALUES (@MasterData, N'Catalog', N'Items, Suppliers, Customers, Lookups', 1, SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM [Identity].[Modules] WHERE [Name] = N'Procurement')
    INSERT INTO [Identity].[Modules] ([MainMenuId],[Name],[SubMenuName],[IsActive],[CreatedAtUtc]) VALUES (@Transactions, N'Procurement', N'Purchase Orders, Import', 1, SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM [Identity].[Modules] WHERE [Name] = N'Quality')
    INSERT INTO [Identity].[Modules] ([MainMenuId],[Name],[SubMenuName],[IsActive],[CreatedAtUtc]) VALUES (@Transactions, N'Quality', N'Receiving, Checklists', 1, SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM [Identity].[Modules] WHERE [Name] = N'Laboratory')
    INSERT INTO [Identity].[Modules] ([MainMenuId],[Name],[SubMenuName],[IsActive],[CreatedAtUtc]) VALUES (@Transactions, N'Laboratory', N'Lab Tests', 1, SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM [Identity].[Modules] WHERE [Name] = N'Warehouse')
    INSERT INTO [Identity].[Modules] ([MainMenuId],[Name],[SubMenuName],[IsActive],[CreatedAtUtc]) VALUES (@Transactions, N'Warehouse', N'Receipts, Issues, Balances', 1, SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM [Identity].[Modules] WHERE [Name] = N'Orders')
    INSERT INTO [Identity].[Modules] ([MainMenuId],[Name],[SubMenuName],[IsActive],[CreatedAtUtc]) VALUES (@Transactions, N'Orders', N'Orders, Move Orders', 1, SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM [Identity].[Modules] WHERE [Name] = N'Manufacturing')
    INSERT INTO [Identity].[Modules] ([MainMenuId],[Name],[SubMenuName],[IsActive],[CreatedAtUtc]) VALUES (@Transactions, N'Manufacturing', N'Plans, Transformation', 1, SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM [Identity].[Modules] WHERE [Name] = N'Reporting')
    INSERT INTO [Identity].[Modules] ([MainMenuId],[Name],[SubMenuName],[IsActive],[CreatedAtUtc]) VALUES (@Transactions, N'Reporting', N'Reports', 1, SYSUTCDATETIME());
GO

/* Administrator keeps every module. */
INSERT INTO [Identity].[RoleModules] ([RoleId],[ModuleId],[IsActive],[CreatedAtUtc])
SELECT r.[Id], m.[Id], 1, SYSUTCDATETIME()
FROM [Identity].[Roles] r CROSS JOIN [Identity].[Modules] m
WHERE r.[Name] = N'Administrator'
  AND NOT EXISTS (SELECT 1 FROM [Identity].[RoleModules] rm WHERE rm.[RoleId] = r.[Id] AND rm.[ModuleId] = m.[Id]);
GO

/* Baseline admin. Password: Admin@123  (change it after first login). */
IF NOT EXISTS (SELECT 1 FROM [Identity].[Users] WHERE [UserName] = N'admin')
    INSERT INTO [Identity].[Users] ([FullName],[UserName],[PasswordHash],[RoleId],[DepartmentId],[IsActive],[CreatedAtUtc])
    VALUES (N'System Administrator', N'admin', N'PBKDF2;SHA256;210000;96CkzEd8k8obDW1QDB/Q+w==;G/n/pJKNyuOf7S9piW67jQgW2hmjgY0tCawpgS7y5DY=',
            (SELECT [Id] FROM [Identity].[Roles] WHERE [Name] = N'Administrator'),
            (SELECT [Id] FROM [Identity].[Departments] WHERE [Name] = N'Warehouse'),
            1, SYSUTCDATETIME());
GO