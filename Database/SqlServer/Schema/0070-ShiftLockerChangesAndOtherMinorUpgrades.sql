BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades')
BEGIN
    EXEC sp_rename N'[slk].[LockerBankCardCredentials]', N'LockerBankSpecialCardCredentials';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades')
BEGIN
    EXEC sp_rename N'[slk].[Tenants].[CardHolderAlias]', N'CardHolderAliasSingular', N'COLUMN';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades')
BEGIN
    ALTER TABLE [slk].[Tenants] ADD [CardHolderAliasPlural] nvarchar(256) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades')
BEGIN
    ALTER TABLE [slk].[Tenants] ADD [HelpPortalUrl] nvarchar(1024) NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades')
BEGIN
    ALTER TABLE [slk].[Lockers] ADD [SecurityType] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades')
BEGIN
    CREATE TABLE [slk].[LockerBankLeaseUsers] (
        [TenantId] uniqueidentifier NOT NULL,
        [LockerBankId] uniqueidentifier NOT NULL,
        [CardHolderId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_LockerBankLeaseUsers] PRIMARY KEY ([TenantId], [LockerBankId], [CardHolderId]),
        CONSTRAINT [FK_LockerBankLeaseUsers_CardHolders_CardHolderId] FOREIGN KEY ([CardHolderId]) REFERENCES [slk].[CardHolders] ([CardHolderId]) ON DELETE CASCADE,
        CONSTRAINT [FK_LockerBankLeaseUsers_LockerBanks_LockerBankId] FOREIGN KEY ([LockerBankId]) REFERENCES [slk].[LockerBanks] ([LockerBankId]) ON DELETE CASCADE,
        CONSTRAINT [FK_LockerBankLeaseUsers_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades')
BEGIN
    CREATE TABLE [slk].[LockerBankUserCardCredentials] (
        [TenantId] uniqueidentifier NOT NULL,
        [LockerBankId] uniqueidentifier NOT NULL,
        [CardCredentialId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_LockerBankUserCardCredentials] PRIMARY KEY ([TenantId], [LockerBankId], [CardCredentialId]),
        CONSTRAINT [FK_LockerBankUserCardCredentials_CardCredentials_CardCredentialId] FOREIGN KEY ([CardCredentialId]) REFERENCES [slk].[CardCredentials] ([CardCredentialId]) ON DELETE CASCADE,
        CONSTRAINT [FK_LockerBankUserCardCredentials_LockerBanks_LockerBankId] FOREIGN KEY ([LockerBankId]) REFERENCES [slk].[LockerBanks] ([LockerBankId]) ON DELETE CASCADE,
        CONSTRAINT [FK_LockerBankUserCardCredentials_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades')
BEGIN
    CREATE TABLE [slk].[LockerOwners] (
        [TenantId] uniqueidentifier NOT NULL,
        [LockerId] uniqueidentifier NOT NULL,
        [CardHolderId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_LockerOwners] PRIMARY KEY ([TenantId], [LockerId], [CardHolderId]),
        CONSTRAINT [FK_LockerOwners_CardHolders_CardHolderId] FOREIGN KEY ([CardHolderId]) REFERENCES [slk].[CardHolders] ([CardHolderId]) ON DELETE CASCADE,
        CONSTRAINT [FK_LockerOwners_Lockers_LockerId] FOREIGN KEY ([LockerId]) REFERENCES [slk].[Lockers] ([LockerId]) ON DELETE CASCADE,
        CONSTRAINT [FK_LockerOwners_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades')
BEGIN
    CREATE INDEX [IX_LockerBankLeaseUsers_CardHolderId] ON [slk].[LockerBankLeaseUsers] ([CardHolderId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades')
BEGIN
    CREATE INDEX [IX_LockerBankLeaseUsers_LockerBankId] ON [slk].[LockerBankLeaseUsers] ([LockerBankId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades')
BEGIN
    CREATE INDEX [IX_LockerBankLeaseUsers_TenantId] ON [slk].[LockerBankLeaseUsers] ([TenantId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades')
BEGIN
    CREATE INDEX [IX_LockerBankUserCardCredentials_CardCredentialId] ON [slk].[LockerBankUserCardCredentials] ([CardCredentialId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades')
BEGIN
    CREATE INDEX [IX_LockerBankUserCardCredentials_LockerBankId] ON [slk].[LockerBankUserCardCredentials] ([LockerBankId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades')
BEGIN
    CREATE INDEX [IX_LockerBankUserCardCredentials_TenantId] ON [slk].[LockerBankUserCardCredentials] ([TenantId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades')
BEGIN
    CREATE INDEX [IX_LockerOwners_CardHolderId] ON [slk].[LockerOwners] ([CardHolderId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades')
BEGIN
    CREATE INDEX [IX_LockerOwners_LockerId] ON [slk].[LockerOwners] ([LockerId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades')
BEGIN
    CREATE INDEX [IX_LockerOwners_TenantId] ON [slk].[LockerOwners] ([TenantId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades', N'7.0.5');
END;
GO

COMMIT;
GO

