BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders')
BEGIN
    ALTER TABLE [slk].[Locks] DROP CONSTRAINT [FK_Locks_Lockers_LockerId];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders')
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[slk].[Lockers]') AND [c].[name] = N'HasTenant');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [slk].[Lockers] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [slk].[Lockers] DROP COLUMN [HasTenant];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders')
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[slk].[Lockers]') AND [c].[name] = N'AbsoluteLeaseExpiry');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [slk].[Lockers] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [slk].[Lockers] ALTER COLUMN [AbsoluteLeaseExpiry] datetimeoffset NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders')
BEGIN
    ALTER TABLE [slk].[Lockers] ADD [CardHolderId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders')
BEGIN
    ALTER TABLE [slk].[Lockers] ADD [CurrentLeaseId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders')
BEGIN
    CREATE TABLE [slk].[LockerLeases] (
        [LockerLeaseId] uniqueidentifier NOT NULL,
        [StartedAt] datetimeoffset NOT NULL,
        [EndedAt] datetimeoffset NULL,
        [LockerBankBehaviour] int NOT NULL,
        [CardCredentialId] uniqueidentifier NULL,
        [CardHolderId] uniqueidentifier NULL,
        [LockerId] uniqueidentifier NULL,
        [LockId] uniqueidentifier NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_LockerLeases] PRIMARY KEY ([LockerLeaseId]),
        CONSTRAINT [FK_LockerLeases_CardCredentials_CardCredentialId] FOREIGN KEY ([CardCredentialId]) REFERENCES [slk].[CardCredentials] ([CardCredentialId]) ON DELETE SET NULL,
        CONSTRAINT [FK_LockerLeases_CardHolders_CardHolderId] FOREIGN KEY ([CardHolderId]) REFERENCES [slk].[CardHolders] ([CardHolderId]) ON DELETE SET NULL,
        CONSTRAINT [FK_LockerLeases_Lockers_LockerId] FOREIGN KEY ([LockerId]) REFERENCES [slk].[Lockers] ([LockerId]) ON DELETE SET NULL,
        CONSTRAINT [FK_LockerLeases_Locks_LockId] FOREIGN KEY ([LockId]) REFERENCES [slk].[Locks] ([LockId]) ON DELETE SET NULL,
        CONSTRAINT [FK_LockerLeases_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders')
BEGIN
    CREATE INDEX [IX_Lockers_CardHolderId] ON [slk].[Lockers] ([CardHolderId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders')
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Lockers_CurrentLeaseId] ON [slk].[Lockers] ([CurrentLeaseId]) WHERE [CurrentLeaseId] IS NOT NULL');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders')
BEGIN
    CREATE INDEX [IX_LockerLeases_CardCredentialId] ON [slk].[LockerLeases] ([CardCredentialId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders')
BEGIN
    CREATE INDEX [IX_LockerLeases_CardHolderId] ON [slk].[LockerLeases] ([CardHolderId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders')
BEGIN
    CREATE INDEX [IX_LockerLeases_LockerId] ON [slk].[LockerLeases] ([LockerId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders')
BEGIN
    CREATE INDEX [IX_LockerLeases_LockId] ON [slk].[LockerLeases] ([LockId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders')
BEGIN
    CREATE INDEX [IX_LockerLeases_TenantId] ON [slk].[LockerLeases] ([TenantId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders')
BEGIN
    ALTER TABLE [slk].[Lockers] ADD CONSTRAINT [FK_Lockers_CardHolders_CardHolderId] FOREIGN KEY ([CardHolderId]) REFERENCES [slk].[CardHolders] ([CardHolderId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders')
BEGIN
    ALTER TABLE [slk].[Lockers] ADD CONSTRAINT [FK_Lockers_LockerLeases_CurrentLeaseId] FOREIGN KEY ([CurrentLeaseId]) REFERENCES [slk].[LockerLeases] ([LockerLeaseId]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders')
BEGIN
    ALTER TABLE [slk].[Locks] ADD CONSTRAINT [FK_Locks_Lockers_LockerId] FOREIGN KEY ([LockerId]) REFERENCES [slk].[Lockers] ([LockerId]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230829173508_AddLockerLeaseHolders', N'7.0.5');
END;
GO

COMMIT;
GO

