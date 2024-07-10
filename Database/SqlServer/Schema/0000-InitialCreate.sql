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

IF SCHEMA_ID(N'slk') IS NULL EXEC(N'CREATE SCHEMA [slk];');
GO

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
GO

CREATE TABLE [slk].[Locations] (
    [LocationId] uniqueidentifier NOT NULL,
    [Name] nvarchar(256) NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_Locations] PRIMARY KEY ([LocationId])
);
GO

CREATE TABLE [slk].[StringifiedLockerBankBehaviour] (
    [Value] int NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_StringifiedLockerBankBehaviour] PRIMARY KEY ([Value])
);
GO

CREATE TABLE [slk].[StringifiedLockOperatingMode] (
    [Value] int NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_StringifiedLockOperatingMode] PRIMARY KEY ([Value])
);
GO

CREATE TABLE [slk].[Tenants] (
    [TenantId] uniqueidentifier NOT NULL,
    [Name] nvarchar(256) NOT NULL,
    [CardHolderAlias] nvarchar(256) NOT NULL,
    CONSTRAINT [PK_Tenants] PRIMARY KEY ([TenantId])
);
GO

CREATE TABLE [slk].[CardCredentials] (
    [CardCredentialId] uniqueidentifier NOT NULL,
    [SerialNumber] varchar(16) NOT NULL,
    [CardType] int NOT NULL,
    [CardHolderId] uniqueidentifier NULL,
    [TenantId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_CardCredentials] PRIMARY KEY ([CardCredentialId]),
    CONSTRAINT [FK_CardCredentials_CardHolders_CardHolderId] FOREIGN KEY ([CardHolderId]) REFERENCES [slk].[CardHolders] ([CardHolderId])
);
GO

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
GO

CREATE TABLE [slk].[LockerBankAdmins] (
    [TenantId] uniqueidentifier NOT NULL,
    [LockerBankId] uniqueidentifier NOT NULL,
    [CardHolderId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_LockerBankAdmins] PRIMARY KEY ([TenantId], [LockerBankId], [CardHolderId]),
    CONSTRAINT [FK_LockerBankAdmins_CardHolders_CardHolderId] FOREIGN KEY ([CardHolderId]) REFERENCES [slk].[CardHolders] ([CardHolderId]) ON DELETE CASCADE,
    CONSTRAINT [FK_LockerBankAdmins_LockerBanks_LockerBankId] FOREIGN KEY ([LockerBankId]) REFERENCES [slk].[LockerBanks] ([LockerBankId]) ON DELETE CASCADE
);
GO

CREATE TABLE [slk].[LockerBankCardCredentials] (
    [TenantId] uniqueidentifier NOT NULL,
    [LockerBankId] uniqueidentifier NOT NULL,
    [CardCredentialId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_LockerBankCardCredentials] PRIMARY KEY ([TenantId], [LockerBankId], [CardCredentialId]),
    CONSTRAINT [FK_LockerBankCardCredentials_CardCredentials_CardCredentialId] FOREIGN KEY ([CardCredentialId]) REFERENCES [slk].[CardCredentials] ([CardCredentialId]) ON DELETE CASCADE,
    CONSTRAINT [FK_LockerBankCardCredentials_LockerBanks_LockerBankId] FOREIGN KEY ([LockerBankId]) REFERENCES [slk].[LockerBanks] ([LockerBankId]) ON DELETE CASCADE
);
GO

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
GO

CREATE TABLE [slk].[LockerCardCredentials] (
    [TenantId] uniqueidentifier NOT NULL,
    [LockerId] uniqueidentifier NOT NULL,
    [CardCredentialId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_LockerCardCredentials] PRIMARY KEY ([TenantId], [LockerId], [CardCredentialId]),
    CONSTRAINT [FK_LockerCardCredentials_CardCredentials_CardCredentialId] FOREIGN KEY ([CardCredentialId]) REFERENCES [slk].[CardCredentials] ([CardCredentialId]) ON DELETE CASCADE,
    CONSTRAINT [FK_LockerCardCredentials_Lockers_LockerId] FOREIGN KEY ([LockerId]) REFERENCES [slk].[Lockers] ([LockerId]) ON DELETE CASCADE
);
GO

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
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Value', N'Name') AND [object_id] = OBJECT_ID(N'[slk].[StringifiedLockOperatingMode]'))
    SET IDENTITY_INSERT [slk].[StringifiedLockOperatingMode] ON;
