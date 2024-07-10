BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918155140_AddedDescriptionToLockConfigAudit')
BEGIN
    ALTER TABLE [slk].[LockConfigEventAudits] ADD [AdditionalDescription] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918155140_AddedDescriptionToLockConfigAudit')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230918155140_AddedDescriptionToLockConfigAudit', N'7.0.5');
END;
GO

COMMIT;
GO

