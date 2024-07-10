BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230511131853_TenantAddLogo')
BEGIN
    ALTER TABLE [slk].[Tenants] ADD [Logo] varbinary(max) NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230511131853_TenantAddLogo')
BEGIN
    ALTER TABLE [slk].[Tenants] ADD [LogoMimeType] nvarchar(256) NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230511131853_TenantAddLogo')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230511131853_TenantAddLogo', N'7.0.5');
END;
GO

COMMIT;
GO

