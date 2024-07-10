SELECT c.[TenantId] [Tenant Id], c.[LockConfigEventAuditId] [Event Audit Id], c.[EntityId] [Entity Id],  c.[CreatedAtUTC] [Created At], c.[UpdatedByUserEmail] [User Email],
CASE c.[EventType] WHEN 1 THEN 'Lock' WHEN 2 THEN 'Locker' WHEN 3 THEN 'Locker Bank' END AS [Config Change Type],
COALESCE(lb.[Name], l.[Label], CAST(k.[SerialNumber] AS NVARCHAR)) as [Name / Label / Serial Number],
COALESCE(lb.[Description], l.[ServiceTag]) AS [Description / Service Tag]
FROM [slk].[LockConfigEventAudits] c
LEFT JOIN [slk].[Locks] k ON k.[LockId] = c.[EntityId] AND c.[EventType] = 1
LEFT JOIN [slk].[Lockers] l ON l.[LockerId] = c.[EntityId] AND c.[EventType] = 2
LEFT JOIN [slk].[LockerBanks] lb ON lb.[LockerBankId] = c.[EntityId] AND c.[EventType] = 3
ORDER BY c.[CreatedAtUTC] DESC;