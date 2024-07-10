BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships')
BEGIN
    EXEC sp_rename N'[slk].[Tenants].[UniqueIdentifierAlias]', N'CardHolderUniqueIdentifierAlias', N'COLUMN';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships')
BEGIN
    ALTER TABLE [slk].[LockerBanks] ADD [CurrentReferenceImageId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships')
BEGIN
    ALTER TABLE [slk].[Locations] ADD [CurrentReferenceImageId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships')
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

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships')
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

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships')
BEGIN
    CREATE UNIQUE INDEX [IX_LockerBanks_CurrentReferenceImageId] ON [slk].[LockerBanks] ([CurrentReferenceImageId]) WHERE [CurrentReferenceImageId] IS NOT NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships')
BEGIN
    CREATE UNIQUE INDEX [IX_Locations_CurrentReferenceImageId] ON [slk].[Locations] ([CurrentReferenceImageId]) WHERE [CurrentReferenceImageId] IS NOT NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships')
BEGIN
    CREATE INDEX [IX_LocationReferenceImages_LocationId] ON [slk].[LocationReferenceImages] ([LocationId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships')
BEGIN
    CREATE INDEX [IX_LocationReferenceImages_TenantId] ON [slk].[LocationReferenceImages] ([TenantId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships')
BEGIN
    CREATE INDEX [IX_LockerBankReferenceImages_LockerBankId] ON [slk].[LockerBankReferenceImages] ([LockerBankId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships')
BEGIN
    CREATE INDEX [IX_LockerBankReferenceImages_TenantId] ON [slk].[LockerBankReferenceImages] ([TenantId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships')
BEGIN
    ALTER TABLE [slk].[Locations] ADD CONSTRAINT [FK_Locations_LocationReferenceImages_CurrentReferenceImageId] FOREIGN KEY ([CurrentReferenceImageId]) REFERENCES [slk].[LocationReferenceImages] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships')
BEGIN
    ALTER TABLE [slk].[LockerBanks] ADD CONSTRAINT [FK_LockerBanks_LockerBankReferenceImages_CurrentReferenceImageId] FOREIGN KEY ([CurrentReferenceImageId]) REFERENCES [slk].[LockerBankReferenceImages] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231026190928_AddsReferenceImageTablesAndRelationships')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20231026190928_AddsReferenceImageTablesAndRelationships', N'7.0.5');
END;
GO

COMMIT;
GO