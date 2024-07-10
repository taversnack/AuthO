BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231215133737_AddsKioskAccessCodeTable')
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
END
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231215133737_AddsKioskAccessCodeTable')
BEGIN
    CREATE INDEX [IX_KioskAccessCodes_CardHolderId] ON [slk].[KioskAccessCodes] ([CardHolderId]);
END
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231215133737_AddsKioskAccessCodeTable')
BEGIN
    CREATE INDEX [IX_KioskAccessCodes_TenantId] ON [slk].[KioskAccessCodes] ([TenantId]);
END
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231215133737_AddsKioskAccessCodeTable')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20231215133737_AddsKioskAccessCodeTable', N'7.0.5');
END
GO

COMMIT;
GO