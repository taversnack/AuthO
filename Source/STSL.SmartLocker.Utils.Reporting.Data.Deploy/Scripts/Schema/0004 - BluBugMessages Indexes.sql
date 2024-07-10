CREATE NONCLUSTERED INDEX IX_BluBugMessages_OriginAddress_ServerTimestamp ON slkmart.BluBugMessages
(
    OriginAddress,
    ServerTimestamp
);

CREATE NONCLUSTERED INDEX IX_BluBugMessages_OriginAddress_ReadingSeqno ON slkmart.BluBugMessages
(
    OriginAddress,
    ReadingSeqno
);

GO
