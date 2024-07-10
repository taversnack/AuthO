DROP VIEW IF EXISTS slk.LockersWithStatus;
GO

CREATE VIEW slk.LockersWithStatus
AS
SELECT
    LockerBanks.TenantId
    ,LockerBanks.LocationId
    ,LockerBanks.LockerBankId
    ,Lockers.LockerId
    ,Locks.LockId
    ,Lockers.[Label]
    ,Lockers.ServiceTag
    ,Lockers.SecurityType
    ,SingleAssignedTo.[Name] AS AssignedTo
    ,SingleAssignedTo.CardHolderId AS AssignedToCardHolderId
    ,SingleAssignedTo.[UniqueIdentifier] AS AssignedToUniqueIdentifier
    ,AssignedToMany.[Count] AS AssignedToManyCount
    ,Locks.SerialNumber AS LockSerialNumber
    ,Locks.FirmwareVersion AS LockFirmwareVersion
    ,Locks.OperatingMode AS LockOperatingMode
    ,LockCurrentStatuses.Battery AS BatteryVoltage
    ,LockCurrentStatuses.AuditType AS LastAudit
    ,LockCurrentStatuses.AuditCategory AS LastAuditCategory
    ,LockCurrentStatuses.AuditDescription AS LastAuditDescription
    ,LockCurrentStatuses.AuditTime AS LastAuditTime
    ,ch1.CardHolderId AS LastAuditSubjectId
    ,ch2.CardHolderId AS LastAuditObjectId
    ,ch1.[UniqueIdentifier] AS LastAuditSubjectUniqueIdentifier
    ,ch2.[UniqueIdentifier] AS LastAuditObjectUniqueIdentifier
    ,ISNULL(ch1.FirstName + N' ' + ch1.LastName, LockCurrentStatuses.AuditSubject) AS LastAuditSubject
    ,ISNULL(ch2.FirstName + N' ' + ch2.LastName, LockCurrentStatuses.AuditObject) AS LastAuditObject
    ,LockCurrentStatuses.AuditSubject AS LastAuditSubjectSN
    ,LockCurrentStatuses.AuditObject AS LastAuditObjectSN
    ,cc1.CardCredentialId AS LastAuditSubjectCardCredentialId
    ,cc2.CardCredentialId AS LastAuditObjectCardCredentialId
    ,LockCurrentStatuses.LastCommunication
    ,LockCurrentStatuses.LastBoundaryAddress AS BoundaryAddress
FROM
    slk.LockerBanks
    INNER JOIN slk.Lockers ON LockerBanks.LockerBankId = Lockers.LockerBankId
    LEFT OUTER JOIN slk.Locks ON Lockers.LockerId = Locks.LockerId
    LEFT OUTER JOIN slkmart.LockCurrentStatuses on Locks.SerialNumber = LockCurrentStatuses.LockId
    LEFT OUTER JOIN slk.CardCredentials AS cc1 ON LockCurrentStatuses.AuditSubject = cc1.SerialNumber
    LEFT OUTER JOIN slk.CardCredentials AS cc2 ON LockCurrentStatuses.AuditObject = cc1.SerialNumber
    LEFT OUTER JOIN slk.CardHolders AS ch1 ON cc1.CardHolderId = ch1.CardHolderId
    LEFT OUTER JOIN slk.CardHolders AS ch2 ON cc2.CardHolderId = ch2.CardHolderId
OUTER APPLY
(
    SELECT TOP 1
        x.CardHolderId
        ,x.[Name]
        ,x.[UniqueIdentifier]
    FROM
	(
		SELECT 
			ch.CardHolderId as CardHolderId
			,ch.FirstName + N' ' + ch.LastName AS [Name]
			,ch.[UniqueIdentifier]
		FROM slk.CardHolders ch INNER JOIN slk.LockerOwners lo ON lo.CardHolderId = ch.CardHolderId
		WHERE lo.LockerId = Lockers.LockerId
		UNION ALL
		SELECT 
			ch.CardHolderId as CardHolderId
			,ch.FirstName + N' ' + ch.LastName AS [Name]
			,ch.[UniqueIdentifier]
		FROM slk.CardHolders ch INNER JOIN slk.LockerLeases ll ON ll.CardHolderId = ch.CardHolderId
		WHERE ll.LockerLeaseId = Lockers.CurrentLeaseId
	) AS x
	-- Theres probably a much smarter way to do this ^
) AS SingleAssignedTo
OUTER APPLY
(
	SELECT COUNT(*) AS [Count]
	FROM slk.CardHolders ch INNER JOIN slk.LockerOwners lo ON lo.CardHolderId = ch.CardHolderId
	WHERE lo.LockerId = Lockers.LockerId
) AS AssignedToMany;

GO