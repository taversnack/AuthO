DROP VIEW IF EXISTS slkmart.LockCurrentStatuses;
GO

CREATE VIEW slkmart.LockCurrentStatuses
AS
SELECT
    l.LockId
    ,l.LastBatteryVoltage AS Battery
    ,l.LastAuditEventAuditType AS AuditType
    ,t.Category AS AuditCategory
    ,t.[Description] AS AuditDescription
    ,CAST(la.[Time] AS datetime2(0)) AS AuditTime
    ,cc1.CardCredential AS AuditSubject
    ,cc2.CardCredential AS AuditObject
    ,CAST(
        (
            SELECT MAX(v.[time])
            FROM (VALUES (la.[Time]), (l.LastKeepAliveTime), (l.LastBatteryVoltageTime)) AS v ([time])
        ) AS datetime2(0)
    ) AS LastCommunication
    ,l.LastBoundaryAddress
FROM
    slkmart.Locks AS l
    LEFT OUTER JOIN slkmart.AuditTypes AS t ON l.LastAuditEventAuditType = t.AuditType
    LEFT OUTER JOIN slkmart.LockAudits AS la ON l.LastAuditEventLockAuditId = la.LockAuditId
    LEFT OUTER JOIN slkmart.CardCredentials AS cc1 ON la.AuditSubjectId = cc1.CardCredentialId
    LEFT OUTER JOIN slkmart.CardCredentials AS cc2 ON la.AuditObjectId = cc2.CardCredentialId;

GO