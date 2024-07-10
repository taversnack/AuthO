BEGIN TRANSACTION;
GO

DROP INDEX [IX_CardHolders_TenantId_Email] ON [slk].[CardHolders];
GO

DROP INDEX [IX_CardHolders_TenantId_UniqueIdentifier] ON [slk].[CardHolders];
GO

DROP INDEX [IX_CardCredentials_SerialNumber] ON [slk].[CardCredentials];
GO

ALTER TABLE [slk].[Lockers] ADD [ServiceTag] nvarchar(32) NULL;
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[slk].[CardHolders]') AND [c].[name] = N'UniqueIdentifier');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [slk].[CardHolders] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [slk].[CardHolders] ALTER COLUMN [UniqueIdentifier] nvarchar(256) NULL;
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[slk].[CardHolders]') AND [c].[name] = N'Email');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [slk].[CardHolders] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [slk].[CardHolders] ALTER COLUMN [Email] nvarchar(256) NULL;
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[slk].[CardCredentials]') AND [c].[name] = N'SerialNumber');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [slk].[CardCredentials] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [slk].[CardCredentials] ALTER COLUMN [SerialNumber] nvarchar(16) NULL;
GO

ALTER TABLE [slk].[CardCredentials] ADD [CardLabel] nvarchar(256) NULL;
GO

ALTER TABLE [slk].[CardCredentials] ADD [HidNumber] nvarchar(max) NULL;
GO

CREATE UNIQUE INDEX [IX_Tenants_Name] ON [slk].[Tenants] ([Name]);
GO

CREATE UNIQUE INDEX [IX_Lockers_TenantId_ServiceTag] ON [slk].[Lockers] ([TenantId], [ServiceTag]) WHERE [ServiceTag] IS NOT NULL;
GO

CREATE UNIQUE INDEX [IX_CardHolders_TenantId_Email] ON [slk].[CardHolders] ([TenantId], [Email]) WHERE [Email] IS NOT NULL;
GO

CREATE UNIQUE INDEX [IX_CardHolders_TenantId_UniqueIdentifier] ON [slk].[CardHolders] ([TenantId], [UniqueIdentifier]) WHERE [UniqueIdentifier] IS NOT NULL;
GO

CREATE UNIQUE INDEX [IX_CardCredentials_SerialNumber] ON [slk].[CardCredentials] ([SerialNumber]) WHERE [SerialNumber] IS NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230503124254_AddHidNumberToCardsAndRemovedConstraints', N'7.0.5');
GO

COMMIT;
GO

