BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240115104657_AddsTerminationFieldsToCardHolder')
BEGIN
	ALTER TABLE [slk].[KioskAccessCodes] DROP CONSTRAINT [FK_KioskAccessCodes_Tenants_TenantId];
END
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240115104657_AddsTerminationFieldsToCardHolder')
BEGIN
	ALTER TABLE [slk].[CardHolders] ADD [IsTerminated] bit NOT NULL DEFAULT CAST(0 AS bit);
END
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240115104657_AddsTerminationFieldsToCardHolder')
BEGIN
	ALTER TABLE [slk].[CardHolders] ADD [TerminationDate] datetime2 NULL;
END
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240115104657_AddsTerminationFieldsToCardHolder')
BEGIN
	ALTER TABLE [slk].[KioskAccessCodes] ADD CONSTRAINT [FK_KioskAccessCodes_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE NO ACTION;
END
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240115104657_AddsTerminationFieldsToCardHolder')
BEGIN
	INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
	VALUES (N'20240115104657_AddsTerminationFieldsToCardHolder', N'7.0.5');
END
GO

COMMIT;
GO