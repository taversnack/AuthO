CREATE NONCLUSTERED INDEX IX_LockAudits_LockId_ReadingSeqno ON slkmart.LockAudits
(
    LockId,
    ReadingSeqno
);

DROP INDEX IF EXISTS IX_LockAudits_LockId_Time ON slkmart.LockAudits;

CREATE NONCLUSTERED INDEX IX_LockAudits_LockId_Time ON slkmart.LockAudits
(
    LockId,
    [Time]
)
INCLUDE
(
    AuditType,
    AuditSubjectId,
    AuditObjectId
);

GO
