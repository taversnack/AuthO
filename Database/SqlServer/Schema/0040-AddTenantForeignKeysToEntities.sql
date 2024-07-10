BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230523113808_AddTenantForeignKeysToEntities')
BEGIN
    ALTER TABLE [slk].[CardCredentials] ADD CONSTRAINT [FK_CardCredentials_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230523113808_AddTenantForeignKeysToEntities')
BEGIN
    ALTER TABLE [slk].[CardHolders] ADD CONSTRAINT [FK_CardHolders_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230523113808_AddTenantForeignKeysToEntities')
BEGIN
    ALTER TABLE [slk].[Locations] ADD CONSTRAINT [FK_Locations_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230523113808_AddTenantForeignKeysToEntities')
BEGIN
    ALTER TABLE [slk].[LockerBankAdmins] ADD CONSTRAINT [FK_LockerBankAdmins_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230523113808_AddTenantForeignKeysToEntities')
BEGIN
    ALTER TABLE [slk].[LockerBankCardCredentials] ADD CONSTRAINT [FK_LockerBankCardCredentials_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230523113808_AddTenantForeignKeysToEntities')
BEGIN
    ALTER TABLE [slk].[LockerBanks] ADD CONSTRAINT [FK_LockerBanks_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230523113808_AddTenantForeignKeysToEntities')
BEGIN
    ALTER TABLE [slk].[LockerCardCredentials] ADD CONSTRAINT [FK_LockerCardCredentials_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230523113808_AddTenantForeignKeysToEntities')
BEGIN
    ALTER TABLE [slk].[Lockers] ADD CONSTRAINT [FK_Lockers_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230523113808_AddTenantForeignKeysToEntities')
BEGIN
    ALTER TABLE [slk].[Locks] ADD CONSTRAINT [FK_Locks_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230523113808_AddTenantForeignKeysToEntities')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230523113808_AddTenantForeignKeysToEntities', N'7.0.5');
END;
GO

COMMIT;
GO

