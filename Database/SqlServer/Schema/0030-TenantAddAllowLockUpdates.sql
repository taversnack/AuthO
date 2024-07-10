BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230523103418_TenantAddAllowLockUpdates')
BEGIN
    ALTER TABLE [slk].[Tenants] ADD [AllowLockUpdates] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230523103418_TenantAddAllowLockUpdates')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230523103418_TenantAddAllowLockUpdates', N'7.0.5');
END;
GO

COMMIT;
GO

