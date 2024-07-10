BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230831120122_AddEndedByMasterCardFieldToLockerLeases')
BEGIN
    ALTER TABLE [slk].[LockerLeases] ADD [EndedByMasterCard] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230831120122_AddEndedByMasterCardFieldToLockerLeases')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230831120122_AddEndedByMasterCardFieldToLockerLeases', N'7.0.5');
END;
GO

COMMIT;
GO

