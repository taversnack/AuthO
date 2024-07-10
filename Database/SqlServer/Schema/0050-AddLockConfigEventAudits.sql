BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230606123810_AddLockConfigEventAudits')
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

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230606123810_AddLockConfigEventAudits')
BEGIN
    CREATE INDEX [IX_LockConfigEventAudits_TenantId] ON [slk].[LockConfigEventAudits] ([TenantId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230606123810_AddLockConfigEventAudits')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230606123810_AddLockConfigEventAudits', N'7.0.5');
END;
GO

COMMIT;
GO

