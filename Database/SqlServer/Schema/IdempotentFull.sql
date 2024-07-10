IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    IF SCHEMA_ID(N'slk') IS NULL EXEC(N'CREATE SCHEMA [slk];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE TABLE [slk].[CardHolders] (
        [CardHolderId] uniqueidentifier NOT NULL,
        [FirstName] nvarchar(256) NOT NULL,
        [LastName] nvarchar(256) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [UniqueIdentifier] nvarchar(256) NOT NULL,
        [IsVerified] bit NOT NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_CardHolders] PRIMARY KEY ([CardHolderId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE TABLE [slk].[Locations] (
        [LocationId] uniqueidentifier NOT NULL,
        [Name] nvarchar(256) NOT NULL,
        [Description] nvarchar(256) NOT NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_Locations] PRIMARY KEY ([LocationId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE TABLE [slk].[StringifiedLockerBankBehaviour] (
        [Value] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_StringifiedLockerBankBehaviour] PRIMARY KEY ([Value])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE TABLE [slk].[StringifiedLockOperatingMode] (
        [Value] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_StringifiedLockOperatingMode] PRIMARY KEY ([Value])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE TABLE [slk].[Tenants] (
        [TenantId] uniqueidentifier NOT NULL,
        [Name] nvarchar(256) NOT NULL,
        [CardHolderAlias] nvarchar(256) NOT NULL,
        CONSTRAINT [PK_Tenants] PRIMARY KEY ([TenantId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE TABLE [slk].[CardCredentials] (
        [CardCredentialId] uniqueidentifier NOT NULL,
        [SerialNumber] varchar(16) NOT NULL,
        [CardType] int NOT NULL,
        [CardHolderId] uniqueidentifier NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_CardCredentials] PRIMARY KEY ([CardCredentialId]),
        CONSTRAINT [FK_CardCredentials_CardHolders_CardHolderId] FOREIGN KEY ([CardHolderId]) REFERENCES [slk].[CardHolders] ([CardHolderId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE TABLE [slk].[LockerBanks] (
        [LockerBankId] uniqueidentifier NOT NULL,
        [Name] nvarchar(256) NOT NULL,
        [Description] nvarchar(256) NOT NULL,
        [Behaviour] int NOT NULL,
        [DefaultLeaseDuration] time NULL,
        [LocationId] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_LockerBanks] PRIMARY KEY ([LockerBankId]),
        CONSTRAINT [FK_LockerBanks_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [slk].[Locations] ([LocationId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE TABLE [slk].[LockerBankAdmins] (
        [TenantId] uniqueidentifier NOT NULL,
        [LockerBankId] uniqueidentifier NOT NULL,
        [CardHolderId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_LockerBankAdmins] PRIMARY KEY ([TenantId], [LockerBankId], [CardHolderId]),
        CONSTRAINT [FK_LockerBankAdmins_CardHolders_CardHolderId] FOREIGN KEY ([CardHolderId]) REFERENCES [slk].[CardHolders] ([CardHolderId]) ON DELETE CASCADE,
        CONSTRAINT [FK_LockerBankAdmins_LockerBanks_LockerBankId] FOREIGN KEY ([LockerBankId]) REFERENCES [slk].[LockerBanks] ([LockerBankId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE TABLE [slk].[LockerBankCardCredentials] (
        [TenantId] uniqueidentifier NOT NULL,
        [LockerBankId] uniqueidentifier NOT NULL,
        [CardCredentialId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_LockerBankCardCredentials] PRIMARY KEY ([TenantId], [LockerBankId], [CardCredentialId]),
        CONSTRAINT [FK_LockerBankCardCredentials_CardCredentials_CardCredentialId] FOREIGN KEY ([CardCredentialId]) REFERENCES [slk].[CardCredentials] ([CardCredentialId]) ON DELETE CASCADE,
        CONSTRAINT [FK_LockerBankCardCredentials_LockerBanks_LockerBankId] FOREIGN KEY ([LockerBankId]) REFERENCES [slk].[LockerBanks] ([LockerBankId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE TABLE [slk].[Lockers] (
        [LockerId] uniqueidentifier NOT NULL,
        [Label] nvarchar(256) NOT NULL,
        [HasTenant] bit NOT NULL,
        [AbsoluteLeaseExpiry] datetime2 NULL,
        [LockerBankId] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_Lockers] PRIMARY KEY ([LockerId]),
        CONSTRAINT [FK_Lockers_LockerBanks_LockerBankId] FOREIGN KEY ([LockerBankId]) REFERENCES [slk].[LockerBanks] ([LockerBankId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE TABLE [slk].[LockerCardCredentials] (
        [TenantId] uniqueidentifier NOT NULL,
        [LockerId] uniqueidentifier NOT NULL,
        [CardCredentialId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_LockerCardCredentials] PRIMARY KEY ([TenantId], [LockerId], [CardCredentialId]),
        CONSTRAINT [FK_LockerCardCredentials_CardCredentials_CardCredentialId] FOREIGN KEY ([CardCredentialId]) REFERENCES [slk].[CardCredentials] ([CardCredentialId]) ON DELETE CASCADE,
        CONSTRAINT [FK_LockerCardCredentials_Lockers_LockerId] FOREIGN KEY ([LockerId]) REFERENCES [slk].[Lockers] ([LockerId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE TABLE [slk].[Locks] (
        [LockId] uniqueidentifier NOT NULL,
        [SerialNumber] int NOT NULL,
        [InstallationDateUtc] datetimeoffset NOT NULL,
        [FirmwareVersion] nvarchar(256) NOT NULL,
        [OperatingMode] int NOT NULL,
        [LockerId] uniqueidentifier NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_Locks] PRIMARY KEY ([LockId]),
        CONSTRAINT [FK_Locks_Lockers_LockerId] FOREIGN KEY ([LockerId]) REFERENCES [slk].[Lockers] ([LockerId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Value', N'Name') AND [object_id] = OBJECT_ID(N'[slk].[StringifiedLockOperatingMode]'))
        SET IDENTITY_INSERT [slk].[StringifiedLockOperatingMode] ON;
    EXEC(N'INSERT INTO [slk].[StringifiedLockOperatingMode] ([Value], [Name])
    VALUES (0, N''Installation''),
    (1, N''Shared''),
    (2, N''Dedicated''),
    (3, N''Confiscated''),
    (4, N''Reader'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Value', N'Name') AND [object_id] = OBJECT_ID(N'[slk].[StringifiedLockOperatingMode]'))
        SET IDENTITY_INSERT [slk].[StringifiedLockOperatingMode] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Value', N'Name') AND [object_id] = OBJECT_ID(N'[slk].[StringifiedLockerBankBehaviour]'))
        SET IDENTITY_INSERT [slk].[StringifiedLockerBankBehaviour] ON;
    EXEC(N'INSERT INTO [slk].[StringifiedLockerBankBehaviour] ([Value], [Name])
    VALUES (0, N''Unset''),
    (1, N''Permanent''),
    (2, N''Temporary'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Value', N'Name') AND [object_id] = OBJECT_ID(N'[slk].[StringifiedLockerBankBehaviour]'))
        SET IDENTITY_INSERT [slk].[StringifiedLockerBankBehaviour] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CardCredentials_CardHolderId] ON [slk].[CardCredentials] ([CardHolderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CardCredentials_SerialNumber] ON [slk].[CardCredentials] ([SerialNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CardCredentials_TenantId] ON [slk].[CardCredentials] ([TenantId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CardHolders_TenantId] ON [slk].[CardHolders] ([TenantId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CardHolders_TenantId_Email] ON [slk].[CardHolders] ([TenantId], [Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CardHolders_TenantId_UniqueIdentifier] ON [slk].[CardHolders] ([TenantId], [UniqueIdentifier]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Locations_TenantId] ON [slk].[Locations] ([TenantId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Locations_TenantId_Name] ON [slk].[Locations] ([TenantId], [Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LockerBankAdmins_CardHolderId] ON [slk].[LockerBankAdmins] ([CardHolderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LockerBankAdmins_LockerBankId] ON [slk].[LockerBankAdmins] ([LockerBankId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LockerBankAdmins_TenantId] ON [slk].[LockerBankAdmins] ([TenantId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LockerBankCardCredentials_CardCredentialId] ON [slk].[LockerBankCardCredentials] ([CardCredentialId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LockerBankCardCredentials_LockerBankId] ON [slk].[LockerBankCardCredentials] ([LockerBankId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LockerBankCardCredentials_TenantId] ON [slk].[LockerBankCardCredentials] ([TenantId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LockerBanks_LocationId] ON [slk].[LockerBanks] ([LocationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LockerBanks_TenantId] ON [slk].[LockerBanks] ([TenantId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_LockerBanks_TenantId_LocationId_Name] ON [slk].[LockerBanks] ([TenantId], [LocationId], [Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LockerCardCredentials_CardCredentialId] ON [slk].[LockerCardCredentials] ([CardCredentialId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LockerCardCredentials_LockerId] ON [slk].[LockerCardCredentials] ([LockerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LockerCardCredentials_TenantId] ON [slk].[LockerCardCredentials] ([TenantId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Lockers_LockerBankId] ON [slk].[Lockers] ([LockerBankId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Lockers_TenantId] ON [slk].[Lockers] ([TenantId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Lockers_TenantId_LockerBankId_Label] ON [slk].[Lockers] ([TenantId], [LockerBankId], [Label]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Locks_LockerId] ON [slk].[Locks] ([LockerId]) WHERE [LockerId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Locks_SerialNumber] ON [slk].[Locks] ([SerialNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Locks_TenantId] ON [slk].[Locks] ([TenantId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tenants_TenantId] ON [slk].[Tenants] ([TenantId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230419145909_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230419145909_InitialCreate', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230503124254_AddHidNumberToCardsAndRemovedConstraints'
)
BEGIN
    DROP INDEX [IX_CardHolders_TenantId_Email] ON [slk].[CardHolders];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230503124254_AddHidNumberToCardsAndRemovedConstraints'
)
BEGIN
    DROP INDEX [IX_CardHolders_TenantId_UniqueIdentifier] ON [slk].[CardHolders];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230503124254_AddHidNumberToCardsAndRemovedConstraints'
)
BEGIN
    DROP INDEX [IX_CardCredentials_SerialNumber] ON [slk].[CardCredentials];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230503124254_AddHidNumberToCardsAndRemovedConstraints'
)
BEGIN
    ALTER TABLE [slk].[Lockers] ADD [ServiceTag] nvarchar(32) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230503124254_AddHidNumberToCardsAndRemovedConstraints'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[slk].[CardHolders]') AND [c].[name] = N'UniqueIdentifier');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [slk].[CardHolders] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [slk].[CardHolders] ALTER COLUMN [UniqueIdentifier] nvarchar(256) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230503124254_AddHidNumberToCardsAndRemovedConstraints'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[slk].[CardHolders]') AND [c].[name] = N'Email');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [slk].[CardHolders] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [slk].[CardHolders] ALTER COLUMN [Email] nvarchar(256) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230503124254_AddHidNumberToCardsAndRemovedConstraints'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[slk].[CardCredentials]') AND [c].[name] = N'SerialNumber');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [slk].[CardCredentials] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [slk].[CardCredentials] ALTER COLUMN [SerialNumber] nvarchar(16) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230503124254_AddHidNumberToCardsAndRemovedConstraints'
)
BEGIN
    ALTER TABLE [slk].[CardCredentials] ADD [CardLabel] nvarchar(256) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230503124254_AddHidNumberToCardsAndRemovedConstraints'
)
BEGIN
    ALTER TABLE [slk].[CardCredentials] ADD [HidNumber] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230503124254_AddHidNumberToCardsAndRemovedConstraints'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Tenants_Name] ON [slk].[Tenants] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230503124254_AddHidNumberToCardsAndRemovedConstraints'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Lockers_TenantId_ServiceTag] ON [slk].[Lockers] ([TenantId], [ServiceTag]) WHERE [ServiceTag] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230503124254_AddHidNumberToCardsAndRemovedConstraints'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_CardHolders_TenantId_Email] ON [slk].[CardHolders] ([TenantId], [Email]) WHERE [Email] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230503124254_AddHidNumberToCardsAndRemovedConstraints'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_CardHolders_TenantId_UniqueIdentifier] ON [slk].[CardHolders] ([TenantId], [UniqueIdentifier]) WHERE [UniqueIdentifier] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230503124254_AddHidNumberToCardsAndRemovedConstraints'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_CardCredentials_SerialNumber] ON [slk].[CardCredentials] ([SerialNumber]) WHERE [SerialNumber] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230503124254_AddHidNumberToCardsAndRemovedConstraints'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230503124254_AddHidNumberToCardsAndRemovedConstraints', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230511082336_CardCredentialSerialNumberTypeChangedHidNumberMaxLengthAdded'
)
BEGIN
    DROP INDEX [IX_CardCredentials_SerialNumber] ON [slk].[CardCredentials];
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[slk].[CardCredentials]') AND [c].[name] = N'SerialNumber');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [slk].[CardCredentials] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [slk].[CardCredentials] ALTER COLUMN [SerialNumber] char(16) NULL;
    EXEC(N'CREATE UNIQUE INDEX [IX_CardCredentials_SerialNumber] ON [slk].[CardCredentials] ([SerialNumber]) WHERE [SerialNumber] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230511082336_CardCredentialSerialNumberTypeChangedHidNumberMaxLengthAdded'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[slk].[CardCredentials]') AND [c].[name] = N'HidNumber');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [slk].[CardCredentials] DROP CONSTRAINT [' + @var4 + '];');
    EXEC(N'UPDATE [slk].[CardCredentials] SET [HidNumber] = N'''' WHERE [HidNumber] IS NULL');
    ALTER TABLE [slk].[CardCredentials] ALTER COLUMN [HidNumber] nvarchar(32) NOT NULL;
    ALTER TABLE [slk].[CardCredentials] ADD DEFAULT N'' FOR [HidNumber];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230511082336_CardCredentialSerialNumberTypeChangedHidNumberMaxLengthAdded'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CardCredentials_HidNumber] ON [slk].[CardCredentials] ([HidNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230511082336_CardCredentialSerialNumberTypeChangedHidNumberMaxLengthAdded'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230511082336_CardCredentialSerialNumberTypeChangedHidNumberMaxLengthAdded', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230511131853_TenantAddLogo'
)
BEGIN
    ALTER TABLE [slk].[Tenants] ADD [Logo] varbinary(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230511131853_TenantAddLogo'
)
BEGIN
    ALTER TABLE [slk].[Tenants] ADD [LogoMimeType] nvarchar(256) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230511131853_TenantAddLogo'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230511131853_TenantAddLogo', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230523103418_TenantAddAllowLockUpdates'
)
BEGIN
    ALTER TABLE [slk].[Tenants] ADD [AllowLockUpdates] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230523103418_TenantAddAllowLockUpdates'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230523103418_TenantAddAllowLockUpdates', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230523113808_AddTenantForeignKeysToEntities'
)
BEGIN
    ALTER TABLE [slk].[CardCredentials] ADD CONSTRAINT [FK_CardCredentials_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230523113808_AddTenantForeignKeysToEntities'
)
BEGIN
    ALTER TABLE [slk].[CardHolders] ADD CONSTRAINT [FK_CardHolders_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230523113808_AddTenantForeignKeysToEntities'
)
BEGIN
    ALTER TABLE [slk].[Locations] ADD CONSTRAINT [FK_Locations_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230523113808_AddTenantForeignKeysToEntities'
)
BEGIN
    ALTER TABLE [slk].[LockerBankAdmins] ADD CONSTRAINT [FK_LockerBankAdmins_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230523113808_AddTenantForeignKeysToEntities'
)
BEGIN
    ALTER TABLE [slk].[LockerBankCardCredentials] ADD CONSTRAINT [FK_LockerBankCardCredentials_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230523113808_AddTenantForeignKeysToEntities'
)
BEGIN
    ALTER TABLE [slk].[LockerBanks] ADD CONSTRAINT [FK_LockerBanks_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230523113808_AddTenantForeignKeysToEntities'
)
BEGIN
    ALTER TABLE [slk].[LockerCardCredentials] ADD CONSTRAINT [FK_LockerCardCredentials_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230523113808_AddTenantForeignKeysToEntities'
)
BEGIN
    ALTER TABLE [slk].[Lockers] ADD CONSTRAINT [FK_Lockers_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230523113808_AddTenantForeignKeysToEntities'
)
BEGIN
    ALTER TABLE [slk].[Locks] ADD CONSTRAINT [FK_Locks_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230523113808_AddTenantForeignKeysToEntities'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230523113808_AddTenantForeignKeysToEntities', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230606123810_AddLockConfigEventAudits'
)
BEGIN
    CREATE TABLE [slk].[LockConfigEventAudits] (
        [LockConfigEventAuditId] uniqueidentifier NOT NULL,
        [EventType] int NOT NULL,
        [UpdatedByUserEmail] nvarchar(256) NOT NULL,
        [EntityId] uniqueidentifier NOT NULL,
        [CreatedAtUTC] datetimeoffset NOT NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_LockConfigEventAudits] PRIMARY KEY ([LockConfigEventAuditId]),
        CONSTRAINT [FK_LockConfigEventAudits_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230606123810_AddLockConfigEventAudits'
)
BEGIN
    CREATE INDEX [IX_LockConfigEventAudits_TenantId] ON [slk].[LockConfigEventAudits] ([TenantId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230606123810_AddLockConfigEventAudits'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230606123810_AddLockConfigEventAudits', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders'
)
BEGIN
    ALTER TABLE [slk].[Locks] DROP CONSTRAINT [FK_Locks_Lockers_LockerId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[slk].[Lockers]') AND [c].[name] = N'HasTenant');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [slk].[Lockers] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [slk].[Lockers] DROP COLUMN [HasTenant];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[slk].[Lockers]') AND [c].[name] = N'AbsoluteLeaseExpiry');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [slk].[Lockers] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [slk].[Lockers] ALTER COLUMN [AbsoluteLeaseExpiry] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders'
)
BEGIN
    ALTER TABLE [slk].[Lockers] ADD [CardHolderId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders'
)
BEGIN
    ALTER TABLE [slk].[Lockers] ADD [CurrentLeaseId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders'
)
BEGIN
    CREATE TABLE [slk].[LockerLeases] (
        [LockerLeaseId] uniqueidentifier NOT NULL,
        [StartedAt] datetimeoffset NULL,
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

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders'
)
BEGIN
    CREATE INDEX [IX_Lockers_CardHolderId] ON [slk].[Lockers] ([CardHolderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Lockers_CurrentLeaseId] ON [slk].[Lockers] ([CurrentLeaseId]) WHERE [CurrentLeaseId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders'
)
BEGIN
    CREATE INDEX [IX_LockerLeases_CardCredentialId] ON [slk].[LockerLeases] ([CardCredentialId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders'
)
BEGIN
    CREATE INDEX [IX_LockerLeases_CardHolderId] ON [slk].[LockerLeases] ([CardHolderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders'
)
BEGIN
    CREATE INDEX [IX_LockerLeases_LockerId] ON [slk].[LockerLeases] ([LockerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders'
)
BEGIN
    CREATE INDEX [IX_LockerLeases_LockId] ON [slk].[LockerLeases] ([LockId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders'
)
BEGIN
    CREATE INDEX [IX_LockerLeases_TenantId] ON [slk].[LockerLeases] ([TenantId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders'
)
BEGIN
    ALTER TABLE [slk].[Lockers] ADD CONSTRAINT [FK_Lockers_CardHolders_CardHolderId] FOREIGN KEY ([CardHolderId]) REFERENCES [slk].[CardHolders] ([CardHolderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders'
)
BEGIN
    ALTER TABLE [slk].[Lockers] ADD CONSTRAINT [FK_Lockers_LockerLeases_CurrentLeaseId] FOREIGN KEY ([CurrentLeaseId]) REFERENCES [slk].[LockerLeases] ([LockerLeaseId]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders'
)
BEGIN
    ALTER TABLE [slk].[Locks] ADD CONSTRAINT [FK_Locks_Lockers_LockerId] FOREIGN KEY ([LockerId]) REFERENCES [slk].[Lockers] ([LockerId]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230829173508_AddLockerLeaseHolders'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230829173508_AddLockerLeaseHolders', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230831120122_AddEndedByMasterCardFieldToLockerLeases'
)
BEGIN
    ALTER TABLE [slk].[LockerLeases] ADD [EndedByMasterCard] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230831120122_AddEndedByMasterCardFieldToLockerLeases'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230831120122_AddEndedByMasterCardFieldToLockerLeases', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades'
)
BEGIN
    EXEC sp_rename N'[slk].[LockerBankCardCredentials]', N'LockerBankSpecialCardCredentials';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades'
)
BEGIN
    EXEC sp_rename N'[slk].[Tenants].[CardHolderAlias]', N'CardHolderAliasSingular', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades'
)
BEGIN
    ALTER TABLE [slk].[Tenants] ADD [CardHolderAliasPlural] nvarchar(256) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades'
)
BEGIN
    ALTER TABLE [slk].[Tenants] ADD [HelpPortalUrl] nvarchar(1024) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades'
)
BEGIN
    ALTER TABLE [slk].[Lockers] ADD [SecurityType] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades'
)
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

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades'
)
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

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades'
)
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

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades'
)
BEGIN
    CREATE INDEX [IX_LockerBankLeaseUsers_CardHolderId] ON [slk].[LockerBankLeaseUsers] ([CardHolderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades'
)
BEGIN
    CREATE INDEX [IX_LockerBankLeaseUsers_LockerBankId] ON [slk].[LockerBankLeaseUsers] ([LockerBankId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades'
)
BEGIN
    CREATE INDEX [IX_LockerBankLeaseUsers_TenantId] ON [slk].[LockerBankLeaseUsers] ([TenantId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades'
)
BEGIN
    CREATE INDEX [IX_LockerBankUserCardCredentials_CardCredentialId] ON [slk].[LockerBankUserCardCredentials] ([CardCredentialId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades'
)
BEGIN
    CREATE INDEX [IX_LockerBankUserCardCredentials_LockerBankId] ON [slk].[LockerBankUserCardCredentials] ([LockerBankId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades'
)
BEGIN
    CREATE INDEX [IX_LockerBankUserCardCredentials_TenantId] ON [slk].[LockerBankUserCardCredentials] ([TenantId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades'
)
BEGIN
    CREATE INDEX [IX_LockerOwners_CardHolderId] ON [slk].[LockerOwners] ([CardHolderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades'
)
BEGIN
    CREATE INDEX [IX_LockerOwners_LockerId] ON [slk].[LockerOwners] ([LockerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades'
)
BEGIN
    CREATE INDEX [IX_LockerOwners_TenantId] ON [slk].[LockerOwners] ([TenantId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230901112148_ShiftLockerChangesAndOtherMinorUpgrades', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230918155140_AddedDescriptionToLockConfigAudit'
)
BEGIN
    ALTER TABLE [slk].[LockConfigEventAudits] ADD [AdditionalDescription] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230918155140_AddedDescriptionToLockConfigAudit'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230918155140_AddedDescriptionToLockConfigAudit', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231012160640_AddsUniqueIdentifierAliasToTenants'
)
BEGIN
    ALTER TABLE [slk].[Tenants] ADD [UniqueIdentifierAlias] nvarchar(256) NOT NULL DEFAULT N'Unique Identifier';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231012160640_AddsUniqueIdentifierAliasToTenants'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20231012160640_AddsUniqueIdentifierAliasToTenants', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships'
)
BEGIN
    EXEC sp_rename N'[slk].[Tenants].[UniqueIdentifierAlias]', N'CardHolderUniqueIdentifierAlias', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships'
)
BEGIN
    ALTER TABLE [slk].[LockerBanks] ADD [CurrentReferenceImageId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships'
)
BEGIN
    ALTER TABLE [slk].[Locations] ADD [CurrentReferenceImageId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships'
)
BEGIN
    CREATE TABLE [slk].[LocationReferenceImages] (
        [Id] uniqueidentifier NOT NULL,
        [LocationId] uniqueidentifier NULL,
        [MetaData_FileName] nvarchar(512) NOT NULL,
        [MetaData_AzureBlobName] nvarchar(max) NULL,
        [MetaData_PixelWidth] int NOT NULL,
        [MetaData_PixelHeight] int NOT NULL,
        [MetaData_ByteSize] int NOT NULL,
        [MetaData_MimeType] nvarchar(256) NOT NULL,
        [MetaData_UploadedDate] datetimeoffset NOT NULL,
        [MetaData_UploadedByCardHolderEmail] nvarchar(256) NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_LocationReferenceImages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LocationReferenceImages_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [slk].[Locations] ([LocationId]) ON DELETE SET NULL,
        CONSTRAINT [FK_LocationReferenceImages_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships'
)
BEGIN
    CREATE TABLE [slk].[LockerBankReferenceImages] (
        [Id] uniqueidentifier NOT NULL,
        [LockerBankId] uniqueidentifier NULL,
        [MetaData_FileName] nvarchar(512) NOT NULL,
        [MetaData_AzureBlobName] nvarchar(max) NULL,
        [MetaData_PixelWidth] int NOT NULL,
        [MetaData_PixelHeight] int NOT NULL,
        [MetaData_ByteSize] int NOT NULL,
        [MetaData_MimeType] nvarchar(256) NOT NULL,
        [MetaData_UploadedDate] datetimeoffset NOT NULL,
        [MetaData_UploadedByCardHolderEmail] nvarchar(256) NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_LockerBankReferenceImages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LockerBankReferenceImages_LockerBanks_LockerBankId] FOREIGN KEY ([LockerBankId]) REFERENCES [slk].[LockerBanks] ([LockerBankId]) ON DELETE SET NULL,
        CONSTRAINT [FK_LockerBankReferenceImages_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_LockerBanks_CurrentReferenceImageId] ON [slk].[LockerBanks] ([CurrentReferenceImageId]) WHERE [CurrentReferenceImageId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Locations_CurrentReferenceImageId] ON [slk].[Locations] ([CurrentReferenceImageId]) WHERE [CurrentReferenceImageId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships'
)
BEGIN
    CREATE INDEX [IX_LocationReferenceImages_LocationId] ON [slk].[LocationReferenceImages] ([LocationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships'
)
BEGIN
    CREATE INDEX [IX_LocationReferenceImages_TenantId] ON [slk].[LocationReferenceImages] ([TenantId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships'
)
BEGIN
    CREATE INDEX [IX_LockerBankReferenceImages_LockerBankId] ON [slk].[LockerBankReferenceImages] ([LockerBankId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships'
)
BEGIN
    CREATE INDEX [IX_LockerBankReferenceImages_TenantId] ON [slk].[LockerBankReferenceImages] ([TenantId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships'
)
BEGIN
    ALTER TABLE [slk].[Locations] ADD CONSTRAINT [FK_Locations_LocationReferenceImages_CurrentReferenceImageId] FOREIGN KEY ([CurrentReferenceImageId]) REFERENCES [slk].[LocationReferenceImages] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships'
)
BEGIN
    ALTER TABLE [slk].[LockerBanks] ADD CONSTRAINT [FK_LockerBanks_LockerBankReferenceImages_CurrentReferenceImageId] FOREIGN KEY ([CurrentReferenceImageId]) REFERENCES [slk].[LockerBankReferenceImages] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20231026190928_AddsReferenceImageTablesAndRelationships', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231215133737_AddsKioskAccessCodeTable'
)
BEGIN
    CREATE TABLE [slk].[KioskAccessCodes] (
        [KioskAccessCodeId] uniqueidentifier NOT NULL,
        [AccessCode] nvarchar(max) NOT NULL,
        [HasBeenUsed] bit NOT NULL DEFAULT CAST(0 AS bit),
        [ExpiryDate] datetime2 NOT NULL,
        [CardHolderId] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_KioskAccessCodes] PRIMARY KEY ([KioskAccessCodeId]),
        CONSTRAINT [FK_KioskAccessCodes_CardHolders_CardHolderId] FOREIGN KEY ([CardHolderId]) REFERENCES [slk].[CardHolders] ([CardHolderId]) ON DELETE CASCADE,
        CONSTRAINT [FK_KioskAccessCodes_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231215133737_AddsKioskAccessCodeTable'
)
BEGIN
    CREATE INDEX [IX_KioskAccessCodes_CardHolderId] ON [slk].[KioskAccessCodes] ([CardHolderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231215133737_AddsKioskAccessCodeTable'
)
BEGIN
    CREATE INDEX [IX_KioskAccessCodes_TenantId] ON [slk].[KioskAccessCodes] ([TenantId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231215133737_AddsKioskAccessCodeTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20231215133737_AddsKioskAccessCodeTable', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240115104657_AddsTerminationFieldsToCardHolder'
)
BEGIN
    ALTER TABLE [slk].[KioskAccessCodes] DROP CONSTRAINT [FK_KioskAccessCodes_Tenants_TenantId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240115104657_AddsTerminationFieldsToCardHolder'
)
BEGIN
    ALTER TABLE [slk].[CardHolders] ADD [IsTerminated] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240115104657_AddsTerminationFieldsToCardHolder'
)
BEGIN
    ALTER TABLE [slk].[CardHolders] ADD [TerminationDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240115104657_AddsTerminationFieldsToCardHolder'
)
BEGIN
    ALTER TABLE [slk].[KioskAccessCodes] ADD CONSTRAINT [FK_KioskAccessCodes_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240115104657_AddsTerminationFieldsToCardHolder'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240115104657_AddsTerminationFieldsToCardHolder', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240304101449_AddsKioskLockerAssignmentTable'
)
BEGIN
    CREATE TABLE [slk].[KioskLockerAssignments] (
        [KioskLockerAssignmentId] uniqueidentifier NOT NULL,
        [LockerId] uniqueidentifier NULL,
        [CardHolderId] uniqueidentifier NULL,
        [TemporaryCardCredentialId] uniqueidentifier NULL,
        [ReplacedCardCredentialId] uniqueidentifier NULL,
        [AssignmentDate] datetimeoffset NOT NULL,
        [IsTemporaryCardActive] bit NOT NULL DEFAULT CAST(0 AS bit),
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_KioskLockerAssignments] PRIMARY KEY ([KioskLockerAssignmentId]),
        CONSTRAINT [FK_KioskLockerAssignments_CardCredentials_ReplacedCardCredentialId] FOREIGN KEY ([ReplacedCardCredentialId]) REFERENCES [slk].[CardCredentials] ([CardCredentialId]) ON DELETE SET NULL,
        CONSTRAINT [FK_KioskLockerAssignments_CardHolders_CardHolderId] FOREIGN KEY ([CardHolderId]) REFERENCES [slk].[CardHolders] ([CardHolderId]) ON DELETE SET NULL,
        CONSTRAINT [FK_KioskLockerAssignments_Lockers_LockerId] FOREIGN KEY ([LockerId]) REFERENCES [slk].[Lockers] ([LockerId]) ON DELETE SET NULL,
        CONSTRAINT [FK_KioskLockerAssignments_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [slk].[Tenants] ([TenantId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240304101449_AddsKioskLockerAssignmentTable'
)
BEGIN
    CREATE INDEX [IX_KioskLockerAssignments_CardHolderId] ON [slk].[KioskLockerAssignments] ([CardHolderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240304101449_AddsKioskLockerAssignmentTable'
)
BEGIN
    CREATE INDEX [IX_KioskLockerAssignments_LockerId] ON [slk].[KioskLockerAssignments] ([LockerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240304101449_AddsKioskLockerAssignmentTable'
)
BEGIN
    CREATE INDEX [IX_KioskLockerAssignments_ReplacedCardCredentialId] ON [slk].[KioskLockerAssignments] ([ReplacedCardCredentialId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240304101449_AddsKioskLockerAssignmentTable'
)
BEGIN
    CREATE INDEX [IX_KioskLockerAssignments_TenantId] ON [slk].[KioskLockerAssignments] ([TenantId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240304101449_AddsKioskLockerAssignmentTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240304101449_AddsKioskLockerAssignmentTable', N'8.0.4');
END;
GO

COMMIT;
GO

