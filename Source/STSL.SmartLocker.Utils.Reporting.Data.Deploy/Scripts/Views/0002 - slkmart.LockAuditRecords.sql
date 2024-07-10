DROP VIEW IF EXISTS slkmart.LockAuditRecords;
GO

CREATE VIEW slkmart.LockAuditRecords
AS
SELECT
    la.LockId
    ,la.AuditType
    ,ISNULL(t.Category, N'Unknown') AS AuditCategory
    ,ISNULL(t.[Description], N'Unknown event') AS AuditDescription
    ,CAST(la.[Time] AS datetime2(0)) AS AuditTime
    ,cc1.CardCredential AS AuditSubject
    ,cc2.CardCredential AS AuditObject
    ,la.BoundaryAddress
    ,la.LockAuditId
FROM
    slkmart.LockAudits AS la
    LEFT OUTER JOIN slkmart.AuditTypes AS t ON la.AuditType = t.AuditType
    LEFT OUTER JOIN slkmart.CardCredentials AS cc1 ON la.AuditSubjectId = cc1.CardCredentialId
    LEFT OUTER JOIN slkmart.CardCredentials AS cc2 ON la.AuditObjectId = cc2.CardCredentialId;

GO
