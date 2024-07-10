BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240304101449_AddsKioskLockerAssignmentTable')
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
END
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240304101449_AddsKioskLockerAssignmentTable')
BEGIN
	CREATE INDEX [IX_KioskLockerAssignments_CardHolderId] ON [slk].[KioskLockerAssignments] ([CardHolderId]);
END
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240304101449_AddsKioskLockerAssignmentTable')
BEGIN
	CREATE INDEX [IX_KioskLockerAssignments_LockerId] ON [slk].[KioskLockerAssignments] ([LockerId]);
END
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240304101449_AddsKioskLockerAssignmentTable')
BEGIN
	CREATE INDEX [IX_KioskLockerAssignments_ReplacedCardCredentialId] ON [slk].[KioskLockerAssignments] ([ReplacedCardCredentialId]);
END
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240304101449_AddsKioskLockerAssignmentTable')
BEGIN
	CREATE INDEX [IX_KioskLockerAssignments_TenantId] ON [slk].[KioskLockerAssignments] ([TenantId]);
END
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240304101449_AddsKioskLockerAssignmentTable')
BEGIN
	INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
	VALUES (N'20240304101449_AddsKioskLockerAssignmentTable', N'7.0.5');
END
GO

COMMIT;
GO