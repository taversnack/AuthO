IF (NOT EXISTS (SELECT null FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'slkmart' AND  TABLE_NAME = 'KeepAliveMessages'))
BEGIN
    CREATE TABLE slkmart.KeepAliveMessages
    (
        KeepAliveMessagesId bigint IDENTITY
        ,OriginAddress int NOT NULL
        ,OriginTimestamp datetime2(6) NOT NULL
        ,ServerTimestamp datetime2(6) NOT NULL
        ,BoundaryAddress int NOT NULL
        ,ReadingSeqno bigint NOT NULL
        ,_created datetime2 NOT NULL DEFAULT(SYSUTCDATETIME())
        ,CardObserved bit NOT NULL DEFAULT(0)
        ,CONSTRAINT PK_KeepAliveMessages PRIMARY KEY CLUSTERED
        (
            KeepAliveMessagesId
        )
    );

    CREATE NONCLUSTERED INDEX IX_BluBlugKeepAliveMessages_OriginAddress ON slkmart.KeepAliveMessages
    (
    	OriginAddress,
        ReadingSeqno
    )
    INCLUDE
    (
        OriginTimestamp
    );
END
