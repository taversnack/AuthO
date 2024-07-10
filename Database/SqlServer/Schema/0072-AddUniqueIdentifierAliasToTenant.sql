BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231012160640_AddsUniqueIdentifierAliasToTenants')
BEGIN  
    ALTER TABLE [slk].[Tenants] ADD [UniqueIdentifierAlias] nvarchar(256) NOT NULL DEFAULT N'Unique Identifier';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231012160640_AddsUniqueIdentifierAliasToTenants')
BEGIN  
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20231012160640_AddsUniqueIdentifierAliasToTenants', N'7.0.5');
END;
GO

COMMIT;
GO