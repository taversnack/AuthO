BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230511082336_CardCredentialSerialNumberTypeChangedHidNumberMaxLengthAdded')
BEGIN
    DROP INDEX [IX_CardCredentials_SerialNumber] ON [slk].[CardCredentials];
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[slk].[CardCredentials]') AND [c].[name] = N'SerialNumber');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [slk].[CardCredentials] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [slk].[CardCredentials] ALTER COLUMN [SerialNumber] char(16) NULL;
    EXEC(N'CREATE UNIQUE INDEX [IX_CardCredentials_SerialNumber] ON [slk].[CardCredentials] ([SerialNumber]) WHERE [SerialNumber] IS NOT NULL');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230511082336_CardCredentialSerialNumberTypeChangedHidNumberMaxLengthAdded')
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[slk].[CardCredentials]') AND [c].[name] = N'HidNumber');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [slk].[CardCredentials] DROP CONSTRAINT [' + @var1 + '];');
    EXEC(N'UPDATE [slk].[CardCredentials] SET [HidNumber] = N'''' WHERE [HidNumber] IS NULL');
    ALTER TABLE [slk].[CardCredentials] ALTER COLUMN [HidNumber] nvarchar(32) NOT NULL;
    ALTER TABLE [slk].[CardCredentials] ADD DEFAULT N'' FOR [HidNumber];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230511082336_CardCredentialSerialNumberTypeChangedHidNumberMaxLengthAdded')
BEGIN
    CREATE UNIQUE INDEX [IX_CardCredentials_HidNumber] ON [slk].[CardCredentials] ([HidNumber]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230511082336_CardCredentialSerialNumberTypeChangedHidNumberMaxLengthAdded')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230511082336_CardCredentialSerialNumberTypeChangedHidNumberMaxLengthAdded', N'7.0.5');
END;
GO

COMMIT;
GO

