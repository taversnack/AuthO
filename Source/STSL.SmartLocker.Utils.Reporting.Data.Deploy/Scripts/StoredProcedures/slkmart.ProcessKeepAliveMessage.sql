DROP PROCEDURE IF EXISTS slkmart.ProcessKeepAliveMessage;
GO

CREATE PROCEDURE slkmart.ProcessKeepAliveMessage
	@originAddress int
	,@originTimestamp datetime2(6)
    ,@serverTimestamp datetime2(6)
    ,@boundaryAddress int
    ,@readingSeqno bigint
    ,@auditTypeCode tinyint
    ,@isDuplicate bit OUTPUT
AS
BEGIN

    -- check for duplicates

    IF EXISTS(SELECT 1 FROM slkmart.KeepAliveMessages WHERE OriginAddress = @originAddress AND ReadingSeqno = @readingSeqno)
    BEGIN
        -- duplicate, do nothing
        SET @isDuplicate = 1;
        RETURN;
    END

    SET @isDuplicate = 0;

    -- insert the keep alive message
    INSERT INTO slkmart.KeepAliveMessages
    (
        OriginAddress
        ,OriginTimestamp
        ,ServerTimestamp
        ,BoundaryAddress
        ,ReadingSeqno
        ,CardObserved
    )
    VALUES
    (
        @originAddress
        ,@originTimestamp
        ,@serverTimestamp
        ,@boundaryAddress
        ,@readingSeqno
        ,CASE @auditTypeCode WHEN 81 THEN 1 ELSE 0 END
    );

    -- record into the summary Locks table (update or insert)
    IF NOT EXISTS (SELECT LockId FROM slkmart.Locks WHERE LockId = @originAddress)
    BEGIN
        INSERT INTO slkmart.Locks
        (
            LockId
            ,LastKeepAliveTime
            ,LastBoundaryAddress
        )
        VALUES
        (
            @originAddress
            ,@originTimestamp
            ,@boundaryAddress
        );
    END
    ELSE
    BEGIN
        UPDATE slkmart.Locks SET
            LastKeepAliveTime = @originTimestamp
            ,LastBoundaryAddress = @boundaryAddress
        WHERE
            LockId = @originAddress
            -- only update if newer
            AND
            (
                LastKeepAliveTime IS NULL
                OR LastKeepAliveTime <= @originTimestamp
            );
    END

END
GO
