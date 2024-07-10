DROP PROCEDURE IF EXISTS slkmart.ProcessBluBugMessage;
GO

CREATE PROCEDURE slkmart.ProcessBluBugMessage
	@originAddress int
	,@originTimestamp datetime2(6) NULL = null
    ,@serverTimestamp datetime2(6)
    ,@boundaryAddress int
    ,@readingSeqno bigint NULL = null
    ,@auditTypeCode tinyint NULL = null
    ,@auditSubject char(16) NULL = null
    ,@auditObject char(16) NULL = null
    ,@readingBatteryVoltage decimal(4,3) NULL = null
    ,@readingVdd decimal(4,3) NULL = null
    ,@originUrgent bit NULL = null
    ,@isDuplicate bit NULL OUTPUT
AS
BEGIN
    SET @isDuplicate = 0;

    -- keep alive messages are treated as a special case (as high volume)
    IF (@auditTypeCode IN (80, 81))
    BEGIN

        EXEC slkmart.ProcessKeepAliveMessage
            @originAddress = @originAddress
            ,@originTimestamp = @originTimestamp
            ,@serverTimestamp = @serverTimestamp
            ,@boundaryAddress = @boundaryAddress
            ,@readingSeqno = @readingSeqno
            ,@auditTypeCode = @auditTypeCode
            ,@isDuplicate = @isDuplicate OUTPUT

        RETURN;
    END

    -- check for duplicates

    IF (@auditTypeCode IS NULL)
    BEGIN
        -- we expect (@originAddress, @serverTimestamp) to be unique
        IF EXISTS(SELECT 1 FROM slkmart.BluBugMessages WHERE OriginAddress = @originAddress AND ServerTimestamp = @serverTimestamp)
        BEGIN
            -- duplicate, do nothing
            SET @isDuplicate = 1;
            RETURN;
        END
    END
    ELSE -- @auditTypeCode IS NOT NULL
    BEGIN
        -- we expect (@originAddress, @readingSeqno) to be unique
        IF EXISTS(SELECT 1 FROM slkmart.BluBugMessages WHERE OriginAddress = @originAddress AND ReadingSeqno = @readingSeqno)
        BEGIN
            -- duplicate, do nothing
            SET @isDuplicate = 1;
            RETURN;
        END
    END    

    -- insert the unparsed message
    INSERT INTO slkmart.BluBugMessages
    (
        OriginAddress
        ,OriginTimestamp
        ,ServerTimestamp
        ,BoundaryAddress
        ,ReadingSeqno
        ,AuditTypeCode
        ,AuditSubject
        ,AuditObject
        ,ReadingBatteryVoltage
        ,ReadingVdd
        ,OriginUrgent
    )
    VALUES
    (
        @originAddress
        ,@originTimestamp
        ,@serverTimestamp
        ,@boundaryAddress
        ,@readingSeqno
        ,@auditTypeCode
        ,@auditSubject
        ,@auditObject
        ,@readingBatteryVoltage
        ,@readingVdd
        ,@originUrgent
    );

    DECLARE @bluBugMessageId bigint = SCOPE_IDENTITY();

    -- record lock battery voltages
    IF @readingBatteryVoltage IS NOT NULL
    BEGIN
        -- we check again for duplicates, as the BluBugMessages table can be emptied out
        IF EXISTS(SELECT 1 FROM slkmart.LockBatteryVoltages WHERE LockId = @originAddress AND [Time] = @serverTimestamp)
        BEGIN
            SET @isDuplicate = 1;
            RETURN;
        END

        -- record into the fact table
        INSERT INTO slkmart.LockBatteryVoltages
        (
            LockId
            ,[Time]
            ,BatteryVoltage
            ,_bluBugMessageId
        )
        VALUES
        (
            @originAddress
            ,@serverTimestamp
            ,@readingBatteryVoltage
            ,@bluBugMessageId
        );

        -- record into the summary Locks table (update or insert)
        IF NOT EXISTS (SELECT LockId FROM slkmart.Locks WHERE LockId = @originAddress)
        BEGIN
            INSERT INTO slkmart.Locks
            (
                LockId
                ,LastBatteryVoltage
                ,LastBatteryVoltageTime
                ,LastBoundaryAddress
            )
            VALUES
            (
                @originAddress
                ,@readingBatteryVoltage
                ,@serverTimestamp
                ,@boundaryAddress
            );
        END
        ELSE
        BEGIN
            UPDATE slkmart.Locks SET
                LastBatteryVoltage = @readingBatteryVoltage
                ,LastBatteryVoltageTime = @serverTimestamp
                ,LastBoundaryAddress = @boundaryAddress
            WHERE
                LockId = @originAddress
                -- only update if newer
                AND
                (
                    LastBatteryVoltageTime IS NULL
                    OR LastBatteryVoltageTime <= @serverTimestamp
                );
        END

    END -- @readingBatteryVoltage IS NOT NULL

    -- record lock audits
    IF @auditTypeCode IS NOT NULL
    BEGIN
        -- we check again for duplicates, as the BluBugMessages table can be emptied out
        IF EXISTS(SELECT 1 FROM slkmart.LockAudits WHERE LockId = @originAddress AND ReadingSeqno = @readingSeqno)
        BEGIN
            SET @isDuplicate = 1;
            RETURN;
        END

        DECLARE @auditSubjectId int;
        DECLARE @auditObjectId int;
        
        EXEC slkmart.GetWithCreateCardCredentialId @auditSubject, @auditSubjectId OUTPUT;
        EXEC slkmart.GetWithCreateCardCredentialId @auditObject, @auditObjectId OUTPUT;

        -- record in the LockAuditsTable
        INSERT INTO slkmart.LockAudits
        (
            LockId
            ,AuditType
            ,AuditSubjectId
            ,AuditObjectId
            ,[Time]
            ,ReadingSeqno
            ,_bluBugMessageId
            ,BoundaryAddress
        )
        VALUES
        (
            @originAddress
            ,@auditTypeCode
            ,@auditSubjectId
            ,@auditObjectId
            ,@originTimestamp
            ,@readingSeqno
            ,@bluBugMessageId
            ,@boundaryAddress
        );

        DECLARE @lockAuditId bigint = SCOPE_IDENTITY();

        -- record into the summary Locks table (update or insert)
        IF NOT EXISTS (SELECT LockId FROM slkmart.Locks WHERE LockId = @originAddress)
        BEGIN
            INSERT INTO slkmart.Locks
            (
                LockId
                ,LastAuditEventAuditType
                ,LastAuditEventTime
                ,LastAuditEventLockAuditId
                ,LastBoundaryAddress
            )
            VALUES
            (
                @originAddress
                ,@auditTypeCode
                ,@originTimestamp
                ,@lockAuditId
                ,@boundaryAddress
            );
        END
        ELSE
        BEGIN
            UPDATE slkmart.Locks SET
                LastAuditEventAuditType = @auditTypeCode
                ,LastAuditEventTime = @originTimestamp
                ,LastAuditEventLockAuditId = @lockAuditId
                ,LastBoundaryAddress = @boundaryAddress
            WHERE
                LockId = @originAddress
                -- only update if newer
                AND
                (
                    LastAuditEventTime IS NULL
                    OR LastAuditEventTime <= @originTimestamp
                );
        END

    END -- @auditTypeCode IS NOT NULL

END
GO