INSERT INTO [slk].[StringifiedLockOperatingMode] ([Value], [Name])
VALUES (0, N'Installation'),
(1, N'Shared'),
(2, N'Dedicated'),
(3, N'Confiscated'),
(4, N'Reader');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Value', N'Name') AND [object_id] = OBJECT_ID(N'[slk].[StringifiedLockOperatingMode]'))
    SET IDENTITY_INSERT [slk].[StringifiedLockOperatingMode] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Value', N'Name') AND [object_id] = OBJECT_ID(N'[slk].[StringifiedLockerBankBehaviour]'))
    SET IDENTITY_INSERT [slk].[StringifiedLockerBankBehaviour] ON;
INSERT INTO [slk].[StringifiedLockerBankBehaviour] ([Value], [Name])
VALUES (0, N'Unset'),
(1, N'Permanent'),
(2, N'Temporary');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Value', N'Name') AND [object_id] = OBJECT_ID(N'[slk].[StringifiedLockerBankBehaviour]'))
    SET IDENTITY_INSERT [slk].[StringifiedLockerBankBehaviour] OFF;
GO

CREATE INDEX [IX_CardCredentials_CardHolderId] ON [slk].[CardCredentials] ([CardHolderId]);
GO

CREATE UNIQUE INDEX [IX_CardCredentials_SerialNumber] ON [slk].[CardCredentials] ([SerialNumber]);
GO

CREATE INDEX [IX_CardCredentials_TenantId] ON [slk].[CardCredentials] ([TenantId]);
GO

CREATE INDEX [IX_CardHolders_TenantId] ON [slk].[CardHolders] ([TenantId]);
GO

CREATE UNIQUE INDEX [IX_CardHolders_TenantId_Email] ON [slk].[CardHolders] ([TenantId], [Email]);
GO

CREATE UNIQUE INDEX [IX_CardHolders_TenantId_UniqueIdentifier] ON [slk].[CardHolders] ([TenantId], [UniqueIdentifier]);
GO

CREATE INDEX [IX_Locations_TenantId] ON [slk].[Locations] ([TenantId]);
GO

CREATE UNIQUE INDEX [IX_Locations_TenantId_Name] ON [slk].[Locations] ([TenantId], [Name]);
GO

CREATE INDEX [IX_LockerBankAdmins_CardHolderId] ON [slk].[LockerBankAdmins] ([CardHolderId]);
GO

CREATE INDEX [IX_LockerBankAdmins_LockerBankId] ON [slk].[LockerBankAdmins] ([LockerBankId]);
GO

CREATE INDEX [IX_LockerBankAdmins_TenantId] ON [slk].[LockerBankAdmins] ([TenantId]);
GO

CREATE INDEX [IX_LockerBankCardCredentials_CardCredentialId] ON [slk].[LockerBankCardCredentials] ([CardCredentialId]);
GO

CREATE INDEX [IX_LockerBankCardCredentials_LockerBankId] ON [slk].[LockerBankCardCredentials] ([LockerBankId]);
GO

CREATE INDEX [IX_LockerBankCardCredentials_TenantId] ON [slk].[LockerBankCardCredentials] ([TenantId]);
GO

CREATE INDEX [IX_LockerBanks_LocationId] ON [slk].[LockerBanks] ([LocationId]);
GO

CREATE INDEX [IX_LockerBanks_TenantId] ON [slk].[LockerBanks] ([TenantId]);
GO

CREATE UNIQUE INDEX [IX_LockerBanks_TenantId_LocationId_Name] ON [slk].[LockerBanks] ([TenantId], [LocationId], [Name]);
GO

CREATE INDEX [IX_LockerCardCredentials_CardCredentialId] ON [slk].[LockerCardCredentials] ([CardCredentialId]);
GO

CREATE INDEX [IX_LockerCardCredentials_LockerId] ON [slk].[LockerCardCredentials] ([LockerId]);
GO

CREATE INDEX [IX_LockerCardCredentials_TenantId] ON [slk].[LockerCardCredentials] ([TenantId]);
GO

CREATE INDEX [IX_Lockers_LockerBankId] ON [slk].[Lockers] ([LockerBankId]);
GO

CREATE INDEX [IX_Lockers_TenantId] ON [slk].[Lockers] ([TenantId]);
GO

CREATE UNIQUE INDEX [IX_Lockers_TenantId_LockerBankId_Label] ON [slk].[Lockers] ([TenantId], [LockerBankId], [Label]);
GO

CREATE UNIQUE INDEX [IX_Locks_LockerId] ON [slk].[Locks] ([LockerId]) WHERE [LockerId] IS NOT NULL;
GO

CREATE UNIQUE INDEX [IX_Locks_SerialNumber] ON [slk].[Locks] ([SerialNumber]);
GO

CREATE INDEX [IX_Locks_TenantId] ON [slk].[Locks] ([TenantId]);
GO

CREATE INDEX [IX_Tenants_TenantId] ON [slk].[Tenants] ([TenantId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230419145909_InitialCreate', N'7.0.5');
GO

COMMIT;
GO

