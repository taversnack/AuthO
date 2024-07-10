IF NOT EXISTS (SELECT schema_id FROM sys.schemas WHERE [name] = 'slkmart')
BEGIN
    EXEC (N'CREATE SCHEMA [slkmart]');
END

IF (NOT EXISTS (SELECT null FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'slkmart' AND  TABLE_NAME = 'BluBugRawMessages'))
BEGIN
    CREATE TABLE slkmart.BluBugRawMessages
    (
        BluBugRawMessageId bigint IDENTITY
        ,[Message] varchar(max)
        ,_created datetime2 NOT NULL DEFAULT(SYSUTCDATETIME())
        ,CONSTRAINT PK_BluBugRawMessages PRIMARY KEY CLUSTERED
        (
            BluBugRawMessageId
        )
    );
END

IF (NOT EXISTS (SELECT null FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'slkmart' AND  TABLE_NAME = 'BluBugMessages'))
BEGIN
    CREATE TABLE slkmart.BluBugMessages
    (
        BluBugMessageId bigint IDENTITY
        ,BluBugRawMessageId bigint NOT NULL
        ,OriginAddress int NOT NULL
        ,OriginTimestamp datetime2(6)
        ,ServerTimestamp datetime2(6) NOT NULL
        ,BoundaryAddress int NOT NULL
        ,ReadingSeqno bigint
        ,AuditTypeCode int
        ,AuditSubject char(16)
        ,AuditObject char(16)
        ,ReadingBatteryVoltage decimal(4,3)
        ,ReadingVdd decimal(4,3)
        ,OriginUrgent bit
        ,_created datetime2 NOT NULL DEFAULT(SYSUTCDATETIME())
        ,CONSTRAINT PK_BluBugMessages PRIMARY KEY CLUSTERED
        (
            BluBugMessageId
        )
    );
END

IF (NOT EXISTS (SELECT null FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'slkmart' AND  TABLE_NAME = 'AuditTypes'))
BEGIN
    CREATE TABLE slkmart.AuditTypes
    (
        AuditType tinyint NOT NULL
        ,Category nvarchar(25) NOT NULL
        ,[Description] nvarchar(100) NOT NULL
        ,CONSTRAINT PK_AuditTypes PRIMARY KEY CLUSTERED
        (
            AuditType
        )
    );
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (10, N'Reject', N'The card was rejected because it is not registered');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (11, N'Reject', N'Security sweep card is rejected because there is no security sweep active.');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (12, N'Reject', N'An otherwise valid card is rejected because the locker is in a security sweep lockdown.');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (13, N'Reject', N'Card is rejected because the locker has been confiscated.');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (14, N'Reject', N'Refusing to lock because the battery voltage is too low and we might not be able to unlock.');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (16, N'Reject', N'Card is rejected because a welcome card has been used with an incompatible card type');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (17, N'Reject', N'Welcome card is rejected because the shared locker is closed.');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (18, N'Reject', N'Card is rejected because the wrong user is trying to open a shared locker.');

    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (20, N'Failed To Open', N'Failed to open - user');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (21, N'Failed To Open', N'Failed to open - master card');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (22, N'Failed To Open', N'Failed to open - security sweep');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (23, N'Failed To Open', N'Failed to open - install mode');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (24, N'Failed To Open', N'Failed to open - any registered user following security sweep');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (28, N'Failed To Open', N'Failed to open - broken open');

    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (30, N'Operator Error', N'Door not closed in time - user');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (31, N'Operator Error', N'Door not closed in time - master card');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (32, N'Operator Error', N'Door not closed in time - security sweep');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (33, N'Operator Error', N'Door not closed in time - install mode');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (37, N'Operator Error', N'Door not closed in time - welcome card');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (40, N'Operator Error', N'Welcome card presented, but no guest card was welcomed');

    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (50, N'Opening', N'Opened by a user');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (51, N'Opening', N'Master card open');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (52, N'Opening', N'Security sweep open');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (53, N'Opening', N'Opened in install mode');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (54, N'Opening', N'Opened by any registered user following security sweep');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (58, N'Opening', N'Broken open - lock has fully unlocked');

    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (60, N'Locking', N'Locked by a user');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (61, N'Locking', N'Master card lock');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (62, N'Locking', N'Security sweep lock');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (63, N'Locking', N'Locked in install mode');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (67, N'Locking', N'Locked using a welcome card');

    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (80, N'Other', N'Keep-alive');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (81, N'Other', N'Card observed');
    INSERT INTO slkmart.AuditTypes (AuditType, Category, [Description]) VALUES (88, N'Other', N'Lock tampering occurred - no action taken by lock');
END

IF (NOT EXISTS (SELECT null FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'slkmart' AND  TABLE_NAME = 'CardCredentials'))
BEGIN
    CREATE TABLE slkmart.CardCredentials
    (
        CardCredentialId int IDENTITY
        ,CardCredential char(16) NOT NULL
        ,_created datetime2 NOT NULL DEFAULT(SYSUTCDATETIME())
        ,CONSTRAINT PK_CardCredentials PRIMARY KEY CLUSTERED
        (
            CardCredentialId
        )
    );

    CREATE UNIQUE INDEX IX_CardCredentials_CardCredentialId ON slkmart.CardCredentials
    (
	    CardCredential
    )
END

IF (NOT EXISTS (SELECT null FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'slkmart' AND  TABLE_NAME = 'Locks'))
BEGIN
    CREATE TABLE slkmart.Locks
    (
        LockId int NOT NULL
        ,LastBatteryVoltage decimal(4,3)
        ,LastBatteryVoltageTime datetime2(6)
        ,LastAuditEventAuditType tinyint
        ,LastAuditEventTime datetime2(6)
        ,LastAuditEventLockAuditId bigint
        ,LastKeepAliveTime datetime2(6)
        ,CONSTRAINT PK_Locks PRIMARY KEY CLUSTERED
        (
            LockId
        )
    );
END

IF (NOT EXISTS (SELECT null FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'slkmart' AND  TABLE_NAME = 'LockBatteryVoltages'))
BEGIN
    CREATE TABLE slkmart.LockBatteryVoltages
    (
        LockId int NOT NULL
        ,[Time] datetime2(6) NOT NULL
        ,BatteryVoltage decimal(4,3) NOT NULL
        ,_bluBugMessageId bigint
        ,CONSTRAINT PK_LockBatteryVoltages PRIMARY KEY CLUSTERED
        (
            LockId
            ,[Time] DESC
        )
    );
END

IF (NOT EXISTS (SELECT null FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'slkmart' AND  TABLE_NAME = 'LockAudits'))
BEGIN
    CREATE TABLE slkmart.LockAudits
    (
        LockAuditId bigint IDENTITY
        ,LockId int NOT NULL
        ,AuditType tinyint NOT NULL
        ,AuditSubjectId int
        ,AuditObjectId int
        ,[Time] datetime2(6) NOT NULL
        ,ReadingSeqno bigint NOT NULL
        ,_bluBugMessageId bigint
        ,CONSTRAINT PK_LockAudits PRIMARY KEY CLUSTERED
        (
            LockAuditId
        )
    );

    CREATE UNIQUE INDEX IX_LockAudits_LockId_Time ON slkmart.LockAudits
    (
	    LockId
        ,[Time] DESC
    )
    INCLUDE
    (
        AuditType
        ,AuditSubjectId
        ,AuditObjectId
        ,ReadingSeqno
        ,_bluBugMessageId
    )
END

GO