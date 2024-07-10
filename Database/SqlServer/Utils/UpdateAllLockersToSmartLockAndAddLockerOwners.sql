UPDATE [slk].[Lockers]
	SET [SecurityType] = 3
GO

INSERT INTO [slk].[LockerOwners] ([TenantId]
	,[LockerId]
	,[CardHolderId])
SELECT lcc.[TenantId]
	,lcc.[LockerId]
	,cc.[CardHolderId] 
FROM [slk].[LockerCardCredentials] lcc 
INNER JOIN [slk].CardCredentials cc ON lcc.CardCredentialId = cc.CardCredentialId

INSERT INTO [slk].[LockerLeases] ([TenantId]
	,[LockerLeaseId]
	,[StartedAt]
	,[EndedAt]
	,[LockerBankBehaviour]
	,[EndedByMasterCard]
	,[CardCredentialId]
	,[CardHolderId]
	,[LockerId]
	,[LockId])
SELECT lcc.[TenantId]
	,NEWID()
	,SYSDATETIMEOFFSET()
	,null
	,1
	,0
	,cc.CardCredentialId
	,cc.CardHolderId
	,lcc.LockerId
	,l.LockId
FROM [slk].[LockerCardCredentials] lcc 
INNER JOIN [slk].CardCredentials cc ON lcc.CardCredentialId = cc.CardCredentialId
INNER JOIN [slk].Locks l ON lcc.LockerId = l.LockerId
