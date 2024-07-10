
UPDATE slkmart.AuditTypes
SET
	Category = N'Remained Closed'
	,[Description] = N'Remained closed - user'
WHERE
	AuditType = 20;

UPDATE slkmart.AuditTypes
SET
	Category = N'Remained Closed'
	,[Description] = N'Remained closed - master card'
WHERE
	AuditType = 21;

UPDATE slkmart.AuditTypes
SET
	Category = N'Remained Closed'
	,[Description] = N'Remained closed - security sweep'
WHERE
	AuditType = 22;

UPDATE slkmart.AuditTypes
SET
	Category = N'Remained Closed'
	,[Description] = N'Remained closed - install mode'
WHERE
	AuditType = 23;

	UPDATE slkmart.AuditTypes
SET
	Category = N'Remained Closed'
	,[Description] = N'Remained closed - any registered user following security sweep'
WHERE
	AuditType = 24;

UPDATE slkmart.AuditTypes
SET
	Category = N'Remained Closed'
	,[Description] = N'Remained closed - unplanned re-open'
WHERE
	AuditType = 28;

UPDATE slkmart.AuditTypes
SET
	[Description] = N'Unplanned re-open - sensor indicates unexpected unlocked condition'
WHERE
	AuditType = 58;

UPDATE slkmart.AuditTypes
SET
	[Description] = N'Lock tampering may have occurred - no action taken by lock'
WHERE
	AuditType = 88;

IF NOT EXISTS (SELECT * FROM slkmart.AuditTypes WHERE AuditType = 68)
BEGIN
	INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (68, N'Locking', N'Unplanned re-lock - sensor indicates unexpected locked condition')
END;

GO