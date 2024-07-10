DROP PROCEDURE IF EXISTS slk.ListAuditRecordsForLocker;
GO

CREATE PROCEDURE slk.ListAuditRecordsForLocker
    @tenantId uniqueidentifier
    ,@lockerId uniqueidentifier
	,@rowCount int = null OUTPUT
	,@pageNumber int = 1
	,@pageSize int = 100 -- default to returning the top 100 audit records
AS
BEGIN
    DECLARE @results TABLE
    (
        Locker nvarchar(256) NOT NULL
        ,AuditCategory nvarchar(25) NULL
        ,AuditDescription nvarchar(100) NULL
        ,AuditTime datetime2(0) NOT NULL
        ,[Subject] nvarchar(513)
        ,[Object] nvarchar(513)
        ,LockSerialNumber int NOT NULL
        ,AuditType int NOT NULL
        ,SubjectSN char(16)
        ,ObjectSN char(16)
    );

    INSERT INTO @results
    SELECT
        Lockers.[Label] AS Locker
        ,LockAuditRecords.AuditCategory
        ,LockAuditRecords.AuditDescription
        ,LockAuditRecords.AuditTime AS [Time]
        ,ISNULL(ch1.FirstName + N' ' + ch1.LastName, LockAuditRecords.AuditSubject) AS [Subject]
        ,ISNULL(ch2.FirstName + N' ' + ch2.LastName, LockAuditRecords.AuditObject) AS [Object]
        ,Locks.SerialNumber AS LockSerialNumber
        ,LockAuditRecords.AuditType
        ,LockAuditRecords.AuditSubject AS SubjectSN
        ,LockAuditRecords.AuditObject AS ObjectSN
    FROM
        slk.Lockers
        INNER JOIN slk.Locks ON Lockers.LockerId = Locks.LockerId
        INNER JOIN slkmart.LockAuditRecords ON Locks.SerialNumber = LockAuditRecords.LockId
        LEFT OUTER JOIN slk.CardCredentials AS cc1 ON LockAuditRecords.AuditSubject = cc1.SerialNumber
        LEFT OUTER JOIN slk.CardCredentials AS cc2 ON LockAuditRecords.AuditObject = cc1.SerialNumber
        LEFT OUTER JOIN slk.CardHolders AS ch1 ON cc1.CardHolderId = ch1.CardHolderId
        LEFT OUTER JOIN slk.CardHolders AS ch2 ON cc2.CardHolderId = ch2.CardHolderId
    WHERE
        Lockers.TenantId = @tenantId
        AND Lockers.LockerId = @lockerId;

    -- return the total row count
	SELECT @rowCount = COUNT(1) FROM @results;

	-- return paged data
	WITH ctepaging AS
	(
		SELECT
			*,
			ROW_NUMBER() OVER
			(
				ORDER BY
					AuditTime DESC
			) AS rownum
		FROM
			@results
	)
	SELECT
		*
	FROM
		ctepaging
	WHERE
		rownum BETWEEN (@pageNumber - 1) * @pageSize + 1 AND @pageNumber * @pageSize;
END

GO